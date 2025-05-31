using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Custom editor for the prefab set assets.
    /// </summary>
    [CustomEditor(typeof(PrefabSet), true)]
    public class PrefabSetEditor : UnityEditor.Editor
    {
        private PrefabTypeList prefabTypeList;

        private void OnEnable()
        {
            var prefabSet = (PrefabSet) target;
            
            prefabTypeList = new PrefabTypeList(prefabSet);
        }

        private void OnDisable()
        {
            prefabTypeList?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            var prefabSet = (PrefabSet) target;
            
            prefabSet.PrefabWidth = EditorGUILayout.FloatField("Width", prefabSet.PrefabWidth);
            prefabSet.PrefabHeight = EditorGUILayout.FloatField("Height", prefabSet.PrefabHeight);
            prefabSet.PrefabDepth = EditorGUILayout.FloatField("Depth", prefabSet.PrefabDepth);

            GUILayout.Space(20);
            
            prefabTypeList.DoLayoutList();
            
            var buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 12
            };
            
            GUILayout.Space(20);
            
            if (!GUILayout.Button(new GUIContent(
                "Generate & link Named Color Set",
                "Generates a Named Color Set, where each prefab type is a (random) color."), buttonStyle)) return;
            
            var namedColorSet = MapGraphAsset.Create<NamedColorSet>($"{prefabSet.name} Colors");
            namedColorSet.Link(prefabSet);
        }
    }
}