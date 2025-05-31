using System;
using UnityEditor;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph
{
    public class BoolParameterField : IParameterField
    {
        public Type Type => typeof(bool);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var boolValue = Types.ConvertTo<bool>(val);
            return rect != null 
                ? EditorGUI.Toggle(rect.Value, boolValue) 
                : EditorGUILayout.Toggle(valName, boolValue);
        }
    }
}