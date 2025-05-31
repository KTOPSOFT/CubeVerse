using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine; 

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Fills the texture with the given color for each pixel that's above (or below) a threshold.
    /// </summary>
    [ScriptNode("Draw Noise Threshold", "Drawing"), Serializable]
    public class DrawNoiseThresholdNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference]
        private InPort textureIn = null;
        
        [InPort("Noise Data", typeof(float[]), true), SerializeReference]
        private InPort noiseDataIn = null;
        
        [InPort("Draw Color", typeof(Color32), true), SerializeReference]
        private InPort drawColorIn = null;
        
        [InPort("Threshold", typeof(float)), SerializeReference]
        private InPort thresholdIn = null;
        
        [InPort("Below Threshold?", typeof(bool)), SerializeReference]
        private InPort belowThresholdIn = null;
        
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference]
        private OutPort textureOut = null;
        
        
        private TextureData textureData;
#if UNITY_EDITOR
        public TextureData TextureData => textureData;
#endif

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            textureData = instanceProvider.Get<TextureData>();
            textureIn.Get<TextureData>().Clone(textureData);
            
            var noiseData = noiseDataIn.Get<float[]>();
            var drawColor = drawColorIn.Get<Color32>();
            var threshold = thresholdIn.IsConnected ? thresholdIn.Get<float>() / 100f : .5f;
            var belowThreshold = belowThresholdIn.Get<bool>();

            for (var i = 0; i < noiseData.Length; ++i)
            {
                var thresholdReached = belowThreshold 
                    ? noiseData[i] <= threshold 
                    : noiseData[i] >= threshold;

                if (!thresholdReached) continue;
                
                textureData[i] = drawColor;
            }
            
            textureOut.Set(() => textureData);
        }
    }
}