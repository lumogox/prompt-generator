using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PromptAssistant.App.Settings;
using PromptAssistant.App.ViewModels;
using PromptAssistant.App.Views;
using PromptAssistant.Cli.Internal;
using PromptAssistant.Cli.Providers;

namespace PromptAssistant.App;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var renderer = new AnthropicPromptRenderer();
            var window = new MainWindow();

            // Settings: load BEFORE discovering providers so Ollama can pick up its base URL.
            var appDataDir = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PromptAssistant");
            var settingsStore = new SettingsStore(System.IO.Path.Combine(appDataDir, "settings.json"));
            var settings = settingsStore.Load();

            var providers = DiscoverProviders(settings);
            ApplySettingsToProviders(providers, settings);

            async Task CopyToClipboard(string text)
            {
                var clipboard = window.Clipboard;
                if (clipboard is null) return;
                await clipboard.SetTextAsync(text);
            }

            // Wrap any dialog show in a scrim toggle: blur+dim parent content while modal is open.
            async Task<TResult?> ShowWithScrim<TResult>(Window dialog) where TResult : class
            {
                if (window.DataContext is MainWindowViewModel vm)
                {
                    vm.IsModalActive = true;
                    try { return await dialog.ShowDialog<TResult?>(window); }
                    finally { vm.IsModalActive = false; }
                }
                return await dialog.ShowDialog<TResult?>(window);
            }

            async Task<string?> PromptForIdea() =>
                await ShowWithScrim<string>(new BuildFromIdeaDialog());

            async Task<RefineOptions?> PromptForRefineOptions() =>
                await ShowWithScrim<RefineOptions>(new RefineDialog());

            async Task OpenSettings()
            {
                var result = await ShowWithScrim<AppSettings>(new SettingsDialog(settingsStore, settings));
                if (result is null) return; // cancelled

                settingsStore.Save(result);
                // Mutate the live `settings` instance so the next dialog open sees current values.
                settings.GeminiModel = result.GeminiModel;
                settings.ClaudeModel = result.ClaudeModel;
                settings.CodexModel = result.CodexModel;
                settings.OllamaModel = result.OllamaModel;
                settings.OllamaBaseUrl = result.OllamaBaseUrl;
                ApplySettingsToProviders(providers, result);
            }

            async Task<bool> SaveMarkdown(string suggestedName, string content)
            {
                var topLevel = TopLevel.GetTopLevel(window);
                if (topLevel is null) return false;

                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save assembled prompt as Markdown",
                    SuggestedFileName = suggestedName,
                    DefaultExtension = "md",
                    FileTypeChoices =
                    [
                        new FilePickerFileType("Markdown") { Patterns = ["*.md"] },
                        new FilePickerFileType("Text")     { Patterns = ["*.txt"] },
                    ],
                });

                if (file is null) return false;

                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(content);
                return true;
            }

            var logDirectory = System.IO.Path.Combine(appDataDir, "logs");

            window.DataContext = new MainWindowViewModel(
                renderer, providers, CopyToClipboard, PromptForIdea, logDirectory, OpenSettings, SaveMarkdown, PromptForRefineOptions);
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ApplySettingsToProviders(IReadOnlyDictionary<string, IAiCliProvider> providers, AppSettings settings)
    {
        if (providers.TryGetValue("Gemini", out var gemini) && gemini is GeminiCliProvider g)
        {
            g.Model = settings.GeminiModel;
        }
        if (providers.TryGetValue("Claude Code", out var claude) && claude is ClaudeCodeCliProvider c)
        {
            c.Model = settings.ClaudeModel;
        }
        if (providers.TryGetValue("Codex", out var codex) && codex is CodexCliProvider x)
        {
            x.Model = settings.CodexModel;
        }
        if (providers.TryGetValue("Ollama", out var ollama) && ollama is OllamaProvider o)
        {
            o.Model = settings.OllamaModel;
        }
    }

    private static IReadOnlyDictionary<string, IAiCliProvider> DiscoverProviders(AppSettings settings)
    {
        var dict = new Dictionary<string, IAiCliProvider>();

        var gemini = CliDiscovery.Find("gemini");
        if (gemini is not null)
        {
            dict["Gemini"] = new GeminiCliProvider(gemini);
        }

        var claude = CliDiscovery.Find("claude");
        if (claude is not null)
        {
            dict["Claude Code"] = new ClaudeCodeCliProvider(claude);
        }

        var codex = CliDiscovery.Find("codex");
        if (codex is not null)
        {
            dict["Codex"] = new CodexCliProvider(codex);
        }

        // Ollama is HTTP-based — no PATH lookup. Always register; the footer health dot reflects
        // whether the daemon is actually reachable.
        dict["Ollama"] = new OllamaProvider(settings.OllamaBaseUrl);

        return dict;
    }
}
