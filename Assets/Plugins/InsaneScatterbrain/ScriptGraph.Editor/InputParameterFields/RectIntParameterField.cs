using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class RectIntParameterField : IParameterField
    {
        public Type Type => typeof(RectInt);
        public int NumRows => 2;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var rectIntValue = (RectInt)val;
            return rect != null 
                ? EditorGUI.RectIntField(rect.Value, rectIntValue) 
                : EditorGUILayout.RectIntField(valName, rectIntValue);
        }
    }
}