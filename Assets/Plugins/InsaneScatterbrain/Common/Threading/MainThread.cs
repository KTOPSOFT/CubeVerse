using System;
using System.Collections;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Allows calls onto the main thread from any other thread. This can be necessary, because the Unity API
    /// is only accessible from the main thread for the most part.
    /// </summary>
    public static class MainThread
    {
        /// <summary>
        /// The command handler that's used to handle any commands given.
        /// </summary>
        public static IMainThreadCommandHandler CommandHandler { get; set; }
        public static IMainThreadCoroutineCommandHandler CoroutineCommandHandler { get; set; }

        /// <summary>
        /// Reference to the main thread.
        /// </summary>
        private static Thread mainThread;
        
        /// <summary>
        /// This is called on load (either in the editor or in a build) from the main thread, so that a reference
        /// to the main thread is stored to execute commands on.
        /// </summary>
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            mainThread = Thread.CurrentThread;
            CommandHandler = new MainThreadCommandHandler();
            CoroutineCommandHandler = new MainThreadCoroutineCommandHandler();
        }

        public static bool IsCurrent => Thread.CurrentThread == mainThread;

        /// <summary>
        /// Executes the given action on the main thread.
        /// </summary>
        /// <param name="command">The command to execute on the main thread.</param>
        /// <param name="waitForCompletion">
        /// Whether or not the calling thread should wait for completion of this command before continuing code execution.
        /// </param>
        public static void Execute(IMainThreadCommand command, bool waitForCompletion = true)
        {
            if (IsCurrent)
            {
                command.Execute();
                return;
            }
            
            CommandHandler.Queue(command, waitForCompletion);
        }
        
        public static void Execute(IMainThreadCoroutineCommand command, bool waitForCompletion = true)
        {
            if (IsCurrent)
            {
                command.Execute();
                return;
            }
            
            CoroutineCommandHandler.Queue(command, waitForCompletion);
        }

        /// <summary>
        /// Executes the given action on the main thread.
        /// </summary>
        /// <param name="action">The action to execute on the main thread.</param>
        /// <param name="waitForCompletion">
        /// Whether or not the calling thread should wait for completion of this command before continuing code execution.
        /// </param>
        [Obsolete("This overload will likely be removed in 2.0.")]
        public static void Execute(Action action, bool waitForCompletion = true)
        {
            // If this method is called from the main thread, just execute it. No reason to queue it, plus it even
            // results in a crash.
            if (Thread.CurrentThread == mainThread)
            {
                action();
                return;
            }
            CommandHandler.Queue(action, waitForCompletion);
        }

        /// <summary>
        /// Handles the execution for queued commands.
        /// </summary>
        public static void Update()
        {
            CommandHandler.Update();
        }

        /// <summary>
        /// Handles the execution for queued coroutine commands.
        /// </summary>
        public static IEnumerator UpdateCoroutine()
        {
            return CoroutineCommandHandler.UpdateCoroutine();
        }

        /// <summary>
        /// Stops executing any commands that might be running as soon as possible.
        /// </summary>
        public static void Cancel()
        {
            CommandHandler.Cancel();
            CoroutineCommandHandler.Cancel();
        }
    }
}

