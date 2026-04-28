namespace PromptAssistant.Persistence;

public sealed class PromptHistoryRepository(AppDb db)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public async Task<long> SaveAsync(PromptTemplate template, string name, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(template, JsonOptions);
        var createdUtc = DateTime.UtcNow.ToString("o");

        await using var conn = db.Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Templates (Name, CreatedUtc, Json)
            VALUES ($name, $createdUtc, $json);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$createdUtc", createdUtc);
        cmd.Parameters.AddWithValue("$json", json);

        var id = (long)(await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false))!;
        return id;
    }

    public async Task<IReadOnlyList<TemplateSummary>> ListAsync(CancellationToken ct = default)
    {
        await using var conn = db.Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, CreatedUtc FROM Templates ORDER BY CreatedUtc DESC;";

        var results = new List<TemplateSummary>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            results.Add(new TemplateSummary(
                Id: reader.GetInt64(0),
                Name: reader.GetString(1),
                CreatedUtc: DateTime.Parse(reader.GetString(2), styles: System.Globalization.DateTimeStyles.RoundtripKind)));
        }
        return results;
    }

    public async Task<PromptTemplate?> LoadAsync(long id, CancellationToken ct = default)
    {
        await using var conn = db.Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Json FROM Templates WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);

        var json = (string?)await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return json is null ? null : JsonSerializer.Deserialize<PromptTemplate>(json, JsonOptions);
    }

    public async Task<long> RecordRunAsync(
        long templateId,
        string provider,
        string renderedPrompt,
        CliResult result,
        CancellationToken ct = default)
    {
        await using var conn = db.Open();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Runs (TemplateId, Provider, RanUtc, RenderedPrompt, Response, ExitCode, StdErr, DurationMs)
            VALUES ($templateId, $provider, $ranUtc, $renderedPrompt, $response, $exitCode, $stdErr, $durationMs);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$templateId", templateId);
        cmd.Parameters.AddWithValue("$provider", provider);
        cmd.Parameters.AddWithValue("$ranUtc", DateTime.UtcNow.ToString("o"));
        cmd.Parameters.AddWithValue("$renderedPrompt", renderedPrompt);
        cmd.Parameters.AddWithValue("$response", (object?)result.Text ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$exitCode", result.ExitCode);
        cmd.Parameters.AddWithValue("$stdErr", (object?)result.StdErr ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$durationMs", (long)result.Duration.TotalMilliseconds);

        var id = (long)(await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false))!;
        return id;
    }
}
