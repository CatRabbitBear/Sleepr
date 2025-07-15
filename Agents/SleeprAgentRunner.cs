using Sleepr.Controllers;
using Sleepr.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Steps;

namespace Sleepr.Agents;

public class SleeprAgentRunner : IAgentRunner
{
    private readonly ILogger<SleeprAgentRunner> _logger;
    private readonly ISleeprAgentFactory _factory;
    private readonly IPipelineContextFactory _contextFactory;
    private readonly IAgentOutput _outputManager;

    public SleeprAgentRunner(
        ILogger<SleeprAgentRunner> logger,
        ISleeprAgentFactory factory,
        IPipelineContextFactory contextFactory,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _factory = factory;
        _contextFactory = contextFactory;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var pipeline = new AgentPipelineBuilder()
            .Use(new OrchestratePluginsStep(_factory))
            .Use(new RunTaskAgentStep(_factory))
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
