using System;
using System.Threading;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Contains an action to execute on the main thread and its current status.
    /// </summary>
    public class MainThreadCommand : IMainThreadCommand
    {
        private Action action;
        private readonly ManualResetEvent ready = new ManualResetEvent(false);

        private bool isCancelled;
        
        /// <inheritdoc cref="IMainThreadCommand.Done"/>
        public bool Done { get; private set; }

        public MainThreadCommand(Action action)
        {
            this.action = action;
        }

        public MainThreadCommand()
        {
        }

        public void SetAction(Action setAction)
        {
            action = setAction;
        }

        /// <inheritdoc cref="IMainThreadCommand.Execute"/>
        public void Execute()
        {
            try
            {
                if (!isCancelled)
                {
                    action();
                }
            }
            finally
            {
                Done = true;
                ready.Set();
            }
        }

        /// <summary>
        /// Makes the calling thread wait for the completion of this command.
        /// </summary>
        /// <exception cref="Exception">
        /// If an exception occurred while executing the command, it will be rethrown here to the calling thread.
        /// </exception>
        public void WaitForCompletion()
        {
            if (isCancelled)
            {
                Done = true;
                ready.Set();
                return;
            }
            
            Done = false;
            ready.Reset();
            ready.WaitOne();
        }

        /// <summary>
        /// Stops executing the command as soon as possible.
        /// </summary>
        public void Cancel()
        {
            Done = true;
            ready.Set();
            isCancelled = true;
        }
    }
}
