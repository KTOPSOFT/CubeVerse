using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Pairs random positions of each connected area pair in an area graph.
    /// </summary>
    [ScriptNode("Connect Area Graph (Random)", "Areas"), Serializable]
    public class ConnectAreaGraphRandomPointsNode : ProcessorNode
    {
        [InPort("Area Graph", typeof(AreaGraph), true), SerializeReference] 
        private InPort areaGraphIn = null;

        [OutPort("Connected Points", typeof(Pair<Vector2Int>[])), SerializeReference] 
        private OutPort connectedPointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            var areaGraph = areaGraphIn.Get<AreaGraph>();

            var connectedPoints = instanceProvider.Get<List<Pair<Vector2Int>>>();
            connectedPoints.EnsureCapacity(areaGraph.Edges.Count());
            foreach (var areaPair in areaGraph.Edges)
            {
                var areaA = areaPair.Source;
                var areaB = areaPair.Target;
                
                var randomPointAIndex = rng.Next(0, areaA.Points.Count);
                var randomPointBIndex = rng.Next(0, areaB.Points.Count);

                var randomPointA = areaA.Points[randomPointAIndex];
                var randomPointB = areaB.Points[randomPointBIndex];

                connectedPoints.Add(new Pair<Vector2Int>(randomPointA, randomPointB));
            }

            var connectedPointsArray = connectedPoints.ToArray();
            connectedPointsOut.Set(() => connectedPointsArray);
        }
    }
}