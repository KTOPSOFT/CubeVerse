using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(DomainWarpFractalType))]
    public class DomainWarpFractalTypeNodeView : EnumConstantNodeView<DomainWarpFractalType>
    {
        public DomainWarpFractalTypeNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}