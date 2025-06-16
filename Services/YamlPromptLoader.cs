using Microsoft.SemanticKernel;
using Sleepr.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Sleepr.Services;

public class YamlPromptLoader : IPromptLoader 
{
    private readonly string _promptDirectory;
    private readonly IDeserializer _yamlDeserializer;

    public YamlPromptLoader(string promptDirectory = "prompts")
    {
        _promptDirectory = promptDirectory;
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    public async Task<PromptTemplateConfig> LoadAsync(string name, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_promptDirectory, name + ".yaml");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Prompt file not found: {path}");

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        var yaml = await reader.ReadToEndAsync(cancellationToken);

        var config = _yamlDeserializer.Deserialize<PromptTemplateConfig>(yaml);

        return config ?? throw new InvalidDataException($"Failed to deserialize prompt config: {name}");
    }
}
