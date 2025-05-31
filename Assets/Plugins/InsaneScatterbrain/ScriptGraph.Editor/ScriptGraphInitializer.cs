using InsaneScatterbrain.MapGraph.Editor;
using UnityEditor;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// The ScriptGraphInitialize is run on load to run maintenance jobs on the existing script graphs, in case
    /// things have changed. For example, it removes nodes and ports that no longer exist.
    /// </summary>
    [InitializeOnLoad]
    public static class ScriptGraphInitializer
    {
        static ScriptGraphInitializer()
        {
#if !MAP_GRAPH_EDITORCOROUTINES
            var dependencyInstaller = new ScriptGraphDependencyInstaller();
            dependencyInstaller.StartInstallProcess();
#endif
            
#if !UNITY_2021_2_OR_NEWER
            // In never versions of Unity this is solved through an asset post processor
            var updater = new ScriptGraphAssetUpdater();
            updater.Start();
#endif
            
            ScriptGraphDebugger.Initialize();
        }
    }
}