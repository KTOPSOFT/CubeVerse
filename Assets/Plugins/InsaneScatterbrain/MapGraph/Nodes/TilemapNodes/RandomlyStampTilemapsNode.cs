using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Randomly stamps the given tilemap onto the the texture. Useful when you want to use pre-defined tilemap chunks
    /// to generate maps from.
    /// </summary>
    [ScriptNode("Randomly Stamp Tilemaps", "Tilemaps"), Serializable]
    public class RandomlyStampTilemapsNode : ProcessorNode
    {
        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference] 
        private InPort sizeIn = null;
        
        [InPort("Tilemap Set", typeof(TilemapSet), true), SerializeReference] 
        private InPort tilemapSetIn = null;
        
        [InPort("Tilemap Type Name", typeof(string), true), SerializeReference] 
        private InPort tilemapTypeNameIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Min. Fill Percentage", typeof(float)), SerializeReference]
        private InPort minFillPercentageIn = null;
        
        [InPort("Max. Fill Percentage", typeof(float)), SerializeReference] 
        [FormerlySerializedAs("fillPercentageIn")]
        private InPort maxFillPercentageIn = null;
        
        [InPort("Max. placements", typeof(int)), SerializeReference] 
        private InPort maxPlacementsIn = null;

        [InPort("Margin", typeof(int)), SerializeReference] 
        private InPort marginIn = null;
        
        [InPort("Precise Margin?", typeof(bool)), SerializeReference] 
        private InPort preciseMarginIn = null;
        
        
        [OutPort("Tilemap", typeof(TilemapData)), SerializeReference]
         private OutPort tilemapOut = null;
         
        [OutPort("Placements", typeof(Vector2Int[])), SerializeReference]
         private OutPort placementsOut = null;
         
        [OutPort("Margin Placements", typeof(Vector2Int[])), SerializeReference]
         private OutPort marginPlacementsOut = null;
         
        [OutPort("Empty Points", typeof(Vector2Int[])), SerializeReference]
         private OutPort emptyPointsOut = null;
         

#if UNITY_EDITOR
        public TextureData TextureData { get; private set; }
#endif
        
        private Outliner outliner;
        private Outliner Outliner => outliner ?? (outliner = new Outliner());

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            var bounds = sizeIn.Get<Vector2Int>();
            var tilemapSet = tilemapSetIn.Get<TilemapSet>();
            var tilemapTypeName = tilemapTypeNameIn.Get<string>();
            var mask = maskIn.Get<Mask>();
            var minFillPercentage = minFillPercentageIn.IsConnected ? minFillPercentageIn.Get<float>() : 0;
            var maxFillPercentage = maxFillPercentageIn.IsConnected ? maxFillPercentageIn.Get<float>() : 100;
            var maxPlacements = maxPlacementsIn.Get<int>();
            var margin = marginIn.Get<int>();
            var preciseMargin = preciseMarginIn.Get<bool>();

#if UNITY_EDITOR
            TextureData = instanceProvider.Get<TextureData>();
            TextureData.Set(bounds.x, bounds.y);
