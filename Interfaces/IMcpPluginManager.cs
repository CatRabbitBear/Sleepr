using ModelContextProtocol.Client;
using Sleepr.Plugins;

namespace Sleepr.Interfaces;

public interface IMcpPluginManager
{
    IReadOnlyList<PluginManifest> ListAvailableServers();
    PluginManifest? GetManifest(string id);
    Task<IMcpClient> AcquireClientAsync(string id);
    Task ReleaseClientAsync(string id, bool dispose = false);
    Task<IMcpClient> CreateClientFromManifest(PluginManifest m);
    ValueTask DisposeAsync();
}
