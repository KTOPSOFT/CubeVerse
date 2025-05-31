using System.Collections.Generic;
using UnityEngine;

namespace InsaneScatterbrain.Extensions
{
    /// <summary>
    /// Contains all the Texture2D extension methods with respect to drawing on a texture.
    /// </summary>
    public static class Texture2DDrawingExtensions
    {
        /// <summary>
        /// Draws a line onto the texture of the given color and thickness between a start and end point.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="color">The line color.</param>
        /// <param name="thickness">The line thickness.</param>
        public static void DrawLine(this Texture2D texture, Vector2Int start, Vector2Int end, Color color, int thickness)
        {
            if (thickness < 1)
            {
                Debug.LogError("Thickness should be bigger than 0.");
                return;
            }
            
            if (thickness == 1)
            {
                texture.DrawLine(start, end, color);
                return;
            }

            var startToEnd = (Vector2) (end - start);
            
            var angle = Vector2.SignedAngle(startToEnd, Vector2.up);
            
            var startMoveRotation = Quaternion.AngleAxis(90, Vector3.forward);
            var startMoveDir = startMoveRotation * startToEnd.normalized;

            var movedStart = startMoveDir * thickness * .5f;
            start = new Vector2Int(start.x + (int)movedStart.x, start.y + (int)movedStart.y);
            var width = Mathf.RoundToInt(startToEnd.magnitude) + 1;

            texture.DrawRectFill(start, thickness, width, angle, color);
        }

        /// <summary>
        /// Draws a line onto the texture of the given color between a start and end point.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        /// <param name="color">The line color.</param>
        public static void DrawLine(this Texture2D texture, Vector2Int start, Vector2Int end, Color color)
        {
            var diff = new Vector2Int(
                Mathf.Abs(end.x - start.x),
                -Mathf.Abs(end.y - start.y));

            var dir = new Vector2Int(
                start.x < end.x ? 1 : -1,
                start.y < end.y ? 1 : -1);
            
            var error = diff.x + diff.y;

            var width = texture.width;
            var height = texture.height;

            var current = start;

            while (current.x != end.x || current.y != end.y)
            {
                if (IsPixelInRange(current, width, height))
                {
                    texture.SetPixel(current.x, current.y, color);
                }

                var doubleError = 2 * error;
                if (doubleError >= diff.y)
                {
                    error += diff.y;
                    current.x += dir.x;
                }

                if (doubleError <= diff.x)
                {
                    error += diff.x;
                    current.y += dir.y;
                }
            }
            
            if (IsPixelInRange(current, width, height))
            {
                texture.SetPixel(current.x, current.y, color);
            }
        }
        
        /// <summary>
        /// Draws a line onto the texture of the given color between an array of points. Points[0] is connected to Points[1],
        /// Points[1] to Points[2], etc.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="color">The line color.</param>
        /// <param name="points">The points to draw lines between.</param>
        public static void DrawLines(this Texture2D texture, Color color, params Vector2Int[] points)
        {
            for (var i = 1; i < points.Length; ++i)
            {
                var pointA = points[i - 1];
                var pointB = points[i];
                texture.DrawLine(pointA, pointB, color);
            }
        }
        
        /// <summary>
        /// Calculates rectangle corners, based on the first corner point, of the given width, height and rotated by the given angle.
        /// </summary>
        /// <param name="p0">The first corner point.</param>
        /// <param name="width">The line color.</param>
        /// <param name="height">The height.</param>
        /// <param name="angle">The width.</param>
        /// <param name="p1">The second corner point.</param>
        /// <param name="p2">The third corner point.</param>
        /// <param name="p3">The fourth corner point.</param>
        private static void RectPoints(Vector2Int p0, int width, int height, float angle, out Vector2Int p1, out Vector2Int p2,
            out Vector2Int p3)
        {
            var rotation = Quaternion.AngleAxis(-angle, Vector3.forward);

            Vector2 dirX = rotation * Vector2.right * (width - 1);
            Vector2 dirY = rotation * Vector2.up * (height - 1);

            var p1Float = p0 + dirY;
            var p3Float = p0 + dirX;
            
            p1 = new Vector2Int(Mathf.RoundToInt(p1Float.x), Mathf.RoundToInt(p1Float.y));
            p3 = new Vector2Int(Mathf.RoundToInt(p3Float.x), Mathf.RoundToInt(p3Float.y));
            p2 = p1 + p3 - p0;
        }

