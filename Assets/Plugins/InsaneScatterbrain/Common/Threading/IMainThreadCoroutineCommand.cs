using System.Collections;

namespace InsaneScatterbrain.Threading
{
    public interface IMainThreadCoroutineCommand : IMainThreadCommand
    {
        IEnumerator ExecuteCoroutine();
        
        /// <summary>
        /// Gets whether the command has been cancelled.
        /// </summary>
        bool IsCancelled { get; }
    }
}