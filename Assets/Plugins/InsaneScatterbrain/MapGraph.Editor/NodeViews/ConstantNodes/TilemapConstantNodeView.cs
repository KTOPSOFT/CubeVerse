using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(Tilemap))]
    public class TilemapConstantNodeView : ConstantNodeView
    {
        public TilemapConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var tilemap = node.Value as Tilemap;
            AddDefaultObjectField(tilemap);
        }
    }
}