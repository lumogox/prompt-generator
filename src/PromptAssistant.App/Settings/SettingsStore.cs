using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptAssistant.App.Settings;

public sealed class SettingsStore
{
    private static readonly ILogger _log = Log.ForContext<SettingsStore>();

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string FilePath { get; }

    public SettingsStore(string filePath)
    {
        FilePath = filePath;
    }

    public AppSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            _log.Information("No settings file at {Path}; using defaults", FilePath);
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
            _log.Information("Loaded settings from {Path}", FilePath);
            return settings;
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to load settings from {Path}; falling back to defaults", FilePath);
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var json = JsonSerializer.Serialize(settings, Options);
            File.WriteAllText(FilePath, json);
            _log.Information("Saved settings to {Path}", FilePath);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save settings to {Path}", FilePath);
            throw;
        }
    }

    public string Serialize(AppSettings settings) =>
        JsonSerializer.Serialize(settings, Options);

    public AppSettings? TryDeserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
