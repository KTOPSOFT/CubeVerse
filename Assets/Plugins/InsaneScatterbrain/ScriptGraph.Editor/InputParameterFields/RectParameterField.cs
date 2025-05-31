using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class RectParameterField : IParameterField
    {
        public Type Type => typeof(Rect);
        public int NumRows => 2;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var rectValue = (Rect)val;
            return rect != null 
                ? EditorGUI.RectField(rect.Value, rectValue) 
                : EditorGUILayout.RectField(valName, rectValue);
        }
    }
}