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

    // Captured explicitly so Reset() can refer to it without triggering CS9124 (primary-ctor
    // parameter cannot be both field-initializer and captured-into-method-body in the same class).
    private readonly bool _defaultEnabled = defaultEnabled;

    [ObservableProperty]
    private bool _enabled = defaultEnabled;

    public override PromptSection ToSection() =>
        new ToggleSection(Kind, Enabled, CanonicalText);

    public override void Reset()
    {
        base.Reset();
        Enabled = _defaultEnabled;
    }
}
