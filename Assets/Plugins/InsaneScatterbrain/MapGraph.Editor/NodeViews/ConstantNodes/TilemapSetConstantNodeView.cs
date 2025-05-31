using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(TilemapSet))]
    public class TilemapSetConstantNodeView : ConstantNodeView
    {
        public TilemapSetConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var texture = node.Value as TilemapSet;
            AddDefaultObjectField(texture);
        }
    }
}