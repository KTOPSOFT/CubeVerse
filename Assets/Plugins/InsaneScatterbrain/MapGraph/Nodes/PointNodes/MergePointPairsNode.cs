using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Merge Point Pairs", "Points"), Serializable]
    public class MergePointPairsNode: ProcessorNode
    {
        [InPort("Point Pairs A", typeof(Pair<Vector2Int>[]), true), SerializeReference]
        private InPort pointPairsAIn = null;
    
        [InPort("Point Pairs B", typeof(Pair<Vector2Int>[]), true), SerializeReference]
        private InPort pointPairsBIn = null;

    
        [OutPort("Point Pairs", typeof(Pair<Vector2Int>[])), SerializeReference]
        private OutPort pointPairsOut = null;

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
        
            var newPairs = instanceProvider.Get<List<Pair<Vector2Int>>>();
        
            var pointPairsA = pointPairsAIn.Get<Pair<Vector2Int>[]>();
            var pointPairsB = pointPairsBIn.Get<Pair<Vector2Int>[]>();
        
            newPairs.AddRange(pointPairsA);
            newPairs.AddRange(pointPairsB);
        
            var newPairsArray = newPairs.ToArray();
            pointPairsOut.Set(() => newPairsArray);
        }
    }
}