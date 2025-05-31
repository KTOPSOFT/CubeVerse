using System;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Interface for basic handler of main thread commands.
    /// </summary>
    public interface IMainThreadCommandHandler
    {
        void Queue(IMainThreadCommand command, bool waitForCompletion = true);
        /// <summary>
        /// Queues the command for executing on the next update.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="waitForCompletion">
        /// Whether or not the calling thread should wait for completion of this command before continuing code execution.
        /// </param>
        [Obsolete("This overload will likely be removed in 2.0.")]
        void Queue(Action action, bool waitForCompletion = true);
        
        /// <summary>
        /// Executes all queued commands on the main thread. If they're done, they are removed from the queue.
        /// </summary>
        void Update();

        /// <summary>
        /// Stops executing the command as soon as possible.
        /// </summary>
        void Cancel();
    }
}