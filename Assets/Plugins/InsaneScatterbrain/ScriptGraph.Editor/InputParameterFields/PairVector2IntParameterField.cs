using System;
using InsaneScatterbrain.DataStructures;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    public class PairVector2IntParameterField : IParameterField
    {
        public Type Type => typeof(Pair<Vector2Int>);
        public int NumRows => 2;
        
        public object Field(string valName, Type valType, object val, Rect? rect)
        {
            var vector2IntValue = (Pair<Vector2Int>)val;
            var valueA = vector2IntValue.First;
            var valueB = vector2IntValue.Second;
                
            if (rect != null)
            {
                var halfHeight = rect.Value.height / 2;
                var rectA = new Rect(rect.Value.x, rect.Value.y, rect.Value.width, halfHeight);
                var rectB = new Rect(rect.Value.x, rect.Value.y + halfHeight, rect.Value.width, halfHeight);
                    
                valueA = EditorGUI.Vector2IntField(rectA, string.Empty, valueA);
                valueB = EditorGUI.Vector2IntField(rectB, string.Empty, valueB);
                    
                return new Pair<Vector2Int>(valueA, valueB);
            }
                
            valueA = EditorGUILayout.Vector2IntField(string.Empty, valueA);
            valueB = EditorGUILayout.Vector2IntField(string.Empty, valueB);

            return new Pair<Vector2Int>(valueA, valueB);
        }
    }
}