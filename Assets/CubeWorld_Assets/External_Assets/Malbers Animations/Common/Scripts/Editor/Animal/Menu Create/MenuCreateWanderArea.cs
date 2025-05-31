#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public static class MenuCreateWanderArea
    {
        [MenuItem("GameObject/Malbers Animations/Create WanderArea", false, 10)]
        public static void CreateWanderArea()
        {
            GameObject newObject = new GameObject("WanderArea");
            newObject.AddComponent<AIWanderArea>();

            if (Selection.activeGameObject != null)
            {
                newObject.transform.SetParent(Selection.activeGameObject.transform);
                newObject.transform.ResetLocal();
            }

            Selection.activeGameObject = newObject;
            Undo.RegisterCreatedObjectUndo(newObject, "Create WanderArea");

        }
    }    
}
#endif
