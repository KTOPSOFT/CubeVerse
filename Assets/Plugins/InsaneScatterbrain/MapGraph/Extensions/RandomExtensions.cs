using UnityEngine;
using Random = System.Random;

namespace InsaneScatterbrain.MapGraph
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random rng, double minValue, double maxValue)
        {
            return rng.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static float NextFloat(this Random rng)
        {
            return (float) rng.NextDouble();
        }
        
        public static float NextFloat(this Random rng, float minValue, float maxValue)
        {
            return rng.NextFloat() * (maxValue - minValue) + minValue;
        }

        public static Color32 NextColor(this Random rng)
        {
            return new Color32(
                (byte) rng.Next(0, 256), 
                (byte) rng.Next(0, 256), 
                (byte) rng.Next(0, 256), 
                255);
        }
    }
}