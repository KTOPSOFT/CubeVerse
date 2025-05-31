using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Areas Special Border Points", "Areas"), Serializable]
    public class AreasSpecialBorderPointsNode : ProcessorNode
    {
        [InPort("Areas", typeof(Area[]), true), SerializeReference]
        private InPort areasIn = null;
        
        
        [OutPort("Top Left", typeof(Vector2Int[])), SerializeReference]
        private OutPort topLeftPointsOut = null;
        
        [OutPort("Top", typeof(Vector2Int[])), SerializeReference]
        private OutPort topPointsOut = null;

        [OutPort("Top Right", typeof(Vector2Int[])), SerializeReference]
        private OutPort topRightPointsOut = null;

        [OutPort("Left", typeof(Vector2Int[])), SerializeReference]
        private OutPort leftPointsOut = null;
        
        [OutPort("Right", typeof(Vector2Int[])), SerializeReference]
        private OutPort rightPointsOut = null;
    
        [OutPort("Bottom Left", typeof(Vector2Int[])), SerializeReference]
        private OutPort bottomLeftPointsOut = null;
        
        [OutPort("Bottom", typeof(Vector2Int[])), SerializeReference]
        private OutPort bottomPointsOut = null;
        
        [OutPort("Bottom Right", typeof(Vector2Int[])), SerializeReference]
        private OutPort bottomRightPointsOut = null;
        
        [OutPort("All", typeof(Vector2Int[])), SerializeReference]
        private OutPort allPointsOut = null;


        private Area[] areas;

        private List<Vector2Int> topLeftPoints;
        private List<Vector2Int> topPoints;
        private List<Vector2Int> topRightPoints;
        
        private List<Vector2Int> leftPoints;
        private List<Vector2Int> rightPoints;
        
        private List<Vector2Int> bottomLeftPoints;
        private List<Vector2Int> bottomPoints;
        private List<Vector2Int> bottomRightPoints;

        private List<Vector2Int> allPoints;
        
#if UNITY_EDITOR
        public Area[] Areas => areas;

        private ReadOnlyCollection<Vector2Int> readOnlyTopLeftPoints;
        private ReadOnlyCollection<Vector2Int> readOnlyTopPoints;
        private ReadOnlyCollection<Vector2Int> readOnlyTopRightPoints;
        
        private ReadOnlyCollection<Vector2Int> readOnlyLeftPoints;
        private ReadOnlyCollection<Vector2Int> readOnlyRightPoints;
        
        private ReadOnlyCollection<Vector2Int> readOnlyBottomLeftPoints;
        private ReadOnlyCollection<Vector2Int> readOnlyBottomPoints;
        private ReadOnlyCollection<Vector2Int> readOnlyBottomRightPoints;
        
        private ReadOnlyCollection<Vector2Int> readOnlyAllPoints;

        public IEnumerable<Vector2Int> TopLeftPoints => readOnlyTopLeftPoints;
        public IEnumerable<Vector2Int> TopPoints => readOnlyTopPoints;
        public IEnumerable<Vector2Int> TopRightPoints => readOnlyTopRightPoints;
        
        public IEnumerable<Vector2Int> LeftPoints => readOnlyLeftPoints;
        public IEnumerable<Vector2Int> RightPoints => readOnlyRightPoints;
        
        public IEnumerable<Vector2Int> BottomLeftPoints => readOnlyBottomLeftPoints;
        public IEnumerable<Vector2Int> BottomPoints => readOnlyBottomPoints;
        public IEnumerable<Vector2Int> BottomRightPoints => readOnlyBottomRightPoints;
        
        
        public IEnumerable<Vector2Int> AllPoints => readOnlyAllPoints;
#endif
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            areas = areasIn.Get<Area[]>();

            topLeftPoints = instanceProvider.Get<List<Vector2Int>>();
            topPoints = instanceProvider.Get<List<Vector2Int>>();
            topRightPoints = instanceProvider.Get<List<Vector2Int>>();
            
            leftPoints = instanceProvider.Get<List<Vector2Int>>();
            rightPoints = instanceProvider.Get<List<Vector2Int>>();
            
            bottomLeftPoints = instanceProvider.Get<List<Vector2Int>>();
            bottomPoints = instanceProvider.Get<List<Vector2Int>>();
            bottomRightPoints = instanceProvider.Get<List<Vector2Int>>();
            
            allPoints = instanceProvider.Get<List<Vector2Int>>();

#if UNITY_EDITOR
            readOnlyTopLeftPoints = topLeftPoints.AsReadOnly();
            readOnlyTopPoints = topPoints.AsReadOnly();
            readOnlyTopRightPoints = topRightPoints.AsReadOnly();
            
            readOnlyLeftPoints = leftPoints.AsReadOnly();
            readOnlyRightPoints = rightPoints.AsReadOnly();
            
            readOnlyBottomLeftPoints = bottomLeftPoints.AsReadOnly();
            readOnlyBottomPoints = bottomPoints.AsReadOnly();
            readOnlyBottomRightPoints = bottomRightPoints.AsReadOnly();
            
            readOnlyAllPoints = allPoints.AsReadOnly();
#endif

            foreach (var area in areas)
            {
                var bounds = area.BoundingBox;

                var boundsTopLeft = new Vector2(bounds.xMin, bounds.yMax).FloorToInt();
                var boundsTop = new Vector2(bounds.center.x, bounds.yMax).FloorToInt();
                var boundsTopRight = new Vector2(bounds.xMax, bounds.yMax).FloorToInt();

                var boundsLeft = new Vector2(bounds.xMin, bounds.center.y).FloorToInt();
                var boundsRight = new Vector2(bounds.xMax, bounds.center.y).FloorToInt();
                
                var boundsBottomLeft = new Vector2(bounds.xMin, bounds.yMin).FloorToInt();
                var boundsBottom = new Vector2(bounds.center.x, bounds.yMin).FloorToInt();
                var boundsBottomRight = new Vector2(bounds.xMax, bounds.yMin).FloorToInt();


                var areaTopLeft = boundsBottomRight;
                var areaTop = boundsBottom;
                var areaTopRight = boundsBottomLeft;

                var areaLeft = boundsRight;
                var areaRight = boundsLeft;

                var areaBottomLeft = boundsTopRight;
                var areaBottom = boundsTop;
                var areaBottomRight = boundsTopLeft;
                

                foreach (var point in area.BorderPoints)
                {
                    if (boundsTopLeft.ManhattanDistance(point) <= boundsTopLeft.ManhattanDistance(areaTopLeft)) areaTopLeft = point;
                    if (boundsTop.ManhattanDistance(point) <= boundsTop.ManhattanDistance(areaTop)) areaTop = point;
                    if (boundsTopRight.ManhattanDistance(point) <= boundsTopRight.ManhattanDistance(areaTopRight)) areaTopRight = point;
                    
                    if (boundsLeft.ManhattanDistance(point) <= boundsLeft.ManhattanDistance(areaLeft)) areaLeft = point;
                    if (boundsRight.ManhattanDistance(point) <= boundsRight.ManhattanDistance(areaRight)) areaRight = point;
                    
                    if (boundsBottomLeft.ManhattanDistance(point) <= boundsBottomLeft.ManhattanDistance(areaBottomLeft)) areaBottomLeft = point;
                    if (boundsBottom.ManhattanDistance(point) <= boundsBottom.ManhattanDistance(areaBottom)) areaBottom = point;
                    if (boundsBottomRight.ManhattanDistance(point) <= boundsBottomRight.ManhattanDistance(areaBottomRight)) areaBottomRight = point;
                }
                
                
                topLeftPoints.Add(areaTopLeft);
                topPoints.Add(areaTop);
                topRightPoints.Add(areaTopRight);
                
                leftPoints.Add(areaLeft);
                rightPoints.Add(areaRight);
                
                bottomLeftPoints.Add(areaBottomLeft);
                bottomPoints.Add(areaBottom);
                bottomRightPoints.Add(areaBottomRight);
                
                allPoints.Add(areaTopLeft);
                allPoints.Add(areaTop);
                allPoints.Add(areaTopRight);
                
                allPoints.Add(areaLeft);
                allPoints.Add(areaRight);
                
                allPoints.Add(areaBottomLeft);
                allPoints.Add(areaBottom);
                allPoints.Add(areaBottomRight);
            }
        
            var topLeftPointsArray = topLeftPoints.ToArray();
            var topPointsArray = topPoints.ToArray();
            var topRightPointsArray = topRightPoints.ToArray();
            
            var leftPointsArray = leftPoints.ToArray();
            var rightPointsArray = rightPoints.ToArray();
            
            var bottomLeftPointsArray = bottomLeftPoints.ToArray();
            var bottomPointsArray = bottomPoints.ToArray();
            var bottomRightPointsArray = bottomRightPoints.ToArray();

            var allPointsArray = allPoints.ToArray();
            
        
            topLeftPointsOut.Set(() => topLeftPointsArray);
            topPointsOut.Set(() => topPointsArray);
            topRightPointsOut.Set(() => topRightPointsArray);

            leftPointsOut.Set(() => leftPointsArray);
            rightPointsOut.Set(() => rightPointsArray);
            
            bottomLeftPointsOut.Set(() => bottomLeftPointsArray);
            bottomPointsOut.Set(() => bottomPointsArray);
            bottomRightPointsOut.Set(() => bottomRightPointsArray);
            
            allPointsOut.Set(() => allPointsArray);
        }
    }
}