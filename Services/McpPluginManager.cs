using ModelContextProtocol.Client;
using Sleepr.Interfaces;
using Sleepr.Plugins;
using Quickenshtein;

namespace Sleepr.Services;

public class McpPluginManager: IAsyncDisposable
{
    // Load all manifests up-front (from JSON files, DB, etc.)
    private readonly ILogger<McpPluginManager> _logger;
    private readonly List<PluginManifest> _manifests;
    private readonly McpServerPool _pool;

    public McpPluginManager(IEnumerable<PluginManifest> manifests, ILogger<McpPluginManager> logger)
    {
        _manifests = manifests.ToList();
        _pool = new McpServerPool();
        _logger = logger;
    }

    // Repo-like methods:
    public IReadOnlyList<PluginManifest> ListAvailableServers()
        => _manifests.AsReadOnly();

    /// <summary>
    /// Try exact lookup first, then fuzzy‐match by given key selector.
    /// Throws if nothing within threshold.
    /// </summary>
    private PluginManifest LookupManifest(
        string input,
        Func<PluginManifest, string> keySelector,
        int maxDistance = 3)
    {
        // exact match
        var exact = _manifests.FirstOrDefault(m =>
            string.Equals(keySelector(m), input, StringComparison.OrdinalIgnoreCase));
        if (exact != null) return exact;

        // fuzzy fallback
        var best = _manifests
            .Select(m => new
            {
                Manifest = m,
                Distance = Levenshtein.GetDistance(
                    input,
                    keySelector(m))
            })
            .OrderBy(x => x.Distance)
            .First();

        if (best.Distance <= maxDistance)
            return best.Manifest;

        // no close match found
        _logger.LogWarning(
            "No exact match for '{Input}' found. Closest match '{BestMatch}' with distance {Distance} exceeds threshold {MaxDistance}.",
            input, best.Manifest.Id, best.Distance, maxDistance);

        throw new KeyNotFoundException(
            $"No manifest matching '{input}' and no close fuzzy match found.");
    }

    public PluginManifest GetManifestById(string id)
        => LookupManifest(id, m => m.Id);

    public PluginManifest GetManifestByName(string name)
        => LookupManifest(name, m => m.Name);

    public async Task<IMcpClient> AcquireClientAsync(string idOrName)
    {
        // Let callers pass either legal Id or descriptive Name
        var manifest = _manifests.Any(m =>
                string.Equals(m.Id, idOrName, StringComparison.OrdinalIgnoreCase))
            ? GetManifestById(idOrName)
            : GetManifestByName(idOrName);

        // proceed with pooling
        return await _pool.AcquireAsync(
            serverKey: manifest.Id,
            createClient: () => CreateClientFromManifest(manifest));
    }

    public Task ReleaseClientAsync(string id, bool dispose = false)
        => _pool.ReleaseAsync(id, dispose);

    // Builds the actual IMcpClient process from the manifest:
    private Task<IMcpClient> CreateClientFromManifest(PluginManifest m)
    {
        var opts = new StdioClientTransportOptions
        {
            Name = m.Id,
            Command = m.Runtime ?? "npx",
            Arguments = m.Args?.ToArray() ?? Array.Empty<string>(),
            EnvironmentVariables = m.Env?.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value) ?? new Dictionary<string, string?>(),
        };
        return McpClientFactory.CreateAsync(new StdioClientTransport(opts));
    }

    public async ValueTask DisposeAsync() => await _pool.DisposeAsync();
}
