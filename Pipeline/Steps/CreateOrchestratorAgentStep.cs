using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Sleepr.Interfaces;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Creates the orchestrator agent if it doesn't already exist in the context.
/// </summary>
public class CreateOrchestratorAgentStep : IAgentPipelineStep
{
    private readonly IPromptLoader _promptLoader;
    private readonly IPromptTemplateFactory _templateFactory;
    private readonly string _path;
    private readonly string _agentKey;

    public CreateOrchestratorAgentStep(
        IPromptLoader promptLoader,
        IPromptTemplateFactory templateFactory,
        string path = "orchestrator",
        string agentKey = "orchestrator")
    {
        _promptLoader = promptLoader;
        _templateFactory = templateFactory;
        _path = path;
        _agentKey = agentKey;
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (context.Agents.ContainsKey(_agentKey))
        {
            return;
        }

        var config = await _promptLoader.LoadAsync(_path);
        var agent = new ChatCompletionAgent(config, _templateFactory)
        {
            Kernel = context.Kernel.Clone(),
        };
        context.Agents[_agentKey] = new AgentContext(agent);
    }
}
