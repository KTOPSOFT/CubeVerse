using UnityEngine;
using UnityEngine.Pool;

namespace MalbersAnimations
{
    public interface IPoolGameObject
    {
        IObjectPool<GameObject> Pool { get; set; }

        void Pool_Release(GameObject gameObject);
        GameObject Pool_Get();
    }
}