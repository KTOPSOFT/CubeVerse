using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain
{
    public static class TrigMath
    {
        /// <summary>
        /// Checks whether point p lays within the triangle made up by point p0, p1 and p2.
        /// </summary>
        /// <param name="p">The point to check if it lays within the triangle.</param>
        /// <param name="p0">The first triangle corner point.</param>
        /// <param name="p1">The second triangle corner point.</param>
        /// <param name="p2">The third triangle corner point.</param>
        /// <returns></returns>
        public static bool IsPointInTriangle(Vector2Int p, Vector2Int p0, Vector2Int p1, Vector2Int p2)
        {
            var a = .5f * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
            var sign = a < 0 ? -1 : 1;
            
            var s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) * sign;
            var t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) * sign;
            
            return s >= 0 && t >= 0 && s + t <= 2 * a * sign;
        }
        
        /// <summary>
        /// Checks whether point p lays within a rectangle made up by point p0, p1, p2 and p3.
        /// </summary>
        /// <param name="point">The point to check if it lays within the rectangle.</param>
        /// <param name="p0">The first rectangle corner point.</param>
        /// <param name="p1">The second rectangle corner point.</param>
        /// <param name="p2">The third rectangle corner point.</param>
        /// <param name="p3">The fourth rectangle corner point.</param>
        /// <returns></returns>
        public static bool IsPointInRectangle(Vector2Int point, Vector2Int p0, Vector2Int p1, Vector2Int p2, Vector2Int p3)
        {
            return 
                IsPointInTriangle(point, p0, p1, p3) || 
                IsPointInTriangle(point, p2, p1, p3);
        }

        [Obsolete("This overload will likely be removed in version 2.0.")]
        public static Vector2Int[] CalculateAllPointsWithinCircle(int radius)
        {
            var points = new List<Vector2Int>();
            CalculateAllPointsWithinCircle(radius, points);
            return points.ToArray();
        }

        /// <summary>
        /// Calculates all the integer points that lay within a circle of the given radios
        /// </summary>
        /// <param name="radius">The circle's radius.</param>
        /// <param name="points">The list to fill with the calculated points.</param>
        /// <returns>An array containing all the points within the circle.</returns>
        public static void CalculateAllPointsWithinCircle(int radius, List<Vector2Int> points)
        {
            points.Clear();
            var radiusSquared = radius*radius;
            
            for (var x = -radius; x <= radius; ++x)
            {
                var height = (int) Mathf.Sqrt(radiusSquared - x * x);

                for (var y = -height; y <= height; ++y)
                {
                    points.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}