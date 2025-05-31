
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Managers/Load Scene")]
    public class MLoadScene : MonoBehaviour
    {
        [HelpBox]
        public string descr = "The Scene must be added to the Build Settings!";

#if UNITY_EDITOR
      [RequiredField]  public UnityEditor.SceneAsset AdditiveScenes;
#endif
        [HideInInspector] public string sceneName;

        [MButton("LoadScene",true)]
        public bool LoadButton;

        

        public void LoadScene()
        {
            if (string.IsNullOrEmpty(sceneName)) { return; }
            SceneManager.LoadScene(sceneName);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (AdditiveScenes != null)
            {
                sceneName = AdditiveScenes.name;
            }
        }
#endif

    }
}