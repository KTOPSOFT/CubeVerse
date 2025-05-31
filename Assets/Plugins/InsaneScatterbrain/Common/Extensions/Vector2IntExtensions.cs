using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    public static class Vector2IntExtensions
    {
        public static Vector2Int RotateAround(this Vector2Int v, Vector2 rotateAround, float angle)
        {
            var vFloat = ((Vector2) v).RotateAround(rotateAround, -angle);

            return new Vector2Int(
                Mathf.RoundToInt(vFloat.x),
                Mathf.RoundToInt(vFloat.y));
        }

        public static int ManhattanDistance(this Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        public static float ManhattanDistance(this Vector2Int a, Vector2 b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        
        public static Vector3 SquareToCube(this Vector2Int squareVector)
        {
            var x = squareVector.x - (squareVector.y - squareVector.y % 2) / 2;
            var y = squareVector.y;
            var z = -x - y;
            
            return new Vector3(x, y, z);
        }
        
        private static readonly Vector2Int[] squareNeighbours = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };
        
        private static readonly Vector2Int[] hexNeighboursEven = {
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1)
        };
        
        private static readonly Vector2Int[] hexNeighboursOdd = {
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0)
        };

        public static Vector2Int[] GetRelativeNeighbours(this Vector2Int vector, bool hexagonal = false)
        {
            if (hexagonal)
            {
                return vector.y % 2 == 0 ? hexNeighboursEven : hexNeighboursOdd;
            }

            return squareNeighbours;
        }
    }
}