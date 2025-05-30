using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BackPack))]
public class BackPackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BackPack backPack = (BackPack)target;

        if (GUILayout.Button("Load Prefabs Into Backpack"))
        {
            LoadPrefabs(backPack);
        }
    }

    private void LoadPrefabs(BackPack backPack)
    {
        foreach (var categoryList in backPack.backPackLists)
        {
            categoryList.Data.Clear();

            foreach (string path in categoryList.Paths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;

                string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

                foreach (string guid in prefabGuids)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                    if (prefab != null)
                    {
                        BackPack.BackPackData data = new BackPack.BackPackData
                        {
                            BackPackName = prefab.name,
                            BackPackProp = prefab
                        };
                        categoryList.Data.Add(data);
                    }
                }
            }
        }

        EditorUtility.SetDirty(backPack);
        AssetDatabase.SaveAssets();

        Debug.Log("Finished loading prefabs into BackPack.");
    }
}
