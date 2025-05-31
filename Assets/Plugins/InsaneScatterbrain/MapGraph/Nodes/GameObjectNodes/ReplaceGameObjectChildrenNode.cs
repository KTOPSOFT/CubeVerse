using System;
using System.Collections;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// Destroys all children from the parent and replaces them with the provided children.
    /// </summary>
    [ScriptNode("Replace GameObject Children", "GameObjects"), Serializable]
    public class ReplaceGameObjectChildrenNode : ProcessorNode
    {
        [InPort("Parent", typeof(GameObject), true), SerializeField] 
        private InPort parentIn = null;
        
        [InPort("Children", typeof(GameObject[]), true), SerializeField]
        private InPort childrenIn = null;

        [InPort("Use XZ Plane", typeof(bool)), SerializeField]
        private InPort useXzPlaneIn = null;

        protected override IEnumerator OnProcessMainThreadCoroutine()
        {
            var parent = parentIn.Get<GameObject>();
            var childrenPrefabs = childrenIn.Get<GameObject[]>();
            var useXzPlane = useXzPlaneIn.Get<bool>();
            
            while (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(0).gameObject;
                Object.DestroyImmediate(child);

                yield return null;
            }

            foreach (var childPrefab in childrenPrefabs)
            {
                var instance = Object.Instantiate(childPrefab, parent.transform);

                if (useXzPlane)
                {
                    var localPosition = instance.transform.localPosition;
                    localPosition = new Vector3(
                        localPosition.x,
                        localPosition.z,
                        localPosition.y);
                    instance.transform.localPosition = localPosition;
                }

                instance.name = childPrefab.name;

                yield return null;
            }
        }
    }
}