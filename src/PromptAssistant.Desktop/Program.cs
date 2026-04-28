using PromptAssistant.App;
using PromptAssistant.Desktop;
using Serilog;

LoggingSetup.Configure();

try
{
    Log.Information("Application starting (args length: {ArgsLength})", args.Length);
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    Log.Information("Application exited cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static AppBuilder BuildAvaloniaApp() =>
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
