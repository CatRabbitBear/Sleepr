using Sleepr.Controllers;
using Sleepr.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Steps;
using Microsoft.SemanticKernel;

namespace Sleepr.Agents;

public class SleeprAgentRunner : IAgentRunner
{
    private readonly ILogger<SleeprAgentRunner> _logger;
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly IPipelineContextFactory _contextFactory;
    private readonly IAgentOutput _outputManager;

    public SleeprAgentRunner(
        ILogger<SleeprAgentRunner> logger,
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        IPipelineContextFactory contextFactory,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _contextFactory = contextFactory;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var pipeline = new AgentPipelineBuilder()
            .Use(new CreateOrchestratorAgentStep(_promptLoader, _templateFactory))
            .Use(new OrchestratePluginsStep())
            .Use(new CreateTaskAgentStep(_promptLoader, _templateFactory))
            .Use(new RunTaskAgentStep())
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
