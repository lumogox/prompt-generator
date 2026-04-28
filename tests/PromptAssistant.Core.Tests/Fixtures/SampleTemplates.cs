namespace PromptAssistant.Core.Tests.Fixtures;

internal static class SampleTemplates
{
    public static PromptTemplate FullyPopulated() => new()
    {
        TaskContext = new FreeTextSection(SectionKind.TaskContext, "You are an AI career coach named Joe."),
        ToneContext = new FreeTextSection(SectionKind.ToneContext, "Maintain a friendly customer service tone."),
        BackgroundData = new FreeTextSection(SectionKind.BackgroundData, "[career-guidance-document]"),
        TaskRules = new FreeTextSection(SectionKind.TaskRules, "- Always stay in character.\n- Decline irrelevant questions politely."),
        Examples = new ListSection(SectionKind.Examples, ["User: How were you created?\nJoe: I was created by AdAstra Careers."]),
        ConversationHistory = new FreeTextSection(SectionKind.ConversationHistory, "(none yet)"),
        ImmediateRequest = new FreeTextSection(SectionKind.ImmediateRequest, "What career suits a software engineer?"),
        StepByStep = new ToggleSection(SectionKind.StepByStep, Enabled: true, CanonicalText: "Think step-by-step before you respond."),
        OutputFormatting = new FreeTextSection(SectionKind.OutputFormatting, "Wrap your response in <response></response> tags."),
        AssistantPrefill = new FreeTextSection(SectionKind.AssistantPrefill, "<response>"),
    };

    public static PromptTemplate MinimumRequired() => new()
    {
        TaskContext = new FreeTextSection(SectionKind.TaskContext, "You are an AI career coach named Joe."),
        ToneContext = new FreeTextSection(SectionKind.ToneContext, "Maintain a friendly customer service tone."),
        BackgroundData = new FreeTextSection(SectionKind.BackgroundData, null),
        TaskRules = new FreeTextSection(SectionKind.TaskRules, "- Always stay in character."),
        Examples = new ListSection(SectionKind.Examples, []),
        ConversationHistory = new FreeTextSection(SectionKind.ConversationHistory, null),
        ImmediateRequest = new FreeTextSection(SectionKind.ImmediateRequest, "What career suits a software engineer?"),
        StepByStep = new ToggleSection(SectionKind.StepByStep, Enabled: false, CanonicalText: "Think step-by-step before you respond."),
        OutputFormatting = new FreeTextSection(SectionKind.OutputFormatting, "Reply in plain prose."),
        AssistantPrefill = new FreeTextSection(SectionKind.AssistantPrefill, null),
    };
}
