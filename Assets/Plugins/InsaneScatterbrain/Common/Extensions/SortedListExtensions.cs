using System.Collections.Generic;

namespace InsaneScatterbrain.Extensions
{
    public static class SortedListExtensions
    {
        public static void EnsureCapacity<T1,T2>(this SortedList<T1,T2> list, int count)
        {
            if (list.Capacity < count) list.Capacity = count;
        }
    }
}