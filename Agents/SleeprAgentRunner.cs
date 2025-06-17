using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Interfaces;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Sleepr.Agents;

public class ToolsResponse
{
    public List<string> Tools { get; set; }
}

public class SleeprAgentRunner : IAgentRunner
{
    private readonly ISleeprAgentFactory _factory;
    private readonly IMcpPluginManager _pluginManager;
    private readonly IAgentOutput _outputManager;

    public SleeprAgentRunner(
        ISleeprAgentFactory factory,
        IMcpPluginManager pluginManager,
        IAgentOutput outputManager)
    {
        _factory = factory;
        _pluginManager = pluginManager;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var (user_message, thread) = ChatHistoryBuilder.FromAgentRequest(history);
        var orchestrator = await _factory.CreateOrchestratorAgentAsync(path: "orchestrator");

        var toolsDict = new Dictionary<string, string>
        {
            ["Weather"] = "Get weather forecasts based on location",
            ["Email"] = "Use FastMail API to retrieve, send and read emails",
            ["Maps"] = "Use Google Maps API to retrieve map data"
        };

        string toolsList = string.Join("\n", toolsDict
           .Select(kv => $"- **{kv.Key}**: {kv.Value}"));

        var args = new KernelArguments
        {
            ["tools_list"] = toolsList
        };

        //var thread = new ChatHistoryAgentThread();

        //var pluginNames = await orchestrator.InvokeAsync("Will I need a raincoat in London tomorrow?", thread, new AgentInvokeOptions
        //{
        //    KernelArguments = args
        //});
     
        List<string> pluginNames = new List<string>();
        await foreach (ChatMessageContent message in orchestrator.InvokeAsync(user_message, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                // Process the assistant's response
                pluginNames = GetToolsFromJsonResponse(message.Content!);
                Console.WriteLine($"INFO: Received assistant message: {message.Content}. PluginNames: {pluginNames.Count}");
            }
            else if (message.Role == AuthorRole.Tool)
            {
                // Process the user's response
                Console.WriteLine($"WARN: Tool call attmepted in orchestrator");
            }
            else
            {
                // Handle other roles if necessary
                Console.WriteLine($"INFO: Received message from {message.Role}: {message.Content}");
            }
        }

        // TODO after testing 
        //var taskAgent = await _factory.CreateTaskAgentAsync(path: "task_executor", pluginNames);

        //var response = await taskAgent.InvokeAsync("Do the thing");
        //var path = await _outputManager.SaveAsync(response.Content);

        // return new AgentResponse { Result = response.Content, FilePath = path };
        Console.WriteLine($"Tool list: {GetStringFromToolList(pluginNames)}");
        return new AgentResponse { Result = GetStringFromToolList(pluginNames), FilePath = "" };

    }

    public List<string> GetToolsFromJsonResponse(string jsonResponse)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        try
        {
            var firstJson = ExtractFirstJsonObject(jsonResponse);
            var toolsResponse = JsonSerializer.Deserialize<ToolsResponse>(firstJson, options);
            var tools = toolsResponse?.Tools;
            Console.WriteLine($"tools: {tools}");
            return tools ?? new List<string>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON response: {ex.Message}");
            return new List<string>();
        }
    }

    public string GetStringFromToolList(List<string> tools)
    {
        return string.Join(", ", tools);
    }

    public string ExtractFirstJsonObject(string input)
    {
        var m = Regex.Match(input, @"\{[\s\S]*\}");
        return m.Success ? m.Value : input; // fallback to original if no match
    }
}
