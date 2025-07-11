using Microsoft.SemanticKernel;
using Sleepr.Controllers;
using Sleepr.Services;

namespace Sleepr.Pipeline;

/// <summary>
/// Carries request data and state between pipeline steps.
/// </summary>
public class PipelineContext
{
    public List<AgentRequestItem> RequestHistory { get; }
    public Kernel Kernel { get; set; }
    public McpPluginManager PluginManager { get; }
    public IList<string> SelectedPlugins { get; set; } = new List<string>();
    public string? FinalResult { get; set; }

    public PipelineContext(
        List<AgentRequestItem> history,
        Kernel kernel,
        McpPluginManager pluginManager)
    {
        RequestHistory = history;
        Kernel = kernel;
        PluginManager = pluginManager;
    }
}
