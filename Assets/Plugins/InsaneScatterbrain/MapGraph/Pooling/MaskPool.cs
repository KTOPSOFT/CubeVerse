using InsaneScatterbrain.ScriptGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class MaskPool : Pool<Mask>
    {
        protected override Mask New() => new Mask();
        protected override void Reset(Mask instance) => instance.Reset();
    }
}