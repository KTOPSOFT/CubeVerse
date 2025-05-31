using System;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents a node in a BSP tree.
    /// </summary>
    public class BspNode
    {
        private BspTree bspTree;
        private BspNode parent;
        private RectInt bounds;
        private BspNode leftChild;
        private BspNode rightChild;
        private BspNode sibling;
        
        /// <summary>
        /// Gets the bounds of this node.
        /// </summary>
        public RectInt Bounds => bounds;
        
        /// <summary>
        /// Gets the parent node.
        /// </summary>
        public BspNode Parent => parent;
        
        /// <summary>
        /// Gets the left child.
        /// </summary>
        public BspNode LeftChild => leftChild;
        
        /// <summary>
        /// Gets the right child.
        /// </summary>
        public BspNode RightChild => rightChild;
        
        /// <summary>
        /// Gets this node's sibling.
        /// </summary>
        public BspNode Sibling => sibling;
        
        /// <summary>
        /// Creates a new BSP node.
        /// </summary>
        /// <param name="bspTree">The BSP tree.</param>
        /// <param name="parent">The node's parent.</param>
        /// <param name="bounds">The node's bounds.</param>
        [Obsolete("Please use the pool manager to get a BspNode instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        public BspNode(BspTree bspTree, BspNode parent, RectInt bounds)
        {
            this.bspTree = bspTree;
            this.bounds = bounds;
            this.parent = parent;
            
            bspTree.AddLeaf(this);    // This is a leaf because it doesn't have any nodes under it (yet)
        }

        public BspNode()
        {
        }

        /// <summary>
        /// Gets the leaf closest to this node, based on the given point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The closest leaf node.</returns>
        public BspNode GetLeafClosestToPoint(Vector2 point)
        {
            // This is the leaf. Return it.
            if (leftChild == null) return this;
            
            var distanceToLeft = Vector2.Distance(point, leftChild.bounds.center);
            var distanceToRight = Vector2.Distance(point, rightChild.bounds.center);
            
            // Get the child closest to the point and keep recursing through their children until the closest leaf is reached.
            return distanceToLeft < distanceToRight ? leftChild.GetLeafClosestToPoint(point) : rightChild.GetLeafClosestToPoint(point);
        }

        public void Reset()
        {
            bspTree = default;
            bounds = default;
            parent = default;
            leftChild = default;
            rightChild = default;
            sibling = default;
        }

        public void Set(BspTree tree, BspNode parentNode, RectInt leafBounds)
        {
            bspTree = tree;
            bounds = leafBounds;
            parent = parentNode;

            bspTree.AddLeaf(this);    // This is a leaf because it doesn't have any nodes under it (yet)
        }

        /// <summary>
        /// Split the BSP node in two. Creating two new nodes.
        /// </summary>
        /// <param name="splitPoint">The split point between 0 and 1, where .5 is the center.</param>
        /// <param name="splitVertical">If true, the node is split vertically, else horizontally.</param>
        [Obsolete("Will be removed in version 2.0. Please use the version passing the BSP nodes.")]
        public void Split(float splitPoint = .5f, bool splitVertical = false)
        {
            Split(new BspNode(), new BspNode());
        }

        /// <summary>
        /// Split the BSP node in two. Creating two new nodes.
        /// </summary>
        /// <param name="leftChildNode">The node to assign left child data to.</param>
        /// <param name="rightChildNode">The node to assign right child data to.</param>
        /// <param name="splitPoint">The split point between 0 and 1, where .5 is the center.</param>
        /// <param name="splitVertical">If true, the node is split vertically, else horizontally.</param>
        public void Split(BspNode leftChildNode, BspNode rightChildNode, float splitPoint = .5f, bool splitVertical = false)
        {
            leftChild = leftChildNode;
            rightChild = rightChildNode;

            RectInt leftRect;
            RectInt rightRect;
            
            if (splitVertical)
            {
                var leftHeight = Mathf.FloorToInt(bounds.height * splitPoint);
                var rightHeight = bounds.height - leftHeight;
                
                leftRect = new RectInt(bounds.x, bounds.y, bounds.width, leftHeight);
                rightRect = new RectInt(bounds.x, bounds.y + leftHeight, bounds.width, rightHeight);
            }
            else
            {
                var leftWidth = Mathf.FloorToInt(bounds.width * splitPoint);
                var rightWidth = bounds.width - leftWidth;
                
                leftRect = new RectInt(bounds.x, bounds.y, leftWidth, bounds.height);
                rightRect = new RectInt(bounds.x + leftWidth, bounds.y, rightWidth, bounds.height);
            }
            
            leftChild.Set(bspTree, this, leftRect);
            rightChild.Set(bspTree, this, rightRect);

            leftChild.sibling = rightChild;
            rightChild.sibling = leftChild;
            
            bspTree.RemoveLeaf(this);    // This is longer a leaf because it has nodes under it now
        }
    }
}
