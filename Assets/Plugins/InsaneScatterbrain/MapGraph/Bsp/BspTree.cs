using System;
using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A binary space partitioning tree.
    /// </summary>
    public class BspTree
    {
        private readonly HashSet<BspNode> leafs = new HashSet<BspNode>();
        
        /// <summary>
        /// Gets all the tree's leafs.
        /// </summary>
        public IReadOnlyCollection<BspNode> Leafs => leafs;

        /// <summary>
        /// Gets the tree's root node.
        /// </summary>
        public BspNode Root { get; private set; }
        
        /// <summary>
        /// Creates a new BSP tree for the space represented by the given bounds.
        /// </summary>
        /// <param name="rootBounds">The root node's bounds.</param>
        [Obsolete("Please use the pool manager to get a TextureData instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        public BspTree(RectInt rootBounds)
        {
            Root = new BspNode(this, null, rootBounds);
        }
        
        public BspTree()
        {
        }

        public void Reset()
        {
            leafs.Clear();
            Root = default; 
        }

        /// <summary>
        /// Creates a new BSP tree for the space represented by the given bounds.
        /// </summary>
        /// <param name="rootNode">The node instance to use as root node.</param>
        /// <param name="rootBounds">The root node's bounds.</param>
        public void Set(BspNode rootNode, RectInt rootBounds)
        {
            Root = rootNode;
            Root.Set(this, null, rootBounds);
        }

        /// <summary>
        /// Adds a leaf to the tree.
        /// </summary>
        /// <param name="leaf">The leaf node to add.</param>
        public void AddLeaf(BspNode leaf)
        {
            leafs.Add(leaf);
        }

        /// <summary>
        /// Removes a leaf from the tree.
        /// </summary>
        /// <param name="leaf">The leaf node to remove.</param>
        public void RemoveLeaf(BspNode leaf)
        {
            leafs.Remove(leaf);
        }
    }
}