using Sleepr.Controllers;

namespace Sleepr.Pipeline.Interfaces;

/// <summary>
/// Creates <see cref="PipelineContext"/> instances for agent pipelines.
/// </summary>
public interface IPipelineContextFactory
{
    PipelineContext Create(List<AgentRequestItem> history);
}
