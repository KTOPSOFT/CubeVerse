using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class AreaPool : Pool<Area>
    {
        protected override Area New() => new Area();
        protected override void Reset(Area instance) => instance.Reset();
    }
}