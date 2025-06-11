using Sleepr.Interfaces;

namespace Sleepr.Services;

public class FileAgentOutput : IAgentOutput
{
    private readonly string _outputDirectory;
    public FileAgentOutput(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
    }
    public async Task<string> SaveAsync(string content)
    {
        var fileName = $"{Guid.NewGuid()}.txt";
        var filePath = Path.Combine(_outputDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
        return filePath;
    }
}