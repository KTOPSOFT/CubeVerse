/*
 * This file contains a modified version of the Delaunator class of the DelaunatorSharp project. The modifications
 * made or to reduce the number of allocations that are made, to avoid triggering the garbage collector.
 *
 * The license and copyright notice of DelaunatorSharp can be found in the ThirdPartyLibraries directory.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using InsaneScatterbrain.Extensions;

namespace InsaneScatterbrain.DelaunatorSharp
{
    public class DelaunatorNoAlloc
    {
        private readonly double EPSILON = Math.Pow(2, -52);
        private readonly int[] EDGE_STACK = new int[512];

        private readonly List<int> triangles = new List<int>();
        private readonly List<int> halfEdges = new List<int>();

        private int hashSize;

        private double cx;
        private double cy;

        private int trianglesLen;
        private int hullStart;
        private int hullSize;
        
        private readonly List<int> hullPrev = new List<int>();
        private readonly List<int> hullNext = new List<int>();
        private readonly List<int> hullTri = new List<int>();
        private readonly List<int> hullHash = new List<int>();
        private readonly List<int> hull = new List<int>();
        private readonly List<int> ids = new List<int>();
         
        private readonly List<double> coords = new List<double>();
        private readonly List<double> dists = new List<double>();

        private readonly List<IPoint> points = new List<IPoint>();

        public void Triangulate(IEnumerable<IPoint> pointsToTriangulate)
        {
            points.Clear();
            points.AddRange(pointsToTriangulate);
            
            if (points.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(pointsToTriangulate), "Need at least 3 points");
            }
            
            coords.Pad(points.Count * 2);

            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i];
                coords[2 * i] = p.X;
                coords[2 * i + 1] = p.Y;
            }

            var n = points.Count;
            var maxTriangles = 2 * n - 5;

            var trianglesCount = maxTriangles * 3;
            triangles.Pad(trianglesCount);
            halfEdges.Pad(trianglesCount);

            hashSize = (int)Math.Ceiling(Math.Sqrt(n));

            hullPrev.Pad(n);
            hullNext.Pad(n);
            hullTri.Pad(n);
            
            hullHash.Pad(hashSize);
            
            ids.Pad(n);

            var minX = double.PositiveInfinity;
            var minY = double.PositiveInfinity;
            var maxX = double.NegativeInfinity;
            var maxY = double.NegativeInfinity;

            for (var i = 0; i < n; i++)
            {
                var x = coords[2 * i];
                var y = coords[2 * i + 1];
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
                ids[i] = i;
            }

            var centerX = (minX + maxX) / 2;
            var centerY = (minY + maxY) / 2;

            var minDist = double.PositiveInfinity;
            var i0 = 0;
            var i1 = 0;
            var i2 = 0;

            // pick a seed point close to the center
            for (var i = 0; i < n; i++)
            {
                var d = Dist(centerX, centerY, coords[2 * i], coords[2 * i + 1]);
                if (d < minDist)
                {
                    i0 = i;
                    minDist = d;
                }
            }

            var i0x = coords[2 * i0];
            var i0y = coords[2 * i0 + 1];

            minDist = double.PositiveInfinity;

            // find the point closest to the seed
            for (int i = 0; i < n; i++)
            {
                if (i == i0) continue;
                var d = Dist(i0x, i0y, coords[2 * i], coords[2 * i + 1]);
                if (d < minDist && d > 0)
                {
                    i1 = i;
                    minDist = d;
                }
            }

            var i1x = coords[2 * i1];
            var i1y = coords[2 * i1 + 1];

            var minRadius = double.PositiveInfinity;

            // find the third point which forms the smallest circumcircle with the first two
            for (int i = 0; i < n; i++)
            {
                if (i == i0 || i == i1) continue;
                var r = Circumradius(i0x, i0y, i1x, i1y, coords[2 * i], coords[2 * i + 1]);
                if (r < minRadius)
                {
                    i2 = i;
                    minRadius = r;
                }
            }

            var i2x = coords[2 * i2];
            var i2y = coords[2 * i2 + 1];

            if (double.IsPositiveInfinity(minRadius))
            {
                throw new Exception("No Delaunay triangulation exists for this input.");
            }

            if (Orient(i0x, i0y, i1x, i1y, i2x, i2y))
            {
                var i = i1;
                var x = i1x;
                var y = i1y;
                i1 = i2;
                i1x = i2x;
                i1y = i2y;
                i2 = i;
                i2x = x;
                i2y = y;
            }

            var center = Circumcenter(i0x, i0y, i1x, i1y, i2x, i2y);
            cx = center.X;
            cy = center.Y;
            
            dists.Pad(n);
            for (var i = 0; i < n; i++)
            {
                dists[i] = Dist(coords[2 * i], coords[2 * i + 1], center.X, center.Y);
            }

            // sort the points by distance from the seed triangle circumcenter
            Quicksort(ids, dists, 0, n - 1);

            // set up the seed triangle as the starting hull
            hullStart = i0;
            hullSize = 3;

            hullNext[i0] = hullPrev[i2] = i1;
            hullNext[i1] = hullPrev[i0] = i2;
            hullNext[i2] = hullPrev[i1] = i0;

            hullTri[i0] = 0;
            hullTri[i1] = 1;
            hullTri[i2] = 2;

            hullHash[HashKey(i0x, i0y)] = i0;
            hullHash[HashKey(i1x, i1y)] = i1;
            hullHash[HashKey(i2x, i2y)] = i2;

            trianglesLen = 0;
            AddTriangle(i0, i1, i2, -1, -1, -1);

            double xp = 0;
            double yp = 0;

            for (var k = 0; k < ids.Count; k++)
            {
                var i = ids[k];
                var x = coords[2 * i];
                var y = coords[2 * i + 1];

                // skip near-duplicate points
                if (k > 0 && Math.Abs(x - xp) <= EPSILON && Math.Abs(y - yp) <= EPSILON) continue;
                xp = x;
                yp = y;

                // skip seed triangle points
                if (i == i0 || i == i1 || i == i2) continue;

                // find a visible edge on the convex hull using edge hash
                var start = 0;
                for (var j = 0; j < hashSize; j++)
                {
                    var key = HashKey(x, y);
                    start = hullHash[(key + j) % hashSize];
                    if (start != -1 && start != hullNext[start]) break;
                }


                start = hullPrev[start];
                var e = start;
                var q = hullNext[e];

                while (!Orient(x, y, coords[2 * e], coords[2 * e + 1], coords[2 * q], coords[2 * q + 1]))
                {
                    e = q;
                    if (e == start)
                    {
                        e = int.MaxValue;
                        break;
                    }

                    q = hullNext[e];
                }

                if (e == int.MaxValue) continue; // likely a near-duplicate point; skip it

                // add the first triangle from the point
                var t = AddTriangle(e, i, hullNext[e], -1, -1, hullTri[e]);

                // recursively flip triangles from the point until they satisfy the Delaunay condition
                hullTri[i] = Legalize(t + 2);
                hullTri[e] = t; // keep track of boundary triangles on the hull
                hullSize++;

                // walk forward through the hull, adding more triangles and flipping recursively
                var next = hullNext[e];
                q = hullNext[next];

                while (Orient(x, y, coords[2 * next], coords[2 * next + 1], coords[2 * q], coords[2 * q + 1]))
                {
                    t = AddTriangle(next, i, q, hullTri[i], -1, hullTri[next]);
                    hullTri[i] = Legalize(t + 2);
                    hullNext[next] = next; // mark as removed
                    hullSize--;
                    next = q;

                    q = hullNext[next];
                }

                // walk backward from the other side, adding more triangles and flipping
                if (e == start)
                {
                    q = hullPrev[e];

                    while (Orient(x, y, coords[2 * q], coords[2 * q + 1], coords[2 * e], coords[2 * e + 1]))
                    {
                        t = AddTriangle(q, i, e, -1, hullTri[e], hullTri[q]);
                        Legalize(t + 2);
                        hullTri[q] = t;
                        hullNext[e] = e; // mark as removed
                        hullSize--;
                        e = q;

                        q = hullPrev[e];
                    }
                }

                // update the hull indices
                hullStart = hullPrev[i] = e;
                hullNext[e] = hullPrev[next] = i;
                hullNext[i] = next;

                // save the two new edges in the hash table
                hullHash[HashKey(x, y)] = i;
                hullHash[HashKey(coords[2 * e], coords[2 * e + 1])] = e;
            }
            
            hull.Pad(hullSize);

            var s = hullStart;
            for (var i = 0; i < hullSize; i++)
            {
                hull[i] = s;
                s = hullNext[s];
            }

            //// trim typed triangle mesh arrays
            subTriangles.Clear();
            subHalfEdges.Clear();

            subTriangles.EnsureCapacity(trianglesLen);
            subHalfEdges.EnsureCapacity(trianglesLen);
            
            subTriangles.AddRange(triangles.Take(trianglesLen));
            subHalfEdges.AddRange(halfEdges.Take(trianglesLen));
            
            triangles.Clear();
            halfEdges.Clear();
            
            triangles.AddRange(subTriangles);
            halfEdges.AddRange(subHalfEdges);
        }

        private readonly List<int> subTriangles = new List<int>();
        private readonly List<int> subHalfEdges = new List<int>();


        private int Legalize(int a)
        {
            var i = 0;
            int ar;

            // recursion eliminated with a fixed-size stack
            while (true)
            {
                var b = halfEdges[a];

                /* if the pair of triangles doesn't satisfy the Delaunay condition
                 * (p1 is inside the circumcircle of [p0, pl, pr]), flip them,
                 * then do the same check/flip recursively for the new pair of triangles
                 *
                 *           pl                    pl
                 *          /||\                  /  \
                 *       al/ || \bl            al/    \a
                 *        /  ||  \              /      \
                 *       /  a||b  \    flip    /___ar___\
                 *     p0\   ||   /p1   =>   p0\---bl---/p1
                 *        \  ||  /              \      /
                 *       ar\ || /br             b\    /br
                 *          \||/                  \  /
                 *           pr                    pr
                 */
                int a0 = a - a % 3;
                ar = a0 + (a + 2) % 3;

                if (b == -1)
                {
                    // convex hull edge
                    if (i == 0) break;
                    a = EDGE_STACK[--i];
                    continue;
                }

                var b0 = b - b % 3;
                var al = a0 + (a + 1) % 3;
                var bl = b0 + (b + 2) % 3;

                var p0 = triangles[ar];
                var pr = triangles[a];
                var pl = triangles[al];
                var p1 = triangles[bl];

                var illegal = InCircle(
                    coords[2 * p0], coords[2 * p0 + 1],
                    coords[2 * pr], coords[2 * pr + 1],
                    coords[2 * pl], coords[2 * pl + 1],
                    coords[2 * p1], coords[2 * p1 + 1]);

                if (illegal)
                {
                    triangles[a] = p1;
                    triangles[b] = p0;

                    var hbl = halfEdges[bl];

                    // edge swapped on the other side of the hull (rare); fix the halfedge reference
                    if (hbl == -1)
                    {
                        var e = hullStart;
                        do
                        {
                            if (hullTri[e] == bl)
                            {
                                hullTri[e] = a;
                                break;
                            }

                            e = hullPrev[e];
                        } while (e != hullStart);
                    }

                    Link(a, hbl);
                    Link(b, halfEdges[ar]);
                    Link(ar, bl);

                    var br = b0 + (b + 1) % 3;

                    // don't worry about hitting the cap: it can only happen on extremely degenerate input
                    if (i < EDGE_STACK.Length)
                    {
                        EDGE_STACK[i++] = br;
                    }
                }
                else
                {
                    if (i == 0) break;
                    a = EDGE_STACK[--i];
                }
            }

            return ar;
        }

        private static bool InCircle(double ax, double ay, double bx, double by, double cx, double cy, double px,
            double py)
        {
            var dx = ax - px;
            var dy = ay - py;
            var ex = bx - px;
            var ey = by - py;
            var fx = cx - px;
            var fy = cy - py;

            var ap = dx * dx + dy * dy;
            var bp = ex * ex + ey * ey;
            var cp = fx * fx + fy * fy;

            return dx * (ey * cp - bp * fy) -
                dy * (ex * cp - bp * fx) +
                ap * (ex * fy - ey * fx) < 0;
        }

        private int AddTriangle(int i0, int i1, int i2, int a, int b, int c)
        {
            var t = trianglesLen;

            triangles[t] = i0;
            triangles[t + 1] = i1;
            triangles[t + 2] = i2;

            Link(t, a);
            Link(t + 1, b);
            Link(t + 2, c);

            trianglesLen += 3;
            return t;
        }

        private void Link(int a, int b)
        {
            halfEdges[a] = b;
            if (b != -1) halfEdges[b] = a;
        }

        private int HashKey(double x, double y) => (int)(Math.Floor(PseudoAngle(x - cx, y - cy) * hashSize) % hashSize);

        private static double PseudoAngle(double dx, double dy)
        {
            var p = dx / (Math.Abs(dx) + Math.Abs(dy));
            return (dy > 0 ? 3 - p : 1 + p) / 4; // [0..1]
        }

        private static void Quicksort(List<int> ids, List<double> dists, int left, int right)
        {
            if (right - left <= 20)
            {
                for (var i = left + 1; i <= right; i++)
                {
                    var temp = ids[i];
                    var tempDist = dists[temp];
                    var j = i - 1;
                    while (j >= left && dists[ids[j]] > tempDist) ids[j + 1] = ids[j--];
                    ids[j + 1] = temp;
                }
            }
            else
            {
                var median = (left + right) >> 1;
                var i = left + 1;
                var j = right;
                Swap(ids, median, i);
                if (dists[ids[left]] > dists[ids[right]]) Swap(ids, left, right);
                if (dists[ids[i]] > dists[ids[right]]) Swap(ids, i, right);
                if (dists[ids[left]] > dists[ids[i]]) Swap(ids, left, i);

                var temp = ids[i];
                var tempDist = dists[temp];
                while (true)
                {
                    do i++;
                    while (dists[ids[i]] < tempDist);
                    do j--;
                    while (dists[ids[j]] > tempDist);
                    if (j < i) break;
                    Swap(ids, i, j);
                }

                ids[left + 1] = ids[j];
                ids[j] = temp;

                if (right - i + 1 >= j - left)
                {
                    Quicksort(ids, dists, i, right);
                    Quicksort(ids, dists, left, j - 1);
                }
                else
                {
                    Quicksort(ids, dists, left, j - 1);
                    Quicksort(ids, dists, i, right);
                }
            }
        }

        private static void Swap(List<int> arr, int i, int j)
        {
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }

        private static bool Orient(double px, double py, double qx, double qy, double rx, double ry) =>
            (qy - py) * (rx - qx) - (qx - px) * (ry - qy) < 0;

        private static double Circumradius(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;
            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = (ey * bl - dy * cl) * d;
            var y = (dx * cl - ex * bl) * d;
            return x * x + y * y;
        }

        private static Point Circumcenter(double ax, double ay, double bx, double by, double cx, double cy)
        {
            var dx = bx - ax;
            var dy = by - ay;
            var ex = cx - ax;
            var ey = cy - ay;
            var bl = dx * dx + dy * dy;
            var cl = ex * ex + ey * ey;
            var d = 0.5 / (dx * ey - dy * ex);
            var x = ax + (ey * bl - dy * cl) * d;
            var y = ay + (dx * cl - ex * bl) * d;

            return new Point(x, y);
        }

        private static double Dist(double ax, double ay, double bx, double by)
        {
            var dx = ax - bx;
            var dy = ay - by;
            return dx * dx + dy * dy;
        }

        public void GetEdges(List<IEdge> edges)
        {
            edges.Clear();
            edges.EnsureCapacity(triangles.Count);
            for (var i = 0; i < triangles.Count; i++)
            {
                if (i <= halfEdges[i]) continue;
                
                var p = points[triangles[i]];
                var q = points[triangles[NextHalfEdge(i)]];
                edges.Add(new Edge(i, p, q));
            }
        }

        private static int NextHalfEdge(int e) => e % 3 == 2 ? e - 2 : e + 1;
    }
}