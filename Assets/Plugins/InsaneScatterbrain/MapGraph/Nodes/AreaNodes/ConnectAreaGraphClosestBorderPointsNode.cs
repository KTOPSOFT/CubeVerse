using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Pairs the closest border positions of each connected area pair in an area graph.
    /// </summary>
    [ScriptNode("Connect Area Graph (Closest Border Points)", "Areas"), Serializable]
    public class ConnectAreaGraphClosestBorderPointsNode : ProcessorNode 
    {
        [InPort("Area Graph", typeof(AreaGraph), true), SerializeReference] 
        private InPort areaGraphIn = null;

        [OutPort("Connected Points", typeof(Pair<Vector2Int>[])), SerializeReference] 
        private OutPort connectedPointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var areaGraph = areaGraphIn.Get<AreaGraph>();

            var connectedPoints = instanceProvider.Get<List<Pair<Vector2Int>>>();
            connectedPoints.EnsureCapacity(areaGraph.Edges.Count());
            foreach (var areaPair in areaGraph.Edges)
            {
                var areaA = areaPair.Source;
                var areaB = areaPair.Target;

                var closestDistance = float.MaxValue;
                var closestPointA = new Vector2Int();
                var closestPointB = new Vector2Int();

                var closestCentroidDistance = float.MaxValue;

                // Compare all border points with each other and find the ones that are closest together.
                foreach (var pointA in areaA.BorderPoints)
                foreach (var pointB in areaB.BorderPoints)
                {
                    var distance = Vector2Int.Distance(pointA, pointB);
                    
                    if (distance > closestDistance) continue;
                    
                    var distanceToCentroidA = Vector2Int.Distance(pointA, areaA.Centroid);
                    var distanceToCentroidB = Vector2Int.Distance(pointB, areaB.Centroid);

                    var totalCentroidDistance = distanceToCentroidA + distanceToCentroidB;

                    // If there are multiple pairs that have equal distance from each other, prefer the ones
                    // that are closer to their area centers.
                    if (Math.Abs(distance - closestDistance) < .001f && totalCentroidDistance > closestCentroidDistance) continue;

                    closestCentroidDistance = totalCentroidDistance;
                    closestDistance = distance;
                    closestPointA = pointA;
                    closestPointB = pointB;
                }
                
                connectedPoints.Add(new Pair<Vector2Int>(closestPointA, closestPointB));
            }

            var connectedPointsArray = connectedPoints.ToArray();
            connectedPointsOut.Set(() => connectedPointsArray);
        }
    }
}