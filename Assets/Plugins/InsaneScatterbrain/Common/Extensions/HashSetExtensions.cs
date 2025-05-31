using System.Collections.Generic;

namespace InsaneScatterbrain.Extensions
{
    public static class HashSetExtensions
    {
        public static void AddRange<T>(this HashSet<T> stack, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                stack.Add(item);
            }
        }
    }
}