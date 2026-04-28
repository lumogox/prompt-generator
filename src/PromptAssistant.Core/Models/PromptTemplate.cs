namespace PromptAssistant.Core.Models;

public sealed record PromptTemplate
{
    public required FreeTextSection TaskContext { get; init; }
    public required FreeTextSection ToneContext { get; init; }
    public required FreeTextSection BackgroundData { get; init; }
    public required FreeTextSection TaskRules { get; init; }
    public required ListSection Examples { get; init; }
    public required FreeTextSection ConversationHistory { get; init; }
    public required FreeTextSection ImmediateRequest { get; init; }
    public required ToggleSection StepByStep { get; init; }
    public required FreeTextSection OutputFormatting { get; init; }
    public required FreeTextSection AssistantPrefill { get; init; }
}
