using Sleepr.Interfaces;

namespace Sleepr.Services;

public class FileAgentOutput : IAgentOutput
{
    private ILogger<FileAgentOutput> _logger;
    private readonly string _outputDirectory;
    public FileAgentOutput(ILogger<FileAgentOutput> logger, string outputDirectory)
    {
        _logger = logger;
        _outputDirectory = outputDirectory;
    }
    public async Task<string> SaveAsync(string content)
    {
        var fileName = $"{Guid.NewGuid()}.txt";
        var filePath = Path.Combine(_outputDirectory, fileName);
        _logger.LogInformation("Saving agent output to file: {FilePath}", filePath);
        await File.WriteAllTextAsync(filePath, content);
        return filePath;
    }
}