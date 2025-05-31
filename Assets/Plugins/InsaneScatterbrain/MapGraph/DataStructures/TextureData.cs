using System;
using System.Collections;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Contains texture data. This is used to be able to manipulate texture data without requiring access to the
    /// Unity API, which is not possible on any thread that is not the main thread.
    /// </summary>
    public class TextureData
    {
        public class Color32ArrayFacade : IList<Color32>
        {
            private readonly List<Color32> innerList;

            public Color32ArrayFacade(List<Color32> list) => innerList = list;
            
            public int Length => innerList.Count;
            
            public IEnumerator<Color32> GetEnumerator() => innerList.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => innerList.GetEnumerator();
            public void CopyTo(Color32[] array, int arrayIndex) => innerList.CopyTo(array, arrayIndex);

            public void Add(Color32 item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Contains(Color32 item) => throw new NotSupportedException();
            public bool Remove(Color32 item) => throw new NotSupportedException();
            public int Count => throw new NotSupportedException();
            public bool IsReadOnly => throw new NotSupportedException();
            public int IndexOf(Color32 item) => throw new NotSupportedException();
            public void Insert(int index, Color32 item) => throw new NotSupportedException();
            public void RemoveAt(int index) => throw new NotSupportedException();

            public Color32 this[int index]
            {
                get => innerList[index];
                set => innerList[index] = value;
            }
        }
        
        private readonly List<Color32> colors = new List<Color32>();
        private readonly List<Color32> oldColors = new List<Color32>();
        private readonly Color32ArrayFacade colorArray;
        private IReadOnlyList<Color32> colorsReadOnly;
        
        /// <summary>
        /// Gets the texture width.
        /// </summary>
        public int Width { get; private set; }
        
        /// <summary>
        /// Gets the texture height.
        /// </summary>
        public int Height { get; private set; }
        
        public int ColorCount => colors.Count;

        public TextureData()
        {
            colorArray = new Color32ArrayFacade(colors);
        }

        /// <summary>
        /// Creates new texture data.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        [Obsolete("Please use the pool manager to get a TextureData instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        public TextureData(int width, int height)
        {
            Width = width;
            Height = height;
            colors.Pad(width * height);
            colorArray = new Color32ArrayFacade(colors);
        }

        public Color32ArrayFacade Colors => colorArray;
        
        private Color32 GetColor(int index) => colors[index];

        private void SetColor(int index, Color32 color)
        {
            if (index >= colors.Count)
            {
                throw new IndexOutOfRangeException(
                    $"Cannot set color at index {index} in TextureData with {colors.Count} colors.");
            }
            colors[index] = color;
        } 

        public IReadOnlyList<Color32> GetColors() => colorsReadOnly ?? (colorsReadOnly = colors.AsReadOnly());

        public Color32 this[int index]
        {
            get => GetColor(index);
            set => SetColor(index, value);
        }

        public void Reset()
        {
            Width = 0;
            Height = 0;
            colors.Clear();
            oldColors.Clear();
        }
        
        /// <summary>
        /// Creates new texture data.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void Set(int width, int height)
        {
            Width = width;
            Height = height;
            colors.Pad(width * height);
        }

        /// <summary>
        /// Resizes the texture to the given size.
        /// </summary>
        /// <param name="width">The new width.</param>
        /// <param name="height">The new height.</param>
        public void Resize(int width, int height)
        {
            var oldWidth = Width;
            var oldHeight = Height;
            
            oldColors.Clear();
            oldColors.EnsureCapacity(width * height);
            oldColors.AddRange(colors);

            colors.Clear();

            Set(width, height);

            var smallestWidth = Mathf.Min(oldWidth, width);
            var smallestHeight = Mathf.Min(oldHeight, height);

            for (var x = 0; x < smallestWidth; ++x)
            for (var y = 0; y < smallestHeight; ++y)
            {
                var i = y * width + x;
                var oldIndex = y * oldWidth + x;

                colors[i] = oldColors[oldIndex];
            }
        }

        /// <summary>
        /// Creates a new instance of this texture data.
        /// </summary>
        /// <param name="clone">The instance that will be used to clone this into.</param>
        public void Clone(TextureData clone)
        {
            clone.Width = Width;
            clone.Height = Height;
            clone.colors.Clear();
            clone.colors.AddRange(colors);
        }
        
        /// <summary>
        /// Creates a new instance of this texture data.
        /// </summary>
        /// <returns>A clone of this texture data</returns>
        [Obsolete("Please use the overload that takes an instance of TextureData to clone into. This overload will likely be removed in 2.0.")]
        public TextureData Clone()
        {
            var clone = new TextureData
            {
                Width = Width,
                Height = Height
            };
            clone.colors.AddRange(colors);
            return clone;
        }

        /// <summary>
        /// Creates a new texture from the texture data.
        /// </summary>
        /// <returns>The new texture.</returns>
        public Texture2D ToTexture2D()
        {
            var texture = Texture2DFactory.CreateDefault(Width, Height);
            
            if (colors.Count <= 0) return texture;
            
            texture.SetPixels32(colors.ToArray());
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Creates texture data from texture.
        /// </summary>
        /// <param name="textureData">The texturedata instance to fill with the texture's data.</param>
        /// <param name="texture">The texture.</param>
        public static void CreateFromTexture(TextureData textureData, Texture2D texture)
        {
            textureData.Reset();
            textureData.Width = texture.width;
            textureData.Height = texture.height;
            textureData.colors.AddRange(texture.GetPixels32());
        }
        
        /// <summary>
        /// Creates texture data from texture.
        /// </summary>
        /// <param name="texture">The texture.</param>
        /// <returns>The new texture data.</returns>
        public static TextureData CreateFromTexture(Texture2D texture)
        {
            var textureData = new TextureData();
            textureData.Set(texture.width, texture.height);
            textureData.colors.AddRange(texture.GetPixels32());
            return textureData;
        }
    }
}