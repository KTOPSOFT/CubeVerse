
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [ExecuteInEditMode]
    public class ScaleUnifier : MonoBehaviour
    {
        [Tooltip("Name of the New Mesh")]
        public string meshName = "NewMesh";
        [Tooltip("Folder path in which the new mesh will be saved")]
        public string folderPath = "Assets/";

        public void UnifyScale()
        {
            // Get the object's mesh filter
            if (!TryGetComponent<MeshFilter>(out var meshFilter))
            {
                Debug.LogError("MeshFilter component not found.");
                return;
            }

            // Get the original mesh
            Mesh originalMesh = meshFilter.sharedMesh;
            if (originalMesh == null)
            {
                Debug.LogError("Original mesh not found.");
                return;
            }

            // Duplicate the original mesh
            Mesh newMesh = DuplicateMesh(originalMesh);

            // Apply the object's scale to the new mesh
            Vector3[] vertices = newMesh.vertices;
            Vector3 scale = transform.localScale;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.Scale(vertices[i], scale);
            }
            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            // Save the new mesh as an asset
#if UNITY_EDITOR
            string path = $"{folderPath}/{meshName}.asset";
            AssetDatabase.CreateAsset(newMesh, path);
            AssetDatabase.SaveAssets();
#endif

            // Assign the new mesh to the object
            meshFilter.sharedMesh = newMesh;

            // Set the object's scale to 1 to maintain shape
            transform.localScale = Vector3.one;

            Debug.LogWarning($"{gameObject.name}: <color=orange>Check the collider and reset or modify if needed.</color>", this);

#if UNITY_EDITOR
            // Mark the mesh and the object as dirty to ensure changes are saved
            EditorUtility.SetDirty(meshFilter);
            EditorUtility.SetDirty(gameObject);
#endif
        }

        Mesh DuplicateMesh(Mesh originalMesh)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = originalMesh.vertices;
            newMesh.normals = originalMesh.normals;
            newMesh.uv = originalMesh.uv;
            newMesh.triangles = originalMesh.triangles;
            newMesh.colors = originalMesh.colors;
            newMesh.tangents = originalMesh.tangents;
            newMesh.uv2 = originalMesh.uv2;
            newMesh.uv3 = originalMesh.uv3;
            newMesh.uv4 = originalMesh.uv4;
            newMesh.bindposes = originalMesh.bindposes;
            newMesh.boneWeights = originalMesh.boneWeights;
            return newMesh;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ScaleUnifier))]
    public class ScaleUnifierEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("This component creats a new mesh an unfies the scale. It can be removed after usage.");
            DrawDefaultInspector();

            ScaleUnifier script = (ScaleUnifier)target;
            if (GUILayout.Button("Unify Scale"))
            {
                script.UnifyScale();
            }
        }
    }
#endif
}