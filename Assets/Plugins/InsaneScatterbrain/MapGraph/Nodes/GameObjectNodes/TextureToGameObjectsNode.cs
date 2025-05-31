using System;
using System.Collections;
using System.Collections.Generic;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Generates the GameObjects from the prefab set, based on the texture.
    /// </summary>
    [ScriptNode("Texture To GameObjects", "GameObjects"), Serializable]
    public class TextureToGameObjectsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField] 
        private InPort textureIn = null;
        
        [InPort("Prefab Set", typeof(PrefabSet), true), SerializeField] 
        private InPort prefabSetIn = null;
        
        [InPort("Use XZ Plane", typeof(bool)), SerializeField]
        private InPort useXzPlaneIn = null;
        
        [InPort("Depth Data", typeof(float[])), SerializeField]
        private InPort depthDataIn = null;
        
        [InPort("Depth Layers", typeof(float)), SerializeField]
        private InPort depthLayersIn = null;
        
        [InPort("Depth Layer Size", typeof(float)), SerializeField]
        private InPort depthLayerSizeIn = null;
        
        [InPort("Depth Snap To Layers", typeof(bool)), SerializeField]
        private InPort depthSnapToLayersIn = null;
        
        
        [OutPort("GameObjects", typeof(GameObject[])), SerializeReference] 
        private OutPort gameObjectsOut = null;

        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            var instanceProvider = Get<IInstanceProvider>();
            var namedColorSet = Get<NamedColorSet>();
            var depthData = depthDataIn.Get<float[]>();
            var depthLayers = depthLayersIn.IsConnected ? depthLayersIn.Get<float>() : 1f;
            var depthLayerSize = depthLayerSizeIn.IsConnected ? depthLayerSizeIn.Get<float>() : 1f;
            var depthSnapToLayers = depthSnapToLayersIn.IsConnected && depthSnapToLayersIn.Get<bool>();

            var useXzPlane = useXzPlaneIn.Get<bool>();
            var textureData = textureIn.Get<TextureData>();
            var prefabSet = prefabSetIn.Get<PrefabSet>();
            var prefabWidth = prefabSet.PrefabWidth;
            var prefabHeight = prefabSet.PrefabHeight;
            var prefabDepth = prefabSet.PrefabDepth;
            
            var width = textureData.Width;
            
            var gameObjects = instanceProvider.Get<List<GameObject>>();
            gameObjects.EnsureCapacity(textureData.ColorCount);

            var factory = Get<GameObjectsFromTextureFactory>();
            var factoryEnumerator = factory.CreateObjects(textureData, prefabSet, namedColorSet, width, prefabWidth,
                prefabHeight, prefabDepth, depthData, depthLayerSize, depthLayers, depthSnapToLayers, useXzPlane);
            
            while (factoryEnumerator.MoveNext())
            {
                var instance = factoryEnumerator.Current;
                if (instance == null)
                {
                    yield return null;
                    continue;
                }
                
                ScriptGraphComponents.RegisterTemporaryObject(instance);

                gameObjects.Add(instance);
                yield return null;
            }

            gameObjectsOut.Set(() => gameObjects.ToArray());
        }
    }
}