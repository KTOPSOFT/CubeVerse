using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Extract Points With Neighbours", "Points"), Serializable]
    public class ExtractPointsWithNeighboursNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Extract Color", typeof(Color32)), SerializeReference]
        private InPort extractColorIn = null;
        
        [InPort("Neighbour Color", typeof(Color32)), SerializeReference]
        private InPort neighbourColorIn = null;

        [InPort("Left?", typeof(bool)), SerializeReference]
        private InPort leftIn = null;
        
        [InPort("Right?", typeof(bool)), SerializeReference]
        private InPort rightIn = null;
        
        [InPort("Top?", typeof(bool)), SerializeReference]
        private InPort topIn = null;
        
        [InPort("Bottom?", typeof(bool)), SerializeReference]
        private InPort bottomIn = null;
        
        [InPort("Inverse?", typeof(bool)), SerializeReference]
        private InPort inverseIn = null;


        [OutPort("Points", typeof(Vector2Int[])), SerializeReference]
        private OutPort pointsOut = null;


        private int width;
        private int height;
        private Color32 neighbourColor;
        private bool inverse;
        private List<Vector2Int> matchingPoints;
        private TextureData texture;


#if UNITY_EDITOR
        private ReadOnlyCollection<Vector2Int> readOnlyPoints;
        public ReadOnlyCollection<Vector2Int> Points => readOnlyPoints ?? (readOnlyPoints = matchingPoints.AsReadOnly());
        public TextureData InputTextureData => texture;
#endif

        private bool IsValid(Vector2Int coords)
        {
            if (coords.x < 0 || coords.y < 0) return false;
            if (coords.x >= width || coords.y >= height) return false;
            return true;
        }

        private bool IsNeighbourMatch(Vector2Int neighbourPoint)
        {
            if (!IsValid(neighbourPoint)) return false;
            
            var index = neighbourPoint.y * width + neighbourPoint.x;

            bool isMatch;
            if (neighbourColorIn.IsConnected)
            {
                isMatch = texture[index].IsEqualTo(neighbourColor);
            }
            else
            {
                isMatch = texture[index].a > 0;
            }

            if (inverse) isMatch = !isMatch;

            return isMatch;
        }

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            texture = textureIn.Get<TextureData>();
            var pointColor = extractColorIn.Get<Color32>();
            neighbourColor = neighbourColorIn.Get<Color32>();
            
            var checkWest = leftIn.Get<bool>();
            var checkEast = rightIn.Get<bool>();
            var checkNorth = topIn.Get<bool>();
            var checkSouth = bottomIn.Get<bool>();

            inverse = inverseIn.Get<bool>();
            
            width = texture.Width;
            height = texture.Height;

            matchingPoints = instanceProvider.Get<List<Vector2Int>>();
            matchingPoints.EnsureCapacity(texture.ColorCount);
            
            for (var i = 0; i < texture.ColorCount; ++i)
            {
                if (!texture[i].IsEqualTo(pointColor)) continue;
                
                var point = new Vector2Int(i % width, i / width);

                var west = new Vector2Int(point.x - 1, point.y);
                var east = new Vector2Int(point.x + 1, point.y);
                var north = new Vector2Int(point.x, point.y + 1);
                var south = new Vector2Int(point.x, point.y - 1);
                
                if (checkWest && !IsNeighbourMatch(west)) continue;
                if (checkEast && !IsNeighbourMatch(east)) continue;
                if (checkNorth && !IsNeighbourMatch(north)) continue;
                if (checkSouth && !IsNeighbourMatch(south)) continue;
                
                matchingPoints.Add(point);
            }

            var matchingPointsArray = matchingPoints.ToArray();
            
            pointsOut.Set(() => matchingPointsArray);
        }
    }
}