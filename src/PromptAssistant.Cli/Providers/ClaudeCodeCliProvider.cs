using PromptAssistant.Cli.Internal;

namespace PromptAssistant.Cli.Providers;

public sealed class ClaudeCodeCliProvider(string executablePath) : IAiCliProvider
{
    private static readonly ILogger _log = Log.ForContext<ClaudeCodeCliProvider>();

    private readonly ProcessRunner _runner = new(executablePath);

    public string ProviderName => "Claude Code";

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
        _log.Information("Claude call starting (prompt {PromptChars} chars, model {Model})", prompt.Length, Model ?? "<default>");
        var raw = await _runner.RunAsync(ProviderDefaults.Claude(prompt, Model), ct).ConfigureAwait(false);
        _log.Information("Claude exit={ExitCode} stdout={StdoutChars} duration={DurationMs}ms",
            raw.ExitCode, raw.Text.Length, raw.Duration.TotalMilliseconds);

        // Always try to parse the JSON envelope first — Claude Code returns structured error info
        // (e.g. "Not logged in · Please run /login") in the `result` field even when it exits non-zero.
        var json = ExtractJsonWindow(raw.Text);
        if (json is null)
        {
            _log.Warning("Claude response had no JSON envelope; returning {Mode}",
                raw.Succeeded ? "raw stdout" : "empty");
            return raw with { Text = raw.Succeeded ? raw.Text.Trim() : "" };
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var isError =
                (root.TryGetProperty("is_error", out var isErrorProp) && isErrorProp.ValueKind == JsonValueKind.True)
                || !raw.Succeeded;

            var resultText = root.TryGetProperty("result", out var resultProp) && resultProp.ValueKind == JsonValueKind.String
                ? resultProp.GetString() ?? ""
                : "";

            if (isError)
            {
                var stderr = !string.IsNullOrWhiteSpace(resultText)
                    ? $"Claude reported error: {resultText}"
                    : "Claude reported an error with no message.";
                _log.Warning("Claude reported error: {ErrorText}", resultText);
                return raw with { Text = "", ExitCode = raw.ExitCode == 0 ? -1 : raw.ExitCode, StdErr = stderr };
            }

            if (!string.IsNullOrEmpty(resultText))
            {
                _log.Information("Claude parsed OK ({ResponseChars} chars)", resultText.Length);
                return raw with { Text = resultText };
            }

            _log.Warning("Claude envelope missing 'result' field");
            return raw with { Text = "", ExitCode = -1, StdErr = "Claude envelope missing 'result' field." };
        }
        catch (JsonException ex)
        {
            _log.Warning(ex, "Failed to parse Claude JSON");
            return raw with { Text = "", ExitCode = -1, StdErr = $"Failed to parse Claude JSON: {ex.Message}" };
        }
    }

    private static string? ExtractJsonWindow(string text)
    {
        var first = text.IndexOf('{');
        var last = text.LastIndexOf('}');
        return (first >= 0 && last > first) ? text[first..(last + 1)] : null;
    }
}
