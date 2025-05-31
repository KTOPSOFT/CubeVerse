using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class BoundsParameterField : IParameterField
    {
        public Type Type => typeof(Bounds);
        public int NumRows => 2;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var boundsValue = (Bounds)val;
            return rect != null 
                ? EditorGUI.BoundsField(rect.Value, boundsValue) 
                : EditorGUILayout.BoundsField(valName, boundsValue);
        }
    }
}