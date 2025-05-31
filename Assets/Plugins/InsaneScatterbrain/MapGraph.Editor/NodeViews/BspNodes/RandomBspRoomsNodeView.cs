using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomBspRoomsNode))]
    public class RandomBspRoomsNodeView : ScriptNodeView
    {
        public RandomBspRoomsNodeView(RandomBspRoomsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomBspRoomsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomBspRoomsNode node)
        {
            var rootBounds = node.Tree.Root.Bounds;

            var texture = Texture2DFactory.CreateDefault(rootBounds.width, rootBounds.height);
            texture.Fill(Color.black);
                
            var roomBounds = node.Bounds;
            texture.DrawRects(roomBounds, Color.white);
            texture.Apply();

            return texture;
        }
    }
}