using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Represents a collection of points that are masked. Usually used to determine whether something is allowed to
    /// be placed on a certain position or not.
    /// </summary>
    public class Mask
    {
        private readonly HashSet<int> unmaskedPoints = new HashSet<int>();
        
        /// <summary>
        /// Gets all the unmasked points.
        /// </summary>
        public IReadOnlyCollection<int> UnmaskedPoints => unmaskedPoints;

        public Mask()
        {
        }

        [Obsolete("Please use the pool manager to get a Mask instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        private Mask(IEnumerable<int> existingUnmaskedPoints)
        {
            Set(existingUnmaskedPoints);
        }
        
        public void Reset()
        {
            unmaskedPoints.Clear();
        }

        public void Set(IEnumerable<int> initialUnmaskedPoints)
        {
            unmaskedPoints.Clear();
            foreach (var unmaskedPoint in initialUnmaskedPoints)
            {
                unmaskedPoints.Add(unmaskedPoint);
            }
        }

        public void Set(int numUnmaskedPoints)
        {
            unmaskedPoints.Clear();
            for (var i = 0; i < numUnmaskedPoints; ++i)
            {
                unmaskedPoints.Add(i);
            }
        }

        /// <summary>
        /// Marks a point as unmasked.
        /// </summary>
        /// <param name="point">The point.</param>
        public void UnmaskPoint(int point)
        {
            unmaskedPoints.Add(point);
        }

        /// <summary>
        /// Marks a point as masked.
        /// </summary>
        /// <param name="point">The point.</param>
        public void MaskPoint(int point)
        {
            unmaskedPoints.Remove(point);
        }
        
        /// <summary>
        /// Creates a clone of this mask.
        /// </summary>
        /// <returns>The cloned mask instance.</returns>
        [Obsolete("Will be removed in version 2.0. Please pass a Mask instance to assign the result to.")]
        public Mask Clone()
        {
            return new Mask(unmaskedPoints);
        }

        /// <summary>
        /// Creates a clone of this mask.
        /// </summary>
        /// <param name="mask">The mask instance to clone into.</param>
        /// <returns>The cloned mask instance.</returns>
        public void Clone(Mask mask) => mask.Set(unmaskedPoints);

        
        /// <summary>
        /// Merges this mask and the other mask together to create a new mask instance.
        /// </summary>
        /// <param name="otherMask">The other mask.</param>
        /// <returns>The new merged mask instance.</returns>
        [Obsolete("Will be removed in version 2.0. Please pass a Mask instance to assign the result to.")]
        public Mask Merge(Mask otherMask)
        {
            var mergedMask = new Mask();
            Merge(otherMask, mergedMask);
            return mergedMask;
        }
        
        /// <summary>
        /// Merges this mask and the other mask together to create a new mask instance.
        /// </summary>
        /// <param name="otherMask">The other mask.</param>
        /// <param name="result">The mask instance that will contain the merged result.</param>
        /// <returns>The new merged mask instance.</returns>
        public void Merge(Mask otherMask, Mask result)
        {
            result.unmaskedPoints.Clear();
            result.unmaskedPoints.AddRange(UnmaskedPoints);
            result.unmaskedPoints.IntersectWith(otherMask.UnmaskedPoints);
        }

        /// <summary>
        /// Checks whether the given point is masked or not according to this mask.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>True if the point is masked, false otherwise.</returns>
        public bool IsPointMasked(int point)
        {
            return !unmaskedPoints.Contains(point);
        }
    }
}