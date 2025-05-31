#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// This class makes sure that the main thread command handler is actually run on update.
    /// </summary>
    public class MainThreadUpdater : MonoBehaviour
    {
        private static MainThreadUpdater instance;
    
#if UNITY_EDITOR
        /// <summary>
        /// Initializes the main thread updater in the editor.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            EditorApplication.update += () =>
            {
                if (Application.isPlaying) return;

                MainThread.Update();
            };
            
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingPlayMode) return;

                // Whenever we exit playmode any active commands running on the main thread originating from
                // other threads should be cancelled, otherwise they might just keep going and apply
                // the changes that should've been done during play mode, in edit mode instead. This
                // will also cause the editor to freeze whenever re-entering play mode after this.
                MainThread.Cancel();
            };
        }
#endif
    
        /// <summary>
        /// Initializes the main thread updater in a standalone build.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
#if UNITY_2020_3_OR_NEWER
                instance = FindAnyObjectByType<MainThreadUpdater>();
#else
                instance = FindObjectOfType<MainThreadUpdater>();
#endif
            }

            if (instance != null) return;
            
            var gameObject = new GameObject("[Main Thread Updater]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            gameObject.AddComponent<MainThreadUpdater>();
            DontDestroyOnLoad(gameObject);
        }
    
        private void Update()
        {
            MainThread.Update(); 
        }
    }
}
