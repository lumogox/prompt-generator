namespace PromptAssistant.App.Views;

public sealed partial class RefineLens(string label, string description, string instruction) : ObservableObject
{
    public string Label { get; } = label;
    public string Description { get; } = description;
    public string Instruction { get; } = instruction;

    [ObservableProperty]
    private bool _isSelected;
}
