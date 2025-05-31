using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Custom inspector for the script graph runner.
    /// </summary>
    [CustomEditor(typeof(ScriptGraphRunner), true)]
    public class ScriptGraphRunnerEditor : UnityEditor.Editor
    {
        private bool showInputParameters = true;
        private bool showOutputParameters = true;
        
        private Dictionary<Type, IParameterField> fields;
        
        private SerializedProperty graphProcessorProp;
        private SerializedProperty runOnAwakeProp;
        private SerializedProperty runOnStartProp;
        private SerializedProperty runAsynchronously;
        private SerializedProperty enableMainThreadTimePerFrameLimit;
        private SerializedProperty targetTimePerFrame;
        private SerializedProperty enableMultiThreading;
        private SerializedProperty skipChecks;
        private SerializedProperty enableObjectPooling;
        private SerializedProperty graphProp;
        private SerializedProperty isSeedRandomProp;
        private SerializedProperty seedTypeProp;
        private SerializedProperty seedProp;
        private SerializedProperty seedGuidProp;
        private SerializedProperty processedProp;
        private SerializedProperty componentDependenciesProp;
        
        private readonly Dictionary<string, bool> foldoutOpen = new Dictionary<string, bool>();
        private readonly Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();
        private readonly ArrayList tmpList = new ArrayList();

        private ReorderableList dependenciesList;
        
        private void OnEnable()
        {
            graphProcessorProp = serializedObject.FindProperty("graphProcessor");
            runOnAwakeProp = serializedObject.FindProperty("runOnAwake");
            runOnStartProp = serializedObject.FindProperty("runOnStart");
            runAsynchronously = serializedObject.FindProperty("runAsynchronously");
            enableMainThreadTimePerFrameLimit = serializedObject.FindProperty("enableMainThreadTimePerFrameLimit");
            targetTimePerFrame = serializedObject.FindProperty("targetTimePerFrame");
            enableMultiThreading = serializedObject.FindProperty("enableMultiThreading");
            skipChecks = serializedObject.FindProperty("skipChecks");
            enableObjectPooling = serializedObject.FindProperty("enableObjectPooling");
            processedProp = serializedObject.FindProperty("processed");
            
            graphProp = graphProcessorProp.FindPropertyRelative("graph");
            isSeedRandomProp = graphProcessorProp.FindPropertyRelative("isSeedRandom");
            seedTypeProp = graphProcessorProp.FindPropertyRelative("seedType");
            seedProp = graphProcessorProp.FindPropertyRelative("seed");
            seedGuidProp = graphProcessorProp.FindPropertyRelative("seedGuid");
            componentDependenciesProp = graphProcessorProp.FindPropertyRelative("componentDependencies");

            var parameterFieldTypes = Types.ConcreteChildrenOf<IParameterField>();
            
            // Find all parameter field types, instantiate them and add them to a list, so they
            // can be used to draw the correct editor fields later on.
            fields = new Dictionary<Type, IParameterField>();
            foreach (var parameterFieldType in parameterFieldTypes)
            {
                var inputParameterField = (IParameterField) Activator.CreateInstance(parameterFieldType);
                fields.Add(inputParameterField.Type, inputParameterField);
            }

            ScriptGraphRunner.OnRun += graph => Repaint(); 
            ScriptGraphRunner.OnStop += graph => Repaint();
            
            var listFactory = new ComponentDependencyListFactory(componentDependenciesProp);
            dependenciesList = listFactory.Create();
        }

        public override void OnInspectorGUI()
        {
            var scriptGraphRunner = target as ScriptGraphRunner;
            
            var graphProcessor = scriptGraphRunner.GraphProcessor;
            
            if (scriptGraphRunner.GraphProcessor.IsProcessing)
            {
                // If the graph is already being processed, disable all the controls (except the "Open Graph" button),
                // so no changes can be made halfway through and to avoid processing the same graph multiple times
                // at the same time. As these things can lead to unexpected results and errors.
                GUI.enabled = false;
            }
            
            GUILayout.Space(20);

            EditorGUILayout.PropertyField(graphProp, new GUIContent("Graph"));

            if (graphProcessor.Graph != null)
            {
                GUILayout.Space(20);
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(runOnAwakeProp, new GUIContent("Run On Awake", "When enabled, the runner will start processing the graph when OnAwake is triggered."));
                if (EditorGUI.EndChangeCheck())
                {
                    if (runOnAwakeProp.boolValue)
                    {
                        runOnStartProp.boolValue = false;
                    }
                }
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(runOnStartProp, new GUIContent("Run On Start", "When enabled, the runner will start processing the graph when OnStart is triggered."));
                if (EditorGUI.EndChangeCheck())
                {
                    if (runOnStartProp.boolValue)
                    {
                        runOnAwakeProp.boolValue = false;
                    }
                }
                
                GUILayout.Space(20);
                
                EditorGUILayout.PropertyField(skipChecks, new GUIContent("Skip Checks", "When enabled, the graph runner won't check if all parameters are assigned and all the necessary ports are connected, before processing the graph."));
                
                GUILayout.Space(20);
                
                EditorGUILayout.PropertyField(runAsynchronously, new GUIContent("Run Asynchronously", "When enabled, the graph will run on a separate thread (where possible).\n\nThis avoids the game or editor freezing while it waits for the graph runner to complete.\n\nThe downside is that it might take longer to complete processing the graph.\n\nThis can be useful if you need to run a graph during critical gameplay."));

                if (!scriptGraphRunner.RunAsynchronously) GUI.enabled = false;
                EditorGUILayout.PropertyField(enableMainThreadTimePerFrameLimit, new GUIContent("Enable Main Thread Time Per Frame Limit", "When enabled Map Graph will attempt to spread operations that must be run on the main thread out over multiple frames.\n\nEach chunk will run for approximately for the amount of time set as Target Time Per Frame each frame to avoid locking up the game or editor."));

                if (!scriptGraphRunner.MainThreadTimePerFrameLimitEnabled || !scriptGraphRunner.RunAsynchronously) GUI.enabled = false;
                EditorGUILayout.PropertyField(targetTimePerFrame, new GUIContent("Target Time Per Frame", "Time (in milliseconds) the limit main thread code execution to per frame.\n\nIncreasing this number will make the graph complete faster, but will also decrease the framerate.\nDecreasing this number will make the graph take longer to complete, but will lessen the impact on the framerate."));
                GUI.enabled = !scriptGraphRunner.GraphProcessor.IsProcessing && scriptGraphRunner.RunAsynchronously;
                
                EditorGUILayout.PropertyField(enableMultiThreading, new GUIContent("Enable Multi-threading", "When enabled, the graph will run nodes in parallel on separate threads (where possible).\n\nMulti-threading will mostly benefit \"tall\" graphs, meaning graphs that largely consist of nodes that depend on the output of multiple other nodes."));
                GUI.enabled = !scriptGraphRunner.GraphProcessor.IsProcessing;

                GUILayout.Space(20);
                
                EditorGUILayout.PropertyField(enableObjectPooling, new GUIContent("Enable Object Pooling", "When enabled, Map Graph will pool and reuse any object instances it creates so that they don't get picked up by the garbage collector (where possible).\n\nThis avoids triggering the garbage collector, which can cause stuttering.\n\nThe downside of object pooling is that it might result in Map Graph hogging a lot of memory that it's not actively using.\n\nThis can be useful if you need to run a graph during critical gameplay."));
                
                var graph = graphProcessor.Graph;
            
                GUILayout.Space(20);

                EditorGUILayout.PropertyField(isSeedRandomProp, new GUIContent("Use Random Seed", "When enabled, a different RNG seed is used each time the graph is processed.\n\nDisabling this allows you to set your own seed, resulting in the same output each time.\n\nThis can be useful for testing purposes or for creating daily challenges, for example, where the same level should be generated for everyone."));
                if (!graphProcessor.IsSeedRandom)
                {
                    EditorGUILayout.PropertyField(seedTypeProp, new GUIContent("Seed Type"));

                    if (graphProcessor.SeedType == SeedType.Int)
                    {
                        EditorGUILayout.PropertyField(seedProp, new GUIContent("Seed"));
                    }
                    else if (graphProcessor.SeedType == SeedType.Guid)
                    {
                        if (string.IsNullOrEmpty(graphProcessor.SeedGuid))
                        {
                            graphProcessor.SeedGuid = Guid.NewGuid().ToString();
                        }
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(seedGuidProp, new GUIContent("Seed"));
                        if (GUILayout.Button("New Seed", GUILayout.Width(80)))
                        {
                            graphProcessor.SeedGuid = Guid.NewGuid().ToString();
                        }
                        GUILayout.EndHorizontal();
                    }
                }

                if (graph.InputParameters.OrderedIds.Count > 0)
                {
                    GUILayout.Space(20);
                
                    showInputParameters = EditorGUILayout.Foldout(showInputParameters, "Input Parameters");

                    if (showInputParameters)
                    {
                        // Generate input fields for the input parameters.
                        foreach (var paramId in graph.InputParameters.OrderedIds)
                        {
                            var inputName = graph.InputParameters.GetName(paramId);
                            var inputType = graph.InputParameters.GetType(paramId);

                            Func<string, Type, object, Rect?, object> field = null;

                            var elementType = inputType.GetElementType();
                            if (elementType != null)
                            {
                                inputType = elementType;
                            }
                            
                            if (fields.ContainsKey(inputType))
                            {
                                field = fields[inputType].Field;
                            }
                            else
                            {
                                // If there's no direct match between the input parameter type and any of the types
                                // that have fields registered to them, loop through all the parent's types and 
                                // use the field for the closest parent's type.
                                var parentType = inputType.BaseType;
                                while (parentType != null)
                                {
                                    if (fields.ContainsKey(parentType))
                                    {
                                        field = fields[parentType].Field;
                                        break;
                                    }
                                
                                    parentType = parentType.BaseType;
                                }

                                // If no field is available, just show a label to show that the input parameter exists,
                                // but is not assignable from the inspector.
                                if (field == null)
                                {
                                    EditorGUILayout.LabelField(inputName, inputType.Name);
                                    continue;
                                }
                            }
                        
                            // Get the field's current value from the graph.
                            var inputParamValue = scriptGraphRunner.GetIn(inputName);

                            if (inputParamValue == null)
                            {
                                if (elementType != null)
                                {
                                    // If it's an array, make sure there's a default empty array assigned.
                                    inputParamValue = Array.CreateInstance(elementType, 0);
                                }
                                else if (inputType.IsValueType)
                                {
                                    // If it's a value type, make sure that it has a default value if it isn't set yet.
                                    inputParamValue = Activator.CreateInstance(inputType);
                                }
                            }
                            
                            
                            // If this value is an array, create a list and fields for each element in it.
                            if (elementType != null)
                            {
                                EditorGUI.indentLevel++;
                                
                                if (!foldoutOpen.ContainsKey(paramId)) foldoutOpen.Add(paramId, false);

                                EditorGUILayout.BeginHorizontal();
                                
                                foldoutOpen[paramId] = EditorGUILayout.Foldout(foldoutOpen[paramId], inputName, true);
                                GUILayout.FlexibleSpace();
                                
                                var array = (Array) inputParamValue;
                                var arrayLength = array.Length;

                                EditorGUI.BeginChangeCheck();
                                arrayLength = EditorGUILayout.DelayedIntField(arrayLength, GUILayout.Width(60));
                                if (EditorGUI.EndChangeCheck())
                                {
                                    array = ResizeArray(array, inputType, arrayLength);
                                    scriptGraphRunner.SetIn(inputName, array);
                                    EditorUtility.SetDirty(scriptGraphRunner);
                                    lists.Remove(paramId);
                                    return;
                                }
                                
                                EditorGUILayout.EndHorizontal();
                                
                                if (foldoutOpen[paramId])
                                {
                                    var list = CreateOrGetList(paramId, array, field, inputName, inputType, scriptGraphRunner);
                                    if (list.index >= list.count)
                                    {
                                        list.index = -1; 
                                    }
                                    list.DoLayoutList();
                                }
                                
                                EditorGUI.indentLevel--;
                            }
                            else
                            {
                                // Assign the fields new value to the graph.
                                EditorGUI.BeginChangeCheck();
                                inputParamValue = field(inputName, inputType, inputParamValue, null);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    scriptGraphRunner.SetInById(paramId, inputParamValue);
                                    EditorUtility.SetDirty(scriptGraphRunner);
                                }
                            }
                        }
                    }
                }

                if (graph.OutputParameters.OrderedIds.Count > 0)
                {
                    GUILayout.Space(20);
                    
                    showOutputParameters = EditorGUILayout.Foldout(showOutputParameters, "Output Parameters (Last Result)");

                    if (showOutputParameters)
                    {
                        foreach (var paramId in graph.OutputParameters.OrderedIds)
                        {
                            var outputName = graph.OutputParameters.GetName(paramId);
                            var outputType = graph.OutputParameters.GetType(paramId);
                            
                            var resultText = OutputParameterResultFormatter.Format(scriptGraphRunner.LatestResult, outputName);

                            // Show a list of output parameters and their types
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField($"{outputName} ({outputType.Name})");
                            EditorGUILayout.SelectableLabel(resultText, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                            
                            // Show a button to open a popup window that shows the value in a larger field (in case it's a large
                            // value that's not readable in the small field).
                            if (GUILayout.Button("...", GUILayout.Width(30)))
                            {
                                OutputParameterResultWindow.ShowResult(scriptGraphRunner, outputName);
                            }
                            
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }

                GUILayout.Space(20);

                dependenciesList.DoLayoutList();
                
                EditorGUILayout.PropertyField(processedProp);
                
                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                
                GUI.enabled = true;
                if (GUILayout.Button("Open Graph", GUILayout.Height(40)))
                {
                    ScriptGraphViewWindow.CreateGraphViewWindow(graph);
                }
                
                if (scriptGraphRunner.GraphProcessor.IsProcessing)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Run Graph", GUILayout.Height(40)))
                {
                    // Make sure the inspector is repainted after the graph is processed, in case the GUI
                    // hasn't updated in the meantime, which can happen if the mouse hasn't been moved after
                    // pressing the Run button for example.
                    
                    // First remove unsubscribe the methods from the events in case they have been added before.
                    scriptGraphRunner.OnProcessed -= RepaintAfterProcessing;
                    scriptGraphRunner.OnProcessed += RepaintAfterProcessing;
                    scriptGraphRunner.OnError -= Repaint;
                    scriptGraphRunner.OnError += Repaint;
                    scriptGraphRunner.Run();
                }
                GUILayout.EndHorizontal();

                if (graphProcessor.LatestExecutionTime > -1)
                {
                    GUILayout.Space(20);
                    GUILayout.Label($"Latest execution time: {graphProcessor.LatestExecutionTime / 1000f}s");
                } 
                
                GUILayout.Space(20);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Array ResizeArray(Array array, Type elementType, int newSize)
        {
            var newArray = Array.CreateInstance(elementType, newSize);

            if (array != null) 
                Array.Copy(array, newArray, Math.Min(array.Length, newArray.Length));
            
            return newArray;
        }

        private ReorderableList CreateOrGetList(string paramId, Array array, Func<string, Type, object, Rect?, object> field, string inputName, Type inputType, ScriptGraphRunner runner)
        {
            if (lists.ContainsKey(paramId)) return lists[paramId];
            
            var list = new ReorderableList(array, inputType, true, false, true, true)
            {
                headerHeight = 4,
                drawElementCallback = (rect, index, active, focused) =>
                {
                    EditorGUI.BeginChangeCheck();
                    var newValue = field(inputName, inputType, array.GetValue(index), rect);
                    
                    if (!EditorGUI.EndChangeCheck()) return;
                    
                    array.SetValue(newValue, index);
                    runner.SetInById(paramId, array); 
                    EditorUtility.SetDirty(runner);
                },
                onAddCallback = reorderableList =>
                {
                    if (array == null)
                    {
                        array = Array.CreateInstance(inputType, 1);
                    }
                    
                    array = ResizeArray(array, inputType, array.Length + 1);
                    reorderableList.list = array;

                    runner.SetInById(paramId, array);
                    
                    EditorUtility.SetDirty(runner); 
                },
                onRemoveCallback = reorderableList =>
                {
                    tmpList.AddRange(array);
                    tmpList.RemoveAt(reorderableList.index);
                    array = tmpList.ToArray(inputType);
                    tmpList.Clear();
                    
                    reorderableList.list = array;
                    runner.SetInById(paramId, array);
                    
                    EditorUtility.SetDirty(runner);  
                }
            };
            
            list.elementHeight *= fields[inputType].NumRows;
            
            lists.Add(paramId, list);
            return list;
        }

        private void RepaintAfterProcessing(IReadOnlyDictionary<string, object> result)
        {
            Repaint();
        } 
    }
}