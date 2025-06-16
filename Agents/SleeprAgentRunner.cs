using Sleepr.Interfaces;
using static Sleepr.Controllers.AgentController;

namespace Sleepr.Agents;

public class SleeprAgentRunner : IAgentRunner
{
    private readonly ISleeprAgentFactory _factory;
    private readonly IMcpPluginManager _pluginManager;
    private readonly IAgentOutput _outputManager;

    public async Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history)
    {
        var chatHistory = ChatHistoryBuilder.FromAgentRequest(history);
        var orchestrator = await _factory.CreateOrchestratorAgentAsync(chatHistory);

        var pluginNames = await orchestrator.InvokeAsync("Which tools are needed?");
        var taskAgent = await _factory.CreateTaskAgentAsync(pluginNames, chatHistory);

        var response = await taskAgent.InvokeAsync("Do the thing");
        var path = await _outputManager.SaveAsync(response.Content);

        return new AgentResponse { Result = response.Content, FilePath = path };
    }
}
