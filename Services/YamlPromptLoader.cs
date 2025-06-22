using Microsoft.SemanticKernel;
using Sleepr.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sleepr.Services;

public class YamlPromptLoader : IPromptLoader 
{
    private readonly string _promptDirectory;
    private readonly ILogger<YamlPromptLoader> _logger;
    private readonly IDeserializer _yamlDeserializer;

    // promptDirectory is the directory where YAML prompt files are stored.
    public YamlPromptLoader(ILogger<YamlPromptLoader> logger, string promptDirectory = "prompts")
    {
        _promptDirectory = promptDirectory;
        _logger = logger;
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    }

    public async Task<PromptTemplateConfig> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_promptDirectory, name + ".yaml");

        if (!File.Exists(path))
        {
            _logger.LogError("Prompt file not found: {Path}", path);
            throw new FileNotFoundException($"Prompt file not found: {path}");
        }
            

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        var yaml = await reader.ReadToEndAsync(cancellationToken);

        var config = _yamlDeserializer.Deserialize<PromptTemplateConfig>(yaml);

        if (config == null)
        {
            _logger.LogError("Failed to deserialize prompt config: {Name}", name);
            throw new InvalidDataException($"Failed to deserialize prompt config: {name}");
        }

        return config;
    }
}
