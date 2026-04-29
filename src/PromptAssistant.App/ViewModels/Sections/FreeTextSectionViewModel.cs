namespace PromptAssistant.App.ViewModels.Sections;

public sealed partial class FreeTextSectionViewModel : SectionViewModelBase
{
    public string Placeholder { get; }

    /// <summary>
    /// Optional one-click templates for this field. Click a chip → its text replaces Content.
    /// Empty list = no chips shown. Used today for the Prefilled-response steering chips.
    /// </summary>
    public IReadOnlyList<string> Suggestions { get; }

    public bool HasSuggestions => Suggestions.Count > 0;

    [ObservableProperty]
    private string _content = "";

    public FreeTextSectionViewModel(
        SectionKind kind,
        string title,
        string description,
        string placeholder,
        bool isRequired = true,
        IReadOnlyList<string>? suggestions = null)
        : base(kind, title, description, isRequired)
    {
        Placeholder = placeholder;
        Suggestions = suggestions ?? [];
    }

    [RelayCommand]
    private void ApplySuggestion(string? suggestion)
    {
        if (string.IsNullOrEmpty(suggestion)) return;
        Content = suggestion;
    }

    public override PromptSection ToSection() =>
        new FreeTextSection(Kind, string.IsNullOrWhiteSpace(Content) ? null : Content)
        {
            IsIncluded = IsIncluded,
        };

    public override void Reset()
    {
        base.Reset();
        Content = "";
    }
}
