using PromptAssistant.Core.Tests.Fixtures;

namespace PromptAssistant.Core.Tests;

public class PromptHistoryRepositoryTests : IAsyncLifetime
{
    private string _dbPath = "";
    private AppDb _db = null!;
    private PromptHistoryRepository _repo = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"pa-test-{Guid.NewGuid():N}.db");
        _db = new AppDb($"Data Source={_dbPath}");
        await _db.EnsureSchemaAsync();
        _repo = new PromptHistoryRepository(_db);
    }

    public Task DisposeAsync()
    {
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* best effort */ }
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Save_then_load_round_trips_template()
    {
        var original = SampleTemplates.FullyPopulated();

        var id = await _repo.SaveAsync(original, "round-trip");
        var loaded = await _repo.LoadAsync(id);

        Assert.NotNull(loaded);
        Assert.Equal(original, loaded);
    }

    [Fact]
    public async Task List_returns_saved_templates_newest_first()
    {
        await _repo.SaveAsync(SampleTemplates.FullyPopulated(), "first");
        await Task.Delay(10);
        await _repo.SaveAsync(SampleTemplates.MinimumRequired(), "second");

        var summaries = await _repo.ListAsync();

        Assert.Equal(2, summaries.Count);
        Assert.Equal("second", summaries[0].Name);
        Assert.Equal("first", summaries[1].Name);
    }

    [Fact]
    public async Task RecordRun_links_to_template()
    {
        var templateId = await _repo.SaveAsync(SampleTemplates.FullyPopulated(), "with-run");
        var result = new CliResult(
            Text: "model response",
            ExitCode: 0,
            StdErr: null,
            Duration: TimeSpan.FromMilliseconds(123));

        var runId = await _repo.RecordRunAsync(templateId, "Claude Code", "rendered prompt body", result);

        Assert.True(runId > 0);
    }

    [Fact]
    public async Task LoadAsync_returns_null_for_missing_id()
    {
        var loaded = await _repo.LoadAsync(99999);
        Assert.Null(loaded);
    }
}
