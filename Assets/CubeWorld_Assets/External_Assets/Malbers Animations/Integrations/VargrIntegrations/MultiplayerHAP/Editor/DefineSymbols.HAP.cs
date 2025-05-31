#if UNITY_EDITOR

using UnityEditor;

namespace MalbersAnimations.VargrMultiplayer
{
    internal static partial class ScriptingDefines
    {
#if !AC_HAP
        [InitializeOnLoadMethod]
#endif
        public static void HAPDefineSymbols()
        {
            //string versionPrefix = "";
            //string[] currentVersionSplit = ;
            //string thisVersion = $"{versionPrefix}{currentVersionSplit[0]}";

            string[] scriptDefines = new string[]
            {
                //thisVersion
                "AC_HAP"
            };
            
            string[] scriptDefinesOld = new string[]
            {

            };
            
            AddDefineSymbols(scriptDefines, scriptDefinesOld);
        }
    }
}
#endif