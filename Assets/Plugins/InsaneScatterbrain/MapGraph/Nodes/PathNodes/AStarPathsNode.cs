using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    // TODO: square grids are connected in manhattan way, should allow diagonals.
    [ScriptNode("A-star Paths", "Path"), Serializable]
    public class AStarPathsNode : ProcessorNode
    {
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;

        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference] 
        private InPort boundsIn = null;
        
        [InPort("Mask", typeof(Mask)), SerializeReference]
        private InPort maskIn = null;
        
        [InPort("Hex Support?", typeof(bool)), SerializeReference]
        private InPort hexSupportIn = null;

        [InPort("Continue On Failure?", typeof(bool)), SerializeReference]
        private InPort continueOnFailureIn = null;
        
        
        [OutPort("Paths", typeof(Vector2Int[][])), SerializeReference] 
        private OutPort pathsOut = null;
        
        
        public Vector2Int[][] Paths { get; private set; }
        
        public Pair<Vector2Int>[] Connections { get; private set; }
        
        
        private AStarPathfinder pathfinder = new AStarPathfinder();
        private AStarPathfinder Pathfinder => pathfinder ?? (pathfinder = new AStarPathfinder());


        private Vector2Int bounds;

        public Vector2Int Bounds => bounds;
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            Connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var useHexSupport = hexSupportIn.Get<bool>();
            var continueOnFailure = continueOnFailureIn.Get<bool>();
            var mask = maskIn.Get<Mask>();
            bounds = boundsIn.Get<Vector2Int>();
            
            var path = instanceProvider.Get<List<Vector2Int>>();
            var paths = instanceProvider.Get<List<Vector2Int[]>>();

            Func<Vector2Int, bool> isTraversableTest = null;
            if (mask != null)
            {
                isTraversableTest = position =>
                {
                    var i = position.y * bounds.x + position.x;
                    return !mask.IsPointMasked(i);
                };
            }
            
            foreach (var connection in Connections)
            {
                var isPathFound = Pathfinder.CalculatePath(
                    connection.First, 
                    connection.Second, 
                    bounds,
                    path,
                    isTraversableTest,
                    useHexSupport
                );
                
                if (!isPathFound)
                {
                    if (continueOnFailure)
                    {
                        continue;
                    }

                    Debug.LogError("Path not found.");
                    break;
                }
                
                paths.Add(path.ToArray());
            }

            Paths = paths.ToArray();
            pathsOut.Set(() => Paths);
        }
    }
}
