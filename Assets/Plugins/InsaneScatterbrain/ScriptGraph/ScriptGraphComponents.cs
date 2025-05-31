using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph
{
    /// <summary>
    /// This class manages temporary Unity components that are required when processing a script graph.
    /// </summary>
    public static class ScriptGraphComponents
    {
        private static GameObject container;

        private static void InitializeContainer()
        {
            // Always make sure there's only one script graph components container.
            if (container == null)
            {
                container = new GameObject("Script Graph Components [Temp]")
                {
                    hideFlags = HideFlags.DontSave
                };
            }
        }

        /// <summary>
        /// Creates a GameObject with the given component type and returns that component.
        /// </summary>
        /// <typeparam name="T">The requested component type.</typeparam>
        /// <returns>The temporary component.</returns>
        public static T Get<T>() where T : Component
        {
            InitializeContainer();
            
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.transform.SetParent(container.transform);
            var component = gameObject.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// Marks a Unity GameObject as temporary, so that it gets disposed of after a script graph has been 
        /// </summary>
        /// <param name="obj"></param>
        public static void RegisterTemporaryObject(GameObject obj)
        {
            InitializeContainer();
            
            obj.transform.SetParent(container.transform);
        }

        /// <summary>
        /// Destroys the temporary container and all the temporary GameObjects in it.
        /// </summary>
        public static void Clear()
        {
            if (container == null) return;
            
            Object.DestroyImmediate(container);

            container = null;
        }
    }
}