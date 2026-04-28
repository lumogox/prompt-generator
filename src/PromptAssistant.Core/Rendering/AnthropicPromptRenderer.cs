using PromptAssistant.Core.Models;

namespace PromptAssistant.Core.Rendering;

public sealed class AnthropicPromptRenderer : IPromptRenderer
{
    private const int LongPromptThreshold = 1200;

    public RenderedPrompt Render(ref readonly PromptTemplate template)
    {
        var sections = new List<RenderedSection>();

        AddRequiredFreeText(sections, template.TaskContext, "Task context");
        AddRequiredFreeText(sections, template.ToneContext, "Tone context");
        AddOptionalWrapped(sections, template.BackgroundData, "Background data, documents, images",
            openTag: "guide", lead: "Here is the document you should reference when answering the user:");
        AddRequiredFreeText(sections, template.TaskRules, "Detailed task description & rules",
            lead: "Here are some important rules for the interaction:");
        AddExamples(sections, template.Examples);
        AddOptionalWrapped(sections, template.ConversationHistory, "Conversation history",
            openTag: "history",
            lead: "Here is the conversation history (between the user and you) prior to the question. It could be empty if there is no history:");
        AddRequiredWrapped(sections, template.ImmediateRequest, "Immediate task description / request",
            openTag: "question", lead: "Here is the user's question:");
        AddToggle(sections, template.StepByStep, "Thinking step by step");
        AddRequiredFreeText(sections, template.OutputFormatting, "Output formatting");

        var userMessage = string.Join("\n\n", sections.Select(s => s.Content));

        if (userMessage.Length > LongPromptThreshold)
        {
            userMessage += "\n\nNow, please respond to the user's question above.";
        }

        userMessage = userMessage.Replace("\r\n", "\n").TrimEnd() + "\n";

        var prefill = (template.AssistantPrefill.IsIncluded && template.AssistantPrefill.HasContent)
            ? template.AssistantPrefill.Content
            : null;
        return new RenderedPrompt(userMessage, prefill) { Sections = sections };
    }

    private static void AddRequiredFreeText(List<RenderedSection> sections, FreeTextSection section, string label, string? lead = null)
    {
        if (!section.HasContent)
        {
            throw new InvalidOperationException($"Required section {section.Kind} has no content.");
        }

        var content = lead is not null
            ? $"{lead}\n{section.Content!.TrimEnd()}"
            : section.Content!.TrimEnd();

        sections.Add(new RenderedSection(section.Kind, label, content));
    }

    private static void AddRequiredWrapped(List<RenderedSection> sections, FreeTextSection section, string label, string openTag, string lead)
    {
        if (!section.HasContent)
        {
            throw new InvalidOperationException($"Required section {section.Kind} has no content.");
        }

        var content = $"{lead} <{openTag}>{section.Content!.Trim()}</{openTag}>";
        sections.Add(new RenderedSection(section.Kind, label, content));
    }

    private static void AddOptionalWrapped(List<RenderedSection> sections, FreeTextSection section, string label, string openTag, string lead)
    {
        // User explicitly excluded this section — emit nothing (no header, no lead, no empty tags).
        if (!section.IsIncluded) return;

        var content = section.HasContent
            ? $"{lead} <{openTag}>{section.Content!.Trim()}</{openTag}>"
            : $"{lead} <{openTag}></{openTag}>";

        sections.Add(new RenderedSection(section.Kind, label, content));
    }

    private static void AddExamples(List<RenderedSection> sections, ListSection examples)
    {
        if (!examples.IsIncluded) return;
        if (!examples.HasContent) return;

        var sb = new StringBuilder();
        sb.Append(examples.Items.Count == 1
            ? "Here is an example of how to respond:"
            : "Here are examples of how to respond:");

        foreach (var example in examples.Items)
        {
            sb.AppendLine();
            sb.AppendLine("<example>");
            sb.AppendLine(example.Trim());
            sb.Append("</example>");
        }

        sections.Add(new RenderedSection(SectionKind.Examples, "Examples", sb.ToString()));
    }

    private static void AddToggle(List<RenderedSection> sections, ToggleSection toggle, string label)
    {
        if (!toggle.HasContent) return;
        sections.Add(new RenderedSection(toggle.Kind, label, toggle.CanonicalText.Trim()));
    }
}
