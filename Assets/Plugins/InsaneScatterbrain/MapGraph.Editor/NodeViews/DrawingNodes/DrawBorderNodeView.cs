using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawBorderNode))]
    public class DrawBorderNodeView : ScriptNodeView
    {
        public DrawBorderNodeView(DrawBorderNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawBorderNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawBorderNode node) => node.TextureData.ToTexture2D();
    }
}