using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(ScaleTextureNode))]
    public class ScaleTextureNodeView : ScriptNodeView
    {
        public ScaleTextureNodeView(ScaleTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<ScaleTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(ScaleTextureNode node) => node.TextureData.ToTexture2D();
    }
}