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
    private readonly IAgentOutput _outputManager;

    public ChatCompletionsRunner(
        ILogger<ChatCompletionsRunner> logger,
        IPipelineContextFactory contextFactory,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var pipeline = new AgentPipelineBuilder()
            .Use(new LoadChatHistoryStep())
            .Use(new RunChatCompletionStep())
            .Use(new SaveOutputStep(_outputManager))
            .Use(new CleanupAgentsStep())
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
