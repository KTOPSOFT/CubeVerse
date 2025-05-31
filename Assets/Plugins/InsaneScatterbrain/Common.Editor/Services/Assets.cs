using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.Editor.Services
{
    /// <summary>
    /// Collection of static methods for working with asset files.
    /// </summary>
    public static class Assets
    {
        /// <summary>
        /// Finds all assets of the given type.
        /// </summary>
        /// <typeparam name="T">The asset type to look for.</typeparam>
        /// <returns>All assets of the given type.</returns>
        public static IList<T> Find<T>() where T : Object
        {
            var assets = new List<T>();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath); 
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
    }
}