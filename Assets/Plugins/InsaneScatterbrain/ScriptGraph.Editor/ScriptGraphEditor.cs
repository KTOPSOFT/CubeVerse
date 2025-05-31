using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Custom inspector for ScripGraphEditor.
    /// </summary>
    [CustomEditor(typeof(ScriptGraphGraph), true)]
    public class ScriptGraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var graph = (ScriptGraphGraph) target;
            
            if (!GUILayout.Button("Open Editor")) return;
            
            ScriptGraphViewWindow.CreateGraphViewWindow(graph);
        }
    }
}