using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Random Walker Paths", "Path"), Serializable]
    public class RandomWalkerPathsNode : ProcessorNode
    {
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;

        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference]
        private InPort boundsIn = null;
        
        [InPort("Direction Bias (%)", typeof(float)), SerializeReference]
        private InPort biasIn = null;

        [InPort("Randomness (%)", typeof(float)), SerializeReference]
        private InPort randomnessIn = null;
        
        [InPort("Hex Support?", typeof(bool)), SerializeReference]
        private InPort hexSupportIn = null;
        
        
        [OutPort("Paths", typeof(Vector2Int[][])), SerializeReference] 
        private OutPort pathsOut = null;
        
        
        public Vector2Int[][] Paths { get; private set; }
        public Pair<Vector2Int>[] Connections { get; private set; }
        
        private Vector2Int bounds;
        public Vector2Int Bounds => bounds;

        private RandomPathWalker randomPathWalker = new RandomPathWalker();
        private RandomPathWalker RandomPathWalker => randomPathWalker ?? (randomPathWalker = new RandomPathWalker());
        
        protected override void OnProcess()
        {
            var rng = Get<Rng>();
            var instanceProvider = Get<IInstanceProvider>();
            var paths = instanceProvider.Get<List<Vector2Int[]>>();
            
            Connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var bias = biasIn.IsConnected ? biasIn.Get<float>() / 100 : .5f;
            var randomness = randomnessIn.IsConnected ? randomnessIn.Get<float>() / 100 : 1f;
            var useHexSupport = hexSupportIn.Get<bool>();
            
            bounds = boundsIn.Get<Vector2Int>();

            var path = instanceProvider.Get<List<Vector2Int>>();
            
            foreach (var connection in Connections)
            {
                RandomPathWalker.CalculatePath(
                    connection.First,
                    connection.Second,
                    bounds,
                    path,
                    rng,
                    bias,
                    randomness,
                    useHexSupport
                );
                
                paths.Add(path.ToArray());
            }
            
            Paths = paths.ToArray();
            pathsOut.Set(() => Paths);
        }
    }
}
