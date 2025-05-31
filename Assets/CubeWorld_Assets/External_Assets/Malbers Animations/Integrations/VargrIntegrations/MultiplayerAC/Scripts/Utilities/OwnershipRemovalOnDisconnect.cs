#if AC_FISHNET
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class OwnershipRemovalOnDisconnect : NetworkBehaviour
    {
        #if AC_FISHNET
        public override void OnStartServer()
        {
            ServerManager.Objects.OnPreDestroyClientObjects += OnPreDestroyClientObjects;
        }

        public override void OnStopServer()
        {
            if (ServerManager)
                ServerManager.Objects.OnPreDestroyClientObjects -= OnPreDestroyClientObjects;
        }

        private void OnPreDestroyClientObjects(NetworkConnection conn)
        {
            if (conn == Owner)
                RemoveOwnership();
        }
        #endif
    }
}