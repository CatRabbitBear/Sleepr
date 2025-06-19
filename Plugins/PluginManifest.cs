namespace Sleepr.Plugins
{
    public class PluginManifest
    {
        public string Id { get; set; } = null!;              // “weather”
        public string Name { get; set; } = null!;            // “Weather Tools”
        public string? Version { get; set; }                 // “1.2.3”
        public string Description { get; set; } = null!;     // “Query real-time weather forecasts.”
        public string Transport { get; set; } = null!;       // “stdio”
        public string? Runtime { get; set; }                 // “npx”
        public List<string>? Args { get; set; }              // [ "-y", "@sleepr-plugins/weather-server" ]

        /// <summary>
        /// Arbitrary ENV vars (API keys, URLs, flags, etc.).
        /// </summary>
        public Dictionary<string, string>? Env { get; set; }

        /// <summary>
        /// Optional health-check command to verify the server is up.
        /// </summary>
        public HealthCheckConfig? HealthCheck { get; set; }

        /// <summary>
        /// Milliseconds to wait before killing a stalled tool call.
        /// </summary>
        public int? TimeoutMs { get; set; }

        public List<string>? Tags { get; set; }              // [ "weather", "forecast", "location" ]
        public string? Origin { get; set; }                  // Git repo URL, npm package, etc.
    }

    public class HealthCheckConfig
    {
        /// <summary>
        /// The command (and args) to run to check liveness.
        /// e.g. "npx -y @sleepr-plugins/weather-server --ping"
        /// </summary>
        public string Command { get; set; } = null!;

        /// <summary>
        /// How long (in ms) to wait for the health-check to complete.
        /// </summary>
        public int TimeoutMs { get; set; }
    }
}