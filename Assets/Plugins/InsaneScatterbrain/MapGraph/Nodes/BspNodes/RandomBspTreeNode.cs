using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates a random binary space partitioning tree.
    /// </summary>
    [ScriptNode("Random BSP Tree", "BSP"), Serializable]
    public class RandomBspTreeNode : ProcessorNode
    {
        [InPort("Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort sizeIn = null;
        
        /// <summary>
        /// The minimum number of times leaf nodes are split.
        /// </summary>
        [InPort("Min. Number of Divisions", typeof(int)), SerializeReference] 
        private InPort minNumDivisionsIn = null;
        
        /// <summary>
        /// The maximum number of times leaf nodes are split.
        /// </summary>
        [InPort("Max. Number of Divisions", typeof(int), true), SerializeReference] 
        private InPort maxNumDivisionsIn = null;
        
        /// <summary>
        /// The minimum percentage of space each division should take up of its parent node.
        /// </summary>
        [InPort("Min. Divide Offset Percentage", typeof(float)), SerializeReference] 
        private InPort minDivideOffsetPercentageIn = null;
        

        [OutPort("BSP Tree", typeof(BspTree)), SerializeReference] 
        private OutPort treeOut = null;
        

        private BspTree bspTree;
        
        /// <summary>
        /// Gets the latest generated BSP tree.
        /// </summary>
        public BspTree BspTree => bspTree;

        private Vector2Int size;
        
        /// <summary>
        /// Gets the space size.
        /// </summary>
        public Vector2Int Size => size;

        /// <inheritdoc cref="ProcessorNode.OnProcess"/>
        protected override void OnProcess()
        {
            var rng = Get<Rng>();
            var instanceProvider = Get<IInstanceProvider>();
            
            var minNumDivisions = minNumDivisionsIn.Get<int>();
            var maxNumDivisions = maxNumDivisionsIn.Get<int>();
            
            size = sizeIn.Get<Vector2Int>();
            var minSpacePercentage = minDivideOffsetPercentageIn.Get<float>();
            
            var minSpaceRatio = minSpacePercentage / 100f;
            var maxSpaceRatio = 1f - minSpaceRatio;

            // The number of times nodes are split.
            var numDivisions = rng.Next(minNumDivisions, maxNumDivisions + 1);

            var rootNode = instanceProvider.Get<BspNode>();
            bspTree = instanceProvider.Get<BspTree>();
            bspTree.Set(rootNode, new RectInt(0, 0, size.x, size.y));

            var leafs = instanceProvider.Get<List<BspNode>>();

            for (var level = 0; level < numDivisions; ++level)
            {
                leafs.Clear();
                leafs.AddRange(bspTree.Leafs);
                
                foreach (var leaf in leafs)
                {
                    var splitPoint = rng.Next(minSpaceRatio, maxSpaceRatio);
                    
                    // Do a vertical split if the height is bigger than the width, horizontal otherwise. This leads
                    // to "nicer" results.
                    var verticalSplit = leaf.Bounds.height > leaf.Bounds.width;

                    var leftChild = instanceProvider.Get<BspNode>();
                    var rightChild = instanceProvider.Get<BspNode>();
                    leaf.Split(leftChild, rightChild, splitPoint, verticalSplit);
                }
            }
            
            treeOut.Set(() => bspTree);
        }
    }
}