namespace Sleepr.Pipeline.Interfaces;

/// <summary>
/// Executes a configured sequence of pipeline steps.
/// </summary>
public interface IAgentPipeline
{
    Task RunAsync(PipelineContext context);
}
