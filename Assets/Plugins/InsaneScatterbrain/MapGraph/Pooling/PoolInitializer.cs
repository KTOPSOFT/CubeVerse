using System.Collections.Concurrent;
using System.Collections.Generic;
using InsaneScatterbrain.DataStructures;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public class PoolInitializer : IPoolInitializer
    {
        public void Initialize(PoolManager instanceProvider)
        {
            instanceProvider.TryAddPool(new AreaGraphEdgePool());
            instanceProvider.TryAddPool(new AreaGraphPool());
            instanceProvider.TryAddPool(new AreaPool());
            instanceProvider.TryAddPool(new BspNodePool());
            instanceProvider.TryAddPool(new BspTreePool());
            instanceProvider.TryAddPool(new MaskPool());
            instanceProvider.TryAddPool(new TextureDataPool());
            instanceProvider.TryAddPool(new TilemapDataPool());
            
            instanceProvider.TryAddPool(new ListPool<int>());
            instanceProvider.TryAddPool(new ListPool<Vector2Int>());
            instanceProvider.TryAddPool(new ListPool<Vector2Int[]>());
            instanceProvider.TryAddPool(new ListPool<Pair<Vector2Int>>());
            instanceProvider.TryAddPool(new ListPool<RectInt>());
            instanceProvider.TryAddPool(new ListPool<GameObject>());
            instanceProvider.TryAddPool(new ListPool<Area>());
            instanceProvider.TryAddPool(new ListPool<BspNode>());
            instanceProvider.TryAddPool(new CollectionPool<HashSet<Vector2Int>, Vector2Int>());
            instanceProvider.TryAddPool(new CollectionPool<HashSet<Area>, Area>());
            instanceProvider.TryAddPool(new DictionaryPool<ConcurrentDictionary<IProcessorNode, bool>, IProcessorNode, bool>());
            instanceProvider.TryAddPool(new StackPool<Area>());
            instanceProvider.TryAddPool(new StackPool<int>());
        }
    }
}