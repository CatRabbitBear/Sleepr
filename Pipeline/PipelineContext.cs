using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Controllers;
using Sleepr.Services;

namespace Sleepr.Pipeline;

/// <summary>
/// Carries request data and state between pipeline steps.
/// </summary>
public class PipelineContext
{
    public List<AgentRequestItem>? RequestHistory { get; }
    public List<string> SelectedPlugins { get; set; } = new List<string>();
    public ChatHistory? ChatHistory { get; set; }
    public ChatHistoryAgentThread? AgentThread { get; set; }
    public string? UserMessage { get; set; }
    public string? FinalResult { get; set; }
    public string? FilePath { get; set; }

    public PipelineContext(List<AgentRequestItem> history)
    {
        RequestHistory = history;

        // I havent thought this through properly, might introduce a subtle bug
        if (history.Count > 0 && history.Last().Role == MessageType.User)
        {
            UserMessage = history.Last().ToString();
        }
        AgentThread = new ChatHistoryAgentThread();
    }

    public PipelineContext(string userMessage)
    {
        UserMessage = userMessage;
        AgentThread = new ChatHistoryAgentThread();
    }
}
