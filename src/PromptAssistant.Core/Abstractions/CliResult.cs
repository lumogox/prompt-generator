namespace PromptAssistant.Core.Abstractions;

public sealed record CliResult(string Text, int ExitCode, string? StdErr, TimeSpan Duration)
{
    public bool Succeeded => ExitCode == 0;
}
