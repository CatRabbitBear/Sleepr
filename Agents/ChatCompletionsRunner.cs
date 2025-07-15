using Microsoft.SemanticKernel;
using Sleepr.Controllers;
using Sleepr.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Steps;

namespace Sleepr.Agents;

public class ChatCompletionsRunner : IChatCompletionsRunner
{
    private readonly ILogger<ChatCompletionsRunner> _logger;
    private readonly IPipelineContextFactory _contextFactory;
    private readonly Kernel _kernel;
    private readonly IAgentOutput _outputManager;

    public ChatCompletionsRunner(
        ILogger<ChatCompletionsRunner> logger,
        IPipelineContextFactory contextFactory,
        Kernel kernel,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _kernel = kernel;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var pipeline = new AgentPipelineBuilder()
            .Use(new LoadChatHistoryStep())
            .Use(new RunChatCompletionStep(_kernel))
            .Use(new SaveOutputStep(_outputManager))
            .Build();

        var context = _contextFactory.Create(history);
        await pipeline.RunAsync(context);

        return new AgentResponse
        {
            Result = context.FinalResult ?? string.Empty,
            FilePath = context.FilePath
        };
    }
}
