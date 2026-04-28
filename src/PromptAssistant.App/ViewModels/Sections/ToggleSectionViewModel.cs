namespace PromptAssistant.App.ViewModels.Sections;

public sealed partial class ToggleSectionViewModel(
    SectionKind kind,
    string title,
    string description,
    string canonicalText,
    bool defaultEnabled = true)
    : SectionViewModelBase(kind, title, description, isRequired: true)
{
    public string CanonicalText { get; } = canonicalText;

    [ObservableProperty]
    private bool _enabled = defaultEnabled;

    public override PromptSection ToSection() =>
        new ToggleSection(Kind, Enabled, CanonicalText);
}
