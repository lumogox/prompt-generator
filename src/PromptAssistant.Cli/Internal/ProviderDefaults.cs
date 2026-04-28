namespace PromptAssistant.Cli.Internal;

internal static class ProviderDefaults
{
    // Gemini CLI — non-interactive headless mode.
    //   -p, --prompt        triggers single-query headless execution.
    //   --output-format json returns {response, stats, error} envelope (we parse `response`).
    //   --model <name>      optional override; only emitted when set.
    public static IReadOnlyList<string> Gemini(string prompt, string? model = null)
    {
        var args = new List<string> { "-p", prompt, "--output-format", "json" };
        AddModelFlag(args, model);
        return args;
    }

    // Claude Code CLI — programmatic mode for desktop subscription auth.
    //   -p                                            print mode (one-shot, non-interactive).
    //   --output-format json                          parseable result envelope.
    //   --tools ""                                    disables all built-in tools — response only.
    //   --no-session-persistence                      our calls don't pollute session history.
    //   --exclude-dynamic-system-prompt-sections      better prompt-cache reuse across calls.
    //   --model <alias-or-name>                       optional override; only emitted when set.
    public static IReadOnlyList<string> Claude(string prompt, string? model = null)
    {
        var args = new List<string>
        {
            "-p", prompt,
            "--output-format", "json",
            "--tools", "",
            "--no-session-persistence",
            "--exclude-dynamic-system-prompt-sections",
        };
        AddModelFlag(args, model);
        return args;
    }

    // Codex CLI — sandboxed exec with final-message tempfile capture.
    //   exec, --ask-for-approval never, --sandbox read-only, -o <tempfile>
    //   --model <name>      optional override; only emitted when set.
    public static IReadOnlyList<string> Codex(string prompt, string outputFile, string? model = null)
    {
        var args = new List<string>
        {
            "exec", prompt,
            "--ask-for-approval", "never",
            "--sandbox", "read-only",
            "-o", outputFile,
        };
        AddModelFlag(args, model);
        return args;
    }

    private static void AddModelFlag(List<string> args, string? model)
    {
        if (string.IsNullOrWhiteSpace(model)) return;
        args.Add("--model");
        args.Add(model.Trim());
    }
}
