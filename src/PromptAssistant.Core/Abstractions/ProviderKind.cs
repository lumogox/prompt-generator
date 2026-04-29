namespace PromptAssistant.Core.Abstractions;

/// <summary>
/// Coarse classification of how a provider reaches its underlying model. Used by the host to make
/// transport-aware decisions — e.g. distinguishing PATH-discovered CLIs from local HTTP daemons in
/// the UI, or deciding whether a missing dependency is "install a binary" vs. "start a service".
/// </summary>
public enum ProviderKind
{
    /// <summary>External CLI binary discovered on PATH (Claude Code, Gemini, Codex).</summary>
    Cli,

    /// <summary>HTTP API on the local machine (Ollama, LM Studio, etc.).</summary>
    LocalHttp,
}
