using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class Vector3IntParameterField : IParameterField
    {
        public Type Type => typeof(Vector3Int);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var vector3IntValue = (Vector3Int)val;
            return rect != null 
                ? EditorGUI.Vector3IntField(rect.Value, string.Empty, vector3IntValue) 
                : EditorGUILayout.Vector3IntField(valName, vector3IntValue);
        }
    }
}