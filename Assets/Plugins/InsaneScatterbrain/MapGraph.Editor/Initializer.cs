using System.Threading.Tasks;
using UnityEditor;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [InitializeOnLoad]
    public static class Initializer
    {
        /// <summary>
        /// Runs the update checker each time Unity loads.
        /// </summary>
        static Initializer()
        {
            UpdateChecker.CheckForNewUpdatesAsync().ContinueWith(task =>
            {
                var newVersionAvailable = task.Result;
            
                if (!newVersionAvailable) return;
                
                NewUpdatesWindow.ShowWindow();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}