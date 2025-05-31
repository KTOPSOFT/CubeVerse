using System;
using System.Linq;
using System.Text.RegularExpressions;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Custom inspector for the map graph assets.
    /// </summary>
    [CustomEditor(typeof(MapGraphGraph), true)]
    public class MapGraphEditor : UnityEditor.Editor
    {
        private MapGraphGraph graph;

        private bool changed;

        private InputParametersList inputParametersList;
        private OutputParametersList outputParametersList;
        private ReorderableList dataDependenciesList;

        private Type[] previewBehaviourTypes;
        private string[] previewBehaviourNames;
        private int selectedPreviewBehaviourIndex = 0;

        private void OnEnable()
        {
            graph = (MapGraphGraph) target;

            inputParametersList = new InputParametersList(graph.InputParameters, graph);
            outputParametersList = new OutputParametersList(graph.OutputParameters, graph);
            
            var dataDependenciesProp = serializedObject.FindProperty("dataDependencies");
            
            var listFactory = new DataDependencyListFactory(dataDependenciesProp);
            dataDependenciesList = listFactory.Create();

            previewBehaviourTypes = Types.ConcreteChildrenOf<IPreviewBehaviour>().ToArray();
            previewBehaviourNames = new string[previewBehaviourTypes.Length + 1];
            previewBehaviourNames[0] = "Default";
            for (var i = 0; i < previewBehaviourTypes.Length; ++i)
            {
                var previewBehaviourNiceName = previewBehaviourTypes[i].GetFriendlyName();
                previewBehaviourNiceName = previewBehaviourNiceName.Substring(0, previewBehaviourNiceName.LastIndexOf("PreviewBehaviour", StringComparison.Ordinal));
                previewBehaviourNiceName = Regex.Replace(previewBehaviourNiceName, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
                
                previewBehaviourNames[i + 1] = previewBehaviourNiceName;

                if (graph.CustomPreviewBehaviour != null && previewBehaviourTypes[i] == graph.CustomPreviewBehaviour.GetType())
                {
                    selectedPreviewBehaviourIndex = i + 1;
                }
            }
        }

        private void OnDisable()
        {
            inputParametersList.Dispose();
            outputParametersList.Dispose();
        }

        public override void OnInspectorGUI()
        {
            changed = false;

            EditorGUI.BeginChangeCheck();

            graph.NamedColorSet = (NamedColorSet) EditorGUILayout.ObjectField("Named Color Set", graph.NamedColorSet,
                typeof(NamedColorSet), false);
            
            GUILayout.Space(20);

            graph.CanBeAddedAsNode = EditorGUILayout.Toggle(
                new GUIContent(
                    "Show in node menu",
                    "Enabling this will add this graph to the node creation menu, so it can be used as a sub graph."),
                graph.CanBeAddedAsNode);

            if (graph.CanBeAddedAsNode)
            {
                graph.NodePath = EditorGUILayout.TextField(new GUIContent("Menu Path",
                    "The path at which it is placed in the node creation menu"), graph.NodePath).Trim(' ', '/');
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }

            var reloadAll = false;
            if (graph.CanBeAddedAsNode)
            {
                EditorGUI.BeginChangeCheck();
                
                selectedPreviewBehaviourIndex = EditorGUILayout.Popup("Preview Behaviour", selectedPreviewBehaviourIndex, previewBehaviourNames);

                if (EditorGUI.EndChangeCheck())
                {
                    if (selectedPreviewBehaviourIndex == 0)
                    {
                        graph.CustomPreviewBehaviour = null;
                    }
                    else
                    {
                        var newPreviewBehaviourType = previewBehaviourTypes[selectedPreviewBehaviourIndex-1];
                        var newPreviewBehaviour = (IPreviewBehaviour) Activator.CreateInstance(newPreviewBehaviourType);
                        graph.CustomPreviewBehaviour = newPreviewBehaviour;
                    }

                    reloadAll = true;
                    changed = true;
                }
            }

            GUILayout.Space(20);

            inputParametersList.DoLayoutList();
            
            GUILayout.Space(20);
            
            outputParametersList.DoLayoutList();

            GUILayout.Space(20);
            
            dataDependenciesList.DoLayoutList();
            
            GUILayout.Space(20);
            
            var bigButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 40, 
                fontSize = 14
            };

            if (GUILayout.Button("Open in graph editor", bigButtonStyle))
            {
                ScriptGraphViewWindow.CreateGraphViewWindow(graph);
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button(new GUIContent(
                "Add to scene", 
                "Adds a new game object with a Graph Runner component to the current scene and assigns this graph to it.")))
            {
                var gameObject = new GameObject($"{graph.name} Runner");
                var runner = gameObject.AddComponent<ScriptGraphRunner>();
                var processor = runner.GraphProcessor;
                processor.Graph = graph;
            }
            
            if (changed)
            {
                // If anything changed to the graph, and it's opened in the editor, reload the editor.
                if (reloadAll)
                {
                    ScriptGraphViewWindow.ReloadAll();
                }
                else if (ScriptGraphViewWindow.Contains(graph))
                {
                    ScriptGraphViewWindow.Get(graph).Reload();
                }
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                graph.UpdateDataDependencies();
            }
        }

        [OnOpenAsset(0)]
        public static bool OnOpenMapGraph(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var type = EditorUtility.InstanceIDToObject(instanceId).GetType();

            if (type != typeof(MapGraphGraph)) return false;
            
            // If a Map Graph is double-clicked in the project browser, open it in the map graph editor window.
            
            var graph = (MapGraphGraph) obj;
            ScriptGraphViewWindow.CreateGraphViewWindow(graph);
            
            return true;
        }
    }
}