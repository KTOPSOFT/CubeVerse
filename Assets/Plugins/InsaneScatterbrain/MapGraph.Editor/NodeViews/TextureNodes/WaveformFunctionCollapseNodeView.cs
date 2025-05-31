using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(WaveformFunctionCollapseNode))]
    public class WaveformFunctionCollapseNodeView : ScriptNodeView
    {
        public WaveformFunctionCollapseNodeView(WaveformFunctionCollapseNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<WaveformFunctionCollapseNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(WaveformFunctionCollapseNode node) => node.TextureData.ToTexture2D();
    }
}