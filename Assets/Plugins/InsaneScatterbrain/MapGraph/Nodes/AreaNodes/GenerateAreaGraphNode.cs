using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a connected area graph from the given areas.
    /// </summary>
    [ScriptNode("Generate Area Graph", "Areas"), Serializable]
    public class GenerateAreaGraphNode : ProcessorNode
    {
        [InPort("Areas", typeof(Area[]), true), SerializeReference] 
        private InPort areasIn = null;
        
        [OutPort("Area Graph", typeof(AreaGraph)), SerializeReference] 
        private OutPort areaGraphOut = null;
        
        private AreaGraph areaGraph;
        
        /// <summary>
        /// Gets the latest generated area graph.
        /// </summary>
        public AreaGraph AreaGraph => areaGraph;

        private AreaGraphBuilder graphBuilder;
        
        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var areas = areasIn.Get<Area[]>();
            var pools = Get<IInstanceProvider>();

            if (graphBuilder == null)
            {
                graphBuilder = new AreaGraphBuilder(pools.Get<AreaGraphEdge>);
            }

            if (areaGraph == null)
            {
                areaGraph = new AreaGraph();
            }
            
            graphBuilder.BuildGraph(areas, areaGraph);

            areaGraphOut.Set(() => areaGraph);
        }
    }
}