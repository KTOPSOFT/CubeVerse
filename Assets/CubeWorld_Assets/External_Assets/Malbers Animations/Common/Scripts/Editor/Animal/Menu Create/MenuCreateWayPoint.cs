#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class MenuCreateWayPoint
    {
        [MenuItem("GameObject/Malbers Animations/Create WayPoint", false, 10)]
        static void CreateWayPoint() {

            GameObject newObject = new("WayPoint");
            newObject.AddComponent<MWayPoint>();

            if (Selection.activeGameObject != null)
            {
                newObject.transform.SetParent(Selection.activeGameObject.transform);
                newObject.transform.ResetLocal();
            }

            Selection.activeGameObject = newObject;
            Undo.RegisterCreatedObjectUndo(newObject, "Create WayPoint");

        }
    }
}
#endif