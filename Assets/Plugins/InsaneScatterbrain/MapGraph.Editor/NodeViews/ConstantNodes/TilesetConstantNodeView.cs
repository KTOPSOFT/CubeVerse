using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(Tileset))]
    public class TilesetConstantNodeView : ConstantNodeView
    {
        public TilesetConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var tileset = node.Value as Tileset;
            AddDefaultObjectField(tileset);
        }
    }
} 