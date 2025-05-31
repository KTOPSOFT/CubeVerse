using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Custom editor for the tileset assets.
    /// </summary>
    [CustomEditor(typeof(Tileset), true)]
    public class TilesetEditor : UnityEditor.Editor
    {
        private TileTypeList typesList;

        private void OnEnable()
        {
            var tileset = (Tileset) target;

            typesList = new TileTypeList(tileset);
        }
        
        private void OnDisable()
        {
            typesList?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            var tileset = (Tileset) target;
            
            typesList.DoLayoutList();
            
            var buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 12
            };
            
            GUILayout.Space(20);

            if (!GUILayout.Button(new GUIContent(
                "Generate & link Named Color Set",
                "Generates a Named Color Set, where each tile type is a (random) color."), buttonStyle)) return;
            
            var namedColorSet = MapGraphAsset.Create<NamedColorSet>($"{tileset.name} Colors");
            namedColorSet.Link(tileset);
        }
    }
}