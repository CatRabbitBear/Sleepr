using System.Text.Json;
using System.Text.RegularExpressions;
using Sleepr.Services;

namespace Sleepr.Pipeline.Utils;

/// <summary>
/// Utility methods for orchestrating and running plugin agents.
/// </summary>
public static class PluginUtils
{
    private static readonly JsonSerializerOptions _jsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    public static string BuildToolsList(McpPluginManager pluginManager)
    {
        var dict = pluginManager.ListAvailableServers()
            .ToDictionary(m => m.Name, m => m.Description);
        return string.Join("\n", dict.Select(kv => $"- **{kv.Key}**: {kv.Value}"));
    }

    /// <summary>
    /// Attempts to deserialize the first JSON object contained within the
    /// response string to the specified type.
    /// </summary>
    public static bool TryDeserializeFirstJson<T>(string response, out T? result)
    {
        var first = ExtractFirstJsonObject(response);
        try
        {
            result = JsonSerializer.Deserialize<T>(first, _jsonOptions);
            return result != null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }

    public static bool TryGetToolsFromJsonResponse(string jsonResponse, out List<string> tools)
    {
        if (TryDeserializeFirstJson(jsonResponse, out ToolsResponse? response) &&
            response?.Tools != null)
        {
            tools = response.Tools;
            return true;
        }

        tools = new List<string>();
        return false;
    }

    private static string ExtractFirstJsonObject(string input)
    {
        var m = Regex.Match(input, "{[\\s\\S]*?}");
        return m.Success ? m.Value : input;
    }

    private class ToolsResponse
    {
        public List<string> Tools { get; set; } = new();
    }
}
