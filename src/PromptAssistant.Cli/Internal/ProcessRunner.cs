namespace PromptAssistant.Cli.Internal;

internal sealed class ProcessRunner(string executablePath)
{
    private readonly Lock _processLock = new();

    public async Task<CliResult> RunAsync(IReadOnlyList<string> args, CancellationToken ct = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var a in args)
        {
            startInfo.ArgumentList.Add(a);
        }

        using var process = new Process { StartInfo = startInfo };

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var stdoutSync = new object();
        var stderrSync = new object();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            lock (stdoutSync) { stdout.AppendLine(e.Data); }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null) return;
            lock (stderrSync) { stderr.AppendLine(e.Data); }
        };

        var stopwatch = Stopwatch.StartNew();
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            process.StandardInput.Close();
        }
        catch (IOException)
        {
            // Some platforms close stdin during process startup; safe to ignore.
        }

        try
        {
            await process.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            using (_processLock.EnterScope())
            {
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch (InvalidOperationException)
                    {
                        // Process already exited between the HasExited check and Kill — fine.
                    }
                }
            }
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        // Flush async stream readers — documented pattern after WaitForExitAsync.
        process.WaitForExit();
        stopwatch.Stop();

        return new CliResult(
            Text: stdout.ToString().TrimEnd(),
            ExitCode: process.ExitCode,
            StdErr: stderr.Length == 0 ? null : stderr.ToString().TrimEnd(),
            Duration: stopwatch.Elapsed);
    }
}
