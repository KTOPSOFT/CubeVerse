using System;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Split Into Manhattan Connections", "Areas"), Serializable]
    public class SplitConnectionsManhattanNode : ProcessorNode
    {
        [InPort("Connected Points", typeof(Pair<Vector2Int>[]), true), SerializeReference] 
        private InPort connectedPointsIn = null;


        [OutPort("Manhattan Connected Points", typeof(Pair<Vector2Int>[])), SerializeReference]
        private OutPort manhattanConnectionsOut = null;
        
        [OutPort("Horizontal Connections", typeof(Pair<Vector2Int>[])), SerializeReference]
        private OutPort horizontalConnectionsOut = null;
        
        [OutPort("Vertical Connections", typeof(Pair<Vector2Int>[])), SerializeReference]
        private OutPort verticalConnectionsOut = null;


        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            var connections = connectedPointsIn.Get<Pair<Vector2Int>[]>();

            var manhattanConnections = instanceProvider.Get<List<Pair<Vector2Int>>>();
            var horizontalConnections = instanceProvider.Get<List<Pair<Vector2Int>>>();
            var verticalConnections = instanceProvider.Get<List<Pair<Vector2Int>>>();
            
            manhattanConnections.EnsureCapacity(connections.Length * 2);
            horizontalConnections.EnsureCapacity(connections.Length);
            verticalConnections.EnsureCapacity(connections.Length);
            
            foreach (var connection in connections)
            {
                var start = connection.First;
                var end = connection.Second;

                if (start.x == end.x)
                {
                    manhattanConnections.Add(connection);
                    verticalConnections.Add(connection);
                    continue;
                }
                
                if (start.y == end.y)
                {
                    manhattanConnections.Add(connection);
                    horizontalConnections.Add(connection);
                    continue;
                }

                // Otherwise we need to lines to connect the two points.
                Vector2Int cornerPoint;
                var horizontalPoint = new Vector2Int();
                var verticalPoint = new Vector2Int();

                var randomValue = rng.NextDouble();

                /*
                 * There are two possible points to draw lines from/to to create a connection.
                 *
                 * We pick one at random for more varied results
                 * Example:
                 *
                 * 1-----B
                 * |     |
                 * |     |
                 * A-----2
                 *
                 * If we want to connect A to B using only horizontal or vertical lines. We have to connect
                 * both A and B to either point 1 or point 2.
                 */
                if (randomValue > .5f)
                {
                    cornerPoint = new Vector2Int(start.x, end.y);
                    horizontalPoint.x = end.x;
                    horizontalPoint.y = end.y;
                    verticalPoint.x = start.x;
                    verticalPoint.y = start.y;
                }
                else
                {
                    cornerPoint = new Vector2Int(end.x, start.y);
                    horizontalPoint.x = start.x;
                    horizontalPoint.y = start.y;
                    verticalPoint.x = end.x;
                    verticalPoint.y = end.y;
                }
                
                var horizontalConnection = new Pair<Vector2Int>(horizontalPoint, cornerPoint);
                var verticalConnection = new Pair<Vector2Int>(verticalPoint, cornerPoint);
                
                manhattanConnections.Add(horizontalConnection);
                manhattanConnections.Add(verticalConnection);
                
                horizontalConnections.Add(horizontalConnection);
                verticalConnections.Add(verticalConnection);
            }
            
            var manhattanConnectionsArray = manhattanConnections.ToArray();
            var horizontalConnectionsArray = horizontalConnections.ToArray();
            var verticalConnectionsArray = verticalConnections.ToArray();

            manhattanConnectionsOut.Set(() => manhattanConnectionsArray);
            horizontalConnectionsOut.Set(() => horizontalConnectionsArray);
            verticalConnectionsOut.Set(() => verticalConnectionsArray);
        }
    }
}