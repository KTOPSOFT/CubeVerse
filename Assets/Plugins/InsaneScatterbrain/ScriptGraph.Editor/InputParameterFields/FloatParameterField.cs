using System;
using UnityEditor;
using UnityEngine;
using Types = InsaneScatterbrain.Services.Types;

namespace InsaneScatterbrain.ScriptGraph
{
    public class FloatParameterField : IParameterField
    {
        public Type Type => typeof(float);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var floatValue = Types.ConvertTo<float>(val);
            return rect != null
                ? EditorGUI.FloatField(rect.Value, floatValue) 
                : EditorGUILayout.FloatField(valName, floatValue);
        }
    }
}