using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Extract Points", "Points"), Serializable]
    public class ExtractPointsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Color To Extract", typeof(Color32), true), SerializeReference]
        private InPort colorIn = null;
        
        
        [OutPort("Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort pointsOut = null;

        
        private List<Vector2Int> points;
        private TextureData texture;

        
#if UNITY_EDITOR
        private ReadOnlyCollection<Vector2Int> readOnlyPoints;
        public ReadOnlyCollection<Vector2Int> Points => readOnlyPoints ?? (readOnlyPoints = points.AsReadOnly());
        public TextureData InputTextureData => texture;
#endif
        
        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            texture = textureIn.Get<TextureData>();
            var color = colorIn.Get<Color32>();
            
            var width = texture.Width;

            points = instanceProvider.Get<List<Vector2Int>>();
            points.EnsureCapacity(texture.ColorCount);
            
            for (var i = 0; i < texture.ColorCount; ++i)
            {
                if (!texture[i].IsEqualTo(color)) continue;
                
                points.Add(new Vector2Int(
                    i % width,
                    i / width));
            }

            var pointsArray = points.ToArray();
            
            pointsOut.Set(() => pointsArray);
        }
    }
}