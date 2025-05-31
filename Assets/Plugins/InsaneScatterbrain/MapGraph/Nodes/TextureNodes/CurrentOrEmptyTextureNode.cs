using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Current Or Empty Texture", "Textures"), Serializable]
    public class CurrentOrEmptyTextureNode : EmptyTextureNode
    {
        [InPort("Texture", typeof(TextureData)), SerializeReference] 
        private InPort textureIn = null;

        public override void OnLoadInputPorts()
        {
            base.OnLoadInputPorts();
            
            // This size port is optional for this variant, as providing an existing texture is now also a valid option.
            SizeIn.IsConnectionRequired = false;
        }

        protected override void OnProcess()
        {
            if (!SizeIn.IsConnected && !textureIn.IsConnected)
            {
                Debug.LogError("Either the Size or the Texture in port needs to be connected.");
            }
            
            var currentTexture = textureIn.Get<TextureData>();
            if (currentTexture != null)
            {
                // A texture has been provided, output it.
                TextureOut.Set(() => currentTexture);
                return;
            }
            
            // No texture has been provided, execute the empty texture node's default behaviour to create and output one.
            base.OnProcess();
        }
    }
}