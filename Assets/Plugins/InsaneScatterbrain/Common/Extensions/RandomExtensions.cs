using System;

namespace InsaneScatterbrain.Extensions
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a random float an the range between min. and max.
        /// </summary>
        /// <param name="random">The random instance.</param>
        /// <param name="min">The min. value.</param>
        /// <param name="max">The max. value.</param>
        /// <returns>A random float between min and max</returns>
        public static float Next(this Random random, float min, float max)
        {
            return (float) random.NextDouble() * (max - min) + min;
        }
    }
}