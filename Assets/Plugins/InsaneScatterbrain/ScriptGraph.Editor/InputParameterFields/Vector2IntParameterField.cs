using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class Vector2IntParameterField : IParameterField
    {
        public Type Type => typeof(Vector2Int);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var vector2IntValue = (Vector2Int)val;
            return rect != null 
                ? EditorGUI.Vector2IntField(rect.Value, string.Empty, vector2IntValue) 
                : EditorGUILayout.Vector2IntField(valName, vector2IntValue);
        }
    }
}