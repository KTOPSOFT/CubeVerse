using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelaunatorSharp;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents a collection of adjacent points that form a single area.
    /// </summary>
    public class Area : IPoint
    {
        private readonly List<Vector2Int> points = new List<Vector2Int>();
        private readonly HashSet<Vector2Int> pointSet = new HashSet<Vector2Int>();
        
        private RectInt boundingBox;
        public RectInt BoundingBox => boundingBox;

        private ReadOnlyCollection<Vector2Int> readOnlyPoints;
        /// <summary>
        /// Gets all the points the area consists of.
        /// </summary>
        public ReadOnlyCollection<Vector2Int> Points => readOnlyPoints ?? (readOnlyPoints = points.AsReadOnly());
        
        private readonly HashSet<Vector2Int> borderPoints = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, int> numNeighbours = new Dictionary<Vector2Int, int>();

        /// <summary>
        /// Gets all the points on the outer edge of the area.
        /// </summary>
        public IReadOnlyCollection<Vector2Int> BorderPoints => borderPoints;

        /// <summary>
        /// Gets the centroid of the area.
        /// </summary>
        public Vector2Int Centroid { get; private set; }
        
        private Vector2Int centroidTotal = new Vector2Int(0,0);

        public Area()
        {
        }
        
        [Obsolete("Please use the pool manager to get a Area instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        public Area(IEnumerable<Vector2Int> points)
        {
            Set(points);
        }

        public void Reset()
        {
            points.Clear();
            pointSet.Clear();
            boundingBox = default;
            borderPoints.Clear();
            numNeighbours.Clear();
            Centroid = default;
            centroidTotal = default;
        }

        /// <summary>
        /// Creates a new area of the given points.
        /// </summary>
        /// <param name="initialPoints">The points.</param>
        public void Set(IEnumerable<Vector2Int> initialPoints)
        {
            foreach (var point in initialPoints)
            {
                if (pointSet.Contains(point)) continue;
                
                Add(point);
            }
        }
        
        /// <summary>
        /// Create an area from a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle.</param>
        /// <returns>The area.</returns>
        public static Area CreateFromRect(RectInt rect)
        {
            var area = new Area { boundingBox = rect };

            for (var x = 0; x < rect.width; ++x)
            {
                for (var y = 0; y < rect.height; ++y)
                {
                    var point = rect.min + new Vector2Int(x, y);
                    area.AddPoint(point);

                    if (x == 0 || x == rect.width - 1 || y == 0 || y == rect.height - 1)
                    {
                        area.borderPoints.Add(point);
                    }
                }
            }

            return area;
        }

        public void Add(Vector2Int point)
        {
            AddPoint(point);
            UpdatePointAdded(point);
        }

        private void UpdateBorderPoint(Vector2Int point)
        {
            if (numNeighbours[point] < 4)
            {
                borderPoints.Add(point);
            }
            else if (borderPoints.Contains(point))
            {
                borderPoints.Remove(point);
            }
        }  

        private void UpdatePointAdded(Vector2Int point)
        {
            var x = point.x;
            var y = point.y;
            
            var north = new Vector2Int(x, y + 1);
            var south = new Vector2Int(x, y - 1);
            var west = new Vector2Int(x - 1, y);
            var east = new Vector2Int(x + 1, y);

            var neighbourCount = 0;
            if (pointSet.Contains(north))
            {
                neighbourCount++;
                numNeighbours[north]++;
                UpdateBorderPoint(north);
            }

            if (pointSet.Contains(south))
            {
                neighbourCount++;
                numNeighbours[south]++;
                UpdateBorderPoint(south);
            }

            if (pointSet.Contains(west))
            {
                neighbourCount++;
                numNeighbours[west]++;
                UpdateBorderPoint(west);
            }

            if (pointSet.Contains(east))
            {
                neighbourCount++;
                numNeighbours[east]++;
                UpdateBorderPoint(east);
            }
                
            numNeighbours.Add(point, neighbourCount);
            
            UpdateBorderPoint(point);

            if (pointSet.Count == 1)
            {
                boundingBox.xMin = point.x;
                boundingBox.yMin = point.y;
                boundingBox.xMax = point.x + 1;
                boundingBox.yMax = point.y + 1;
                
                return;
            }
            
            if (point.x < boundingBox.xMin) boundingBox.xMin = point.x;
            if (point.x > boundingBox.xMax - 1) boundingBox.xMax = point.x + 1;
            if (point.y < boundingBox.yMin) boundingBox.yMin = point.y;
            if (point.y > boundingBox.yMax - 1) boundingBox.yMax = point.y + 1;
        }

        /// <summary>
        /// Adds a point to the points list.
        /// </summary>
        /// <param name="point"></param>
        private void AddPoint(Vector2Int point)
        {
            points.Add(point);
            pointSet.Add(point);
            
            // Calculate the new centroid position.
            centroidTotal += point;
            Centroid = centroidTotal / points.Count;
        }

        /// <summary>
        /// Checks whether this area overlaps with another.
        /// </summary>
        /// <param name="other">The other area.</param>
        /// <returns>True if they overlap, false otherwise.</returns>
        public bool Overlaps(Area other)
        {
            foreach (var borderPoint in BorderPoints)
            {
                foreach (var otherBorderPoint in other.BorderPoints)
                {
                    if (borderPoint == otherBorderPoint) return true;
                }
            }

            foreach (var point in Points)
            {
                foreach (var otherPoint in other.Points)
                {
                    if (point == otherPoint) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Merges this area and another one together, creating a new area object.
        /// </summary>
        /// <param name="other">The other area.</param>
        /// <returns>The merged area.</returns>
        [Obsolete("This method will be removed in version 2.0. Please use the one with the result parameter instead.")]
        public Area Merge(Area other)
        {
            var mergedPointsSet = new HashSet<Vector2Int>(pointSet);
            mergedPointsSet.UnionWith(other.pointSet);
            return new Area(mergedPointsSet.ToArray());
        }

        /// <summary>
        /// Merges this area and another one together, creating a new area object.
        /// </summary>
        /// <param name="other">The other area.</param>
        /// <param name="result">The merged result.</param>
        public void Merge(Area other, Area result)
        {
            result.Reset();
            result.Set(pointSet);
            result.pointSet.UnionWith(other.pointSet);
        }

        public bool Contains(Vector2Int point) => pointSet.Contains(point);

        public double X
        {
            get => Centroid.x;
            set => throw new NotSupportedException();
        }

        public double Y
        {
            get => Centroid.y;
            set => throw new NotSupportedException();
        }
    }
}