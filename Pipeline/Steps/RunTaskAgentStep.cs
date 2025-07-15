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
        var agentCtx = await _factory.CreateTaskAgentAsync(_path, pluginNames);

        var toolsList = context.Agents.TryGetValue("orchestrator", out var orchestrator)
            ? orchestrator.ToolsList
            : null;
        var thread = new ChatHistoryAgentThread();
        agentCtx.Thread = thread;
        agentCtx.ToolsList = toolsList;
        context.Agents["task-agent"] = agentCtx;

        var args = new KernelArguments();
        if (!string.IsNullOrWhiteSpace(toolsList))
        {
            args["tools_list"] = toolsList;
        }
        string finalResponse = string.Empty;

        await foreach (ChatMessageContent message in agentCtx.Agent.InvokeAsync(context.UserMessage ?? string.Empty, thread, new AgentInvokeOptions { KernelArguments = args }))
        {
            if (message.Role == AuthorRole.Assistant)
            {
                finalResponse = message.Content ?? string.Empty;
            }
        }

        context.FinalResult = finalResponse;
    }
}
