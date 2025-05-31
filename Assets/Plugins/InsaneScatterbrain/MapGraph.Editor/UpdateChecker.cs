using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public static class UpdateChecker
    {
        /// <summary>
        /// Checks if a newer version is available and caches the result so the notification only shows up once for each
        /// new version.
        /// </summary>
        /// <returns>True if there's a new version available, false otherwise.</returns>
        public static async Task<bool> CheckForNewUpdatesAsync()
        {
            var latestKnownVersionFilePath = MapGraphEditorInfo.LatestKnownVersionFilePath;
            const string latestVersionUrl = MapGraphEditorInfo.LatestVersionUrl;
            
            // Check if there's a latest known version stored yet.
            Version latestKnownVersion;
            if (!File.Exists(latestKnownVersionFilePath))
            {
                // If not, the latest know version is the one that's installed.
                File.WriteAllText(latestKnownVersionFilePath, MapGraphEditorInfo.Version.ToString());
                latestKnownVersion = MapGraphEditorInfo.Version;
            }
            else
            {
                latestKnownVersion = new Version(File.ReadAllText(latestKnownVersionFilePath).Trim());
                
                // If the currently installed version is newer than what's supposed to be the known latest version, the
                // current version is considered to be the latest version.
                if (MapGraphEditorInfo.Version > latestKnownVersion)
                {
                    File.WriteAllText(latestKnownVersionFilePath, MapGraphEditorInfo.Version.ToString());
                    latestKnownVersion = MapGraphEditorInfo.Version;
                }
            }

            // Download the file that contains the latest version available online.
            var webClient = new WebClient();
            var latestVersionString = await webClient.DownloadStringTaskAsync(latestVersionUrl);
            var latestVersion = new Version(latestVersionString);

            // There's no new update, meaning that either the current version is the latest version or the user has already
            // been informed about this version.
            if (latestKnownVersion >= latestVersion) return false;
            
            // There's a new version, write it to the latest known version file so that the user won't get notified twice
            // about the same version.
            File.WriteAllText(latestKnownVersionFilePath, latestVersionString);

            return true;
        }
    }
}