#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    internal static partial class ScriptingDefines
    {
        public static void AddDefineSymbols(string[] scriptDefines, string[] scriptDefinesOld)
        {
            // Get data about current target group
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            /* Convert current defines into a hashset. This is so we can
             * determine if any of our defines were added. Only save playersettings
             * when a define is added. */
            HashSet<string> definesHs = new();
            string[] currentArr = currentDefines.Split(';');
            //Add current defines into hs.
            foreach (string item in currentArr)
                definesHs.Add(item);
/*
            int versionPrefixLength = versionPrefix.Length;
            //Remove old versions.
            foreach (string item in definesHs)
            {
                //Do not remove this version.
                if (item == thisVersion)
                    continue;

                //If length is possible to be a version prefix and is so then remove it.
                if (item.Length >= versionPrefixLength && item.Substring(0, versionPrefixLength) == versionPrefix)
                    definesToRemove.Add(item);
            }
*/
            bool modified = false;
            //Add defines.
            foreach (string item in scriptDefines)
                modified |= definesHs.Add(item);

            //Remove old defines.
            foreach (string item in scriptDefinesOld)
                modified |= definesHs.Remove(item);

            if (modified)
            {
                Debug.Log("Added or removed Define Symbols within player settings.");
                string changedDefines = string.Join(";", definesHs);
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, changedDefines);
            }
        }
        
    }
}
#endif