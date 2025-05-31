using UnityEngine;

#if AC_FISHNET
using FishNet.Object;
using FishNet.Object.Synchronizing;
#elif AC_PURRNET
using PurrNet;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class RigidBodyUtility : NetworkBehaviour
    {
        protected Rigidbody m_rigidbody;
#if AC_FISHNET
        public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
		public bool isServer => IsServerStarted;
		protected readonly SyncVar<Vector3> m_velocity = new SyncVar<Vector3>(new SyncTypeSettings(ReadPermission.ExcludeOwner));
		[ServerRpc(RunLocally = true)] protected void CmdVelocityRpc(Vector3 value) => m_velocity.Value = value;
        public Vector3 Velocity
		{
			get
			{
				return m_velocity.Value;
			}
			set
			{
				if(m_velocity.Value == value) return;

				if(isServer && !Owner.IsValid){
					m_velocity.Value = value;
				}else{
					if(isController) CmdVelocityRpc(value);
				}
			}
		}
#elif AC_PURRNET
        protected SyncVar<Vector3> m_velocity = new SyncVar<Vector3>(Vector3.zero, ownerAuth: true);
        public Vector3 Velocity
		{
			get
			{
				return m_velocity.value;
			}
			set
			{
				if(m_velocity.value == value) return;
				if(!isController) return;

				m_velocity.value = value;
			}
		}
#endif

        protected void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }
#if UNITY_6000_0_OR_NEWER
        public void SaveVelocity()
        {
			if(isController) Velocity = m_rigidbody.linearVelocity;
        }

        public void LoadVelocity()
        {
			if(!isController){
                m_rigidbody.isKinematic = true;
            }else{
                m_rigidbody.isKinematic = false;
                m_rigidbody.linearVelocity = Velocity;
            }
        }
#else
        public void SaveVelocity()
        {
            if(isController) Velocity = m_rigidbody.velocity;
        }

        public void LoadVelocity()
        {
            if(!isController){
                m_rigidbody.isKinematic = true;
            }else{
                m_rigidbody.isKinematic = false;
                m_rigidbody.velocity = Velocity;
            }
        }
#endif
    }
}
