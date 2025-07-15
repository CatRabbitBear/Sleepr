using System.Text.Json;
using System.Text.RegularExpressions;
using Sleepr.Services;

namespace Sleepr.Pipeline.Utils;

/// <summary>
/// Utility methods for orchestrating and running plugin agents.
/// </summary>
public static class PluginUtils
{
    public static string BuildToolsList(McpPluginManager pluginManager)
    {
        var dict = pluginManager.ListAvailableServers()
            .ToDictionary(m => m.Name, m => m.Description);
        return string.Join("\n", dict.Select(kv => $"- **{kv.Key}**: {kv.Value}"));
    }

    public static List<string> GetToolsFromJsonResponse(string jsonResponse)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try
        {
            var first = ExtractFirstJsonObject(jsonResponse);
            var toolsResponse = JsonSerializer.Deserialize<ToolsResponse>(first, options);
            return toolsResponse?.Tools ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }

    private static string ExtractFirstJsonObject(string input)
    {
        var m = Regex.Match(input, "{[\\s\\S]*}");
        return m.Success ? m.Value : input;
    }

    private class ToolsResponse
    {
        public List<string> Tools { get; set; } = new();
    }
}
