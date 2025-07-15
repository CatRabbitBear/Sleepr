using Microsoft.SemanticKernel;
using Sleepr.Controllers;
using Sleepr.Services;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline;

/// <summary>
/// Factory for creating <see cref="PipelineContext"/> instances with a cloned kernel.
/// </summary>
public class PipelineContextFactory : IPipelineContextFactory
{
    public PipelineContextFactory()
    {
    }

    /// <summary>
    /// Create a new context for the given request history. The kernel is cloned
    /// so that plugins can be added without affecting other contexts.
    /// </summary>
    public PipelineContext Create(List<AgentRequestItem> history)
    {
        return new PipelineContext(history);
    }
}
