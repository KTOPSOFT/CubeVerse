using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Gets the center points for each BSP leaf node.
    /// </summary>
    [ScriptNode("BSP Leaf Center Points", "BSP"), Serializable]
    public class BspLeafCenterPointsNode : ProcessorNode
    {
        [InPort("BSP Tree", typeof(BspTree), true), SerializeReference] 
        private InPort bspTreeIn = null;
        
        [OutPort("Center Points", typeof(Vector2Int[])), SerializeReference] 
        private OutPort centerPointsOut = null;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var bspTree = bspTreeIn.Get<BspTree>();
            var centerPoints = instanceProvider.Get<List<Vector2Int>>();
            centerPoints.EnsureCapacity(bspTree.Leafs.Count);
            
            foreach (var leaf in bspTree.Leafs)
            {
                var center = leaf.Bounds.center;
                var centerInt = new Vector2Int(
                    Mathf.FloorToInt(center.x), 
                    Mathf.FloorToInt(center.y));
                centerPoints.Add(centerInt);
            }

            var centerPointsArray = centerPoints.ToArray();
            centerPointsOut.Set(() => centerPointsArray);
        }
    }
}