#endif

            var width = bounds.x;
            var height = bounds.y;

            // Create a new empty mask or create a copy of the one provided.
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

            // Randomize the order of the unmasked points for placement.
            var unmaskedPointsList = instanceProvider.Get<List<int>>();
            unmaskedPointsList.AddRange(mask.UnmaskedPoints);
            unmaskedPointsList.Shuffle(rng);
            
            var minNumOfPoints = Mathf.RoundToInt(unmaskedPointsList.Count / 100f * minFillPercentage);
            var maxNumOfPoints = Mathf.RoundToInt(unmaskedPointsList.Count / 100f * maxFillPercentage);

            var targetNumOfPoints = rng.Next(minNumOfPoints, maxNumOfPoints);

            var points = instanceProvider.Get<Stack<int>>();
            points.PushRange(unmaskedPointsList);
            var numPointsFilled = 0;
            var tilemapPlacementCount = 0;

            var tilemap = instanceProvider.Get<TilemapData>();
            var placementSet = instanceProvider.Get<HashSet<Vector2Int>>();
            var marginPlacementSet = instanceProvider.Get<HashSet<Vector2Int>>();
            var blockPlacements = instanceProvider.Get<List<Vector2Int>>();
            var emptyPoints = instanceProvider.Get<List<Vector2Int>>();

            while (points.Count > 0)
            {
                // Get a random tilemap to place
                var tilemapChunk = tilemapSet.GetRandomTilemapData(tilemapTypeName, rng);
                
                var point = points.Pop();
                if (mask.IsPointMasked(point)) continue;
                
                var innerX = point % width;
                var innerY = point / width;

                var innerPosition = new Vector2Int(innerX, innerY);
                
                var innerBounds = tilemapChunk.GetOffsetAdjustedBounds(innerPosition);
                var outerBounds = new RectInt(
                    innerBounds.xMin - margin,
                    innerBounds.yMin - margin,
                    innerBounds.width + margin * 2,
                    innerBounds.height + margin * 2);

                if (outerBounds.xMin < 0 || outerBounds.xMax > width || outerBounds.yMin < 0 || outerBounds.yMax > height)
                {
                    // The tilemap chunk can't be placed in this position, its out of bounds.
                    continue;
                }
                
                if (maxFillPercentageIn.IsConnected)
                {
                    var maxPointsLeft = maxNumOfPoints - numPointsFilled;
                    
                    if (maxPointsLeft < innerBounds.width * innerBounds.height) break; // This will exceed the max. fill percentage. Stop.
                }

                var validPlacement = true;
                
                // Check whether placing the chunk here would be likely to overlap with something else.
                foreach (var outerBoundsPoint in outerBounds.allPositionsWithin)
                {
                    var pointIndex = outerBoundsPoint.y * width + outerBoundsPoint.x;
                    if (!mask.IsPointMasked(pointIndex)) continue;

                    // The tilemap chunk can't be placed in this position, it would intersect with a masked point.
                    validPlacement = false;
                    break;
                }

                if (!validPlacement) continue;

                // Stamp the tiles of the random chunk onto the target tilemap.
                tilemap.SetTilesBlock(innerPosition, tilemapChunk, ref blockPlacements);

                // These points are no longer available for placement, mask them.
                foreach (var blockPlacement in blockPlacements)
                {
                    placementSet.Add(blockPlacement);

                    var pointIndex = blockPlacement.y * width + blockPlacement.x;
                    mask.MaskPoint(pointIndex);
                    
                    numPointsFilled++;
                }

                var marginPlacements = instanceProvider.Get<List<Vector2Int>>();
                
                if (preciseMargin)
                {
                    // If the precise margin should be calculated, that's done by generating the outline of the placed tiles.
                    Outliner.Bounds = bounds;
                    Outliner.Thickness = margin;
                    Outliner.CalculateOutline(blockPlacements, ref marginPlacements);

                    foreach (var placement in marginPlacements)
                    {
                        var pointIndex = placement.y * width + placement.x;
                        mask.MaskPoint(pointIndex);
                        
                        marginPlacementSet.Add(placement);
                    }
                }
                else
                {
                    // Otherwise we just use the outer bounds, used before for determining valid placement.
                    foreach (var outerBoundsPoint in outerBounds.allPositionsWithin)
                    {
                        var pointIndex = outerBoundsPoint.y * width + outerBoundsPoint.x;
                        mask.MaskPoint(pointIndex);

                        if (placementSet.Contains(outerBoundsPoint)) continue;
                        
                        if (innerBounds.Contains(outerBoundsPoint))
                        {
                            emptyPoints.Add(outerBoundsPoint);
                        }
                        else
                        {
                            marginPlacementSet.Add(outerBoundsPoint);
                        }
                    }
                }
                
                // Keep track of the number of stamped chunks and if it's reached the max. number of placement (if provided)
                // stop stamping.
                tilemapPlacementCount++;
                
                if (maxPlacementsIn.IsConnected && tilemapPlacementCount >= maxPlacements) break;
                
                // If the target number of points to fill has been reached, stop stamping.
                if (numPointsFilled >= targetNumOfPoints) break;
            }

            var placements = placementSet.ToArray();
            var marginPlacementsArray = marginPlacementSet.ToArray();
            var emptyPointsArray = emptyPoints.ToArray();


#if UNITY_EDITOR
            foreach (var placement in placements)
            {
                TextureData[placement.y * width + placement.x] = Color.white;
            }
            
            foreach (var placement in marginPlacementsArray)
            {
                TextureData[placement.y * width + placement.x] = Color.red;
            }
            
            foreach (var placement in emptyPointsArray)
            {
                TextureData[placement.y * width + placement.x] = Color.black;
            }
#endif

            tilemapOut.Set(() => tilemap);
            placementsOut.Set(() => placements);
            marginPlacementsOut.Set(() => marginPlacementsArray);
            emptyPointsOut.Set(() => emptyPointsArray);
        }
    }
}