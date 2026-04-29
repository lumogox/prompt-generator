using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace PromptAssistant.Http.Providers;

/// <summary>
/// HTTP-based provider that talks to a local Ollama daemon. Unlike the CLI providers, this one has
/// no executable on PATH — availability is decided by whether the daemon answers on its REST port.
/// </summary>
public sealed class OllamaProvider : IAiProvider, IDisposable
{
    public const string DefaultBaseUrl = "http://localhost:11434";
    public const string DefaultModel = "llama3";

    private static readonly ILogger _log = Log.ForContext<OllamaProvider>();

    private readonly HttpClient _http;
    private readonly Uri _baseUrl;

    public OllamaProvider(string? baseUrl = null)
    {
        _baseUrl = new Uri(string.IsNullOrWhiteSpace(baseUrl) ? DefaultBaseUrl : baseUrl);
        _http = new HttpClient { BaseAddress = _baseUrl, Timeout = TimeSpan.FromMinutes(5) };
    }

    public string ProviderName => "Ollama";

    public ProviderKind Kind => ProviderKind.LocalHttp;

    /// <summary>Override of the default model. Null/empty means "use llama3".</summary>
    public string? Model { get; set; }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        // Dedicated short-timeout client so the footer health dot can't hang on a dead daemon.
        using var probe = new HttpClient { BaseAddress = _baseUrl, Timeout = TimeSpan.FromSeconds(2) };
        try
        {
            using var response = await probe.GetAsync("/api/tags", ct).ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException) { return false; }
        catch (TaskCanceledException) { return false; }
    }

    public async Task<CliResult> ExecuteAsync(string prompt, CancellationToken ct = default)
    {
        var model = string.IsNullOrWhiteSpace(Model) ? DefaultModel : Model!;
        var sw = Stopwatch.StartNew();
        _log.Information("Ollama call starting (prompt {PromptChars} chars, model {Model})", prompt.Length, model);

        // num_predict caps the model's output token budget. 4096 comfortably covers the 10-section
        // JSON envelope (~1500-2500 tokens typical) while still bounding pathological infinite output.
        // Without an explicit cap, smaller local models can hit their default ceiling and truncate the
        // JSON mid-field, producing un-parseable responses.
        var payload = new
        {
            model,
            prompt,
            stream = true,
            options = new { num_predict = 4096 },
        };

        try
        {
            using var response = await _http.PostAsJsonAsync("/api/generate", payload, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _log.Warning("Ollama HTTP {Status} body: {Body}", (int)response.StatusCode, body);
                return new CliResult(
                    Text: "",
                    ExitCode: (int)response.StatusCode,
                    StdErr: $"Ollama returned HTTP {(int)response.StatusCode}: {body}",
                    Duration: sw.Elapsed);
            }

            var buffer = new StringBuilder();
            string? doneReason = null;
            int chunkCount = 0;

            await using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
            {
                if (line.Length == 0) continue;
                JsonNode? node;
                try
                {
                    node = JsonNode.Parse(line);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    _log.Warning(ex, "Skipping malformed NDJSON line from Ollama: {Line}", line);
                    continue;
                }

                if (node?["response"]?.GetValue<string>() is { } chunk)
                {
                    buffer.Append(chunk);
                    chunkCount++;
                }
                if (node?["done"]?.GetValue<bool>() == true)
                {
                    // Capture the reason the daemon stopped — "stop" (natural), "length" (hit num_predict),
                    // "load" (model loading hiccup). Critical for diagnosing truncation.
                    doneReason = node?["done_reason"]?.GetValue<string>();
                    break;
                }
            }

            var fullBody = buffer.ToString();
            _log.Information(
                "Ollama parsed OK (model={Model}, chunks={ChunkCount}, chars={ResponseChars}, doneReason={DoneReason}, duration={DurationMs}ms)",
                model, chunkCount, fullBody.Length, doneReason ?? "<unspecified>", sw.Elapsed.TotalMilliseconds);
            // Full body at Debug so the Information channel stays clean for normal use, but the raw
            // response is still recoverable when diagnosing parse failures (turn on Debug in Serilog
            // config). The summary line above is what you scan first.
            _log.Debug("Ollama full response body ({ResponseChars} chars):\n{Body}", fullBody.Length, fullBody);
            return new CliResult(fullBody, 0, null, sw.Elapsed);
        }
        catch (HttpRequestException ex)
        {
            _log.Warning(ex, "Ollama daemon unreachable at {BaseUrl}", _baseUrl);
            return new CliResult(
                Text: "",
                ExitCode: -1,
                StdErr: $"Ollama daemon not reachable at {_baseUrl}. Is `ollama serve` running? ({ex.Message})",
                Duration: sw.Elapsed);
        }
    }

    public void Dispose() => _http.Dispose();
}
