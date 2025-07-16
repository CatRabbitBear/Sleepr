using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Agents;
using Sleepr.Pipeline.Interfaces;
using Sleepr.Pipeline.Utils;
using Sleepr.Services;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Runs the orchestrator agent to select which plugins should be loaded.
/// </summary>
public class OrchestratePluginsStep : IAgentPipelineStep
{
    private readonly ChatCompletionAgent _agent;
    private readonly McpPluginManager _pluginManager;
    private readonly ILogger _logger;

    public OrchestratePluginsStep(ChatCompletionAgent agent, McpPluginManager pluginManager, ILoggerFactory loggerFactory)
    {
        _pluginManager = pluginManager;
        _agent = agent;
        _logger = loggerFactory.CreateLogger<OrchestratePluginsStep>();
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.UserMessage == null)
        {
            throw new ArgumentNullException(nameof(context.UserMessage), "User message cannot be null in PipelineContext.");
        }

        var toolsList = _pluginManager.ListAvailableServers()
            .Select(p => $"{p.Name}: {p.Description ?? "<no description>"}")
            .ToList();
        var args = new KernelArguments { ["tools_list"] = toolsList };

        const int maxRetries = 2;
        int attempt = 0;
        var pluginNames = new List<string>();

        // Keep track of thread between attempts
        var thread = context.AgentThread;
        var userPrompt = context.UserMessage;

        while (attempt <= maxRetries)
        {
            var messages = await _agent
                .InvokeAsync(
                    userPrompt!,
                    thread,
                    new AgentInvokeOptions { KernelArguments = args }
                )
                .ToListAsync();

            var lastAssistant = messages.LastOrDefault(m => m.Message.Role == AuthorRole.Assistant);
            thread = (ChatHistoryAgentThread?)(messages.LastOrDefault()?.Thread);
            context.AgentThread = thread;

            if (lastAssistant == null)
            {
                _logger.LogWarning("No assistant response received when orchestrating plugins.");
                break;
            }

            var assistantContent = lastAssistant.Message.Content ?? string.Empty;
            if (PluginUtils.TryGetToolsFromJsonResponse(assistantContent, out pluginNames))
            {
                break;
            }

            attempt++;
            if (attempt > maxRetries)
            {
                _logger.LogWarning("Failed to parse plugin list after {Attempts} attempts", attempt);
                break;
            }

            userPrompt = "The previous response was not valid JSON. Please reply only with JSON in the form: {\"tools\": [\"name\"]}";
        }

        context.SelectedPlugins = pluginNames;
    }
}
