namespace PromptAssistant.Core.Abstractions;

public interface IAiCliProvider
{
    string ProviderName { get; }

    Task<bool> IsAvailableAsync(CancellationToken ct = default);

    Task<CliResult> ExecuteAsync(string prompt, CancellationToken ct = default);
}
