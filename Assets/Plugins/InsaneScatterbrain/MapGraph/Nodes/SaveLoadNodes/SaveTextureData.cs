using System;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Save Texture Data", "Save/Load"), Serializable]
    public class SaveTextureData : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeReference] 
        private InPort textureDataIn = null;
        
        [InPort("Save Data", typeof(SaveData), true), SerializeReference] 
        private InPort saveDataIn = null;

        protected override void OnProcess()
        {
            var textureData = textureDataIn.Get<TextureData>();
            var saveData = saveDataIn.Get<SaveData>();
            
            saveData.Save(textureData);
        }
    }
}