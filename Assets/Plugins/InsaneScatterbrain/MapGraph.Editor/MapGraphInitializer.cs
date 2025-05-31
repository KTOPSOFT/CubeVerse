using System.IO;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Class to initialize all the editor stuff for Map Graph.
    /// </summary>
    [InitializeOnLoad]
    public static class MapGraphInitializer
    {
        static MapGraphInitializer()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            DeleteOldFiles();
            
#if !UNITY_2021_2_OR_NEWER
            // In never versions of Unity this is solved through an asset post processor
            var updater = new MapGraphAssetUpdater();
            updater.Start();
#endif
        }

        private static void DeleteOldFiles()
        {
            var thirdPartyLibrariesPath = $"{Application.dataPath}/Plugins/InsaneScatterbrain/ThirdPartyLibraries";

            var filesToDelete = new[]
            {
                $"{thirdPartyLibrariesPath}/DeBroglie.dll",
                $"{thirdPartyLibrariesPath}/DeBroglie.dll.meta",
                $"{thirdPartyLibrariesPath}/DelaunatorSharp.dll",
                $"{thirdPartyLibrariesPath}/DelaunatorSharp.dll.meta",
                $"{thirdPartyLibrariesPath}/QuikGraph.dll",
                $"{thirdPartyLibrariesPath}/QuikGraph.dll.meta",
                $"{thirdPartyLibrariesPath}/MIConvexHull.dll",
                $"{thirdPartyLibrariesPath}/MIConvexHull.dll.meta"
            };

            foreach (var file in filesToDelete)
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }
    }
}