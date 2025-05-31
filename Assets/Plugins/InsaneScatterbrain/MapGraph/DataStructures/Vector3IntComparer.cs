using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public class Vector3IntComparer : IComparer<Vector3Int>
    {
        public int Compare(Vector3Int vectorA, Vector3Int vectorB)
        {
            var zComparison = vectorA.z.CompareTo(vectorB.z);
            if (zComparison != 0) return zComparison;
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