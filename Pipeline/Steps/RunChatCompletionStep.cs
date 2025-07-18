using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;

    public RunChatCompletionStep(Kernel kernel, ILoggerFactory loggerFactory)
    {
        this._kernel = kernel;
        _logger = loggerFactory.CreateLogger<RunChatCompletionStep>();
    }
    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.ChatHistory == null)
        {
            return;
        }

        var chatService = _kernel.GetRequiredService<IChatCompletionService>();
        // TODO: Wrap in try-catch to handle any exceptions from the chat service
        var result = await chatService.GetChatMessageContentAsync(context.ChatHistory);
        _logger.LogInformation("Chat completion service returned result: {Result}", result.Content ?? "<null>");

        context.FinalResult = result.Content ?? result.ToString();
    }
}
