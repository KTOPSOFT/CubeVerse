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
    /// Pairs the centroid positions of each connected area pair in an area graph.
    /// </summary>
    [ScriptNode("Connect Area Graph (Centroids)", "Areas"), Serializable]
    public class ConnectAreaGraphCentroidsNode : ProcessorNode
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

                connectedPoints.Add(new Pair<Vector2Int>(areaA.Centroid, areaB.Centroid));
            }

            var connectedPointsArray = connectedPoints.ToArray();
            connectedPointsOut.Set(() => connectedPointsArray);
        }
    }
}