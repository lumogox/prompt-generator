namespace PromptAssistant.Cli.Tests;

// These tests exercise ProcessRunner against real POSIX commands.
// They early-return on Windows; Windows-equivalent fakes can be added later.
public class ProcessRunnerTests
{
    private static bool IsPosix => !OperatingSystem.IsWindows();

    [Fact]
    public async Task Captures_stdout_on_success()
    {
        if (!IsPosix) return;
        var runner = new ProcessRunner("/bin/echo");

        var result = await runner.RunAsync(["hello", "world"]);

        Assert.True(result.Succeeded);
        Assert.Equal("hello world", result.Text);
        Assert.Null(result.StdErr);
    }

    [Fact]
    public async Task Reports_non_zero_exit_code()
    {
        if (!IsPosix) return;
        var falsePath = File.Exists("/usr/bin/false") ? "/usr/bin/false" : "/bin/false";
        var runner = new ProcessRunner(falsePath);

        var result = await runner.RunAsync([]);

        Assert.False(result.Succeeded);
        Assert.NotEqual(0, result.ExitCode);
    }

    [Fact]
    public async Task Cancellation_kills_long_running_process_quickly()
    {
        if (!IsPosix) return;
        var runner = new ProcessRunner("/bin/sleep");
        using var cts = new CancellationTokenSource();

        var sw = Stopwatch.StartNew();
        var task = runner.RunAsync(["30"], cts.Token);
        await Task.Delay(100);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        sw.Stop();

        Assert.True(sw.Elapsed.TotalSeconds < 2,
            $"Cancellation should kill child within 2s, took {sw.Elapsed.TotalSeconds:F2}s");
    }

    [Fact]
    public async Task Captures_stderr_separately_from_stdout()
    {
        if (!IsPosix) return;
        var runner = new ProcessRunner("/bin/sh");

        var result = await runner.RunAsync(["-c", "echo to-out; echo to-err 1>&2"]);

        Assert.True(result.Succeeded);
        Assert.Equal("to-out", result.Text);
        Assert.Equal("to-err", result.StdErr);
    }
}
