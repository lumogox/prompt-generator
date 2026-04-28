namespace PromptAssistant.App.ViewModels.Sections;

public abstract partial class SectionViewModelBase(
    SectionKind kind,
    string title,
    string description,
    bool isRequired) : ObservableObject
{
    public SectionKind Kind { get; } = kind;
    public string Title { get; } = title;
    public string Description { get; } = description;
    public bool IsRequired { get; } = isRequired;
    public bool IsOptional => !IsRequired;
    public string KindNumber => $"{(int)Kind:D2}";

    /// <summary>
    /// Whether this (optional) section is included in the rendered prompt. Required sections
    /// always behave as included; only optional sections expose the toggle in the UI.
    /// </summary>
    [ObservableProperty]
    private bool _isIncluded = true;

    public abstract PromptSection ToSection();
}
