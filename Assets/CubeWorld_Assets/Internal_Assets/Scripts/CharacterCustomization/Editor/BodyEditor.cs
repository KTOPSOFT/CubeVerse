using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Body))]
public class BodyEditor : Editor
{
    private Body body;

    private void OnEnable()
    {
        body = (Body)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Load Body Textures from Paths"))
        {
            LoadBodyTextures();
        }
    }

    private void LoadBodyTextures()
    {
        Undo.RecordObject(body, "Load Body Textures");

        foreach (var sexData in body.BodyList)
        {
            foreach (var skinData in sexData.Skins)
            {
                if (string.IsNullOrEmpty(skinData.Paths))
                {
                    Debug.LogWarning($"Path is empty for {sexData.Sex} / {skinData.Skin}");
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { skinData.Paths });

                if (guids.Length == 0)
                {
                    Debug.LogWarning($"No textures found in path: {skinData.Paths}");
                    continue;
                }

                skinData.BodyData.Clear();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

                    if (texture != null)
                    {
                        Body.BodyData bodyData = new Body.BodyData
                        {
                            BodyName = texture.name,
                            BodyMap = texture
                        };
                        skinData.BodyData.Add(bodyData);
                    }
                }
            }
        }

        EditorUtility.SetDirty(body);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Loaded all body textures successfully!");
    }
}
