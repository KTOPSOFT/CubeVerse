using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ScaleTextureToNode))]
    public class ScaleTextureToNodeView : ScriptNodeView
    {
        public ScaleTextureToNodeView(ScaleTextureToNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ScaleTextureToNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ScaleTextureToNode node) => node.TextureData.ToTexture2D();
    }
}