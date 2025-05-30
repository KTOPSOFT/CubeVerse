using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Face))]
public class FaceEditor : Editor
{
    private Face face;

    private void OnEnable()
    {
        face = (Face)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Load Face Textures from Paths"))
        {
            LoadFaceTextures();
        }
    }

    private void LoadFaceTextures()
    {
        Undo.RecordObject(face, "Load Face Textures");

        foreach (var sexFaceData in face.FaceDataList)
        {
            foreach (var skinFaceData in sexFaceData.Details)
            {
                if (string.IsNullOrEmpty(skinFaceData.Path))
                {
                    Debug.LogWarning($"Path is empty for {sexFaceData.Sex} / {skinFaceData.Skin}");
                    continue;
                }

                string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { skinFaceData.Path });

                if (guids.Length == 0)
                {
                    Debug.LogWarning($"No textures found in path: {skinFaceData.Path}");
                    continue;
                }

                skinFaceData.Details.Clear();

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

                    if (texture != null)
                    {
                        Face.FaceData faceData = new Face.FaceData
                        {
                            FaceName = texture.name,
                            FaceMap = texture
                        };
                        skinFaceData.Details.Add(faceData);
                    }
                }
            }
        }

        EditorUtility.SetDirty(face);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Loaded all face textures successfully!");
    }
}
