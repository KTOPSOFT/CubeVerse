using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Finds corner points out of the set of points and outputs those points.
    ///
    /// A corner point is defined as a point in the set of input points that has at least one diagonal neighbour
    /// point that isn't in the set of input points and the two neighbour points they share are either both in the
    /// input set or neither of them are.
    /// </summary>
    [ScriptNode("Corner Points", "Points"), Serializable]
    public class CornerPointsNode : ProcessorNode
    {
        [InPort("Points", typeof(Vector2Int[]), true), SerializeReference]
        private InPort pointsIn = null;
        
        
        [OutPort("Corner Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort cornerPointsOut = null;
        
        
        private Vector2Int[] points;
        private List<Vector2Int> cornerPoints;
        private HashSet<Vector2Int> pointSet;
        
#if UNITY_EDITOR
        public int Width { get; private set; }
        public int Height { get; private set; }
        
        public IEnumerable<Vector2Int> Points => points;
        public List<Vector2Int> CornerPoints => cornerPoints;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            points = pointsIn.Get<Vector2Int[]>();

            pointSet = instanceProvider.Get<HashSet<Vector2Int>>();
            pointSet.UnionWith(points);

            cornerPoints = instanceProvider.Get<List<Vector2Int>>();
            cornerPoints.EnsureCapacity(points.Length);
            
#if UNITY_EDITOR
            Width = 0;
            Height = 0;
#endif
            
            foreach (var point in points)
            {
                var x = point.x;
                var y = point.y;
                
#if UNITY_EDITOR
                if (x >= Width) Width = x + 1;
                if (y >= Height) Height = y + 1;
#endif

                var north = new Vector2Int(x, y + 1);
                var south = new Vector2Int(x, y - 1);
                var west = new Vector2Int(x - 1, y);
                var east = new Vector2Int(x + 1, y);
                
                var northWest = new Vector2Int(x - 1, y + 1);
                var northEast = new Vector2Int(x + 1, y + 1);
                var southWest = new Vector2Int(x - 1, y - 1);
                var southEast = new Vector2Int(x + 1, y - 1);

                if (!pointSet.Contains(northWest) && (!pointSet.Contains(north) && !pointSet.Contains(west) || pointSet.Contains(north) && pointSet.Contains(west))  ||
                    !pointSet.Contains(northEast) && (!pointSet.Contains(north) && !pointSet.Contains(east) || pointSet.Contains(north) && pointSet.Contains(east)) ||
                    !pointSet.Contains(southWest) && (!pointSet.Contains(south) && !pointSet.Contains(west) || pointSet.Contains(south) && pointSet.Contains(west)) ||
                    !pointSet.Contains(southEast) && (!pointSet.Contains(south) && !pointSet.Contains(east) || pointSet.Contains(south) && pointSet.Contains(east)))
                {
                    cornerPoints.Add(point);
                }
            }

            var cornerPointsArray = cornerPoints.ToArray();

            cornerPointsOut.Set(() => cornerPointsArray);
        }
    }
}