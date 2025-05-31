using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawGridNode))]
    public class DrawGridNodeView : ScriptNodeView
    {
        public DrawGridNodeView(DrawGridNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawGridNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawGridNode node) => node.TextureData.ToTexture2D();
    }
}