using System;
using InsaneScatterbrain.Versioning;
using UnityEditor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public static class MapGraphAsset
    {
        /// <summary>
        /// Creates a new versioned scriptable object and initializes it with the current Map Graph version.
        /// </summary>
        /// <param name="filename">The asset's filename.</param>
        /// <typeparam name="T">The scriptable object type.</typeparam>
        public static T Create<T>(string filename) where T : VersionedScriptableObject
        {
            var instance = VersionedScriptableObject.CreateInstance<T>(new Version(MapGraphEditorInfo.Version.ToString()));
            ProjectWindowUtil.CreateAsset(instance, $"{filename}.asset");
            return instance;
        }
    }
}