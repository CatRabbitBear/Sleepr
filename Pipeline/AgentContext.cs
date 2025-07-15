using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline;

/// <summary>
/// Holds a created agent instance along with metadata needed for cleanup.
/// </summary>
public class AgentContext
{
    public ChatCompletionAgent Agent { get; }
    public IAgentPipeline Pipeline { get; set; }
    public PipelineContext? PipelineContext { get; set; }

    public AgentContext(ChatCompletionAgent agent, IAgentPipeline pipeline)
    {
        Agent = agent;
        Pipeline = pipeline;
    }
}
