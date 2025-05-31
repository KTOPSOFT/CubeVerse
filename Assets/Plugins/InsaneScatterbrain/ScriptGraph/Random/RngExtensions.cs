using InsaneScatterbrain.RandomNumberGeneration;

namespace InsaneScatterbrain.ScriptGraph
{
    public static class RngExtensions
    {
        public static RngState State(this Rng rng) => new RngState
        {
            a = rng.UInt(),
            b = rng.UInt(),
            c = rng.UInt(),
            d = rng.UInt()
        };

        public static RngState State(this Services.Rng rng) => rng.InnerRng.State();
        public static void SetState(this Services.Rng rng, RngState state) => rng.InnerRng.SetState(state);
        public static RngState GetState(this Services.Rng rng) => rng.InnerRng.GetState();
    }
}