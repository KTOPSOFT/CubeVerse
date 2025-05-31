using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Merges two arrays of points together into a single set.
    /// </summary>
    [ScriptNode("Merge Points", "Points"), Serializable]
    public class MergePointsNode : ProcessorNode
    {
        [InPort("Points A", typeof(Vector2Int[]), true), SerializeReference] 
        private InPort pointsAIn = null;
        
        [InPort("Points B", typeof(Vector2Int[]), true), SerializeReference]
        private InPort pointsBIn = null;
        
        
        [OutPort("Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort mergedPointsOut = null;
        

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var pointsA = pointsAIn.Get<Vector2Int[]>();
            var pointsB = pointsBIn.Get<Vector2Int[]>();
            
            var points = instanceProvider.Get<HashSet<Vector2Int>>();
            foreach (var point in pointsA)
            {
                points.Add(point);
            }

            foreach (var point in pointsB)
            {
                points.Add(point);
            }

            var pointsArray = points.ToArray();

            mergedPointsOut.Set(() => pointsArray);
        }
    }
}