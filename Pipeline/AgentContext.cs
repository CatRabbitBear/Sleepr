using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Sleepr.Pipeline;

/// <summary>
/// Holds a created agent instance along with metadata needed for cleanup.
/// </summary>
public class AgentContext
{
    public ChatCompletionAgent Agent { get; }
    public IList<string> PluginIds { get; }
    public ChatHistoryAgentThread? Thread { get; set; }
    public string? ToolsList { get; set; }

    public AgentContext(ChatCompletionAgent agent, IList<string>? pluginIds = null)
    {
        Agent = agent;
        PluginIds = pluginIds ?? new List<string>();
    }
}
