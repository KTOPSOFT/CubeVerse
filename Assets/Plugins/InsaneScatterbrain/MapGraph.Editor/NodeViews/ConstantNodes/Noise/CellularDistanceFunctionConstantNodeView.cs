using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(FastNoiseLite.CellularDistanceFunction))]
    public class CellularDistanceFunctionConstantNodeView : EnumConstantNodeView<FastNoiseLite.CellularDistanceFunction>
    {
        public CellularDistanceFunctionConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}