#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    public class MenuCreateHuman
    {
        private readonly static string HumanPlayerPrefab = "Assets/Malbers Animations/Animal Controller/Human/Human Player.prefab";
        private readonly static string HumanAIPrefab = "Assets/Malbers Animations/Animal Controller/Human/Human AI.prefab";

        [MenuItem("GameObject/Malbers Animations/Create Human Player", false, -100)]
        static void CreatePlayer(MenuCommand menuCommand)
        {
            var gameObject = menuCommand.context as GameObject;

            if (gameObject != null)
            {
                DoHuman(gameObject, HumanPlayerPrefab);
                Debug.Log("Human Player Created!. Please save your the new Created Player as a Prefab Variant on your project", gameObject);
            }
        }

        [MenuItem("GameObject/Malbers Animations/Create Human AI", false, -100)]
        static void CreateAI(MenuCommand menuCommand)
        {
            var gameObject = menuCommand.context as GameObject;

            if (gameObject != null)
            {
                DoHuman(gameObject, HumanAIPrefab);
                Debug.Log("Human AI Created!. Please save your the new Created Player as a Prefab Variant on your project", gameObject);
            }
        }

        private static void DoHuman(GameObject gameObject, string path)
        {
            if (!gameObject.TryGetComponent<Animator>(out var animator))
                animator = gameObject.AddComponent<Animator>();

            var currentAvatar = animator.avatar;
            var AvatarRoot = animator.avatarRoot;
            var humanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var sceneObj = (GameObject)PrefabUtility.InstantiatePrefab(humanPrefab);
            sceneObj.GetComponent<Animator>().avatar = currentAvatar; //Set the Avatar to the new Player

            gameObject.transform.parent = sceneObj.transform;
            sceneObj.transform.ResetLocal();

            var animal = sceneObj.GetComponent<MAnimal>();
            animal.RootBone = AvatarRoot; //Set the Root Bone on the Animal Controller
            sceneObj.name = gameObject.name;

            Selection.activeGameObject = sceneObj; //Select the new Player
            GameObject.DestroyImmediate(animator); //Remove the animator
        }
    }
}
#endif