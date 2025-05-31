using UnityEngine;
using UnityEngine.Tilemaps;

namespace InsaneScatterbrain.MapGraph
{
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapRenderer))]
    public class MapGraphTilemapPrefab : MonoBehaviour
    {
        [SerializeField] private bool lockShape = false;

        public bool LockShape => lockShape;
    }
}