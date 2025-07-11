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
    private readonly Kernel _kernel;
    private readonly McpPluginManager _pluginManager;

    public PipelineContextFactory(Kernel kernel, McpPluginManager pluginManager)
    {
        _kernel = kernel;
        _pluginManager = pluginManager;
    }

    /// <summary>
    /// Create a new context for the given request history. The kernel is cloned
    /// so that plugins can be added without affecting other contexts.
    /// </summary>
    public PipelineContext Create(List<AgentRequestItem> history)
    {
        var clone = _kernel.Clone();
        return new PipelineContext(history, clone, _pluginManager);
    }
}
