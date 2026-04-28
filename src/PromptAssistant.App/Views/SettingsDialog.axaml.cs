using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PromptAssistant.App.Settings;

namespace PromptAssistant.App.Views;

public partial class SettingsDialog : Window
{
    private readonly SettingsStore _store;
    private AppSettings _working;
    private bool _suppressSync;

    /// <summary>
    /// Parameterless ctor for the Avalonia XAML loader / designer. Production callers must use the
    /// overload that takes a real <see cref="SettingsStore"/>.
    /// </summary>
    public SettingsDialog() : this(new SettingsStore(""), new AppSettings()) { }

    public SettingsDialog(SettingsStore store, AppSettings current)
    {
        _store = store;
        _working = current.Clone();
        InitializeComponent();
        PopulateForm(_working);
        PopulateJson(_working);
        WireSyncOnTabChange();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    // We deliberately access named controls via FindControl<T> rather than relying on Avalonia's
    // auto-generated name fields. The generator's fields are populated only when the generator's
    // own InitializeComponent runs; ours (above) just calls AvaloniaXamlLoader.Load and skips that
    // step, leaving the fields null. FindControl works regardless.
    private TextBox Field(string name) => this.FindControl<TextBox>(name)!;
    private TabControl? TabsControl => this.FindControl<TabControl>("Tabs");
    private TextBlock? JsonErrorLabel => this.FindControl<TextBlock>("JsonError");

    private void PopulateForm(AppSettings settings)
    {
        _suppressSync = true;
        Field("GeminiInput").Text = settings.GeminiModel ?? "";
        Field("ClaudeInput").Text = settings.ClaudeModel ?? "";
        Field("CodexInput").Text = settings.CodexModel ?? "";
        _suppressSync = false;
    }

    private void PopulateJson(AppSettings settings)
    {
        _suppressSync = true;
        Field("JsonInput").Text = _store.Serialize(settings);
        _suppressSync = false;
    }

    private void WireSyncOnTabChange()
    {
        var tabs = TabsControl;
        if (tabs is null) return;

        tabs.SelectionChanged += (_, _) =>
        {
            if (_suppressSync) return;
            // When switching tabs, sync from the *outgoing* representation to keep both views consistent.
            // SelectedIndex 0 = Form (just selected), so JSON was previously edited — sync JSON → Form.
            // SelectedIndex 1 = JSON (just selected), so Form was previously edited — sync Form → JSON.
            if (tabs.SelectedIndex == 0)
            {
                SyncJsonToForm();
            }
            else if (tabs.SelectedIndex == 1)
            {
                SyncFormToJson();
            }
        };
    }

    private void SyncFormToJson()
    {
        var s = ReadForm();
        PopulateJson(s);
        SetJsonError("");
    }

    private void SyncJsonToForm()
    {
        var json = Field("JsonInput").Text ?? "";
        var parsed = _store.TryDeserialize(json);
        if (parsed is null)
        {
            SetJsonError("Invalid JSON — keeping previous form values");
            return;
        }
        PopulateForm(parsed);
        SetJsonError("");
    }

    private void SetJsonError(string text)
    {
        var label = JsonErrorLabel;
        if (label is not null) label.Text = text;
    }

    private AppSettings ReadForm() => new()
    {
        GeminiModel = NullIfEmpty(Field("GeminiInput").Text),
        ClaudeModel = NullIfEmpty(Field("ClaudeInput").Text),
        CodexModel = NullIfEmpty(Field("CodexInput").Text),
    };

    private static string? NullIfEmpty(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private void OnCancel(object? sender, RoutedEventArgs e) => Close(null);

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        AppSettings? toSave;

        // The active tab is the source of truth at save time.
        if (TabsControl?.SelectedIndex == 1)
        {
            var json = Field("JsonInput").Text ?? "";
            toSave = _store.TryDeserialize(json);
            if (toSave is null)
            {
                SetJsonError("Cannot save — JSON is invalid. Switch to Form tab or fix the syntax.");
                return;
            }
        }
        else
        {
            toSave = ReadForm();
        }

        Close(toSave);
    }

    private void OnOpenFile(object? sender, RoutedEventArgs e)
    {
        // Make sure the file exists before asking the OS to open it.
        if (!System.IO.File.Exists(_store.FilePath))
        {
            try { _store.Save(_working); }
            catch { /* swallow — user will see error if it fails on Save */ }
        }

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _store.FilePath,
                    UseShellExecute = true,
                },
            };
            process.Start();
        }
        catch (Exception ex)
        {
            SetJsonError($"Could not open file: {ex.Message}");
        }
    }
}
