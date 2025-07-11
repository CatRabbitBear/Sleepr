using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline;

/// <summary>
/// Default implementation of IAgentPipeline that runs each step sequentially.
/// </summary>
public class AgentPipeline : IAgentPipeline
{
    private readonly IList<IAgentPipelineStep> _steps;

    public AgentPipeline(IList<IAgentPipelineStep> steps)
    {
        _steps = steps;
    }

    public async Task RunAsync(PipelineContext context)
    {
        foreach (var step in _steps)
        {
            await step.ExecuteAsync(context);
        }
    }
}
