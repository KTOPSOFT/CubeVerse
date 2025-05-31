using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomBspTreeNode))]
    public class RandomBspTreeNodeView : ScriptNodeView
    {
        public RandomBspTreeNodeView(RandomBspTreeNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomBspTreeNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomBspTreeNode node)
        {
            var texture = Texture2DFactory.CreateDefault(node.Size.x, node.Size.y);
            var leafs = node.BspTree.Leafs;

            foreach (var leaf in leafs)
            {
                var bounds = leaf.Bounds;
                var x = bounds.x;
                var y = bounds.y;
                var width = bounds.width;
                var height = bounds.height;
                    
                var randomColor = new Color32((byte) Random.Range(0, 256), (byte) Random.Range(0, 256), (byte) Random.Range(0, 256), 255);
                var colors = randomColor.CreateArray(width * height);
                
                if (width < 1 || height < 1) continue;
                    
                texture.SetPixels32(x, y, width, height, colors);
            }
                
            texture.Apply();

            return texture;
        }
    }
}