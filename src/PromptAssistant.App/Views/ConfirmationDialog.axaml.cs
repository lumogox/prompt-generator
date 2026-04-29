using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PromptAssistant.App.Views;

/// <summary>
/// Minimal Yes/No modal for destructive actions. Returns true on confirm, false (or null) on
/// cancel. Construct with the labels you want — header, message, and the confirm button text.
/// </summary>
public partial class ConfirmationDialog : Window
{
    /// <summary>Parameterless ctor for the Avalonia XAML loader / designer.</summary>
    public ConfirmationDialog() : this("Confirm", "Are you sure?", "Confirm") { }

    public ConfirmationDialog(string header, string message, string confirmLabel)
    {
        InitializeComponent();
        this.FindControl<TextBlock>("HeaderText")!.Text = header;
        this.FindControl<TextBlock>("MessageText")!.Text = message;
        this.FindControl<Button>("ConfirmButton")!.Content = confirmLabel;
        Title = header;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(false);

    private void OnConfirm(object? sender, RoutedEventArgs e) => Close(true);
}
