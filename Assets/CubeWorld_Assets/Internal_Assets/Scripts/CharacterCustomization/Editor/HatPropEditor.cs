using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HatProp))]
public class HatPropEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HatProp hatProp = (HatProp)target;

        serializedObject.Update();

        // Draw default fields
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Load Prefabs from Paths"))
        {
            LoadPrefabs(hatProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void LoadPrefabs(HatProp hatProp)
    {
        Undo.RecordObject(hatProp, "Load Hat Prefabs");

        foreach (var hatDataList in hatProp.hatDataList)
        {
            hatDataList.Data.Clear(); // Clear old data first

            foreach (string path in hatDataList.Paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

                    foreach (string guid in guids)
                    {
                        string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                        if (prefab != null)
                        {
                            HatProp.HatData hatData = new HatProp.HatData
                            {
                                HatName = prefab.name,
                                HatProp = prefab
                            };
                            hatDataList.Data.Add(hatData);
                        }
                    }
                }
            }
        }

        EditorUtility.SetDirty(hatProp);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Repaint();

        Debug.Log("Prefabs loaded successfully into HatDataList!");
    }
}
