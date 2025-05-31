using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ResizeTextureNode))]
    public class ResizeTextureNodeView : ScriptNodeView
    {
        public ResizeTextureNodeView(ResizeTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ResizeTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ResizeTextureNode node) => node.TextureData.ToTexture2D();
    }
}