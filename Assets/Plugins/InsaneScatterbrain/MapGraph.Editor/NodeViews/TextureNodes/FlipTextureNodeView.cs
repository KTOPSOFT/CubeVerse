using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(FlipTextureNode))]
    public class FlipTextureNodeView : ScriptNodeView
    {
        public FlipTextureNodeView(FlipTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<FlipTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(FlipTextureNode node) => node.TextureData.ToTexture2D();
    }
}