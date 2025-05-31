using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ShiftTextureNode))]
    public class ShiftTextureNodeView : ScriptNodeView
    {
        public ShiftTextureNodeView(ShiftTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ShiftTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ShiftTextureNode node) => node.TextureData.ToTexture2D();
    }
}