using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Executes the task agent to fulfill the user's request.
/// </summary>
public class RunTaskAgentStep : IAgentPipelineStep
{
    private readonly string _agentKey;

    public RunTaskAgentStep(string agentKey = "task-agent")
    {
        _agentKey = agentKey;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (!context.Agents.TryGetValue(_agentKey, out var agentCtx))
        {
            return;
        }

        var toolsList = context.Agents.TryGetValue("orchestrator", out var orchestrator)
            ? orchestrator.ToolsList
            : null;
        var thread = new ChatHistoryAgentThread();
        agentCtx.Thread = thread;
        agentCtx.ToolsList = toolsList;

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
