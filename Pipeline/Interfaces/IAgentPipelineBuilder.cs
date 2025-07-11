namespace Sleepr.Pipeline.Interfaces;

/// <summary>
/// Fluent builder used to compose an agent pipeline.
/// </summary>
public interface IAgentPipelineBuilder
{
    IAgentPipelineBuilder Use(IAgentPipelineStep step);
    IAgentPipeline Build();
}
