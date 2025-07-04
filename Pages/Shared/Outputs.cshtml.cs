using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Sleepr.Services;

namespace Sleepr.Pages.Shared;

public class OutputsModel : PageModel
{
    private readonly DbOutputOptions _options;
    private readonly ILogger<OutputsModel> _logger;

    public OutputsModel(DbOutputOptions options, ILogger<OutputsModel> logger)
    {
        _options = options;
        _logger = logger;
    }

    public record AgentOutputRecord(long Id, string Content, DateTime CreatedAt);

    public List<AgentOutputRecord> Outputs { get; private set; } = new();

    [BindProperty]
    public string? NewContent { get; set; }
    [BindProperty]
    public int DeleteId { get; set; }
    [BindProperty]
    public int EditId { get; set; }
    [BindProperty]
    public string? EditContent { get; set; }

    public async Task OnGetAsync()
    {
        await EnsureTableAsync();
        await LoadOutputsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewContent))
        {
            using var connection = new SqliteConnection(_options.ConnectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO AgentOutputs (Content, CreatedAt) VALUES ($c, $d);";
            cmd.Parameters.AddWithValue("$c", NewContent);
            cmd.Parameters.AddWithValue("$d", DateTime.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM AgentOutputs WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", DeleteId);
        await cmd.ExecuteNonQueryAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (EditContent is not null)
        {
            using var connection = new SqliteConnection(_options.ConnectionString);
            await connection.OpenAsync();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE AgentOutputs SET Content=$c WHERE Id=$id";
            cmd.Parameters.AddWithValue("$c", EditContent);
            cmd.Parameters.AddWithValue("$id", EditId);
            await cmd.ExecuteNonQueryAsync();
        }
        return RedirectToPage();
    }

    private async Task LoadOutputsAsync()
    {
        Outputs.Clear();
        using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Content, CreatedAt FROM AgentOutputs ORDER BY Id DESC";
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Outputs.Add(new AgentOutputRecord(
                reader.GetInt64(0),
                reader.GetString(1),
                DateTime.Parse(reader.GetString(2))
            ));
        }
    }

    private async Task EnsureTableAsync()
    {
        using var connection = new SqliteConnection(_options.ConnectionString);
        await connection.OpenAsync();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS AgentOutputs (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Content TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );";
        await cmd.ExecuteNonQueryAsync();
    }
}