using System;
using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.InputSystem;
using MalbersAnimations.Utilities;
using UnityEngine.InputSystem;

#if AC_FISHNET


#elif AC_PURRNET
using PurrNet;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
	[AddComponentMenu("Malbers/Multiplayer/PlayerInstance")]
	public class PlayerInstance : AnimalInstance
    {
		public bool isPlayerOwned => isOwner;
		[SerializeField] private PlayerStatsEvents[] m_PlayerStatEvents;
		[SerializeField] protected GameObject player_component;
		protected IInputSource m_input;
		[SerializeField] protected Vector3Event m_OnTeleport = new();
		[SerializeField] protected BoolEvent m_OnGrounded = new();
		[SerializeField] protected BoolEvent m_OnFreeMove = new();
		[SerializeField] protected BoolEvent m_OnFlying = new();
		[SerializeField] protected BoolEvent m_OnDeath = new();

		protected override void Awake()
		{
			base.Awake();
			m_input = GetComponent<IInputSource>();
			m_input.Enable(false);
		}
		
        // Might need to save client here so we can removed it from old owner is er do this.
        protected override void UpdateOwner(bool asServer = false)
		{
			base.UpdateOwner(asServer);
			//Reset Tags
			AnimalTags.tags = new(m_defaultTags);
#if AC_FISHNET
			if(Owner.IsValid && ClientInstance.ClientDirectory.TryGetValue(Owner, out ClientInstance client)) client.UpdateCharacter(this);
#elif AC_PURRNET
			if(ClientInstance.ClientDirectory.TryGetValue((PlayerID)owner, out ClientInstance client)) client.UpdateCharacter(this);
#endif
			
			if(player_component != null) player_component.SetActive(isPlayerOwned);

			if(isPlayerOwned){
				(m_input as MInputLink).playerInput = PlayerInput.GetPlayerByIndex(0);
				foreach (PlayerStatsEvents item in m_PlayerStatEvents) { item.Connections(Vitals.Stat_Get(item.ID), true); }
			}else{
				foreach (PlayerStatsEvents item in m_PlayerStatEvents) { item.Connections(Vitals.Stat_Get(item.ID), false); }
			}

			GetComponent<NetAim>().UseCamera = isPlayerOwned;
			m_input.Enable(isPlayerOwned);

			
		}
		/* ShapeShift Disabled
		protected override void PostShapeShift()
		{
			base.PostShapeShift();
		}
		*/
		public override void setName()
		{
			gameObject.name = "PLAYER";
			NameAddID();
			if(isController) gameObject.name += "-LOCAL";
		}
		public void InvokeDeath(bool data)
		{
			if(!isPlayerOwned) return;

			m_OnDeath.Invoke(data);
		}
		public void InvokeFreeMove(bool data)
		{
			if(!isPlayerOwned) return;

			m_OnFreeMove.Invoke(data);
		}

		public void InvokeGrounded(bool data)
		{
			if(!isPlayerOwned) return;

			m_OnGrounded.Invoke(data);
		}

		public void InvokeFlying(bool data)
		{
			if(!isPlayerOwned) return;

			m_OnFlying.Invoke(data);
		}
		
		public void InvokeTeleport(Vector3 data)
		{
			if(!isPlayerOwned) return;

			m_OnTeleport.Invoke(data);
		}
#if AC_FISHNET
#elif AC_PURRNET
#endif
    }

	[Serializable]
		public struct PlayerStatsEvents
		{
			public StatID ID;
			//public UnityEvent OnStat;
			public FloatEvent OnValueChange;
			public FloatEvent OnValueChangeNormalized;
			public FloatEvent OnMaxValueChange;
			public BoolEvent OnActive;
			public UnityEvent OnStatFull;
			public UnityEvent OnStatEmpty;
			public BoolEvent OnRegenerate;
			public BoolEvent OnDegenerate;
			public UnityEvent OnStatBelow;
			public UnityEvent OnStatAbove;
			
			public void Connections(Stat target, bool link)
			{
				if(link){
					if(OnStatFull != null) target.OnStatFull.AddListener(OnStatFull.Invoke);
					if(OnStatEmpty != null) target.OnStatEmpty.AddListener(OnStatEmpty.Invoke);
					//if(OnStat != null) target.OnStat.AddListener(OnStat.Invoke);

					if(OnStatBelow != null) target.OnStatBelow.AddListener(OnStatBelow.Invoke);
					if(OnStatAbove != null) target.OnStatAbove.AddListener(OnStatAbove.Invoke);

					if(OnValueChangeNormalized != null) target.OnValueChangeNormalized.AddListener(OnValueChangeNormalized.Invoke);
					if(OnValueChange != null) target.OnValueChange.AddListener(OnValueChange.Invoke);
					if(OnMaxValueChange != null) target.OnMaxValueChange.AddListener(OnMaxValueChange.Invoke);
					if(OnDegenerate != null) target.OnDegenerate.AddListener(OnDegenerate.Invoke);
					if(OnRegenerate != null) target.OnRegenerate.AddListener(OnRegenerate.Invoke);
					if(OnActive != null) target.OnActive.AddListener(OnActive.Invoke);
				}else{
					if(OnStatFull != null) target.OnStatFull.RemoveListener(OnStatFull.Invoke);
					if(OnStatEmpty != null) target.OnStatEmpty.RemoveListener(OnStatEmpty.Invoke);

					if(OnStatBelow != null) target.OnStatBelow.RemoveListener(OnStatBelow.Invoke);
					if(OnStatAbove != null) target.OnStatAbove.RemoveListener(OnStatAbove.Invoke);

					if(OnValueChangeNormalized != null) target.OnValueChangeNormalized.RemoveListener(OnValueChangeNormalized.Invoke);
					if(OnValueChange != null) target.OnValueChange.RemoveListener(OnValueChange.Invoke);
					if(OnMaxValueChange != null) target.OnMaxValueChange.RemoveListener(OnMaxValueChange.Invoke);
					if(OnDegenerate != null) target.OnDegenerate.RemoveListener(OnDegenerate.Invoke);
					if(OnRegenerate != null) target.OnRegenerate.RemoveListener(OnRegenerate.Invoke);
					if(OnActive != null) target.OnActive.RemoveListener(OnActive.Invoke);
				}
			}
		}

	
#region Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerInstance))]
    public class PlayerIntEditor : AnimalIntEditor
    {
        private SerializedProperty
            StatsEvents, PlayerComp, RiderComp, onTeleport, onGrounded, onFreeMove, onFlying, onDeath
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
			#if AC_HAP
			RiderComp = serializedObject.FindProperty("m_Rider");
			#endif
			StatsEvents = serializedObject.FindProperty("m_PlayerStatEvents");
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
                EditorGUILayout.PropertyField(PlayerComp, new GUIContent("Player Component", "Game Object that holds all Player Componets that should only be active on the Players System"));
				#if AC_HAP
				EditorGUILayout.PropertyField(RiderComp, new GUIContent("Rider Component", "Rider Script refrance to allow the Player Instance controll over the scipt"));
				#endif
				EditorGUILayout.PropertyField(StatsEvents, new GUIContent("Stats UI Events", "Stats Events to be added on setup, these Events are for UI Calls that are required for the Player."));
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