using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RotateTextureNode))]
    public class RotateTextureNodeView : ScriptNodeView
    {
        public RotateTextureNodeView(RotateTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RotateTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RotateTextureNode node) => node.TextureData.ToTexture2D();
    }
}