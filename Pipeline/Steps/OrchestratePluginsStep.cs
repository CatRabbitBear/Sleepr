using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Utils;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Runs the orchestrator agent to select which plugins should be loaded.
/// </summary>
public class OrchestratePluginsStep : IAgentPipelineStep
{
    private readonly string _agentKey;

    public OrchestratePluginsStep(string agentKey = "orchestrator")
    {
        _agentKey = agentKey;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (!context.Agents.TryGetValue(_agentKey, out var orchestratorCtx))
        {
            return;
        }

        var (userMessage, thread) = ChatHistoryBuilder.FromAgentRequest(context.RequestHistory);
        context.UserMessage = userMessage;

        var toolsList = PluginUtils.BuildToolsList(context.PluginManager);
        var args = new KernelArguments { ["tools_list"] = toolsList };

        orchestratorCtx.Thread = thread;
        orchestratorCtx.ToolsList = toolsList;

        var pluginNames = new List<string>();
        await foreach (ChatMessageContent message in orchestratorCtx.Agent.InvokeAsync(userMessage, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                pluginNames = PluginUtils.GetToolsFromJsonResponse(message.Content ?? string.Empty);
            }
        }
        context.SelectedPlugins = pluginNames;
    }
}
