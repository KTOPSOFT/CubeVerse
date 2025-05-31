using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ChangeTextureSizeNode))]
    public class ChangeTextureSizeNodeView : ScriptNodeView
    {
        public ChangeTextureSizeNodeView(ChangeTextureSizeNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ChangeTextureSizeNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ChangeTextureSizeNode node) => node.TextureData.ToTexture2D();
    }
}