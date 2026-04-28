using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PromptAssistant.App.ViewModels;

namespace PromptAssistant.App.Views;

public partial class RefineDialog : Window
{
    public ObservableCollection<RefineLens> Lenses { get; } =
    [
        new("Tighten",
            "Cut wordiness aggressively; lower the token count by 20–30% while preserving meaning.",
            "Cut wordiness aggressively. Lower the token count by 20–30% across non-empty fields while preserving meaning. Prefer terse, declarative phrasing."),

        new("More formal",
            "Shift register to formal/professional. Remove contractions and casual phrasing.",
            "Shift the register of taskContext, toneContext, taskRules, and outputFormatting to formal and professional. Remove contractions, hedges, and casual phrasing."),

        new("More casual",
            "Soften register to friendly and approachable. Allow contractions; avoid stiffness.",
            "Soften the register to friendly and approachable. Allow contractions and conversational phrasing where appropriate; avoid corporate stiffness."),

        new("More technical",
            "Increase technical specificity. Use precise jargon where it aids clarity for an engineering audience.",
            "Increase technical specificity. Use precise engineering jargon and concrete artifacts (error codes, type signatures, command-line flags) where they aid clarity for a senior engineering audience."),

        new("Plain English",
            "Reduce jargon. Prefer concrete, plain-English phrasings over abstract terms.",
            "Reduce jargon. Prefer concrete, plain-English phrasings over abstract terms. A non-specialist should understand each rule on first read."),

        new("Strengthen rules",
            "Add or strengthen taskRules — uncertainty handling, citation, refusing off-topic. Make them explicit and enforceable.",
            "Strengthen the taskRules field: ensure rules are explicit, enforceable, and ordered from highest- to lowest-priority. Add rules for uncertainty handling, refusing off-topic questions, and citing source spans if backgroundData is non-empty."),

        new("Strengthen examples",
            "Make examples more diverse and realistic. Add specific code/error/data artifacts where appropriate.",
            "Strengthen the examples field: make each user/assistant pair more diverse and realistic. Replace generic phrasings with specific code, error messages, or tool output that mirrors the domain."),
    ];

    public RefineDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);

    private void OnClearLenses(object? sender, RoutedEventArgs e)
    {
        foreach (var lens in Lenses) lens.IsSelected = false;
    }

    private void OnRefine(object? sender, RoutedEventArgs e)
    {
        var instructions = Lenses.Where(l => l.IsSelected).Select(l => l.Instruction).ToList();
        var guidance = this.FindControl<TextBox>("GuidanceInput")?.Text?.Trim();
        if (string.IsNullOrEmpty(guidance)) guidance = null;
        Close(new RefineOptions(instructions, guidance));
    }
}
