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

public sealed partial class ProviderHealthEntry(string name, ProviderHealth initial) : ObservableObject
{
    public string Name { get; } = name;

    [ObservableProperty]
    private ProviderHealth _health = initial;
}
