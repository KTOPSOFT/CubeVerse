using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(StampNode))]
    public class StampNodeView : ScriptNodeView
    {
        public StampNodeView(StampNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<StampNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(StampNode node) => node.TextureData.ToTexture2D();
    }
}