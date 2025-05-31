using System;
using System.IO;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Contains information about the Script Graph Editor package.
    /// </summary>
    public static class ScriptGraphEditorInfo
    {
        public const string VersionFilePathRelative = "/Plugins/InsaneScatterbrain/MapGraph.Editor.Data/version.txt";
        public static readonly string VersionFilePathAbsolute = $"{Application.dataPath}{VersionFilePathRelative}";
        
        private static Version version;

        public static Version Version
        {
            get
            {
                if (version == null)
                {
#if UNITY_EDITOR
                    version = new Version(File.ReadAllText(VersionFilePathAbsolute).Trim());
#else
                    version = new Version();
#endif
                }

                return version;
            }
        }
    }
}
