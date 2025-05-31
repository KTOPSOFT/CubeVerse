using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ApplyMaskNode))]
    public class ApplyMaskNodeView : ScriptNodeView
    {
        public ApplyMaskNodeView(ApplyMaskNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ApplyMaskNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ApplyMaskNode node) => node.TextureData.ToTexture2D();
    }
}