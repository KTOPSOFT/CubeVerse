using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawConnectionsBiasedRandomWalkNode))]
    public class DrawConnectionsBiasedRandomWalkNodeView : ScriptNodeView
    {
        public DrawConnectionsBiasedRandomWalkNodeView(DrawConnectionsBiasedRandomWalkNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawConnectionsBiasedRandomWalkNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawConnectionsBiasedRandomWalkNode node) => node.TextureData.ToTexture2D();
    }
}