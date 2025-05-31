using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawCirclesNode))]
    public class DrawCirclesNodeView : ScriptNodeView
    {
        public DrawCirclesNodeView(DrawCirclesNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawCirclesNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawCirclesNode node) => node.TextureData.ToTexture2D();
    }
}