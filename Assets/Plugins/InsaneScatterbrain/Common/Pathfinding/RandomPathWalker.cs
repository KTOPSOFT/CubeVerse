using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain
{
    public class RandomPathWalker
    {
        private float bias;
        private float randomness;
        private Vector2Int currentPoint;
        private Vector2Int destination;

        private int width;
        private int height;

        private readonly List<Vector2Int> nextPoints = new List<Vector2Int>(4);
        private readonly SortedList<float, Vector2Int> sortedNextPoints = new SortedList<float, Vector2Int>(new DuplicateKeyComparer<float>());
        private readonly List<Vector2Int> bestOptions = new List<Vector2Int>();
        
        private Rng rng;

        private bool hexCoords;
        
        public void CalculatePath(
            Vector2Int start,
            Vector2Int goal, 
            Vector2Int bounds,
            List<Vector2Int> path, 
            Rng rngInstance,
            float biasValue,
            float randomnessValue,
            bool useHexCoords = false)
        {
            rng = rngInstance;
            currentPoint = start;
            destination = goal;
            width = bounds.x;
            height = bounds.y;
            bias = biasValue;
            randomness = randomnessValue;
            hexCoords = useHexCoords;

            path.Clear();
            path.Add(start);
            
            while (currentPoint != destination)
            {
                currentPoint = Next();
                path.Add(currentPoint);
            }
        }
        
        private float NextPointWeight(Vector2Int point)
        {
            var directionPoint = point - currentPoint;
            var directionDestination = destination - currentPoint;

            var angle = Vector2.Angle(directionPoint, directionDestination);
            return .25f + .75f * bias - angle * bias / 180;
        }

        private bool IsPointValid(Vector2Int point)
        {
            if (point.x < 0 || point.x >= width ||
                point.y < 0 || point.y >= height)
            {
                return false;
            }

            return true;
        }

        private class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
        {
            public int Compare(TKey itemA, TKey itemB)
            {
                var result = itemA.CompareTo(itemB);

                return result == 0 ? 1 : result;
            }
        }
        
        private Vector2Int Next()
        {
            nextPoints.Clear();
            sortedNextPoints.Clear();
            bestOptions.Clear();

            var neighbours = currentPoint.GetRelativeNeighbours(hexCoords);

            foreach (var relativeNeighbour in neighbours)
            {
                var neighbour = currentPoint + relativeNeighbour;

                if (!IsPointValid(neighbour))
                {
                    continue;
                }
                
                nextPoints.Add(neighbour);
            }

            var totalWeight = 0f;

            foreach (var point in nextPoints)
            {
                var pointWeight = NextPointWeight(point);
                sortedNextPoints.Add(pointWeight, point);
                totalWeight += pointWeight;
            }

            var nextPoint = sortedNextPoints.Values[sortedNextPoints.Count - 1];
            var pickBest = rng.NextFloat() > randomness;
            if (pickBest)
            {
                if (sortedNextPoints.Count == 1)
                {
                    return sortedNextPoints.Values[0];
                }

                var highestWeight = sortedNextPoints.Keys[sortedNextPoints.Count - 1];
                bestOptions.Add(sortedNextPoints.Values[sortedNextPoints.Count - 1]);

                for (var i = sortedNextPoints.Count - 2; i >= 0; --i)
                {
                    var pointWeight = sortedNextPoints.Keys[i];
                    if (pointWeight < highestWeight)
                    {
                        break;
                    }
                    
                    var point = sortedNextPoints.Values[i];
                    bestOptions.Add(point);
                }
                
                var randomIndex = rng.Next(0, bestOptions.Count);
                return bestOptions[randomIndex];
            }

            var weight = rng.NextFloat(0, totalWeight);
            var currentWeight = 0f;
            foreach (var sortedNextPoint in sortedNextPoints)
            {
                var pointWeight = sortedNextPoint.Key;
                var point = sortedNextPoint.Value;
            
                currentWeight += pointWeight;

                if (weight > currentWeight) continue;
                
                nextPoint = point;
                break;
            }

            return nextPoint;
        }
    }
}