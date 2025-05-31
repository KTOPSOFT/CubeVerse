using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Texture To Noise Data", "Noise"), Serializable]
    public class TextureToNoiseDataNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureIn = null;
        
        
        [OutPort("Noise Data", typeof(float[])), SerializeReference] 
        private OutPort noiseDataOut = null;


        protected override void OnProcess()
        {
            var texture = textureIn.Get<TextureData>();
            var noiseData = new float[texture.ColorCount];
            var colors = texture.GetColors();

            for (var i = 0; i < colors.Count; i++)
            {
                var color = colors[i];
                var value = color.r / 255f;
                noiseData[i] = value;
            }

            noiseDataOut.Set(() => noiseData);
        }
    }
}