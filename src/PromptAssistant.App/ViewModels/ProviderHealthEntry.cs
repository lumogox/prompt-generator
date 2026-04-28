namespace PromptAssistant.App.ViewModels;

public enum ProviderHealth
{
    /// <summary>CLI not found on PATH. Red. Permanent until user installs.</summary>
    NotFound,
    /// <summary>CLI found but not yet verified (no operation tried yet). Yellow.</summary>
    Unknown,
    /// <summary>CLI found and last operation succeeded. Green.</summary>
    Authenticated,
    /// <summary>CLI found but last operation failed (auth, rate limit, etc). Yellow.</summary>
    Failed,
}

public sealed partial class ProviderHealthEntry(string name, ProviderHealth initial) : ObservableObject
{
    public string Name { get; } = name;

    [ObservableProperty]
    private ProviderHealth _health = initial;
}