        /// <summary>
        /// Draws unfilled rectangle of given color, based on the first corner point, the given width, height and rotation.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="p0">The first corner point.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="color">The draw color.</param>
        public static void DrawRect(this Texture2D texture, Vector2Int p0, int width, int height, float angle, Color color)
        {
            RectPoints(p0, width, height, angle, out var p1, out var p2, out var p3);

            texture.DrawLines(color, p0, p1, p2, p3, p0);
        }

        /// <summary>
        /// Checks if the given pixel position is in a valid range, based on the width and height.
        /// </summary>
        /// <param name="pixel">The pixel position.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>True if pixel position lays within the given width and height. False, otherwise.</returns>
        private static bool IsPixelInRange(Vector2Int pixel, int width, int height)
        {
            if (pixel.x < 0 || pixel.y < 0) return false;
            if (pixel.x >= width || pixel.y >= height) return false;
            return true;
        }

        /// <summary>
        /// Draws filled rectangle of given color, based on the first corner point, the given width, height and rotation.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <param name="p0">The first corner point.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="color">The draw color.</param>
        public static void DrawRectFill(this Texture2D texture, Vector2Int p0, int width, int height, float angle, Color color)
        {
            if (width < 2)
            {
                Debug.LogError("Width should be bigger than 1. Alternatively, use DrawLine instead.");
            }
            
            if (height < 2)
            {
                Debug.LogError("Height should be bigger than 1. Alternatively, use DrawLine instead.");
            }

            var textureWidth = texture.width;
            var textureHeight = texture.height;

            RectPoints(p0, width, height, angle, out var p1, out var p2, out var p3);
            var center = (p0 + p1 + p2 + p3) / 4;

            // This stack is used to keep track for which the neighbouring pixels need to be checked.
            var pixels = new Stack<Vector2Int>();
            var pixelsDone = new HashSet<Vector2Int>();
            
            // Update the first pixel's color and add it as the first pixel to the stack.
            pixels.Push(center);
            
            texture.DrawLine(p0, p1, color);
            texture.DrawLine(p1, p2, color);
            texture.DrawLine(p2, p3, color);
            texture.DrawLine(p3, p0, color);
            
            while (pixels.Count > 0)
            {
                var pixel = pixels.Pop();
                
                texture.SetPixel(pixel.x, pixel.y, color);
                
                var west = new Vector2Int(pixel.x - 1, pixel.y);
                var east = new Vector2Int(pixel.x + 1, pixel.y);
                var north = new Vector2Int(pixel.x, pixel.y - 1);
                var south = new Vector2Int(pixel.x, pixel.y + 1);
            
                // Check for each neighbouring pixel (up, down, left and right) if they are indeed a valid pixel, if
                // they contain the color that needs to be changed and if they are within the bounds of the rectangle.
                // If so, their color is changed and their neighbours also need to be checked.
                
                if (IsPixelInRange(west, textureWidth, textureHeight) && TrigMath.IsPointInRectangle(west, p0, p1, p2, p3) && !pixelsDone.Contains(west) )
                {
                    pixels.Push(west);
                }
                
                if (IsPixelInRange(east, textureWidth, textureHeight) && TrigMath.IsPointInRectangle(east, p0, p1, p2, p3) && !pixelsDone.Contains(east))
                {
                    pixels.Push(east);
                }
                
                if (IsPixelInRange(north, textureWidth, textureHeight) && TrigMath.IsPointInRectangle(north, p0, p1, p2, p3) && !pixelsDone.Contains(north))
                {
                    pixels.Push(north);
                }
            
                if (IsPixelInRange(south, textureWidth, textureHeight) && TrigMath.IsPointInRectangle(south, p0, p1, p2, p3) && !pixelsDone.Contains(south))
                {
                    pixels.Push(south);
                }
                
                pixelsDone.Add(pixel);
            }
        }

        public static void DrawRects(this Texture2D texture, IEnumerable<RectInt> rects, Color32 color)
        {
            foreach (var rect in rects)
            {
                var x = rect.x;
                var y = rect.y;
                var width = rect.width;
                var height = rect.height;

                var fillColor = color;
                var colors = fillColor.CreateArray(width * height);
                    
                texture.SetPixels32(x, y, width, height, colors);
            }
        }
    }
}