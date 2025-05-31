using System;
using System.Collections.Generic;
using System.Linq;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using QuikGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a random spanning tree area graph from the given areas connected area graph.
    /// </summary>
    [ScriptNode("Generate Area Graph (Random)", "Areas"), Serializable]
    public class GenerateRstAreaGraphNode : ProcessorNode
    { 
        [InPort("Area Graph" ,typeof(AreaGraph), true), SerializeReference] 
        private InPort areaGraphIn = null;
        
        
        [OutPort("Random Area Graph", typeof(AreaGraph)), SerializeReference] 
        private OutPort areaGraphOut = null;

        [OutPort("Unused Edges", typeof(AreaGraphEdge[])), SerializeReference]
        private OutPort unusedEdgesOut = null;      
        
        
        private AreaGraph areaGraph;
        
        /// <summary>
        /// Gets the latest generated MST area graph.
        /// </summary>
        public AreaGraph AreaGraph => areaGraph;
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            var originalAreaGraph = areaGraphIn.Get<AreaGraph>();

            var vertices = instanceProvider.Get<Stack<Area>>();
            var visited = instanceProvider.Get<HashSet<Area>>();

            areaGraph = instanceProvider.Get<AreaGraph>();
            areaGraph.AddVertexRange(originalAreaGraph.Vertices);

            var vertexArray = originalAreaGraph.Vertices.ToArray();

            // Get a random vertex to start creating edges from.
            var firstVertexIndex = rng.Next(0, vertexArray.Length);
            var firstVertex = vertexArray[firstVertexIndex];
            
            // Add it to the stack and mark it as visited.
            vertices.Push(firstVertex);
            visited.Add(firstVertex);

            while (vertices.Count > 0)
            {
                // Get the next vertex to create a edge from.
                var vertex = vertices.Pop();
                
                // Get all the connected edges.
                var edges = originalAreaGraph.AdjacentEdges(vertex).ToArray();
                
                // And randomize them.
                edges.Shuffle(rng);
                
                foreach (var edge in edges)
                {
                    var neighbour = edge.GetOtherVertex(vertex);
                    
                    // Find a neighbour that hasn't been visited yet.
                    if (visited.Contains(neighbour)) continue;
                    
                    // The current vertex is pushed back to the stack so that we'll backtrack to this one once
                    // the next vertices' neighbours have all been visited.
                    vertices.Push(vertex);  
                    
                    // Add the edge between the current vertex and the neighbour.
                    areaGraph.AddEdge(edge);
                    
                    // Mark the neighbour as visited and push it to the stack (also for backtracking purposes).
                    visited.Add(neighbour);
                    vertices.Push(neighbour);
                    break;
                }
            }
            
            areaGraphOut.Set(() => areaGraph);

            if (!unusedEdgesOut.IsConnected) return;
            
            var unusedEdges = originalAreaGraph.Edges.Except(areaGraph.Edges).ToArray();
            unusedEdgesOut.Set(() => unusedEdges);
        }
    }
}