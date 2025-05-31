using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Unpairs the connected points and returns them in a single set, any duplicates are omitted.
    /// </summary>
    [ScriptNode("Connected Points To Points", "Convert"), Serializable]
    public class ConnectedPointsToPointsNode : ProcessorNode
    {
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectedPointsIn = null;

        [OutPort("Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort pointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var connectedPoints = connectedPointsIn.Get<Pair<Vector2Int>[]>();

            var points = instanceProvider.Get<HashSet<Vector2Int>>();
            foreach (var pair in connectedPoints)
            {
                points.Add(pair.First);
                points.Add(pair.Second);
            }

            var pointsArray = points.ToArray();

            pointsOut.Set(() => pointsArray);
        }
    }
}