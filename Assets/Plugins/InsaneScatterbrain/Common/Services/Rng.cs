using System;
using System.Threading;
using InsaneScatterbrain.RandomNumberGeneration;

namespace InsaneScatterbrain.Services
{
    /// <summary>
    /// Conveniently wrapped ThreadLocal&lt;Random&gt; class.
    /// </summary>
    public class Rng : Random
    {
        private readonly ThreadLocal<RandomNumberGeneration.Rng> random;

        public RandomNumberGeneration.Rng InnerRng => random.Value;

        public Rng(int seed)
        {
            random = new ThreadLocal<RandomNumberGeneration.Rng>(() => new RandomNumberGeneration.Rng(RngState.FromInt(seed)));
        }

        public Rng(Guid seed)
        {
            random = new ThreadLocal<RandomNumberGeneration.Rng>(() =>
                new RandomNumberGeneration.Rng(RngState.FromBytes(seed.ToByteArray())));
        }

        public Rng()
        {
            random = new ThreadLocal<RandomNumberGeneration.Rng>(() => new RandomNumberGeneration.Rng(RngState.New()));
        }

        public override int Next() => InnerRng.Int();
        
        public override int Next(int maxValue) => InnerRng.Int(maxValue);
        
        public override int Next(int minValue, int maxValue) => InnerRng.Int(minValue, maxValue);

        public override void NextBytes(byte[] buffer) => InnerRng.Bytes(buffer);
    
        public override double NextDouble() => InnerRng.Double();
        
        public double NextDouble(double maxValue) => InnerRng.Double(maxValue);
        
        public double NextDouble(double minValue, double maxValue) => InnerRng.Double(minValue, maxValue);
        
        public bool NextBool() => InnerRng.Bool();
        
        public float NextFloat() => InnerRng.Float();
        
        public float NextFloat(float maxValue) => InnerRng.Float(maxValue);
        
        public float NextFloat(float minValue, float maxValue) => InnerRng.Float(minValue, maxValue);
    }
}