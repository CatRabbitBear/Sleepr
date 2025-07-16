using Microsoft.Extensions.Logging;
using Sleepr.Interfaces;
using Sleepr.Pipeline.Interfaces;

namespace Sleepr.Pipeline.Steps;

/// <summary>
/// Persists the final result using an IAgentOutput implementation.
/// </summary>
public class SaveOutputStep : IAgentPipelineStep
{
    private readonly IAgentOutput _output;
    private readonly ILogger _logger;

    public SaveOutputStep(IAgentOutput output, ILoggerFactory loggerFactory)
    {
        _output = output;
        _logger = loggerFactory.CreateLogger<SaveOutputStep>();
    }

    public async Task ExecuteAsync(PipelineContext context)
    {
        if (string.IsNullOrWhiteSpace(context.FinalResult))
        {
            return;
        }

        context.FilePath = await _output.SaveAsync(context.FinalResult);
    }
}
