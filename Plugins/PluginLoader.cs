using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Sleepr.Plugins;  // adjust to your namespace

namespace Sleepr.Plugins;
public static class PluginLoader
{
    /// <summary>
    /// Loads all PluginManifest JSON files from the given folder.
    /// </summary>
    public static List<PluginManifest> LoadManifests(string folderPath)
    {
        var manifests = new List<PluginManifest>();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        // Ensure folder exists
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException($"Plugin folder not found: {folderPath}");
        }

        // Iterate over every .json file
        foreach (var file in Directory.GetFiles(folderPath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var manifest = JsonSerializer.Deserialize<PluginManifest>(json, options);
                if (manifest != null)
                {
                    manifests.Add(manifest);
                }
                else
                {
                    Console.WriteLine($"Warning: {file} deserialized to null.");
                }
            }
            catch (JsonException je)
            {
                Console.WriteLine($"Error parsing {file}: {je.Message}");
            }
            catch (IOException ioe)
            {
                Console.WriteLine($"Error reading {file}: {ioe.Message}");
            }
        }

        return manifests;
    }
}