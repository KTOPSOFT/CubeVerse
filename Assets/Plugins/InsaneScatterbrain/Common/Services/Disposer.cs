using System;
using System.Collections.Concurrent;

namespace InsaneScatterbrain.Services
{
    /// <summary>
    /// Class to keep track of objects that should all be disposed at some point, when Dispose is called.
    /// </summary>
    public class Disposer
    {
        private readonly ConcurrentStack<IDisposable> disposables = new ConcurrentStack<IDisposable>();

        /// <summary>
        /// Adds the object for disposing.
        /// </summary>
        /// <param name="disposable">The disposable object.</param>
        public void Add(IDisposable disposable)
        {
            disposables.Push(disposable);
        }
        
        /// <summary>
        /// Disposes all added disposable objects.
        /// </summary>
        public void Dispose()
        {
            while (disposables.Count > 0)
            {
                disposables.TryPop(out var obj);
                obj?.Dispose();
            }
        }
    }
}