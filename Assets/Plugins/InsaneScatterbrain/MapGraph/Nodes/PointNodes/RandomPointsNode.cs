using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Provides a set of random points within the given bounds.
    /// </summary>
    [ScriptNode("Random Points", "Points"), Serializable]
    public class RandomPointsNode : ProcessorNode
    {
        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference] 
        private InPort boundsIn = null;
      
        [InPort("Mask", typeof(Mask)), SerializeReference] 
        private InPort maskIn = null;
        
        [InPort("Percentage", typeof(float), true), SerializeReference] 
        private InPort percentageIn = null;
        
        [InPort("Max. placements", typeof(int)), SerializeReference] 
        private InPort maxPlacementsIn = null;
        
        
        [OutPort("Points", typeof(Vector2Int[])), SerializeReference] 
        private OutPort pointsOut = null;
        
        /// <summary>
        /// If a mask is provided, random points will not be masked points.
        /// </summary>
        [OutPort("Mask", typeof(Mask)), SerializeReference] 
        private OutPort maskOut = null;
        

        private Vector2Int bounds;
        private Vector2Int[] points;
        
#if UNITY_EDITOR
        /// <summary>
        /// The latest given bounds. Only available in the editor.
        /// </summary>
        public Vector2Int Bounds => bounds;
        
        /// <summary>
        /// The latest generated random points. Only available in the editor.
        /// </summary>
        public Vector2Int[] Points => points;
#endif

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            bounds = boundsIn.Get<Vector2Int>();
            var fillPercentage = percentageIn.Get<float>();
            var maxPlacements = maxPlacementsIn.Get<int>();
            var mask = maskIn.Get<Mask>();
            
            var width = bounds.x;
            var height = bounds.y;
            
            Mask outputMask = null;

            var availablePoints = instanceProvider.Get<List<int>>();

            if (mask != null)
            {
                outputMask = instanceProvider.Get<Mask>();
                mask.Clone(outputMask);
                
                availablePoints.AddRange(mask.UnmaskedPoints);
            }
            else
            {
                availablePoints.EnsureCapacity(width * height);
                for (var i = 0; i < width * height; ++i)
                {
                    availablePoints.Add(i);
                }
            }
            
            availablePoints.Shuffle(rng);
            
            var numOfPoints = Mathf.RoundToInt(availablePoints.Count / 100f * fillPercentage);
            if (maxPlacementsIn.IsConnected && numOfPoints > maxPlacements)
            {
                numOfPoints = maxPlacements;
            }

            var pointIndices = availablePoints.GetRange(0, numOfPoints);
            var pointsList = instanceProvider.Get<List<Vector2Int>>();
            pointsList.EnsureCapacity(pointIndices.Count);

            foreach (var index in pointIndices)
            {
                outputMask?.MaskPoint(index);
                pointsList.Add(new Vector2Int(index % width, index / width));
            }

            points = pointsList.ToArray();
            
            maskOut.Set(() => outputMask);
            pointsOut.Set(() => points);
        }
    }
}