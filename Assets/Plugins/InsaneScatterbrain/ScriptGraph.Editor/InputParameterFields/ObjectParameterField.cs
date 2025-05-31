using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.ScriptGraph
{
    public class ObjectParameterField : IParameterField
    {
        public Type Type => typeof(Object);
        public int NumRows => 1;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var objectValue = (Object)val;
            return rect != null 
                ? EditorGUI.ObjectField(rect.Value, objectValue, valType, true) 
                : EditorGUILayout.ObjectField(valName, objectValue, valType, true);
        }
    }
}