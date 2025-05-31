using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [InitializeOnLoad]
    public static class MapGraphEditorSettingsWindowInitializer
    {
        static MapGraphEditorSettingsWindowInitializer()
        {
            ScriptGraphView.MinZoom = EditorPrefs.GetFloat(MapGraphEditorSettingsWindow.MinZoomKey, .1f);
            ScriptGraphView.MaxZoom = EditorPrefs.GetFloat(MapGraphEditorSettingsWindow.MaxZoomKey, ContentZoomer.DefaultMaxScale);
            ScriptGraphView.ZoomStep = EditorPrefs.GetFloat(MapGraphEditorSettingsWindow.ZoomStepKey, ContentZoomer.DefaultScaleStep);
        }
    }
    
    public class MapGraphEditorSettingsWindow : EditorWindow
    {
        public const string MinZoomKey = "MapGraph_Editor_MinZoom";
        public const string MaxZoomKey = "MapGraph_Editor_MaxZoom"; 
        public const string ZoomStepKey = "MapGraph_Editor_ZoomStep"; 
        
        public static void ShowWindow()
        {
            GetWindow(typeof(MapGraphEditorSettingsWindow));
        }
        
        private void OnGUI()
        {
            titleContent = new GUIContent("Map Graph Editor Settings");
            
            var minZoomLabel = new GUIContent("Min. zoom", "The minimum zoom level for the graph editor.");
            var maxZoomLabel = new GUIContent("Max. zoom", "The maximum zoom level for the graph editor.");
            var zoomStepLabel = new GUIContent("Zoom step", "The amount the zoom level changes when zooming in or out.");
            
            EditorGUI.BeginChangeCheck();
            ScriptGraphView.MinZoom = EditorGUILayout.DelayedFloatField(minZoomLabel, ScriptGraphView.MinZoom);
            ScriptGraphView.MaxZoom = EditorGUILayout.DelayedFloatField(maxZoomLabel, ScriptGraphView.MaxZoom);
            ScriptGraphView.ZoomStep = EditorGUILayout.DelayedFloatField(zoomStepLabel, ScriptGraphView.ZoomStep);
            
            // Button to restore the setting defaults.
            if (GUILayout.Button("Restore Defaults"))
            {
                ScriptGraphView.MinZoom = .1f;
                ScriptGraphView.MaxZoom = ContentZoomer.DefaultMaxScale;
                ScriptGraphView.ZoomStep = ContentZoomer.DefaultScaleStep;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat(MinZoomKey, ScriptGraphView.MinZoom);
                EditorPrefs.SetFloat(MaxZoomKey, ScriptGraphView.MaxZoom);
                EditorPrefs.SetFloat(ZoomStepKey, ScriptGraphView.ZoomStep);
                
                ScriptGraphViewWindow.ReloadAll();
            }
        }
    }
}