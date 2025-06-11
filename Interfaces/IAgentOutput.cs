namespace Sleepr.Interfaces;

public interface IAgentOutput
{
    Task<string> SaveAsync(string content);
}
