using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Calculates points that create an outline around a set of points.
    /// </summary>
    public class Outliner
    {
        private int thicknessIndex;
        
        /// <summary>
        /// Get/sets the bounds in which the outline points should be.
        /// </summary>
        public Vector2Int Bounds { get; set; } = -Vector2Int.one;
        
        /// <summary>
        /// Get/sets whether or not to include corner points into the outline.
        /// </summary>
        public bool Corners { get; set; } = true;

        /// <summary>
        /// Get/sets the thickness of the outline.
        /// </summary>
        public int Thickness
        {
            set
            {
                ThicknessTop = value;
                ThicknessBottom = value;
                ThicknessLeft = value;
                ThicknessRight = value;
            }
        }

        public int ThicknessTop { get; set; } = 1;
        public int ThicknessBottom { get; set; } = 1;
        public int ThicknessLeft { get; set; } = 1;
        public int ThicknessRight { get; set; } = 1;

        public Mask Mask { get; set; }

        private readonly List<Vector2Int> outlinePoints = new List<Vector2Int>();
        private readonly List<Vector2Int> totalOutlinePoints = new List<Vector2Int>();
        
        private readonly HashSet<Vector2Int> pointsToOutline = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> filledPoints = new HashSet<Vector2Int>();
        
        private bool IsInBounds(int x, int y)
        {
            if (x < 0 || y < 0) return false;
            if (Bounds.magnitude > -1 && (x >= Bounds.x || y >= Bounds.y)) return false;
            return true;
        }
        
        private bool ProcessNeighbour(int xNeighbour, int yNeighbour, bool applyOutline)
        {
            var neighbour = new Vector2Int(xNeighbour, yNeighbour);
            if (!IsInBounds(xNeighbour, yNeighbour) || filledPoints.Contains(neighbour)) return false;

            if (!applyOutline) return true;
            
            AddPoint(neighbour);

            return true;
        }
        
        private bool ProcessNeighbours(int x, int y, bool applyOutline)
        {
            var count = 0;
            
            if (thicknessIndex < ThicknessTop && ProcessNeighbour(x, y + 1, applyOutline)) count++;
            if (thicknessIndex < ThicknessBottom && ProcessNeighbour(x, y - 1, applyOutline)) count++;
            if (thicknessIndex < ThicknessLeft && ProcessNeighbour(x - 1, y, applyOutline)) count++;
            if (thicknessIndex < ThicknessRight && ProcessNeighbour(x + 1, y, applyOutline)) count++;

            if (!Corners) return count > 0;
            
            // Also process the corners if necessary.
            if (thicknessIndex < ThicknessTop && thicknessIndex < ThicknessRight && 
                ProcessNeighbour(x + 1, y + 1, applyOutline)) count++;
            if (thicknessIndex < ThicknessBottom && thicknessIndex < ThicknessLeft && 
                ProcessNeighbour(x - 1, y - 1, applyOutline)) count++;
            if (thicknessIndex < ThicknessTop && thicknessIndex < ThicknessLeft && 
                ProcessNeighbour(x - 1, y + 1, applyOutline)) count++;
            if (thicknessIndex < ThicknessBottom && thicknessIndex < ThicknessRight && 
                ProcessNeighbour(x + 1, y - 1, applyOutline)) count++;
            
            return count > 0;
        }

        /// <summary>
        /// Calculates the points that outline the given set of points and places them in the result list.
        /// </summary>
        /// <param name="points">The points to outline.</param>
        /// <param name="result">The list to fill with the outline points.</param>
        public void CalculateOutline(IEnumerable<Vector2Int> points, ref List<Vector2Int> result) => 
            CalculateOutline(points, false, ref result);
        
        /// <summary>
        /// Calculates the points that inline the given set of points and places them in the result list.
        /// </summary>
        /// <param name="points">The points to inline.</param>
        /// <param name="result">The list to fill with the outline points.</param>
        public void CalculateInline(IEnumerable<Vector2Int> points, ref List<Vector2Int> result) => 
            CalculateOutline(points, true, ref result);

        private void AddPoint(Vector2Int point)
        {
            var pointIndex = point.y * Bounds.x + point.x;
            if (Mask != null && Mask.IsPointMasked(pointIndex)) return;
            
            outlinePoints.Add(point);
            filledPoints.Add(point);
        }
        
        private void CalculateOutline(IEnumerable<Vector2Int> points, bool inline, ref List<Vector2Int> result)
        {
            outlinePoints.Clear();
            filledPoints.Clear();
            pointsToOutline.Clear();
            totalOutlinePoints.Clear();

            foreach (var point in points)
            {
                filledPoints.Add(point);
                pointsToOutline.Add(point);
            }

            var thickness = ThicknessTop;
            thickness = ThicknessBottom > thickness ? ThicknessBottom : thickness;
            thickness = ThicknessLeft > thickness ? ThicknessLeft : thickness;
            thickness = ThicknessRight > thickness ? ThicknessRight : thickness;

            for (thicknessIndex = 0; thicknessIndex < thickness; ++thicknessIndex)
            {
                outlinePoints.Clear();

                foreach (var point in pointsToOutline)
                {
                    if (outlinePoints.Contains(point)) continue;

                    var x = point.x;
                    var y = point.y;

                    if (inline)
                    {
                        // If the outline should be drawn on the inside of the shape instead of the outside. Then the
                        // color shouldn't applied to the neighbouring pixels that we would normally consider
                        // part of an outline, but on the pixel itself.
                        if (!ProcessNeighbours(x, y, false)) continue;
                    
                        AddPoint(point);
                    }
                    else
                    {
                        ProcessNeighbours(x, y, true);
                    }
                }

                if (inline)
                {
                    foreach (var outlinePoint in outlinePoints)
                    {
                        pointsToOutline.Remove(outlinePoint);
                        filledPoints.Remove(outlinePoint);
                    }
                }
                else
                {
                    foreach (var outlinePoint in outlinePoints)
                    {
                        pointsToOutline.Add(outlinePoint);
                    }
                }
                
                totalOutlinePoints.AddRange(outlinePoints);
            }

            result.Clear();
            result.AddRange(totalOutlinePoints);
        }
    }
}
