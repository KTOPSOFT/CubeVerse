using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    /// <summary>
    /// Custom inspector for the script graph input components.
    /// </summary>
    [CustomEditor(typeof(ScriptGraphInput), true)]
    public class ScriptGraphInputEditor : UnityEditor.Editor
    {
        private readonly List<string> parameterNameOptions = new List<string>();

        public override void OnInspectorGUI()
        {
            var input = (ScriptGraphInput) target;

            var runnerProp = serializedObject.FindProperty("runner");
            var parameterProp = serializedObject.FindProperty("parameterId");
            var defaultValueProp = serializedObject.FindProperty("defaultValue");

            var parameterId = parameterProp.stringValue;

            // Add an object field for the runner this input applies to.
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(runnerProp, new GUIContent("Runner"));
            if (EditorGUI.EndChangeCheck())
            {
                parameterProp.stringValue = string.Empty;
            }
            
            serializedObject.ApplyModifiedProperties();
            
            var runner = runnerProp.objectReferenceValue as ScriptGraphRunner;

            if (runner == null) return;
            
            EditorGUILayout.Space(20);

            if (runner.GraphProcessor == null || runner.Graph == null)
            {
                EditorGUILayout.HelpBox("No graph set on the runner.", MessageType.Warning);
                return;
            }

            // Get all the input parameters that match the type of this input and show them in a popup.
            parameterNameOptions.Clear();
            
            var graph = runner.Graph;
            var inputParameters = graph.InputParameters;
            
            foreach (var inputParamId in inputParameters.OrderedIds)
            {
                var inputParamName = inputParameters.GetName(inputParamId);
                var inputParamType = inputParameters.GetType(inputParamId);
                
                if (inputParamType != input.Type) continue;
                
                parameterNameOptions.Add(inputParamName);
            }

            if (parameterNameOptions.Count == 0)
            {
                EditorGUILayout.HelpBox("No parameters of this type.", MessageType.Warning);
                return;
            }

            var parameterIndex = -1;
            if (inputParameters.ContainsId(parameterId))
            {
                var parameterName = inputParameters.GetName(parameterId);
                parameterIndex = parameterNameOptions.IndexOf(parameterName);
            }
            
            var newParameterIndex = EditorGUILayout.Popup("Parameter", parameterIndex, parameterNameOptions.ToArray());

            if (newParameterIndex != parameterIndex)
            {
                var newParameterName = parameterNameOptions[newParameterIndex];
                var newParameterId = inputParameters.GetId(newParameterName);
                parameterProp.stringValue = newParameterId;
            }
            
            EditorGUILayout.PropertyField(defaultValueProp, new GUIContent("Default Value"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}