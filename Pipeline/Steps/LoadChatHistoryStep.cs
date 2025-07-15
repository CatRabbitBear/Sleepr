using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Converts the request history into a ChatHistory instance.
/// </summary>
public class LoadChatHistoryStep : IAgentPipelineStep
{
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
