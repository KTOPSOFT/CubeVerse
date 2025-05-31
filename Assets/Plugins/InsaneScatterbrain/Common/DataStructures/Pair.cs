using System;
using System.Collections.Generic;

namespace InsaneScatterbrain.DataStructures
{
    /// <summary>
    /// An unordered pair of objects of the same type. Two instance with the first and second values switched
    /// are considered equal.
    /// </summary>
    /// <typeparam name="T">The type of pair.</typeparam>
    public readonly struct Pair<T> : IEquatable<Pair<T>>
    {
        public T First { get; }
        
        public T Second { get; }

        public Pair(T first, T second)
        {
            First = first;
            Second = second;
        }
        
        public bool Equals(Pair<T> other)
        {
            var comparer = EqualityComparer<T>.Default;
            return
                comparer.Equals(First, other.First) && comparer.Equals(Second, other.Second) ||
                comparer.Equals(First, other.Second) && comparer.Equals(Second, other.First);
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return First.GetHashCode() ^ Second.GetHashCode();
        }
    }
}