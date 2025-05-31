using MalbersAnimations.Events;
using UnityEngine;

using MalbersAnimations.HAP;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
/******
ADD TO Mount

public void Awake() => public virtual void Awake()

public virtual MRider NearbyRider { get; set; }

******/

namespace MalbersAnimations.VargrMultiplayer
{
    public class NetMount : Mount
    {
        protected AnimalInstance m_NetAnimal;
        protected OwnershipSync m_Ownership;
        private bool m_death = false;
        public bool ReturnOnDismount = true;

        public List<MRider> m_nearbyRiders = new List<MRider>();

        public bool CheckNearbyRider(MRider mRider)
        {
            if(mRider != null && m_nearbyRiders.Contains(mRider)) return true;

            return false;
        }

        public MRider GetNearbyRider(MRider mRider)
        {
            return m_nearbyRiders.Find(x => x == mRider);
        }

        public override MRider NearbyRider
        {
            get
            {
                return null;
            }
            set
            {
                if(value != null){
                    if(!m_nearbyRiders.Contains(value)) m_nearbyRiders.Add(value);
                }
            }
        }

        public void JoinSync(GameObject rider, bool isMounted)
        {
            if(m_NetAnimal.isController) return;
            
            Rider = rider != null ? rider.GetComponent<MRider>() : null;
            mounted = isMounted;
        }

        public void MountDeath()
		{
			if(!m_NetAnimal.isController || !Mounted) return;
            Rider.DismountAnimal();
            m_death = true;
		}

        public bool TryMount(GameObject requester)
        {
            if(m_Ownership != null){
                return m_Ownership.TryOwning(requester);
            }else{
                return m_NetAnimal.isOwner;
            }
        }

        public override void StartMounting(MRider rider) //** Leaving as is for now
        {
            base.StartMounting(rider);
        }

        public override void End_Mounting() //** Leaving as is for now
        {
            base.End_Mounting();
        }

        public override void Start_Dismounting() //** Leaving as is for now
        {
            base.Start_Dismounting();
        }

        public override void EndDismounting() //** Just added a return to Server if return is on
        {
            base.EndDismounting();
            if(m_death) Animal.State_Activate(10);
            if(m_NetAnimal.isController && ReturnOnDismount) m_Ownership.ReturnOwnership();
        }

        protected override void OnDisable()
        {
            Animal.OnStateActivate.RemoveListener(AnimalStateChange);
            Animal.OnSpeedChange.RemoveListener(SetAnimatorSpeed);

            AllMounts.Remove(this);
            foreach(var rider in m_nearbyRiders){
                rider.MountTriggerExit();
            }
        }

        public virtual void ExitMountTrigger(NetRider netRider)
        {
            if(m_nearbyRiders.Contains(netRider)){
                OnCanBeMounted.Invoke(false);
                m_nearbyRiders.Remove(netRider);
            }

            base.ExitMountTrigger();
        }

        #region UnityEvents
        [SerializeField] protected GameObjectEvent m_PlayerOnMounted = new();
        [SerializeField] protected GameObjectEvent m_PlayerOnDismounted = new();
        [SerializeField] protected BoolEvent m_PlayerOnCanBeMounted = new();
        [SerializeField] protected GameObjectEvent m_PlayerOnCalled = new();
        #endregion
        
        public override void Awake()
        {
            m_NetAnimal = Animal.GetComponent<AnimalInstance>();
            m_Ownership = Animal.GetComponent<OwnershipSync>();

            base.Awake();

            OnMounted.AddListener(PlayerOnMounted);
            OnDismounted.AddListener(PlayerOnDismounted);
            OnCanBeMounted.AddListener(PlayerOnCanBeMounted);
            OnCalled.AddListener(PlayerOnCalled);
        }

        protected void PlayerOnMounted(GameObject target)
        {
            AnimalInstance animal = target.FindComponent<AnimalInstance>();
            if(animal == null) return;
            if(animal.isOwner && animal is PlayerInstance) m_PlayerOnMounted.Invoke(target);
        }

        protected void PlayerOnDismounted(GameObject target)
        {
            AnimalInstance animal = target.FindComponent<AnimalInstance>();
            if(animal == null) return;
            if(animal.isOwner && animal is PlayerInstance) m_PlayerOnDismounted.Invoke(target);
        }

        protected void PlayerOnCanBeMounted(bool target)
        {
            if(ClientInstance.Instance == null || ClientInstance.Instance.PlayerInstance == null) return;
            MRider localPlayer = ClientInstance.Instance.PlayerInstance.GetComponent<NetRider>();
            if(localPlayer == null || !m_nearbyRiders.Contains(localPlayer)) return;
            m_PlayerOnCanBeMounted.Invoke(target);
        }

        protected void PlayerOnCalled(GameObject target)
        {
            AnimalInstance animal = target.FindComponent<AnimalInstance>();
            if(animal == null) return;
            if(animal.isOwner && animal is PlayerInstance) m_PlayerOnCalled.Invoke(target);
        }
    }
#region INSPECTOR
#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(NetMount))]
    public class NetMountEd : MountEd
    {
        private SerializedProperty
            //NetSetWeapon, 
            Editor_Tabs1,Editor_Tabs2,
            PlayerOnMount, PlayerOnDismount, PlayerOnCanBeMounted, PlayerOnCalled,
            NetLockToOwner, NetReturnOnDismount
            ;
        
        protected void FindProperties()
        {
            PlayerOnMount = serializedObject.FindProperty("m_PlayerOnMounted");
            PlayerOnDismount = serializedObject.FindProperty("m_PlayerOnDismounted");
            PlayerOnCanBeMounted = serializedObject.FindProperty("m_PlayerOnCanBeMounted");
            PlayerOnCalled = serializedObject.FindProperty("m_PlayerOnCalled");
            //NetLockToOwner = serializedObject.FindProperty("LockToOwner");
            NetReturnOnDismount = serializedObject.FindProperty("ReturnOnDismount");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            FindProperties();

            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) ShowNetGeneral();

            Selection = Editor_Tabs2.intValue;
            if (Selection == 1) DrawPlayerEvents();

            serializedObject.ApplyModifiedProperties();  
        }
        
        protected virtual void DrawPlayerEvents()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Player Events", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(PlayerOnMount);
                EditorGUILayout.PropertyField(PlayerOnDismount);
                EditorGUILayout.PropertyField(PlayerOnCanBeMounted);
                EditorGUILayout.PropertyField(PlayerOnCalled);
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void ShowNetGeneral()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Network General", EditorStyles.boldLabel);
                }
                EditorGUILayout.EndHorizontal();
                //EditorGUILayout.PropertyField(NetLockToOwner, new GUIContent("Lock to Owner", "Locks the Mount to the Owner once mounted."));
                EditorGUILayout.PropertyField(NetReturnOnDismount, new GUIContent("Return on Dismount", "Returns Ownership to Server on Dismount"));
            }
            EditorGUILayout.EndVertical();
        }
    }
#endif
#endregion
}