using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BodyProp))]
public class BodyPropEditor : Editor
{
    private string folderPath = "Assets/"; // Default folder

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BodyProp bodyProp = (BodyProp)target;

        GUILayout.Space(10);
        GUILayout.Label("Body Prop Prefab Loader", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Prefab Folder Path", folderPath);

        if (GUILayout.Button("Load Prefabs Into BodyPropList"))
        {
            LoadPrefabs(bodyProp, folderPath);
        }
    }

    private void LoadPrefabs(BodyProp bodyProp, string path)
    {
        // Check if the folder path is valid
        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"Invalid folder path: {path}");
            return;
        }

        // Clear the existing data in the list
        bodyProp.BodyPropList.Clear();

        // Find all prefabs in the specified folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

        foreach (string guid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab != null)
            {
                // Create a new BodyPropData object and populate it with the prefab information
                BodyProp.BodyPropData data = new BodyProp.BodyPropData
                {
                    BodyPropName = prefab.name,
                    BodyPropObj = prefab
                };

                // Add the data to the BodyPropList
                bodyProp.BodyPropList.Add(data);
            }
        }

        // Mark the object as dirty and save the changes
        EditorUtility.SetDirty(bodyProp);
        AssetDatabase.SaveAssets();

        Debug.Log($"Loaded {bodyProp.BodyPropList.Count} prefabs into BodyPropList.");
    }
}
