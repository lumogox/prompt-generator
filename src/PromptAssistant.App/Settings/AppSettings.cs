namespace PromptAssistant.App.Settings;

/// <summary>
/// Per-provider model overrides. Null/empty means "use the CLI's default model" — no flag is passed.
/// Persisted as JSON at ~/Library/Application Support/PromptAssistant/settings.json (or the platform equivalent).
/// </summary>
public sealed class AppSettings
{
    public string? GeminiModel { get; set; }
    public string? ClaudeModel { get; set; }
    public string? CodexModel { get; set; }

    public AppSettings Clone() => new()
    {
        GeminiModel = GeminiModel,
        ClaudeModel = ClaudeModel,
        CodexModel = CodexModel,
    };
}
