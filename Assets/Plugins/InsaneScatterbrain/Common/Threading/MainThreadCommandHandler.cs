using System;
using System.Collections.Concurrent;

namespace InsaneScatterbrain.Threading
{
    /// <summary>
    /// Basic handler of main thread commands. It queues the commands and executes any commands in the queue on update.
    /// </summary>
    public class MainThreadCommandHandler : IMainThreadCommandHandler
    {
        private bool isCancelled;
        
        private readonly ConcurrentQueue<IMainThreadCommand> queue = new ConcurrentQueue<IMainThreadCommand>();

        private readonly ConcurrentDictionary<IMainThreadCommand, object> activeCommands = new ConcurrentDictionary<IMainThreadCommand, object>();

        public void Queue(IMainThreadCommand command, bool waitForCompletion = true)
        {
            if (isCancelled) return;
            
            queue.Enqueue(command);

            if (!waitForCompletion) return;
            
            activeCommands.TryAdd(command, null);
            command.WaitForCompletion();
            activeCommands.TryRemove(command, out _);
        }

        /// <inheritdoc>
        ///     <cref>IMainThreadCommandHandler.Queue</cref>
        /// </inheritdoc>
        [Obsolete("This overload will likely be removed in 2.0.")]
        public void Queue(Action action, bool waitForCompletion = true)
        {
            var command = new MainThreadCommand(action);
            queue.Enqueue(command);

            if (!waitForCompletion) return;
            
            activeCommands.TryAdd(command, null);
            command.WaitForCompletion();
            activeCommands.TryRemove(command, out _);
        }

        /// <inheritdoc cref="IMainThreadCommandHandler.Update"/>
        public void Update()
        {
            if (queue.Count == 0) return;
                
            while (queue.Count > 0)
            {
                queue.TryDequeue(out var command);
                command.Execute();
                if (!command.Done) queue.Enqueue(command);
            }
        }

        /// <summary>
        /// Stops executing any active commands as soon as possible.
        /// </summary>
        public void Cancel()
        {
            isCancelled = true;
            
            var commands = activeCommands.Keys;
            foreach (var command in commands)
            {
                if (!activeCommands.TryRemove(command, out _)) return;

                command.Cancel();
            }
        }
    }
}
