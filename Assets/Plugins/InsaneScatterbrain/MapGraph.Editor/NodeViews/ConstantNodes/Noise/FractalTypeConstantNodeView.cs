using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(FractalType))]
    public class FractalTypeNodeView : EnumConstantNodeView<FractalType>
    {
        public FractalTypeNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}