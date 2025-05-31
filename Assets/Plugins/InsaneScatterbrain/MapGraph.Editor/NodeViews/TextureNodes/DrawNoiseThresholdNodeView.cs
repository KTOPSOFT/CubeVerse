using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawNoiseThresholdNode))]
    public class DrawNoiseThresholdNodeView : ScriptNodeView
    {
        public DrawNoiseThresholdNodeView(DrawNoiseThresholdNode drawNoiseThresholdNode, ScriptGraphView graphView) : base(drawNoiseThresholdNode, graphView)
        {
            this.AddPreview<DrawNoiseThresholdNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawNoiseThresholdNode node) => node.TextureData.ToTexture2D();
    }
}