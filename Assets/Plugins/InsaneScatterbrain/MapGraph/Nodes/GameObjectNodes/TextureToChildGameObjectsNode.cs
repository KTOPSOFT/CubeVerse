using System;
using System.Collections;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Texture To Child GameObjects", "GameObjects"), Serializable]
    public class TextureToChildGameObjectsNode : ProcessorNode
    {
        [InPort("Texture", typeof(TextureData), true), SerializeField] 
        private InPort textureIn = null;
        
        [InPort("Prefab Set", typeof(PrefabSet), true), SerializeField] 
        private InPort prefabSetIn = null;
        
        [InPort("Parent", typeof(GameObject), true), SerializeField] 
        private InPort parentIn = null;

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
        
        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            var namedColorSet = Get<NamedColorSet>();
            
            var parent = parentIn.Get<GameObject>();
            var useXzPlane = useXzPlaneIn.Get<bool>();
            var textureData = textureIn.Get<TextureData>();
            var depthData = depthDataIn.Get<float[]>();
            var depthLayers = depthLayersIn.IsConnected ? depthLayersIn.Get<float>() : 1f;
            var depthLayerSize = depthLayerSizeIn.IsConnected ? depthLayerSizeIn.Get<float>() : 1f;
            var depthSnapToLayers = depthSnapToLayersIn.IsConnected && depthSnapToLayersIn.Get<bool>();
            
            var prefabSet = prefabSetIn.Get<PrefabSet>();
            var prefabWidth = prefabSet.PrefabWidth;
            var prefabHeight = prefabSet.PrefabHeight;
            var prefabDepth = prefabSet.PrefabDepth;

            var width = textureData.Width;

            // Remove any existing child game objects from the parent object.
            while (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0).gameObject;
                Object.DestroyImmediate(child);
                
                yield return null;
            }
            
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
                
                instance.transform.SetParent(parent.transform, false);
                yield return null;
            }
        }
    }
}