using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Hair))]
public class HairEditor : Editor
{
    private Hair hair;

    private void OnEnable()
    {
        hair = (Hair)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Load Hair Prefabs from Paths"))
        {
            LoadHairPrefabs();
        }
    }

    private void LoadHairPrefabs()
    {
        Undo.RecordObject(hair, "Load Hair Prefabs");

        foreach (var sexHairData in hair.HairDataList)
        {
            if (string.IsNullOrEmpty(sexHairData.Path))
            {
                Debug.LogWarning($"Path is empty for {sexHairData.Sex}");
                continue;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { sexHairData.Path });

            if (guids.Length == 0)
            {
                Debug.LogWarning($"No prefabs found in path: {sexHairData.Path}");
                continue;
            }

            sexHairData.HairData.Clear();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (prefab != null)
                {
                    Hair.HairData hairData = new Hair.HairData
                    {
                        HairName = prefab.name,
                        HairObj = prefab
                    };
                    sexHairData.HairData.Add(hairData);
                }
            }
        }

        EditorUtility.SetDirty(hair);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Loaded all hair prefabs successfully!");
    }
}
