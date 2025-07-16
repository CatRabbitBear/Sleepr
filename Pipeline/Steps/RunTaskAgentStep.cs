using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Utils;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Executes the task agent to fulfill the user's request.
/// </summary>
public class RunTaskAgentStep : IAgentPipelineStep
{
    private readonly ChatCompletionAgent _agent;
    private readonly ILogger _logger;

    public RunTaskAgentStep(ChatCompletionAgent agent, ILoggerFactory loggerFactory)
    {
        _agent = agent;
        _logger = loggerFactory.CreateLogger<RunTaskAgentStep>();
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if ( context.UserMessage == null )
        {
            throw new ArgumentNullException(nameof(context.UserMessage), "User message cannot be null in PipelineContext.");
        }

        // 1) Collect all the chat messages into a List

        var args = new KernelArguments
        {
            ["tools_list"] = String.Join(", ", context.SelectedPlugins)
        };
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
        }
        else
        {
            // handle the case where no assistant response was returned - implement logging for steps
        }
        context.FinalResult = lastAssistant?.Message.Content ?? string.Empty;
    }
}
