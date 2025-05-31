using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(FastNoiseLite.CellularReturnType))]
    public class CellularReturnTypeConstantNodeView : EnumConstantNodeView<FastNoiseLite.CellularReturnType>
    {
        public CellularReturnTypeConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}