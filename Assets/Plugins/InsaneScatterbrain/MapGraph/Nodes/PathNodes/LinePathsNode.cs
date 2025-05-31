using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Line Paths", "Path"), Serializable]
    public class LinePathsNode : ProcessorNode
    {
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectionsIn = null;

        [InPort("Bounds", typeof(Vector2Int), true), SerializeReference]
        private InPort boundsIn = null;
        
        [InPort("Hex Support?", typeof(bool)), SerializeReference]
        private InPort hexSupportIn = null;
        
        
        [OutPort("Paths", typeof(Vector2Int[][])), SerializeReference] 
        private OutPort pathsOut = null;
        
        
        public Vector2Int[][] Paths { get; private set; }
        
        public Pair<Vector2Int>[] Connections { get; private set; }

        
        private Vector2Int bounds;

        public Vector2Int Bounds => bounds;
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            Connections = connectionsIn.Get<Pair<Vector2Int>[]>();
            var useHexSupport = hexSupportIn.Get<bool>();
            bounds = boundsIn.Get<Vector2Int>();
            
            var path = instanceProvider.Get<List<Vector2Int>>();
            var paths = instanceProvider.Get<List<Vector2Int[]>>();

            foreach (var connection in Connections)
            {
                var start = connection.First;
                var end = connection.Second;

                path.Clear();
                if (useHexSupport)
                {
                    GetPathHex(start, end, path);
                }
                else
                {
                    GetPathSquare(start, end, path);
                }

                paths.Add(path.ToArray());
            }

            Paths = paths.ToArray();
            pathsOut.Set(() => Paths);
        }

        private void GetPathSquare(Vector2Int start, Vector2Int end, List<Vector2Int> path)
        {
            var distance = Vector2Int.Distance(start, end);
            
            for (var i = 0; i <= distance; i++)
            {
                var step = 1f / distance;
                var point = Vector2.Lerp(start, end, step * i);
                path.Add(new Vector2Int(
                    Mathf.RoundToInt(point.x),
                    Mathf.RoundToInt(point.y)
                ));
            }
        }

        private void GetPathHex(Vector2Int start, Vector2Int end, List<Vector2Int> path)
        {
            var startCube = start.SquareToCube();
            var endCube = end.SquareToCube();
            
            var distance = Mathf.FloorToInt(startCube.CubeDistanceTo(endCube));
            
            for (var i = 0; i <= distance; i++)
            {
                var step = 1f / distance;

                var cube = Vector3.Lerp(startCube, endCube, step * i).CubeRoundToInt();
                var squarePoint = cube.CubeToSquare();
                
                // Sometimes a path is calculated that runs outside the bounds in hex space, so we clamp it inside.
                // This should still be fine as this should only occur when the start and/or end points are right at
                // the edge and having the point inside the bounds instead of outside, should still create a valid
                // path.
                squarePoint.Clamp(Vector2Int.zero, bounds - Vector2Int.one);
                
                path.Add(squarePoint);
            }
        }
    }
}
