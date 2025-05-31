using System;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Merges two sets of areas into one. Any overlapping areas will be merged into a single area.
    /// </summary>
    [ScriptNode("Merge Areas", "Areas"), Serializable]
    public class MergeAreasNode : ProcessorNode
    {
        [InPort("Areas A" ,typeof(Area[]), true), SerializeReference] 
        private InPort areasAIn = null;
        
        [InPort("Areas B" ,typeof(Area[]), true), SerializeReference] 
        private InPort areasBIn = null;
        
        [InPort("Connect Diagonals?", typeof(bool)), SerializeReference] 
        private InPort connectDiagonalsIn = null;
        
        [InPort("Simple Merge?", typeof(bool)), SerializeReference]
        private InPort simpleMergeIn = null;
        
        
        [OutPort("Merged Areas" ,typeof(Area[])), SerializeReference] 
        private OutPort areasOut = null;
        
        
        private Area[] mergedAreas;
        
        private AreaExtractor areaExtractor;
#if UNITY_EDITOR
        public IEnumerable<Area> MergedAreas => mergedAreas;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var areasA = areasAIn.Get<Area[]>();
            var areasB = areasBIn.Get<Area[]>();
            var connectDiagonals = connectDiagonalsIn.Get<bool>();
            var simpleMerge = simpleMergeIn.Get<bool>();

            var areas = instanceProvider.Get<List<Area>>();
            areas.EnsureCapacity(areasA.Length + areasB.Length);
            areas.AddRange(areasA);
            areas.AddRange(areasB);

            if (simpleMerge)
            {
                mergedAreas = areas.ToArray();
                areasOut.Set(() => mergedAreas);
                return;
            }

            var width = 0;
            var height = 0;
            foreach (var area in areas)
            {
                foreach (var point in area.Points)
                {
                    if (point.x >= width)
                    {
                        width = point.x + 1;
                    }

                    if (point.y >= height)
                    {
                        height = point.y + 1;
                    }
                }
            }

            var textureData = instanceProvider.Get<TextureData>();
            textureData.Set(width, height);
            textureData.DrawAreas(areasA, Color.black);
            textureData.DrawAreas(areasB, Color.black);

            areaExtractor = Get<AreaExtractor>();
            areaExtractor.Reset();
            areaExtractor.ColorToExtract = Color.black;
            areaExtractor.ConnectDiagonals = connectDiagonals;

            areaExtractor.ExtractAreas(textureData);

            mergedAreas = areaExtractor.Areas;
            
            areasOut.Set(() => mergedAreas);
        }
    }
}