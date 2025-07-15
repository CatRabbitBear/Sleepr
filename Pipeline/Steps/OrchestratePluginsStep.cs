using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Agents;
using Sleepr.Interfaces;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Pipeline.Utils;
using Sleepr.Services;

namespace Sleepr.Pipeline.Steps;

public class OrchestratePluginsStep : IAgentPipelineStep
{
    private readonly ISleeprAgentFactory _factory;
    private readonly string _path;

    public OrchestratePluginsStep(ISleeprAgentFactory factory, string path = "orchestrator")
    {
        _factory = factory;
        _path = path;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        var (userMessage, thread) = ChatHistoryBuilder.FromAgentRequest(context.RequestHistory);
        context.UserMessage = userMessage;

        var toolsList = PluginUtils.BuildToolsList(context.PluginManager);
        var args = new KernelArguments { ["tools_list"] = toolsList };

        var orchestrator = await _factory.CreateOrchestratorAgentAsync(_path);
        context.Agents["orchestrator"] = new AgentContext(orchestrator)
        {
            Thread = thread,
            ToolsList = toolsList
        };

        var pluginNames = new List<string>();
        await foreach (ChatMessageContent message in orchestrator.InvokeAsync(userMessage, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                pluginNames = PluginUtils.GetToolsFromJsonResponse(message.Content ?? string.Empty);
            }
        }
        context.SelectedPlugins = pluginNames;
    }
}
