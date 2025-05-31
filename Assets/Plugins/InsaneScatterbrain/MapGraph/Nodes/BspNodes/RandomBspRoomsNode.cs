using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a room/rectangle within the bounds of each BSP leaf node.
    /// </summary>
    [ScriptNode("Random BSP Rooms", "BSP"), Serializable]
    public class RandomBspRoomsNode : ProcessorNode
    {
        [InPort("BSP Tree", typeof(BspTree), true), SerializeReference] 
        private InPort bspTreeIn = null;
        
        /// <summary>
        /// Rooms minimum distance from node bounds.
        /// </summary>
        [InPort("Margin", typeof(int)), SerializeReference] 
        private InPort marginIn = null;
        
        /// <summary>
        /// Minimum percentage the generated rooms should cover of the node leaf bounds.
        /// </summary>
        [InPort("Min. Room To Node Ratio", typeof(float)), SerializeReference] 
        private InPort minRoomNodeRatioIn = null;
        
        /// <summary>
        /// Maximum percentage the generated rooms should cover of the node leaf bounds.
        /// </summary>
        [InPort("Max. Room To Node Ratio", typeof(float), true), SerializeReference] 
        private InPort maxRoomNodeRatioIn = null;

        
        [OutPort("Rectangles", typeof(RectInt[])), SerializeReference] 
        private OutPort boundsOut = null; 

        
        private List<RectInt> bounds;
        public List<RectInt> Bounds => bounds;

        private BspTree tree;
        public BspTree Tree => tree;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var rng = Get<Rng>();
            
            tree = bspTreeIn.Get<BspTree>();
            
            var margin = marginIn.Get<int>();
            var minRoomNodeRatio = minRoomNodeRatioIn.Get<float>();
            var maxRoomNodeRatio = maxRoomNodeRatioIn.Get<float>();
            
            var minRoomRatio = minRoomNodeRatio / 100f;
            var maxRoomRatio = maxRoomNodeRatio / 100f;

            bounds = instanceProvider.Get<List<RectInt>>();
            bounds.EnsureCapacity(tree.Leafs.Count);
            
            foreach (var leaf in tree.Leafs)
            {
                var nodeBounds = leaf.Bounds;

                var possibleRoomWidth = nodeBounds.width - margin * 2;
                var possibleRoomHeight = nodeBounds.height - margin * 2;

                // Can't place a room here, it's doesn't fit.
                if (possibleRoomWidth < 1 || possibleRoomHeight < 1) continue;
                
                var widthRatio = rng.Next(minRoomRatio, maxRoomRatio);
                var heightRatio = rng.Next(minRoomRatio, maxRoomRatio);
                var width = Mathf.RoundToInt(possibleRoomWidth * widthRatio);
                var height = Mathf.RoundToInt(possibleRoomHeight * heightRatio);

                // If the resulting width or height is too small, give it a minimum of 1.
                if (width < 1) width = 1;
                if (height < 1) height = 1;

                var xMin = nodeBounds.x + margin;
                var xMax = nodeBounds.x + nodeBounds.width - margin - width;
                var x = rng.Next(xMin, xMax);
                
                var yMin = nodeBounds.y + margin;
                var yMax = nodeBounds.y + nodeBounds.height - margin - height;
                var y = rng.Next(yMin, yMax);
                
                var roomBounds = new RectInt(
                    Mathf.RoundToInt(x), Mathf.RoundToInt(y), 
                    width, height);

                bounds.Add(roomBounds);
            }

            var boundsArray = bounds.ToArray();
            
            boundsOut.Set(() => boundsArray);
        }
    }
}