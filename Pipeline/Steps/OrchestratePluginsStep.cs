using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Utils;
using Sleepr.Services;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Runs the orchestrator agent to select which plugins should be loaded.
/// </summary>
public class OrchestratePluginsStep : IAgentPipelineStep
{
    private readonly ChatCompletionAgent _agent;
    private readonly McpPluginManager _pluginManager;
    private readonly ILogger _logger;

    public OrchestratePluginsStep(ChatCompletionAgent agent, McpPluginManager pluginManager, ILoggerFactory loggerFactory)
    {
        _pluginManager = pluginManager;
        _agent = agent;
        _logger = loggerFactory.CreateLogger<OrchestratePluginsStep>();
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.UserMessage == null)
        {
            throw new ArgumentNullException(nameof(context.UserMessage), "User message cannot be null in PipelineContext.");
        }

        var toolsList = _pluginManager.ListAvailableServers()
            .Select(p => $"{p.Name}: {p.Description ?? "<no description>"}")
            .ToList();
        var args = new KernelArguments { ["tools_list"] = toolsList };

        var pluginNames = new List<string>();
        // 1) Collect all the chat messages into a List
        var messages = await _agent
            .InvokeAsync(
                context.UserMessage,
                context.AgentThread,
                new AgentInvokeOptions { KernelArguments = args }
            )
            .ToListAsync();

        // 2) Find the last assistant reply
        var lastAssistant = messages
            .LastOrDefault(m => m.Message.Role == AuthorRole.Assistant);

        context.AgentThread = (ChatHistoryAgentThread?)(messages.LastOrDefault()?.Thread);

        if (lastAssistant is not null)
        {
            // 4) Pull out the content for further processing
            var assistantContent = lastAssistant.Message.Content ?? string.Empty;
            pluginNames = PluginUtils.GetToolsFromJsonResponse(assistantContent);
        }
        else
        {
            // handle the case where no assistant response was returned - implement logging for steps
        }
        context.SelectedPlugins = pluginNames;
    }
}
