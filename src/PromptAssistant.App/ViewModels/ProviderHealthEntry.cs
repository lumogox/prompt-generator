namespace PromptAssistant.App.ViewModels;

public enum ProviderHealth
{
    /// <summary>Provider unavailable (e.g. CLI binary missing from PATH). Red. Sticky for the session.</summary>
    NotFound,
    /// <summary>Provider registered but not yet exercised. Yellow.</summary>
    Unknown,
    /// <summary>Last operation succeeded. Green.</summary>
    Authenticated,
    /// <summary>Last operation failed (auth, rate limit, daemon down, etc). Yellow.</summary>
    Failed,
}

public sealed partial class ProviderHealthEntry(string name, ProviderKind kind, ProviderHealth initial) : ObservableObject
{
    public string Name { get; } = name;

    public ProviderKind Kind { get; } = kind;

    /// <summary>True for local HTTP providers (Ollama et al). Bound by the footer DataTemplate to
    /// show a small LOCAL badge so the kind is distinguishable at a glance.</summary>
    public bool IsLocal => Kind == ProviderKind.LocalHttp;

    [ObservableProperty]
    private ProviderHealth _health = initial;
}
