using Microsoft.Data.Sqlite;
using Sleepr.Interfaces;

namespace Sleepr.Services;

public class DbAgentOutput : IAgentOutput
{
    private readonly ILogger<DbAgentOutput> _logger;
    private readonly DbOutputOptions _options;

    public DbAgentOutput(ILogger<DbAgentOutput> logger, DbOutputOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<string> SaveAsync(string content)
    {
        using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync();

        var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"CREATE TABLE IF NOT EXISTS AgentOutputs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Content TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );";
        await createCmd.ExecuteNonQueryAsync();

        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO AgentOutputs (Content, CreatedAt) VALUES ($content, $createdAt);";
        insertCmd.Parameters.AddWithValue("$content", content);
        insertCmd.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
        await insertCmd.ExecuteNonQueryAsync();

        var idCmd = connection.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid();";
        var id = (long)(await idCmd.ExecuteScalarAsync() ?? 0L);
        _logger.LogInformation("Saved agent output to DB with id {Id}", id);
        return id.ToString();
    }
}
