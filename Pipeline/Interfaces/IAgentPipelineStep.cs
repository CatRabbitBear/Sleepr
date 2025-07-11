namespace Sleepr.Pipeline.Interfaces;

/// <summary>
/// Represents a single unit of work in the agent pipeline.
/// </summary>
public interface IAgentPipelineStep
{
    Task ExecuteAsync(PipelineContext context);
}
