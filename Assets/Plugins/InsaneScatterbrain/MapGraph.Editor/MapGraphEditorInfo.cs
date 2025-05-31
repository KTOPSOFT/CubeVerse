using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Contains information about the Map Graph Editor package.
    /// </summary>
    public static class MapGraphEditorInfo
    {
        public const string LatestVersionUrl = "https://mapgraph.insanescatterbrain.com/latest-version.txt"; 
        public static readonly string LatestKnownVersionFilePath = $"{Application.dataPath}/Plugins/InsaneScatterbrain/MapGraph.Editor.Data/.latest-version";
        
        public const string VersionFilePathRelative = "/Plugins/InsaneScatterbrain/MapGraph.Editor.Data/version.txt";
        public static readonly string VersionFilePathAbsolute = $"{Application.dataPath}{VersionFilePathRelative}";
        
        private static Version version;

        private static Version latestVersion;

        public static Version Version
        {
            get
            {
                if (version == null)
                {
                    version = new Version(File.ReadAllText(VersionFilePathAbsolute).Trim());
                }

                return version;
            }
        }

        public static async Task<Version> GetLatestVersion()
        {
            if (latestVersion != null) return latestVersion;
            
            // Download the file that contains the latest version available online.
            var webClient = new WebClient();
            var latestVersionString = await webClient.DownloadStringTaskAsync(LatestVersionUrl);
            latestVersion = new Version(latestVersionString);
            return latestVersion;
        }
    }
}