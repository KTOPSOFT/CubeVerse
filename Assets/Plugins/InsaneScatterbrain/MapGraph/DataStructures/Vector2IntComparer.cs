using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public class Vector2IntComparer : IComparer<Vector2Int>
    {
        public int Compare(Vector2Int vectorA, Vector2Int vectorB)
        {
            var yComparison = vectorA.y.CompareTo(vectorB.y);
            if (yComparison != 0) return yComparison;
            var xComparison = vectorA.x.CompareTo(vectorB.x);
            if (xComparison != 0) return xComparison;
            var magnitudeComparison = vectorA.magnitude.CompareTo(vectorB.magnitude);
            if (magnitudeComparison != 0) return magnitudeComparison;
            return vectorA.sqrMagnitude.CompareTo(vectorB.sqrMagnitude);
        }
    }
}