using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 RotateAround(this Vector2 v, Vector2 rotateAround, float angle)
        {
            var rotation = Quaternion.Euler(0, 0, angle);
            v -= rotateAround;
            v = rotation * v;
            v += rotateAround;
            return v;
        }
        
        public static float ManhattanDistance(this Vector2 a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        public static float ManhattanDistance(this Vector2 a, Vector2 b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        public static Vector2Int RoundToInt(this Vector2 v)
        {
            return new Vector2Int(
                Mathf.RoundToInt(v.x),
                Mathf.RoundToInt(v.y));
        }

        public static Vector2Int FloorToInt(this Vector2 v)
        {
            return new Vector2Int(
                Mathf.FloorToInt(v.x),
                Mathf.FloorToInt(v.y));
        }
    }
}