using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(TilemapToTextureNode))]
    public class TilemapToTextureNodeView : ScriptNodeView
    {
        public TilemapToTextureNodeView(TilemapToTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<TilemapToTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(TilemapToTextureNode node) => node.TextureData.ToTexture2D();
    }
}