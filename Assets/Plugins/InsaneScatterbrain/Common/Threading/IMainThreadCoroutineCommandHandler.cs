using System.Collections;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Interface for basic handler of main thread commands.
    /// </summary>
    public interface IMainThreadCoroutineCommandHandler
    {
        void Queue(IMainThreadCoroutineCommand command, bool waitForCompletion = true);

        /// <summary>
        /// Executes all queued commands on the main thread. If they're done, they are removed from the queue.
        /// </summary>
        IEnumerator UpdateCoroutine();

        /// <summary>
        /// Stops executing the command as soon as possible.
        /// </summary>
        void Cancel();
    }
}