using System;
using UnityEditor;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph
{
    public class StringParameterField : IParameterField
    {
        public Type Type => typeof(string);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var stringValue = Types.ConvertTo<string>(val);
            return rect != null 
                ? EditorGUI.TextField(rect.Value, stringValue) 
                : EditorGUILayout.TextField(valName, stringValue);
        }
    }
}