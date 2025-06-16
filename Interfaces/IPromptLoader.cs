using Microsoft.SemanticKernel;

namespace Sleepr.Interfaces;

public interface IPromptLoader
{
    Task<PromptTemplateConfig> LoadAsync(string name, CancellationToken cancellationToken = default);
}
