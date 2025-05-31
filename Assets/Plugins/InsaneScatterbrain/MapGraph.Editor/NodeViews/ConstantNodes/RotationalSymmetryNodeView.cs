using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ConstantNodeView(typeof(RotationalSymmetry))]
    public class RotationalSymmetryNodeView : EnumConstantNodeView<RotationalSymmetry>
    {
        public RotationalSymmetryNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
        }
    }
}