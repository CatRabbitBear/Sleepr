using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Runs the chat completion service using the built chat history.
/// </summary>
public class RunChatCompletionStep : IAgentPipelineStep
{
    private readonly Kernel _kernel;

    public RunChatCompletionStep(Kernel kernel)
    {
        this._kernel = kernel;
    }
    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.ChatHistory == null)
        {
            return;
        }

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        var result = await chatService.GetChatMessageContentAsync(context.ChatHistory);

        context.FinalResult = result.Content ?? result.ToString();
    }
}
