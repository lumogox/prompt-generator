using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace PromptAssistant.Desktop;

internal static class LoggingSetup
{
    public static string LogDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "PromptAssistant",
        "logs");

    public static string LogFilePath => Path.Combine(LogDirectory, "prompt-assistant-.log");

    public static void Configure()
    {
        Directory.CreateDirectory(LogDirectory);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.WithProperty("App", "PromptAssistant")
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: LogFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Logger configured. Writing to {LogDirectory}", LogDirectory);
    }
}
