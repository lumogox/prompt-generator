using PromptAssistant.Cli.Internal;

namespace PromptAssistant.Cli.Providers;

public sealed class CodexCliProvider(string executablePath) : IAiProvider
{
    private static readonly ILogger _log = Log.ForContext<CodexCliProvider>();

    private readonly ProcessRunner _runner = new(executablePath);

    public string ProviderName => "Codex";

    public ProviderKind Kind => ProviderKind.Cli;

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
        var tempPath = Path.Combine(Path.GetTempPath(), $"codex-out-{Guid.NewGuid():N}.txt");
        try
        {
            _log.Information("Codex call starting (prompt {PromptChars} chars, tempfile {TempPath}, model {Model})", prompt.Length, tempPath, Model ?? "<default>");
            var raw = await _runner.RunAsync(ProviderDefaults.Codex(prompt, tempPath, Model), ct).ConfigureAwait(false);
            _log.Information("Codex exit={ExitCode} duration={DurationMs}ms",
                raw.ExitCode, raw.Duration.TotalMilliseconds);

            if (!raw.Succeeded)
            {
                _log.Warning("Codex exited non-zero ({ExitCode}); stderr: {StdErr}", raw.ExitCode, raw.StdErr);
                return raw with { Text = "" };
            }

            if (!File.Exists(tempPath))
            {
                _log.Warning("Codex did not write expected output file at {TempPath}", tempPath);
                return raw with { Text = "", ExitCode = -1, StdErr = "Codex did not write the expected output file." };
            }

            var text = await File.ReadAllTextAsync(tempPath, ct).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(text))
            {
                _log.Warning("Codex output file at {TempPath} was empty", tempPath);
                return raw with { Text = "", ExitCode = -1, StdErr = "Codex output file was empty." };
            }

            _log.Information("Codex parsed OK ({ResponseChars} chars)", text.Length);
            return raw with { Text = text.TrimEnd() };
        }
        finally
        {
            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch (IOException) { /* swallow cleanup races */ }
            catch (UnauthorizedAccessException) { /* idem */ }
        }
    }
}
