using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawOutlineNode))]
    public class DrawOutlineNodeView : ScriptNodeView
    {
        public DrawOutlineNodeView(DrawOutlineNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawOutlineNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawOutlineNode node) => node.TextureData.ToTexture2D();
    }
}