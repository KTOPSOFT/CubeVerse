using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain
{
    public class Color32EqualityComparer : IEqualityComparer<Color32>
    {
        public bool Equals(Color32 x, Color32 y)
        {
            return x.r == y.r && x.g == y.g && x.b == y.b && x.a == y.a;
        }
        
        public int GetHashCode(Color32 obj)
        {
            unchecked
            {
                var hashCode = obj.r.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.g.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.b.GetHashCode();
                hashCode = (hashCode * 397) ^ obj.a.GetHashCode();
                return hashCode;
            }
        }
    }
}