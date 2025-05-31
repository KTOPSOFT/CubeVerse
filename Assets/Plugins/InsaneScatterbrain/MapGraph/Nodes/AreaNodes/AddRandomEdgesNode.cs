using System;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Takes random edges from the source graph and adds them to the area graph.
    /// </summary>
    [ScriptNode("Add Random Edges", "Areas"), Serializable]
    public class AddRandomEdgesNode : ProcessorNode
    {
        [InPort("Area Graph", typeof(AreaGraph), true), SerializeReference] 
        private InPort areaGraphIn = null;
        
        [InPort("Edges", typeof(AreaGraphEdge[]), true), SerializeReference] 
        private InPort edgesIn = null;
        
        [InPort("Percentage", typeof(float)), SerializeReference] 
        private InPort percentageIn = null;
        
        [InPort("Min. Edges", typeof(int)), SerializeReference]
        private InPort minEdgesIn = null;
        
        [InPort("Max. Edges", typeof(int)), SerializeReference]
        private InPort maxEdgesIn = null;
        
        
        [OutPort("Area Graph", typeof(AreaGraph)), SerializeReference] 
        private OutPort areaGraphOut = null;
        
        
        private AreaGraph areaGraph;
        
        /// <summary>
        /// Gets the latest area graph result.
        /// </summary>
        public AreaGraph AreaGraph => areaGraph;
        

        protected override void OnProcess()
        {
            var rng = Get<Rng>();
            
            areaGraph = areaGraphIn.Get<AreaGraph>().Clone();
            
            var edges = edgesIn.Get<AreaGraphEdge[]>();
            var percentage = percentageIn.Get<float>();
            var minEdges = minEdgesIn.Get<int>();
            var maxEdges = maxEdgesIn.Get<int>();
            
            if (!percentageIn.IsConnected && maxEdgesIn.IsConnected)
            {
                percentage = 100;
            }
            
            if (!percentageIn.IsConnected && !maxEdgesIn.IsConnected && !minEdgesIn.IsConnected)
            {
                Debug.LogWarning("Percentage, Min. Edges and Max. Edges are not connected. This means they will be treated as 0 and the node will never add any edges.");
            }
            else if (maxEdgesIn.IsConnected && minEdges > maxEdges)
            {
                Debug.LogWarning($"Min. Edges ({minEdges}) shouldn't be bigger than Max. Edges ({maxEdges}). Max. Edges takes priority.");
            }
            
            var numOfEdgesToAdd = Mathf.RoundToInt(edges.Length / 100f * percentage);
            if (maxEdgesIn.IsConnected && numOfEdgesToAdd > maxEdges)
            {
                numOfEdgesToAdd = maxEdges;
            }
            else if (minEdgesIn.IsConnected && numOfEdgesToAdd < minEdges)
            {
                if (minEdges > edges.Length)
                {
                    Debug.LogWarning("Min. Edges is smaller than the number of available edges.");
                    numOfEdgesToAdd = edges.Length;
                }
                else
                {
                    numOfEdgesToAdd = minEdges;
                }
            }

            edges.Shuffle(rng);

            for (var i = 0; i < numOfEdgesToAdd; ++i)
            {
                areaGraph.AddEdge(edges[i]);
            }

            areaGraphOut.Set(() => areaGraph);
        }
    }
}