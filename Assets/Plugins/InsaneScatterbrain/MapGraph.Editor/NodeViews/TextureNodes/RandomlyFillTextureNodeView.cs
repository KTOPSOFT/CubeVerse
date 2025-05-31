using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(RandomlyFillTextureNode))]
    public class RandomlyFillTextureNodeView : ScriptNodeView
    {
        public RandomlyFillTextureNodeView(RandomlyFillTextureNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<RandomlyFillTextureNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(RandomlyFillTextureNode node) => node.TextureData.ToTexture2D();
    }
}