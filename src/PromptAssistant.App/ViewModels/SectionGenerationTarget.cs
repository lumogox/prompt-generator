namespace PromptAssistant.App.ViewModels;

public sealed partial class SectionGenerationTarget(int number, string label, SectionKind kind) : ObservableObject
{
    public int Number { get; } = number;
    public string Label { get; } = label;
    public SectionKind Kind { get; } = kind;
    public string NumberLabel => $"{Number:D2}";

    [ObservableProperty]
    private bool _isSelected = true;
}
