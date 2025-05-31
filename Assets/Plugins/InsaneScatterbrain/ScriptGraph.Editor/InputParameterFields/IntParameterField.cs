using System;
using UnityEditor;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph
{
    public class IntParameterField : IParameterField
    {
        public Type Type => typeof(int);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var intValue = Types.ConvertTo<int>(val);
            return rect != null 
                ? EditorGUI.IntField(rect.Value, valName, intValue) 
                : EditorGUILayout.IntField(valName, intValue);
        }
    }
}