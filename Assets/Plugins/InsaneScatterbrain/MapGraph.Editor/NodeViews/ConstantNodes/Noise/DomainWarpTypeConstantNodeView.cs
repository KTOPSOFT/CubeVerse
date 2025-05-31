using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(DomainWarpType))]
    public class DomainWarpTypeConstantNodeView : EnumConstantNodeView<DomainWarpType>
    {
        public DomainWarpTypeConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView) { }
    }
}