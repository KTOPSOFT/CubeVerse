using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(CellularAutomataSmoothingNewNode))]
    public class CellularAutomataSmoothingNodeNewView : ScriptNodeView
    {
        public CellularAutomataSmoothingNodeNewView(CellularAutomataSmoothingNewNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            this.AddPreview<CellularAutomataSmoothingNewNode>(GetPreviewTexture);
        }

        private Texture2D GetPreviewTexture(CellularAutomataSmoothingNewNode node) => node.TextureData.ToTexture2D();
    }
}