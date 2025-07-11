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
        context.ChatHistory = ChatHistoryBuilder.FromChatRequest(context.RequestHistory);
        return Task.CompletedTask;
    }
}
