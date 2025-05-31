using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class Vector3ParameterField : IParameterField
    {
        public Type Type => typeof(Vector3);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var vector3Value = (Vector3)val;
            return rect != null 
                ? EditorGUI.Vector3Field(rect.Value, string.Empty, vector3Value) 
                : EditorGUILayout.Vector3Field(valName, vector3Value);
        }
    }
}