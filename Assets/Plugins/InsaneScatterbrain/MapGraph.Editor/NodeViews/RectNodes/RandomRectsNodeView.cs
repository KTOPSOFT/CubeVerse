using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomRectsNode))]
    public class RandomRectsNodeView : ScriptNodeView
    {
        public RandomRectsNodeView(RandomRectsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomRectsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomRectsNode node)
        {
            var width = node.Bounds.x;
            var height = node.Bounds.y;

            var texture = Texture2DFactory.CreateDefault(width, height);
            texture.Fill(Color.black);

            foreach (var rect in node.Rects)
            { 
                var colors = Color.white.CreateArray(rect.width * rect.height);

                texture.SetPixels(rect.x, rect.y, rect.width, rect.height, colors);
            }
                
            texture.Apply();

            return texture;
        }
    }
}