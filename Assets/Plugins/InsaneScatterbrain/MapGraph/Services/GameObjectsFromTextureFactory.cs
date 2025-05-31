using System;
using System.Collections.Generic;
using InsaneScatterbrain.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    public class GameObjectsFromTextureFactory
    {
        private readonly Rng rng;
        
        public GameObjectsFromTextureFactory(Rng rng)
        {
            this.rng = rng;
        }
        
        public IEnumerator<GameObject> CreateObjects(
            TextureData textureData, 
            PrefabSet prefabSet,
            NamedColorSet namedColorSet, 
            int width, 
            float prefabWidth, 
            float prefabHeight,
            float prefabDepth,
            float[] depthData,
            float depthLayerSize,
            float depthLayers,
            bool depthSnapToLayers,
            bool useXzPlane)
        {
            if (depthLayers < 1)
            {
                throw new Exception("Depth layers must be at least 1.");
            }
            
            if (depthLayerSize <= 0)
            {
                throw new Exception("Depth layer size must be greater than 0.");
            }
            
            for (var i = 0; i < textureData.ColorCount; ++i)
            {
                var color = textureData[i];
                if (color == new Color(0, 0, 0, 0))
                {
                    yield return null;
                    continue; // Empty space, skip it 
                }

                var prefabTypeName = namedColorSet.GetName(color);
                if (prefabTypeName == null)
                {
                    Debug.LogError($"Color doesn't exist in Named Color Set: {color}", namedColorSet);
                }

                var x = i % width * prefabWidth;
                var y = i / width * (useXzPlane ? prefabDepth : prefabHeight);

                var depth = 0f;

                // Depth data is provided, so we offset the placement on the Z axis (or Y axis if useXzPlane is true).
                if (depthData != null)
                {
                    // One layer is as deep as a prefab is.
                    var layerDepth = depthLayerSize * (useXzPlane ? prefabHeight : prefabDepth);

                    // So the total depth range is the layer depth calculated by the number of layers.
                    var totalDepth = layerDepth * (depthLayers - 1);

                    // Then we take the depth data which should be normalized between 0 and 1 and bring it into the
                    // depth range, we're using.
                    depth = depthData[i] * totalDepth;

                    if (depthSnapToLayers)
                    {
                        // Round depth to the closest layers. For example, if the layer depth is 1.5 and the depth is 1.3
                        // it would get snapped to 1.5, whereas 2.7 would snap to 3.
                        depth = Mathf.Round(depth / layerDepth) * layerDepth;
                    }
                }

                var prefab = prefabSet.GetRandomObject(prefabTypeName, rng);
                var position = useXzPlane ? new Vector3(x, depth, y) : new Vector3(x, y, depth);
                position += prefab.transform.position;
                
                var instance = Object.Instantiate(prefab, position, prefab.transform.rotation);
                instance.name = prefab.name;

                yield return instance;
            }
        }
    }
}