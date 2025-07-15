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
    private readonly IPipelineContextFactory _contextFactory;
    private readonly IAgentFactory _agentFactory;
    private readonly IAgentOutput _outputManager;

    public SleeprAgentRunner(
        ILogger<SleeprAgentRunner> logger,
        IPipelineContextFactory contextFactory,
        IAgentFactory agentFactory,
        IAgentOutput outputManager)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _agentFactory = agentFactory;
        _outputManager = outputManager;
    }

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        _logger.LogInformation("Running task with history of {Count} items", history.Count);
        if (history == null || history.Count == 0)
        {
            _logger.LogWarning("No history provided, returning empty response.");
            return new AgentResponse { Result = string.Empty, FilePath = string.Empty };
        }

        if (history.Last().Role != MessageType.User)
        {
            _logger.LogWarning("Last message in history is not from the user, expected a user message.");
        }

        var orchestratorAgent = await _agentFactory.CreateOrchestratorAgentAsync("orchestrator");
        var orchestratorContext = orchestratorAgent.PipelineContext ?? _contextFactory.Create(history);

        await orchestratorAgent.Pipeline.RunAsync(orchestratorContext);
        var plugins = orchestratorContext.SelectedPlugins;

        var taskAgent = await _agentFactory.CreateTaskAgentAsync("task", plugins);
        var taskContext = taskAgent.PipelineContext ?? _contextFactory.Create(history);
        
        await taskAgent.Pipeline.RunAsync(taskContext);

        return new AgentResponse
        {
            Result = taskContext.FinalResult ?? string.Empty,
            FilePath = taskContext.FilePath
        };
    }
}
