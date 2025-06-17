using Sleepr.Controllers;


namespace Sleepr.Interfaces;

public interface ISleeprAgent
{
    Task<AgentResponse> InvokeAsync(string input);
    string Name { get; }
    string Type { get; }
}
