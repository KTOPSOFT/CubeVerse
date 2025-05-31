using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using UnityEngine;

namespace InsaneScatterbrain
{
    public class AStarPathfinder
    {
        private readonly HashSet<Vector2Int> openSet = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        private readonly Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        private readonly Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        
        private readonly Vector2Int[] squareNeighbours = {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 1),
            new Vector2Int(-1, 1),
        };
        
        private readonly Vector2Int[] hexNeighboursEven = {
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 1),
        };
        
        private readonly Vector2Int[] hexNeighboursOdd = {
            new Vector2Int(0, -1),
            new Vector2Int(1, -1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
        };

        private void ReconstructPath(Vector2Int current, List<Vector2Int> path)
        {
            path.Add(current);
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
        }

        private bool IsValid(Vector2Int position, Vector2Int bounds, Func<Vector2Int, bool> isTraversableTest = null)
        {
            if (position.x < 0 || position.x > bounds.x - 1 ||
                position.y < 0 || position.y > bounds.y - 1)
            {
                return false;
            }

            if (isTraversableTest != null && !isTraversableTest(position))
            {
                return false;
            }

            return true;
        }
        
        public bool CalculatePath(
            Vector2Int start, 
            Vector2Int goal, 
            Vector2Int bounds,
            List<Vector2Int> path, 
            Func<Vector2Int, bool> isTraversableTest = null,
            bool useHexCoords = false)
        {
            if (!IsValid(start, bounds, isTraversableTest) || 
                !IsValid(goal, bounds, isTraversableTest))
            {
                return false;
            }
            
            path.Clear();

            openSet.Clear();
            closedSet.Clear();
            cameFrom.Clear();
            gScore.Clear();
            fScore.Clear();
            
            gScore[start] = 0;
            fScore[start] = start.ManhattanDistance(goal);

            openSet.Add(start);

            while (openSet.Count > 0)
            {
                var current = new Vector2Int();
                var bestFScore = float.MaxValue;
                
                foreach (var point in openSet)
                {
                    if (fScore[point] >= bestFScore)
                    {
                        continue;
                    }
                    
                    current = point;
                    bestFScore = fScore[point];
                }
                
                if (current == goal)
                {
                    ReconstructPath(current, path);
                    return true;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                var neighbours = current.GetRelativeNeighbours(useHexCoords);
                
                foreach (var relativeNeighbour in neighbours)
                {
                    var neighbour = current + relativeNeighbour;

                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    if (!IsValid(neighbour, bounds, isTraversableTest))
                    {
                        continue;
                    }

                    if (!gScore.ContainsKey(neighbour))
                    {
                        gScore.Add(neighbour, int.MaxValue);
                    }

                    var tentativeGScore = gScore[current] + 1;

                    if (tentativeGScore >= gScore[neighbour])
                    {
                        continue;
                    }
                    
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + neighbour.ManhattanDistance(goal);

                    openSet.Add(neighbour);
                }
            }

            return false;
        }
    }
}
