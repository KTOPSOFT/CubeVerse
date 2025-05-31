using System;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using QuikGraph.Algorithms;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a minimum spanning tree area graph from the given areas connected area graph.
    /// </summary>
    [ScriptNode("Generate Area Graph (Minimum)", "Areas"), Serializable]
    public class GenerateMstAreaGraphNode : ProcessorNode
    {
        [InPort("Area Graph" ,typeof(AreaGraph), true), SerializeReference] 
        private InPort areaGraphIn = null;
        
        /// <summary>
        /// If true, the precise check is used for determining which areas are closer together. It leads to
        /// more accurate results, but is way more expensive.
        /// </summary>
        [InPort("Precise?", typeof(bool)), SerializeReference] 
        private InPort preciseIn = null;
        
        
        [OutPort("MST Area Graph", typeof(AreaGraph)), SerializeReference] 
        private OutPort areaGraphOut = null;

        [OutPort("Unused Edges", typeof(AreaGraphEdge[])), SerializeReference]
        private OutPort unusedEdgesOut = null;        
        
        private AreaGraph mstAreaGraph;
        
        /// <summary>
        /// Gets the latest generated MST area graph.
        /// </summary>
        public AreaGraph MstAreaGraph => mstAreaGraph;

        /// <summary>
        /// Returns the approximate distance between two areas, by measuring the distance between their centroids.
        /// </summary>
        /// <param name="edge">The edge connecting two areas.</param>
        /// <returns>The distance between the areas centroids.</returns>
        private double ApproximateCheck(AreaGraphEdge edge)
        {
            return Vector2Int.Distance(edge.Source.Centroid, edge.Target.Centroid);
        }

        /// <summary>
        /// Returns the precise distance between two areas, by finding the pair of border points on each area that
        /// are closest to each other.
        /// </summary>
        /// <param name="edge">The edge connecting two areas.</param>
        /// <returns>The closest distance between the areas.</returns>
        private double PreciseCheck(AreaGraphEdge edge)
        {
            var closestDistance = float.MaxValue;
            var closestPointA = new Vector2Int();
            var closestPointB = new Vector2Int();
                
            foreach (var pointA in edge.Source.BorderPoints)
            {
                foreach (var pointB in edge.Target.BorderPoints)
                {
                    var distance = Vector2Int.Distance(pointA, pointB);
                        
                    if (distance > closestDistance) continue;
                        
                    closestDistance = distance;
                    closestPointA = pointA;
                    closestPointB = pointB;
                }
            }
                
            return Vector2Int.Distance(closestPointA, closestPointB);
        }
        
        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var precise = preciseIn.Get<bool>();
            var areaGraph = areaGraphIn.Get<AreaGraph>();

            Func<AreaGraphEdge, double> check = ApproximateCheck;
            if (precise)
            {
                check = PreciseCheck;
            }

            var minimumSpanningTree = areaGraph.MinimumSpanningTreeKruskal(check);

            mstAreaGraph = instanceProvider.Get<AreaGraph>();
            mstAreaGraph.AddVertexRange(areaGraph.Vertices);
            mstAreaGraph.AddEdgeRange(minimumSpanningTree);

            areaGraphOut.Set(() => mstAreaGraph);

            if (!unusedEdgesOut.IsConnected) return;
            
            var unusedEdges = areaGraph.Edges.Except(mstAreaGraph.Edges).ToArray();
            unusedEdgesOut.Set(() => unusedEdges);
        }
    }
}