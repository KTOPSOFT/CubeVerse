using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(Beard))]
public class BeardEditor : Editor
{
    private string folderPath = "Assets/"; // Default folder

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Beard beard = (Beard)target;

        GUILayout.Space(10);
        GUILayout.Label("Beard Prefab Loader", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Prefab Folder Path", folderPath);

        if (GUILayout.Button("Load Prefabs Into BeardDataList"))
        {
            LoadPrefabs(beard, folderPath);
        }
    }

    private void LoadPrefabs(Beard beard, string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"Invalid folder path: {path}");
            return;
        }

        beard.BeardDataList.Clear();

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                Beard.BeardData data = new Beard.BeardData
                {
                    BeardName = prefab.name,
                    BeardObj = prefab
                };
                beard.BeardDataList.Add(data);
            }
        }

        EditorUtility.SetDirty(beard);
        AssetDatabase.SaveAssets();

        Debug.Log($"Loaded {beard.BeardDataList.Count} prefabs into BeardDataList.");
    }
}
