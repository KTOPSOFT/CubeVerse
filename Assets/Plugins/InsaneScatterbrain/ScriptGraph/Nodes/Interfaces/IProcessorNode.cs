using System;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// Interface for any node that can be used to process something. It can either consume or provide data or both.
    /// </summary>
    public interface IProcessorNode : IProviderNode, IConsumerNode
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets the number of milliseconds it took to process this node the last time it was processed. Returns -1
        /// if it hasn't been processed yet.
        /// </summary>
        long LatestExecutionTime { get; }
#endif

        /// <summary>
        /// Event called whenever processing has been completed.
        /// </summary>
        [Obsolete("Please use the static event NodeProcessingCompleted instead.")]
        event Action ProcessingCompleted;

        /// <summary>
        /// Processes this node.
        /// </summary>
        void Process();
    }
}