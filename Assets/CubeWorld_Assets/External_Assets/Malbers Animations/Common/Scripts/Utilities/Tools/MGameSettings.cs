using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Managers/Game Settings <Simple>")]
    public class MGameSettings : MonoBehaviour, IScene
    {
        public bool HideCursor = false;
        public bool ForceFPS = false;
        [Hide("ForceFPS")]
        [Min(-1)] public int GameFPS = 120;

        [Min(0)] public int vSyncCount = 0;
        public bool DebugBuild = false;



#if UNITY_EDITOR
        [Space, Tooltip("The Scene must be added to the Build Settings!!!")]
        public List<UnityEditor.SceneAsset> AdditiveScenes;
#endif
        [Tooltip("Add the Additive scene in the Editor")]
        public bool InEditor = true;
        [HideInInspector] public List<string> sceneNames;

        void Awake()
        {
            Debug.developerConsoleVisible = DebugBuild;

            transform.parent = null;
            DontDestroyOnLoad(this);

            if (HideCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            QualitySettings.vSyncCount = vSyncCount;
            Application.targetFrameRate = ForceFPS ? GameFPS : -1;

            if (sceneNames != null && !InEditor)
            {
                foreach (var scene in sceneNames)
                {
                    SceneManager.LoadScene(scene, LoadSceneMode.Additive);
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (AdditiveScenes != null)
            {
                sceneNames = new List<string>();

                foreach (var s in AdditiveScenes)
                    if (s != null)
                        sceneNames.Add(s.name);
            }

            vSyncCount = Mathf.Clamp(vSyncCount, 0, 4);
        }
#endif

        [ContextMenu("Add Additive Scene")]
        public void SceneLoaded()
        {
#if UNITY_EDITOR
            if (AdditiveScenes != null && InEditor)
            {

                foreach (var item in AdditiveScenes)
                {
                    var scenePath = UnityEditor.AssetDatabase.GetAssetPath(item);
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                }
            }
#endif
        }
    }
}