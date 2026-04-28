namespace PromptAssistant.Cli.Internal;

public static class CliDiscovery
{
    public static string? Find(string executableName)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path)) return null;

        var ext = OperatingSystem.IsWindows() ? ".exe" : "";
        var candidate = executableName + ext;

        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var full = Path.Combine(dir, candidate);
            if (File.Exists(full))
            {
                return full;
            }
        }
        return null;
    }
}
