using InsaneScatterbrain.Editor.Services;
using UnityEditor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    internal class MapGraphAssetPostProcessor : AssetPostprocessor
    {
#if UNITY_2021_2_OR_NEWER
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
        {
            var mapGraphUpdated = false;
            foreach (var asset in importedAssets)
            {
                if (asset != $"Assets{MapGraphEditorInfo.VersionFilePathRelative}") continue;

                // If the version.txt file has been imported, that means either Map Graph was just installed or updated,
                // so we want to show the about window.
                mapGraphUpdated = true;
            }

            if (deletedAssets.Length > 0)
            {
                // Linked sets might have been deleted, make sure they're unlinked.
                var namedColorSets = Assets.Find<NamedColorSet>();
                foreach (var namedColorSet in namedColorSets)
                {
                    namedColorSet.Update();
                }
            }

            if (mapGraphUpdated)
            {
                AboutWindow.ShowWindow();
            }

#if UNITY_2021_2_OR_NEWER
            // In older versions of Unity OnPostprocessAllAssets is not called when a script is recompiled, so
            // it's done through the MapGraphInitializer.
            if (!didDomainReload)
                return;

            var updater = new MapGraphAssetUpdater();
            updater.Start();
#endif
        }
    }
}