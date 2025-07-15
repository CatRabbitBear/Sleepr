using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Sleepr.Controllers;
using Sleepr.Interfaces;
using Sleepr.Pipeline;
using Sleepr.Pipeline.Steps;
using Sleepr.Services;

namespace Sleepr.Agents;
class AgentFactory : IAgentFactory
{
    private readonly ILogger<AgentFactory> _logger;
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly McpPluginManager _pluginManager;
    private readonly Kernel _kernel;

    public AgentFactory(
        ILogger<AgentFactory> logger,
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        McpPluginManager pluginManager,
        Kernel kernel)
    {
        _logger = logger;
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _pluginManager = pluginManager;
        _kernel = kernel;
    }

    public async Task<AgentContext> CreateOrchestratorAgentAsync(string promptFileName = "orchestrator")
    {
        var config = await _promptLoader.LoadAsync(promptFileName);
        _logger.LogInformation("Creating Orchestrator Agent with config name: {Config}", config.Name);
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            // Clone so we can use different combinations of plugins for different agents
            Kernel = _kernel.Clone(),
        };
        
        var pipeline = new AgentPipelineBuilder()
            .Use(new OrchestratePluginsStep(agent, _pluginManager))
            .Build();
        
        return new AgentContext(agent, pipeline: pipeline);
    }

    public async Task<AgentContext> CreateTaskAgentAsync(string path, List<string> selectedPlugins)
    {
        var config = await _promptLoader.LoadAsync(path);
        var clonedKernel = _kernel.Clone();
        var pluginIds = new List<string>();
        foreach (var plugin in selectedPlugins)
        {
            try
            {
                // Add the plugin to the cloned kernel
                var manifest = _pluginManager.GetManifestByName(plugin);
                var client = await _pluginManager.AcquireClientAsync(manifest.Id);
                var tools = await client.ListToolsAsync();
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                clonedKernel.Plugins.AddFromFunctions(manifest.Id, tools.Select(aiFunction => aiFunction.AsKernelFunction()));
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                pluginIds.Add(manifest.Id);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Plugin not found: {Plugin}", plugin);
                // Handle the case where the plugin is not found
                // You might want to throw an exception or log an error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add plugin: {Plugin}", plugin);
                // Handle other exceptions that may occur
            }
        }
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            Kernel = clonedKernel,
            Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true }) })
        };
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        
        var pipeline = new AgentPipelineBuilder()
            .Use(new RunTaskAgentStep(agent))
            .Build();
        return new AgentContext(agent, pipeline: pipeline);

    }
}