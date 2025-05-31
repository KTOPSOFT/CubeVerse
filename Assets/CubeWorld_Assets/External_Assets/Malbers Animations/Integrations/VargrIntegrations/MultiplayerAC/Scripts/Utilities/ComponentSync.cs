using UnityEngine;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class ComponentSync : NetworkBehaviour
    {
        [SerializeField] protected MonoBehaviour[] m_severComponents;
        [SerializeField] protected MonoBehaviour[] m_observerComponents;
        
        // Start is called before the first frame update
#if AC_FISHNET
        public override void OnStartServer()
        {
            foreach (MonoBehaviour item in m_severComponents)
            {
                item.enabled = true;
            }

            foreach (MonoBehaviour item in m_observerComponents)
            {
                item.enabled = false;
            }
        }
        public override void OnStartClient()
        {
            if(!IsServerStarted){
                foreach (MonoBehaviour item in m_severComponents)
                {
                    item.enabled = false;
                }
            }

            foreach (MonoBehaviour item in m_observerComponents)
            {
                item.enabled = true;
            }
        }       
#elif AC_PURRNET
        protected override void OnSpawned()
        {
            foreach (MonoBehaviour item in m_severComponents)
            {
                item.enabled = isServer;
            }

            foreach (MonoBehaviour item in m_observerComponents)
            {
                item.enabled = isClient;
            }
        }
#endif
    }
}
