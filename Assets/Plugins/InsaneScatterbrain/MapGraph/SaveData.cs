using System.Collections.Generic;
using InsaneScatterbrain.Versioning;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public class SaveData : VersionedScriptableObject
    {
        [SerializeField, HideInInspector] private int width = 0;
        [SerializeField, HideInInspector] private int height = 0;
        [SerializeField, HideInInspector] private List<Color32> colors = new List<Color32>();

        public void Save(TextureData textureData)
        {
            width = textureData.Width;
            height = textureData.Height;
            colors.Clear();
            foreach (var color in textureData.GetColors())
            {
                colors.Add(color);
            }
        }

        public void Load(TextureData emptyTextureData)
        {
            emptyTextureData.Set(width, height);
            for (var i = 0; i < colors.Count; ++i)
            {
                emptyTextureData[i] = colors[i];
            }
        }
    }
}