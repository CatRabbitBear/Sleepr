namespace Sleepr.Plugins;

public class PluginManifest
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Transport { get; set; }
    public string? Runtime { get; set; }
    public List<string>? Args { get; set; }
    public string? Endpoint { get; set; }
    public AuthConfig? Auth { get; set; }
    public List<string>? Tags { get; set; }
    public string? Origin { get; set; }
}

public class AuthConfig
{
    public string Type { get; set; } = "none";
    public string? Variable { get; set; }
}