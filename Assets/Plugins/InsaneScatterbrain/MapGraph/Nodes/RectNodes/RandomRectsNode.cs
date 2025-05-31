using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Creates random rectangles within the given bounds.
    /// </summary>
    [ScriptNode("Random Rectangles", "Rectangles"), Serializable]
    public class RandomRectsNode : ProcessorNode
    {
        [InPort("Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort sizeIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference]
        private InPort maskIn = null;
        
        [InPort("Min. Fill Percentage", typeof(float)), SerializeReference]
        private InPort minFillPercentageIn = null;
        
        [InPort("Max. Fill Percentage", typeof(float)), SerializeReference]
        [FormerlySerializedAs("fillPercentageIn")] 
        private InPort maxFillPercentageIn = null;
        
        [InPort("Max. Rectangles", typeof(int)), SerializeReference] 
        private InPort maxRectsIn = null;
        
        [InPort("Min. Width", typeof(int), true), SerializeReference] 
        private InPort minWidthIn = null;
        
        [InPort("Max. Width", typeof(int), true), SerializeReference] 
        private InPort maxWidthIn = null;
        
        [InPort("Min. Height", typeof(int), true), SerializeReference] 
        private InPort minHeightIn = null;
        
        [InPort("Max. Height", typeof(int), true), SerializeReference] 
        private InPort maxHeightIn = null;
        
        [InPort("Overlap?", typeof(bool)), SerializeReference] 
        private InPort overlapIn = null;
        
        
        [OutPort("Rectangles", typeof(RectInt[])), SerializeReference]
        private OutPort rectsOut = null;
        
        [OutPort("Mask", typeof(Mask)), SerializeReference]
        private OutPort maskOut = null;
        
        
        private Vector2Int bounds;
        private RectInt[] rects;
        
#if UNITY_EDITOR
        /// <summary>
        /// Gets the latest provided bounds. Only available in the editor.
        /// </summary>
        public Vector2Int Bounds => bounds;
        
        /// <summary>
        /// Gets the latest generated rectangles. Only available in the editor.
        /// </summary>
        public RectInt[] Rects => rects;
#endif
        
        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            bounds = sizeIn.Get<Vector2Int>();

            var mask = maskIn.Get<Mask>();
            
            var minWidth = minWidthIn.Get<int>();
            var maxWidth = maxWidthIn.Get<int>();
            
            var minHeight = minHeightIn.Get<int>();
            var maxHeight = maxHeightIn.Get<int>();

            var minFillPercentage = minFillPercentageIn.IsConnected ? minFillPercentageIn.Get<float>() : 0;
            var maxFillPercentage = maxFillPercentageIn.IsConnected ? maxFillPercentageIn.Get<float>() : 100;
            var maxPlacements = maxRectsIn.Get<int>();
            
            var overlap = overlapIn.Get<bool>();

            var rectsList = instanceProvider.Get<List<RectInt>>();
            
            var width = bounds.x;
            var height = bounds.y;
            
            if (mask == null)
            {
                var unmaskedPoints = instanceProvider.Get<List<int>>();
                for (var i = 0; i < width * height; ++i)
                {
                    unmaskedPoints.Add(i);
                }

                mask = instanceProvider.Get<Mask>();
                mask.Set(unmaskedPoints);
            }
            else
            {
                var newMask = instanceProvider.Get<Mask>();
                mask.Clone(newMask);
                mask = newMask;
            }

            var unmaskedPointsList = instanceProvider.Get<List<int>>();
            unmaskedPointsList.AddRange(mask.UnmaskedPoints);
            unmaskedPointsList.Shuffle(rng);

            var minNumOfPoints = Mathf.RoundToInt(unmaskedPointsList.Count / 100f * minFillPercentage);
            var maxNumOfPoints = Mathf.RoundToInt(unmaskedPointsList.Count / 100f * maxFillPercentage);

            var targetNumOfPoints = rng.Next(minNumOfPoints, maxNumOfPoints);

            var points = instanceProvider.Get<Stack<int>>();
            points.PushRange(unmaskedPointsList);
            var pointsFilled = instanceProvider.Get<HashSet<Vector2Int>>();
            var numPointsFilled = 0;

            while (points.Count > 0)
            {
                var rectWidth = rng.Next(minWidth, maxWidth + 1);
                var rectHeight = rng.Next(minHeight, maxHeight + 1);

                if (maxFillPercentageIn.IsConnected)
                {
                    var maxPointsLeft = maxNumOfPoints - numPointsFilled;
                    
                    if (maxPointsLeft < rectWidth * rectHeight) break; // This will exceed the max. fill percentage. Stop.
                }

                var point = points.Pop();
                
                if (mask.IsPointMasked(point)) continue;

                var x = point % width;
                var y = point / width;
                
                var rect = new RectInt(x, y, rectWidth, rectHeight);
                
                // Rectangle would be out of bounds, try with another point.
                if (rect.xMax - 1 >= bounds.x || rect.yMax - 1 >= bounds.y) continue;

                var validRect = true;

                // Check if the rectangle can be placed at the randomly gotten position.
                foreach (var rectPoint in rect.allPositionsWithin)
                {
                    var pointIndex = rectPoint.y * width + rectPoint.x;
                    if (!mask.IsPointMasked(pointIndex)) continue;

                    validRect = false;
                    break;
                }

                if (!validRect) continue;
                
                rectsList.Add(rect);

                if (!overlap)
                {
                    // If rectangles shouldn't overlap, add the points to the mask.
                    foreach (var rectPoint in rect.allPositionsWithin)
                    {
                        var pointIndex = rectPoint.y * width + rectPoint.x;
                        mask.MaskPoint(pointIndex);
                    }
                }
                
                // If a max. number of rectangles is provided, quit placing them if the max. number is reached.
                if (maxRectsIn.IsConnected && rectsList.Count >= maxPlacements) break;
            
                if (!overlap)
                {
                    numPointsFilled += rect.width * rect.height;
                }
                else
                {
                    foreach (var rectPoint in rect.allPositionsWithin)
                    {
                        if (pointsFilled.Contains(rectPoint)) continue;
                        
                        numPointsFilled++;
                        pointsFilled.Add(rectPoint);
                    }
                }

                if (numPointsFilled >= targetNumOfPoints) break;
            }

            if (overlap)
            {
                // If rectangles can overlap, wait with updating the mask until all rectangles have been created,
                // so they don't interfere with the placement process (meaning they won't overlap).
                foreach (var rect in rectsList)
                {
                    foreach (var rectPoint in rect.allPositionsWithin)
                    {
                        var pointIndex = rectPoint.y * width + rectPoint.x;
                        mask.MaskPoint(pointIndex);
                    }
                }
            }

            rects = rectsList.ToArray();

            rectsOut.Set(() => rects);
            maskOut.Set(() => mask);
        }
    }
}