namespace PromptAssistant.Core.Models;

public enum SectionKind
{
    TaskContext = 1,
    ToneContext = 2,
    BackgroundData = 3,
    TaskRules = 4,
    Examples = 5,
    ConversationHistory = 6,
    ImmediateRequest = 7,
    StepByStep = 8,
    OutputFormatting = 9,
    AssistantPrefill = 10,
}
