using UnityEditor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Custom editor for the tilemap set assets.
    /// </summary>
    [CustomEditor(typeof(TilemapSet), true)]
    public class TilemapSetEditor : UnityEditor.Editor
    {
        private TilemapTypeList tilemapTypeList;

        private void OnEnable()
        {
            var tilemapSet = (TilemapSet) target;
            
            tilemapTypeList = new TilemapTypeList(tilemapSet);
        }

        private void OnDisable()
        {
            tilemapTypeList.Dispose();
        }

        public override void OnInspectorGUI()
        {
            tilemapTypeList.DoLayoutList();
        }
    }
}