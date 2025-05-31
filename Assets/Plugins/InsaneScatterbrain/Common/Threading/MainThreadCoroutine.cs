#if UNITY_EDITOR
using UnityEditor;
#if MAP_GRAPH_EDITORCOROUTINES
using Unity.EditorCoroutines.Editor;
#endif
#endif

using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// This class makes sure that the main thread command handler is actually run on update.
    /// </summary>
    public class MainThreadCoroutine : MonoBehaviour
    {
        private static MainThreadCoroutine instance;

        public static float TargetTimePerFrame { get; set; } = 1 / 60f;
        
        private static readonly Stopwatch stopwatch = new Stopwatch();
    
#if UNITY_EDITOR
        /// <summary>
        /// Initializes the main thread updater in the editor.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void StartCoroutine()
        {
#if MAP_GRAPH_EDITORCOROUTINES
            EditorCoroutineUtility.StartCoroutineOwnerless(EditorCoroutine());
#endif
        }
        
        private static IEnumerator EditorCoroutine()
        {
            while (Application.isPlaying)
                yield return null;

            yield return Coroutine();
        }
#endif

        private static IEnumerator Coroutine()
        {
            stopwatch.Start();
            
            var updateEnumerator = MainThread.UpdateCoroutine();
            while (updateEnumerator.MoveNext())
            {
                var value = updateEnumerator.Current;

                // If we haven't hit the target time for this frame yet, and there's still more to go, keep going.
                if (stopwatch.Elapsed.TotalSeconds <= TargetTimePerFrame && value == null)
                    continue;

                // Either we're out of stuff to do or it's taking too long to finish in the current frame, wait one.
                yield return value;
                
                // And reset the stopwatch.
                stopwatch.Restart();
            }
        }

        private IEnumerator Start()
        {
            return Coroutine();
        }
    
        /// <summary>
        /// Initializes the main thread coroutine in a standalone build.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance == null)
            {
#if UNITY_2020_3_OR_NEWER
                instance = FindAnyObjectByType<MainThreadCoroutine>(); 
#else
                instance = FindObjectOfType<MainThreadCoroutine>();
#endif
            }

            if (instance != null) return;
            
            var gameObject = new GameObject("[Main Thread Coroutine]")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            
            gameObject.AddComponent<MainThreadCoroutine>();
            DontDestroyOnLoad(gameObject);
        }
    }
}
