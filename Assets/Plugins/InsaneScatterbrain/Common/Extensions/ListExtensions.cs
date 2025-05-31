using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Randomly shuffles the elements of the list around.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="random">The random instance used to shuffle.</param>
        /// <typeparam name="T">The list type.</typeparam>
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            for (var i = list.Count - 1; i > 0; --i)
            {
                var randomIndex = random.Next(0, i + 1);  
                (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
            }  
        }
        
        public static void Pad<T>(this List<T> list, int count, T val = default)
        {
            list.Clear();
            list.EnsureCapacity(count);

            var numToAdd = count - list.Count;
            if (numToAdd <= 0) return;      // List already has equal or more than the requested number of items.
        
            for (var i = 0; i < numToAdd; ++i)
            {
                list.Add(val);
            }
        }

        public static void EnsureCapacity<T>(this List<T> list, int count)
        {
            if (list.Capacity < count) list.Capacity = count;
        }
    }
}