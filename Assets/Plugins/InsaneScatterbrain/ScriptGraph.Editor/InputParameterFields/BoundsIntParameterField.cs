using System;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class BoundsIntParameterField : IParameterField
    {
        public Type Type => typeof(BoundsInt);
        public int NumRows => 2;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var boundsIntValue = (BoundsInt)val;
            return rect != null 
                ? EditorGUI.BoundsIntField(rect.Value, boundsIntValue) 
                : EditorGUILayout.BoundsIntField(valName, boundsIntValue);
        }
    }
}