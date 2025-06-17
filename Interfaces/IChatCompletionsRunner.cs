using Sleepr.Controllers;

namespace Sleepr.Interfaces;

public interface IChatCompletionsRunner
{
    Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history);
}
