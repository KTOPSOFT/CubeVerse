using UnityEngine;
using MalbersAnimations.Events;

#if AC_FISHNET
using FishNet.Object;
#elif AC_PURRNET
using PurrNet;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class MountInstance : AIInstance
    {
        public NetMount Mount
		{
			get => m_Mount;
			protected set {
				m_Mount = value;
			}	
		}
		[SerializeField] protected NetMount m_Mount;
        [SerializeField] private PlayerStatsEvents[] m_UIStatEvents;
        [SerializeField] protected GameObject player_component;
        [SerializeField] protected Vector3Event m_OnTeleport = new();
		[SerializeField] protected BoolEvent m_OnGrounded = new();
		[SerializeField] protected BoolEvent m_OnFreeMove = new();
		[SerializeField] protected BoolEvent m_OnFlying = new();
		[SerializeField] protected BoolEvent m_OnDeath = new();

        protected override void Awake()
        {
			player_component.SetActive(false);
            base.Awake();
        }

#if UNITY_EDITOR
	#if AC_FISHNET
        protected override void OnValidate() {
            base.OnValidate();
			if(m_Mount != null){
                if(!m_Mount.MountPoint.gameObject.GetComponent<NetworkBehaviour>()){
                    m_Mount.MountPoint.gameObject.AddComponent<EmptyNetworkBehaviour>();
                }
            }else{
                Debug.LogError("Mount is missing Mount Script", this);
            }
		}
	#else
        protected void OnValidate() {
            if(m_Mount == null) m_Mount = this.FindComponent<NetMount>();
		}
	#endif
#endif

        public void MountUIUpdate(bool mounted)
        {
            if(isOwner && mounted && IsRiderPlayer()){
				player_component.SetActive(true);
                foreach (PlayerStatsEvents item in m_UIStatEvents) { item.Connections(Vitals.Stat_Get(item.ID), true); }
			}else{
                player_component.SetActive(false);
				foreach (PlayerStatsEvents item in m_UIStatEvents) { item.Connections(Vitals.Stat_Get(item.ID), false); }
			}
		}

		private bool IsRiderPlayer()
		{
			if(m_Mount.Rider == null) return false;
			if(m_Mount.Rider is NetRider nRider) return nRider.isPlayerOwned;
			return false;
		}

		public override void setName()
		{
			gameObject.name = "MOUNT";
			NameAddID();
			if(isController) gameObject.name += "-LOCAL";
		}
    }
#region Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(MountInstance))]
    public class MountIntEditor : AIIntEditor
    {
        private SerializedProperty
            StatsEvents, PlayerComp, MountComp, onTeleport, onGrounded, onFreeMove, onFlying, onDeath
        ;

		protected override void OnEnable()
        {
			base.OnEnable();

			onTeleport = serializedObject.FindProperty("m_OnTeleport");
			onGrounded = serializedObject.FindProperty("m_OnGrounded");
			onFreeMove = serializedObject.FindProperty("m_OnFreeMove");
			onFlying = serializedObject.FindProperty("m_OnFlying");
			onDeath = serializedObject.FindProperty("m_OnDeath");
			PlayerComp = serializedObject.FindProperty("player_component");
			MountComp = serializedObject.FindProperty("m_Mount");
			StatsEvents = serializedObject.FindProperty("m_UIStatEvents");
		}

		protected override void DrawAnimalHeader()
		{
			MalbersEditor.DrawDescription("Player Instance");
		}

		protected override void DrawGeneral()
		{
			base.DrawGeneral();
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(PlayerComp, new GUIContent("Player Component", "Game Object that holds all Player UI Componets that should only be active on the Players System"));
				EditorGUILayout.PropertyField(MountComp, new GUIContent("Mount Component", "Mount Script refrance to allow the Player Instance controll over the script"));
				EditorGUILayout.PropertyField(StatsEvents, new GUIContent("Stats UI Events", "Stats Events to be added on setup, these Events are for UI Calls that are required for the Owner of the object."));
            }
		}
		protected override void DrawEvents()
		{
			base.DrawEvents();
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(onTeleport, new GUIContent("On Teleport", "Place UI/Camera Related Call here and link InvokeTeleport() to MAnimal to make sure UI Events are only on Owners System"));
				EditorGUILayout.PropertyField(onGrounded, new GUIContent("On Grounded", "Place UI/Camera Related Call here and link InvokeGrounded() to MAnimal to make sure UI Events are only on Owners System"));
				EditorGUILayout.PropertyField(onFreeMove, new GUIContent("On Free Move", "Place UI/Camera Related Call here and link InvokeFreeMove() to MAnimal to make sure UI Events are only on Owners System"));
				EditorGUILayout.PropertyField(onFlying, new GUIContent("On Flying", "Place UI/Camera Related Call here and link InvokeFlying() to MAnimal to make sure UI Events are only on Owners System"));
				EditorGUILayout.PropertyField(onDeath, new GUIContent("On Death", "Place UI/Camera Related Call here and link InvokeDeath() to MAnimal to make sure UI Events are only on Owners System"));
            }
		}
    }
#endif
#endregion
}