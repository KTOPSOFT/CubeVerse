using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 CubeRoundToInt(this Vector3 cubeVector)
        {
            var x = Mathf.Round(cubeVector.x);
            var y = Mathf.Round(cubeVector.y);
            var z = Mathf.Round(cubeVector.z);

            var xDiff = Mathf.Abs(x - cubeVector.x);
            var yDiff = Mathf.Abs(y - cubeVector.y);
            var zDiff = Mathf.Abs(z - cubeVector.z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                x = -y - z;
            }
            else if (yDiff > zDiff)
            {
                y = -x - z;
            }
            else
            {
                z = -x - y;
            }

            return new Vector3Int(
                Mathf.RoundToInt(x), 
                Mathf.RoundToInt(y), 
                Mathf.RoundToInt(z)
            );
        }
    }
}