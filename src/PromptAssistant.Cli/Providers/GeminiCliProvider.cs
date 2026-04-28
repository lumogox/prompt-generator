using PromptAssistant.Cli.Internal;

namespace PromptAssistant.Cli.Providers;

public sealed class GeminiCliProvider(string executablePath) : IAiCliProvider
{
    private static readonly ILogger _log = Log.ForContext<GeminiCliProvider>();

    private readonly ProcessRunner _runner = new(executablePath);

    public string ProviderName => "Gemini";

    /// <summary>Override of the default model. Null/empty means "use the CLI's default".</summary>
    public string? Model { get; set; }

    public async Task<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (!File.Exists(executablePath)) return false;
        try
        {
            var probe = await _runner.RunAsync(["--version"], ct).ConfigureAwait(false);
            return probe.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<CliResult> ExecuteAsync(string prompt, CancellationToken ct = default)
    {
        _log.Information("Gemini call starting (prompt {PromptChars} chars, model {Model})", prompt.Length, Model ?? "<default>");
        var raw = await _runner.RunAsync(ProviderDefaults.Gemini(prompt, Model), ct).ConfigureAwait(false);
        _log.Information("Gemini exit={ExitCode} stdout={StdoutChars} duration={DurationMs}ms",
            raw.ExitCode, raw.Text.Length, raw.Duration.TotalMilliseconds);

        if (!raw.Succeeded)
        {
            _log.Warning("Gemini exited non-zero ({ExitCode}); stderr: {StdErr}", raw.ExitCode, raw.StdErr);
            return raw with { Text = "" };
        }

        // The Gemini CLI sometimes prepends diagnostic banners (e.g. "MCP issues detected. ...")
        // before the JSON envelope. Slice from the first '{' to the last '}' to be robust.
        var json = ExtractJsonWindow(raw.Text);
        if (json is null)
        {
            _log.Warning("Gemini response had no JSON envelope; returning raw text");
            return raw with { Text = raw.Text.Trim() };
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var error) && error.ValueKind is not JsonValueKind.Null)
            {
                var errText = error.ValueKind == JsonValueKind.String ? error.GetString() : error.ToString();
                _log.Warning("Gemini envelope reported error: {ErrorText}", errText);
                return raw with { Text = "", ExitCode = -1, StdErr = $"Gemini reported error: {errText}" };
            }

            if (root.TryGetProperty("response", out var response) && response.ValueKind == JsonValueKind.String)
            {
                var text = response.GetString() ?? "";
                _log.Information("Gemini parsed OK ({ResponseChars} chars)", text.Length);
                return raw with { Text = text };
            }

            _log.Warning("Gemini envelope missing 'response' field");
            return raw with { Text = "", ExitCode = -1, StdErr = "Gemini envelope missing 'response' field." };
        }
        catch (JsonException ex)
        {
            _log.Warning(ex, "Failed to parse Gemini JSON");
            return raw with { Text = "", ExitCode = -1, StdErr = $"Failed to parse Gemini JSON: {ex.Message}" };
        }
    }

    private static string? ExtractJsonWindow(string text)
    {
        var first = text.IndexOf('{');
        var last = text.LastIndexOf('}');
        return (first >= 0 && last > first) ? text[first..(last + 1)] : null;
    }
}
