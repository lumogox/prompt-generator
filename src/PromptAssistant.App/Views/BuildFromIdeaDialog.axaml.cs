using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PromptAssistant.App.Views;

public partial class BuildFromIdeaDialog : Window
{
    public BuildFromIdeaDialog()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);

    private void OnBuild(object? sender, RoutedEventArgs e)
    {
        var input = this.FindControl<TextBox>("IdeaInput");
        var text = input?.Text?.Trim();
        Close(string.IsNullOrEmpty(text) ? null : text);
    }
}
