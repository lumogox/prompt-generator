namespace PromptAssistant.App.ViewModels.Sections;

public sealed partial class ListSectionViewModel(
    SectionKind kind,
    string title,
    string description,
    string userPlaceholder,
    string assistantPlaceholder)
    : SectionViewModelBase(kind, title, description, isRequired: false)
{
    public string UserPlaceholder { get; } = userPlaceholder;
    public string AssistantPlaceholder { get; } = assistantPlaceholder;
    public ObservableCollection<ListItemViewModel> Items { get; } = [];

    [RelayCommand]
    private void AddItem() => Items.Add(new ListItemViewModel());

    [RelayCommand]
    private void RemoveItem(ListItemViewModel item) => Items.Remove(item);

    public override PromptSection ToSection()
    {
        string[] formatted = [.. Items
            .Select(FormatItem)
            .Where(s => s.Length > 0)];
        return new ListSection(Kind, formatted)
        {
            IsIncluded = IsIncluded,
        };
    }

    private static string FormatItem(ListItemViewModel item)
    {
        var user = item.UserMessage.Trim();
        var assistant = item.AssistantResponse.Trim();
        return (user, assistant) switch
        {
            ("", "") => "",
            ("", _) => $"Assistant: {assistant}",
            (_, "") => $"User: {user}",
            _ => $"User: {user}\nAssistant: {assistant}",
        };
    }

    public override void Reset()
    {
        base.Reset();
        Items.Clear();
    }
}

public sealed partial class ListItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _userMessage = "";

    [ObservableProperty]
    private string _assistantResponse = "";
}
