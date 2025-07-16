using Microsoft.Extensions.Logging;
using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Converts the request history into a ChatHistory instance.
/// </summary>
public class LoadChatHistoryStep : IAgentPipelineStep
{
    private readonly ILogger _logger;

    public LoadChatHistoryStep(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoadChatHistoryStep>();
    }

    public Task ExecuteAsync(PipelineContext context)
    {
        if (context.RequestHistory == null || context.RequestHistory.Count == 0)
        {
            context.ChatHistory = null;
            return Task.CompletedTask;
        }
        context.ChatHistory = ChatHistoryBuilder.FromChatRequest(context.RequestHistory);
        return Task.CompletedTask;
    }
}
