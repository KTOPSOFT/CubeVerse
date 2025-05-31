using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Load Texture Data", "Save/Load"), Serializable]
    public class LoadTextureData : ProcessorNode
    {
        [InPort("Save Data", typeof(SaveData), true), SerializeReference] 
        private InPort saveDataIn = null;
        
        [OutPort("Texture", typeof(TextureData)), SerializeReference] 
        private OutPort textureDataOut = null;

        protected override void OnProcess()
        {
            var instanceProvider = Get<IInstanceProvider>();
            
            var saveData = saveDataIn.Get<SaveData>();
            var textureData = instanceProvider.Get<TextureData>();
            
            saveData.Load(textureData);
            
            textureDataOut.Set(() => textureData);
        }
    }
}