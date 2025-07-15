using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline;
using System.Linq;

namespace Sleepr.Pipeline.Steps;

public class RunTaskAgentStep : IAgentPipelineStep
{
    private readonly ISleeprAgentFactory _factory;
    private readonly string _path;

    public RunTaskAgentStep(ISleeprAgentFactory factory, string path = "task-runner")
    {
        _factory = factory;
        _path = path;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        var pluginNames = context.SelectedPlugins.ToList();
        var agent = await _factory.CreateTaskAgentAsync(_path, pluginNames);

        var pluginIds = pluginNames
            .Select(name => context.PluginManager.GetManifestByName(name).Id)
            .ToList();

        var toolsList = context.Agents.TryGetValue("orchestrator", out var orchestrator)
            ? orchestrator.ToolsList
            : null;
        var thread = new ChatHistoryAgentThread();
        context.Agents["task-agent"] = new AgentContext(agent, pluginIds)
        {
            Thread = thread,
            ToolsList = toolsList
        };

        var args = new KernelArguments();
        if (!string.IsNullOrWhiteSpace(toolsList))
        {
            args["tools_list"] = toolsList;
        }
        string finalResponse = string.Empty;

        await foreach (ChatMessageContent message in agent.InvokeAsync(context.UserMessage ?? string.Empty, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                finalResponse = message.Content ?? string.Empty;
            }
        }

        context.FinalResult = finalResponse;
    }
}
