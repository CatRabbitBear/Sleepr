using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Interfaces;
using Sleepr.Services;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Sleepr.Agents;

public class ToolsResponse
{
    public List<string> Tools { get; set; }
}

public class SleeprAgentRunner : IAgentRunner
{
    private readonly ILogger<SleeprAgentRunner> _logger;
    private readonly ISleeprAgentFactory _factory;
    private readonly McpPluginManager _pluginManager;
    private readonly IAgentOutput _outputManager;

    public SleeprAgentRunner(
        ILogger<SleeprAgentRunner> logger,
        ISleeprAgentFactory factory,
        McpPluginManager pluginManager,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _factory = factory;
        _pluginManager = pluginManager;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var (user_message, thread) = ChatHistoryBuilder.FromAgentRequest(history);
        var orchestrator = await _factory.CreateOrchestratorAgentAsync(path: "orchestrator");

        //var toolsDict = new Dictionary<string, string>
        //{
        //    ["Weather"] = "Get weather forecasts based on location",
        //    ["Email"] = "Use FastMail API to retrieve, send and read emails",
        //    ["Maps"] = "Use Google Maps API to retrieve map data"
        //};
        var toolsDict = _pluginManager.ListAvailableServers()
            .ToDictionary(m => m.Name, m => m.Description);

        string toolsList = string.Join("\n", toolsDict
           .Select(kv => $"- **{kv.Key}**: {kv.Value}"));

        _logger.LogInformation("Tools list: {ToolsList}", toolsList);
        var args = new KernelArguments
        {
            ["tools_list"] = toolsList
        };
    
        List<string> pluginNames = new List<string>();
        await foreach (ChatMessageContent message in orchestrator.InvokeAsync(user_message, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                // Process the assistant's response
                pluginNames = GetToolsFromJsonResponse(message.Content!);
                _logger.LogInformation("Received assistant message: {MessageContent}. PluginNames: {PluginCount}", message.Content, pluginNames.Count);
            }
            else if (message.Role == AuthorRole.Tool)
            {
                // Process the user's response
                _logger.LogWarning("Tool call attempted in orchestrator: {MessageContent}", message.Content);
            }
            else
            {
                // Handle other roles if necessary
                _logger.LogInformation("Received message from {Role}: {Content}", message.Role, message.Content);
            }
        }

        var taskAgent = await _factory.CreateTaskAgentAsync(path: "task-runner", pluginNames);
        var taskThread = new ChatHistoryAgentThread();
        string agentFinalResponse = string.Empty;
        await foreach (ChatMessageContent message in taskAgent.InvokeAsync(user_message, taskThread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                // Process the assistant's response
                agentFinalResponse = message.Content ?? string.Empty;
            }
            else if (message.Role == AuthorRole.Tool)
            {
                // Process the user's response
                Console.WriteLine($"INFO: Tool call made {message.Content}");
            }
            else
            {
                // Handle other roles if necessary
                Console.WriteLine($"INFO: Received message from {message.Role}: {message.Content}");
            }
        }

        // return new AgentResponse { Result = response.Content, FilePath = path };
        // var path = await _outputManager.SaveAsync(response.Content);
        // Console.WriteLine($"Tool list: {GetStringFromToolList(pluginNames)}");
        _logger.LogInformation("Final agent response: {AgentResponse}", agentFinalResponse);
        return new AgentResponse { Result = agentFinalResponse, FilePath = "" };

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
            _logger.LogInformation("Extracted tools from orchestrator: {ToolsCount}", tools?.Count ?? 0);
            return tools ?? new List<string>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing JSON response: {JsonResponse}", jsonResponse);
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
