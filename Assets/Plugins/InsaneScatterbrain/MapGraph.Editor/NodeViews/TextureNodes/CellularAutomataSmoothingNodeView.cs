using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(CellularAutomataSmoothingNode))]
    public class CellularAutomataSmoothingNodeView : ScriptNodeView
    {
        public CellularAutomataSmoothingNodeView(CellularAutomataSmoothingNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<CellularAutomataSmoothingNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(CellularAutomataSmoothingNode node) => node.TextureData.ToTexture2D();
    }
}