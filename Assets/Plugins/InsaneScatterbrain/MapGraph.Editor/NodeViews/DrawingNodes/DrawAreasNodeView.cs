using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(DrawAreasNode))]
    public class DrawAreasNodeView : ScriptNodeView
    {
        public DrawAreasNodeView(DrawAreasNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<DrawAreasNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(DrawAreasNode node) => node.TextureData.ToTexture2D();
    }
}