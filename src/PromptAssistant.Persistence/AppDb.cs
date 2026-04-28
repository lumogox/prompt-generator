using System.Reflection;

namespace PromptAssistant.Persistence;

public sealed class AppDb(string connectionString)
{
    public string ConnectionString => connectionString;

    public static string DefaultDatabasePath()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PromptAssistant");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "app.db");
    }

    public static string DefaultConnectionString() =>
        $"Data Source={DefaultDatabasePath()}";

    public SqliteConnection Open()
    {
        var conn = new SqliteConnection(connectionString);
        conn.Open();
        return conn;
    }

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var conn = Open();
        var sql = LoadMigration("001_initial.sql");
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static string LoadMigration(string filename)
    {
        var assembly = typeof(AppDb).Assembly;
        var resourceName = $"PromptAssistant.Persistence.Migrations.{filename}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded migration not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
