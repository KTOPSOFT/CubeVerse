using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawRectsNode))]
    public class DrawRectsNodeView : ScriptNodeView
    {
        public DrawRectsNodeView(DrawRectsNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawRectsNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawRectsNode node) => node.TextureData.ToTexture2D();
    }
}