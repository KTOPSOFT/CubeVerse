using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MapGraphTilemapPrefab))]
    public class TilemapPrefabEditor : UnityEditor.Editor
    {
        private MapGraphTilemapPrefab tilemapPrefab;
        private Grid grid;
        private SerializedProperty lockShapeProp;
        
        private void OnEnable()
        {
            tilemapPrefab = (MapGraphTilemapPrefab) target;
            grid = tilemapPrefab.GetComponent<Grid>();

            lockShapeProp = serializedObject.FindProperty("lockShape");
        }

        public override void OnInspectorGUI()
        {
            if (grid.cellLayout != GridLayout.CellLayout.Hexagon)
            {
                GUI.enabled = false;
            }

            var label = new GUIStyle(EditorStyles.boldLabel);

            GUILayout.Label("Hexagon Grid Options", label);
            EditorGUILayout.PropertyField(lockShapeProp, new GUIContent("Lock Shape"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
