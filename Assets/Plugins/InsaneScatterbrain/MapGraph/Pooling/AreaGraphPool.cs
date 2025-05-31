using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class AreaGraphPool : Pool<AreaGraph>
    {
        protected override AreaGraph New() => new AreaGraph();
        protected override void Reset(AreaGraph instance) => instance.Clear();
    }
}