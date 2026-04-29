namespace PromptAssistant.Core.Abstractions;

/// <summary>
/// Transport-agnostic contract for any AI backend the host can call. Implementations may invoke a
/// subprocess CLI, hit a local HTTP daemon, or talk to a remote API; consumers depend only on this
/// interface and the <see cref="CliResult"/> shape it returns.
/// </summary>
public interface IAiProvider
{
    string ProviderName { get; }

    ProviderKind Kind { get; }

    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    Task<CliResult> ExecuteAsync(string prompt, CancellationToken ct = default);
}
