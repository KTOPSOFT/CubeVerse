using System;
using System.Linq;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Gets the center points from the given areas.
    /// </summary>
    [ScriptNode("Areas Centroid Points", "Areas"), Serializable]
    public class AreasCentroidPointsNode : ProcessorNode
    {
        [InPort("Areas", typeof(Area[]), true), SerializeReference] 
        private InPort areasIn = null;
        
        
        [OutPort("Centroid Points", typeof(Vector2Int[])), SerializeReference] 
        private OutPort pointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var areas = areasIn.Get<Area[]>();
            
            var points = areas.Select(area => area.Centroid);
            var pointsArray = points.ToArray();
            
            pointsOut.Set(() => pointsArray);
        }
    }
}