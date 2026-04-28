namespace PromptAssistant.Core.Models;

public sealed record RenderedSection(
    SectionKind Kind,
    string Label,
    string Content);
