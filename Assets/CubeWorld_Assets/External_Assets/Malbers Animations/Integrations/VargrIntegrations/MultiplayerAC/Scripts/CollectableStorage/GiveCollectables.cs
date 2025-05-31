using UnityEngine;
using UnityEngine.Events;
using VargrIntegrations;

namespace MalbersAnimations.VargrMultiplayer
{
    [AddComponentMenu("Malbers/Fishnet/Give Collectable")]
    public class GiveCollectables : MonoBehaviour 
    {
        public ScriptableObject collectable;
        public int count;

        public UnityEvent OnSuccess  = new();
        public UnityEvent OnFailed = new();
#if UNITY_EDITOR
        protected void OnValidate() {
			if(RSOManager.GetKey(collectable) == 0) RSOManager.Database.Add(collectable);
		}
#endif

        public void GiveCollectable(Collider target) => GiveCollectable(target.gameObject);
        public void GiveCollectable(GameObject target)
        {
            NetCollectables collection = target.FindComponent<NetCollectables>();
            if(collection == null || !collection.isController) return;
            
            if(collection.UpdateCollectable(collectable, count))
            {
                OnSuccess?.Invoke();
            }else{
                OnFailed?.Invoke();
            }
        }
    }
}