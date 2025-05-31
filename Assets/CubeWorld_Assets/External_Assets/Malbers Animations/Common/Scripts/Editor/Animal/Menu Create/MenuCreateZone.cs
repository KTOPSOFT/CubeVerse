#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class MenuCreateZone
    {
        [MenuItem("GameObject/Malbers Animations/Create New Zone", false, 10)]
        static void CreateZoneGameObject()
        {
            GameObject newObject = new("New Zone");
            var col = newObject.AddComponent<BoxCollider>();
            newObject.AddComponent<Zone>();
            var gz = newObject.AddComponent<GizmoVisualizer>();

            col.center = new Vector3(0, 1, 0);
            col.size = new Vector3(2, 2, 2);

            gz.DebugColor = new Color(0.1f, 1, 0.0f, 0.3f);

            if (Selection.activeGameObject != null)
            {
                newObject.transform.SetParent(Selection.activeGameObject.transform);
                newObject.transform.ResetLocal();
            }

            Selection.activeGameObject = newObject;
        }
    }
}
#endif