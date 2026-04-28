namespace PromptAssistant.Core.Models;

public sealed record RenderedPrompt(string UserMessage, string? AssistantPrefill)
{
    public IReadOnlyList<RenderedSection> Sections { get; init; } = [];
}
