using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class Vector3IntExtensions
    {
        public static Vector2Int CubeToSquare(this Vector3 cubeVector)
        {
            var x = cubeVector.x + (cubeVector.y - cubeVector.y % 2) / 2;
            var y = cubeVector.y;
            return new Vector2Int(
                Mathf.RoundToInt(x), 
                Mathf.RoundToInt(y)
            );
        }
        
        public static float CubeDistanceTo(this Vector3 cubeVectorA, Vector3 cubeVectorB)
        {
            var diff = cubeVectorA - cubeVectorB;
            return Mathf.Max(
                Mathf.Abs(diff.x), 
                Mathf.Abs(diff.y), 
                Mathf.Abs(diff.z)
            );
        }
    }
}