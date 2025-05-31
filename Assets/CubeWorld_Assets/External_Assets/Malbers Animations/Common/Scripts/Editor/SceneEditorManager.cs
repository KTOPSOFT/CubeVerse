#if UNITY_EDITOR
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [UnityEditor.InitializeOnLoad]
    public static class SceneEditorManager
    {
        // constructor
        static SceneEditorManager()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
        }

        static void SceneOpenedCallback(Scene _scene, UnityEditor.SceneManagement.OpenSceneMode _mode)
        {
            if (!string.IsNullOrEmpty(_scene.name))
            {
                var allGO = _scene.GetRootGameObjects();

                foreach (var go in allGO)
                {
                    if (go.TryGetComponent<IScene>(out var iscene))
                    {
                        iscene.SceneLoaded();
                        break;
                    }
                }
            }
        }
    }
}
#endif