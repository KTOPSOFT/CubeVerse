using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class Color32ParameterField : IParameterField
    {
        public Type Type => typeof(Color32);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var colorValue = (Color32)val;
            colorValue.a = 255;
            return rect != null 
                ? (Color32) EditorGUI.ColorField(rect.Value, colorValue) 
                : (Color32) EditorGUILayout.ColorField(valName, colorValue);
        }
    }
}