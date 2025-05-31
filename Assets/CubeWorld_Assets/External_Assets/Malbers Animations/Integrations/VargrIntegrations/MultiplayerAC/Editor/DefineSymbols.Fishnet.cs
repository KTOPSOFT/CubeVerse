#if UNITY_EDITOR

using UnityEditor;

namespace MalbersAnimations.VargrMultiplayer
{
    internal static partial class ScriptingDefines
    {
#if FISHNET
#if !AC_FISHNET
        [InitializeOnLoadMethod]
#endif
        public static void ACDefineSymbols()
        {
            //string versionPrefix = "";
            //string[] currentVersionSplit = ;
            //string thisVersion = $"{versionPrefix}{currentVersionSplit[0]}";

            string[] scriptDefines = new string[]
            {
                //thisVersion
                "AC_FISHNET"
            };
            
            string[] scriptDefinesOld = new string[]
            {

            };
            
            AddDefineSymbols(scriptDefines, scriptDefinesOld);
        }
#endif
    }
}
#endif