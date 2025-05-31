using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class AreaGraphEdgePool : Pool<AreaGraphEdge>
    {
        protected override AreaGraphEdge New() => new AreaGraphEdge();
        protected override void Reset(AreaGraphEdge instance) => instance.Set(default, default);
    }
}