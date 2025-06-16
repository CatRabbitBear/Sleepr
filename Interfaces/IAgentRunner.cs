using Sleepr.Controllers;
using static Sleepr.Controllers.AgentController;

namespace Sleepr.Interfaces;

public interface IAgentRunner
{
    Task<AgentResponse> RunTaskAsync(List<AgentRequestItem> history);
}
