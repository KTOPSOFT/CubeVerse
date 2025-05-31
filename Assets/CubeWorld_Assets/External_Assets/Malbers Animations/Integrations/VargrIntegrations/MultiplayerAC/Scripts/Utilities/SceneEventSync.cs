using VargrIntegrations;
using UnityEngine;
using System.Collections.Generic;


#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public partial class SceneEventSync : TriggerEventSync
    {
        #if UNITY_EDITOR
        public List<TriggerEventSyncUser> userList = new List<TriggerEventSyncUser>();
        #endif
    }
}
