using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Merge Paths", "Paths"), Serializable]
    public class MergePathsNode : ProcessorNode
    {
        [InPort("Paths A", typeof(Vector2Int[][]), true), SerializeReference] 
        private InPort pathsAIn = null;
        
        [InPort("Paths B", typeof(Vector2Int[][]), true), SerializeReference] 
        private InPort pathsBIn = null;


        [OutPort("Paths", typeof(Vector2Int[][])), SerializeReference]
        private OutPort pathsOut = null;
        

        protected override void OnProcess()
        {
            var pathsA = pathsAIn.Get<Vector2Int[][]>();
            var pathsB = pathsBIn.Get<Vector2Int[][]>();

            var mergedPaths = new Vector2Int[pathsA.Length + pathsB.Length][];
            
            Array.Copy(pathsA, mergedPaths, pathsA.Length);
            Array.Copy(pathsB, 0, mergedPaths, pathsA.Length, pathsB.Length);
            
            pathsOut.Set(() => mergedPaths);
        }
    }
}