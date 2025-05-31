using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class Vector2ParameterField : IParameterField
    {
        public Type Type => typeof(Vector2);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var vector2Value = (Vector2)val;
            return rect != null 
                ? EditorGUI.Vector2Field(rect.Value, string.Empty, vector2Value) 
                : EditorGUILayout.Vector2Field(valName, vector2Value);
        }
    }
}