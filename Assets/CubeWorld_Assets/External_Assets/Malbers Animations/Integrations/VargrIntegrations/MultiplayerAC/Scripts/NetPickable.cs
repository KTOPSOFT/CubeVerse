using UnityEngine;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    [AddComponentMenu("Malbers/Multiplayer/Interaction/NetPickable")]
    [SelectionBase]
    [RequireComponent(typeof(OwnershipSync))]
    public class NetPickable : Pickable
    {
        protected OwnershipSync m_Ownership;
        protected override void Awake()
        {
            DestroyRbOnPick.Value = false;
            m_Ownership = GetComponent<OwnershipSync>();
            if(PickDelay == 0) PickDelay.Value = 0.1f;
            base.Awake();

            OnFocusedBy.AddListener(Player_FocusedBy);
            OnUnfocusedBy.AddListener(Player_UnFocusedBy);

            OnPickedFailed.AddListener(Player_OnPickedFailed);

            OnPrePicked.AddListener(Player_OnPrePicked);
            OnPicked.AddListener(Player_OnPicked);

            OnPreDropped.AddListener(Player_OnPreDropped);
            OnDropped.AddListener(Player_OnDropped);

            m_Ownership.NetEventSync += RecivedEventSync;
            m_Ownership.OnOwnerChange.AddListener((data) => OnOwnerChange());
        }

        protected void OnOwnerChange()
        {
            if(m_Ownership.ownerObject == null){
                CanBePicked = true;
                RigidBody.isKinematic = m_Ownership.isController ? defaultKinematic : true;
            }else{
                CanBePicked = m_Ownership.isController;
                RigidBody.isKinematic = m_Ownership.isController ? defaultKinematic : true;
            }
        }

        protected override void DestroyPickUp()
        {
            if(m_Ownership.isController) Destroy(gameObject);
        }

        public virtual bool TryPickup(GameObject requester)
        {
            if(m_Ownership.TryOwning(requester)){
                return true;
            }else{
                OnPickedFailed.Invoke(requester);
                return false;
            }
        }

        public override void Drop()
        {
            OnDropEnablePhysics();
            IsPicked = false;
            enabled = true;
            SetParentScale(null, DefaultScale);

            var realPicker = Picker ? Picker.Root.gameObject : null;

            OnDropped.Invoke(realPicker);
            DroppedReaction?.React(realPicker);

            Picker = null;                                          //Reset who did the picking
            CurrentPickTime = Time.time;
            
            m_Ownership.ReturnOwnership();
        }

        protected void Net_Pick()
        {
            if (Collectable) enabled = false;
            if (DestroyOnPick) DestroyPickUp();
        }

        protected void Net_Drop()
        {
            OnDropEnablePhysics();
            IsPicked = false;
            enabled = true;
            Picker = null;
            CurrentPickTime = Time.time;
        }

        public void SetParentScale(Transform holder, Vector3 scale)
        {
            if(holder == null){
                m_Ownership.SetParent(null);
            }else{
                m_Ownership.SetParent(holder);
            }
            transform.localScale = scale;
        }

        protected void Update()
        {
            if(m_Ownership.isServer && m_Ownership.ownerObject == null && !IsPicked)
            {
                if(RigidBody.isKinematic) RigidBody.isKinematic = defaultKinematic;
            }
        }

        #region Events
        public void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.Pickable) return;
            if(m_Ownership.isController) return;

            switch(index)
            {
                case 1: OnFocusedBy.Invoke((GameObject)data); break;
                case 2: OnUnfocusedBy.Invoke((GameObject)data); break;
                case 3: OnPickedFailed.Invoke((GameObject)data); break;
                case 4: OnPrePicked.Invoke((GameObject)data); break;
                case 5: 
                    OnPicked.Invoke((GameObject)data);
                    Net_Pick();
                    break;
                case 6: OnPreDropped.Invoke((GameObject)data); break;
                case 7: 
                    OnDropped.Invoke((GameObject)data);
                    Net_Drop();
                    break;
            }
        }

        [SerializeField] protected BoolEvent m_PlayerOnFocused;
        [SerializeField] protected GameObjectEvent m_PlayerOnFocusedBy;
        [SerializeField] protected GameObjectEvent m_PlayerOnUnfocusedBy;
        [SerializeField] protected GameObjectEvent m_PlayerOnPickedFailed;
        [SerializeField] protected GameObjectEvent m_PlayerOnPicked;
        [SerializeField] protected GameObjectEvent m_PlayerOnPrePicked;
        [SerializeField] protected GameObjectEvent m_PlayerOnDropped;
        [SerializeField] protected GameObjectEvent m_PlayerOnPreDropped;

        public void Net_FocusedBy(GameObject target) => m_Ownership.EventBroadcast(target, 1, NetEventSource.Pickable);
        public void Net_UnFocusedBy(GameObject target) => m_Ownership.EventBroadcast(target, 2, NetEventSource.Pickable);
        public void Net_OnPickedFailed(GameObject target) => m_Ownership.EventBroadcast(target, 3, NetEventSource.Pickable);  
        public void Net_OnPrePicked(GameObject target) => m_Ownership.EventBroadcast(target, 4, NetEventSource.Pickable);
        public void Net_OnPicked(GameObject target) => m_Ownership.EventBroadcast(target, 5, NetEventSource.Pickable);
        public void Net_OnPreDropped(GameObject target) => m_Ownership.EventBroadcast(target, 6, NetEventSource.Pickable);
        public void Net_OnDropped(GameObject target) => m_Ownership.EventBroadcast(target, 7, NetEventSource.Pickable);
        public void Net_EquipSync(GameObject target)
        {
            if(m_Ownership.isController) return;

            MWeaponManager manager = target.FindComponent<MWeaponManager>();
            if(manager != null){
                if(manager.UseHolsters){
                    manager.Holster_SetWeapon(gameObject);
                }else{
                    // External Not Needed Currently
                }
            }
            if (Collectable) enabled = false;
        }

        protected void Player_FocusedBy(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnFocused.Invoke(true);
            if(player.isOwner && player is PlayerInstance) m_PlayerOnFocusedBy.Invoke(target);
        }
        protected void Player_UnFocusedBy(GameObject target)
        {
            if(target == null) return;
            //Debug.Log("Un Focused By" + target.name);
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnFocused.Invoke(false);
            if(player.isOwner && player is PlayerInstance) m_PlayerOnUnfocusedBy.Invoke(target);
        }

        protected void Player_OnPickedFailed(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnPickedFailed.Invoke(target);
        }

        protected void Player_OnPrePicked(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnPrePicked.Invoke(target);
        }

        protected void Player_OnPicked(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnPicked.Invoke(target);
        }

        protected void Player_OnPreDropped(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnPreDropped.Invoke(target);
        }

        protected void Player_OnDropped(GameObject target)
        {
            if(target == null) return;
            AnimalInstance player = target.FindComponent<AnimalInstance>();
            if(player == null) return;

            if(player.isOwner && player is PlayerInstance) m_PlayerOnDropped.Invoke(target);
        }
        #endregion
    }
    //INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(NetPickable)), CanEditMultipleObjects]
    public class NetPickableEditor : PickableEditor
    {
        private SerializedProperty
            EditorTabs,
            PlayerOnFocused, PlayerOnPrePicked, PlayerOnPicked, PlayerOnPreDropped, PlayerOnDropped, PlayerOnFocusedBy, PlayerOnUnFocusedBy, PlayerOnPickedFailed
        ;
        
        protected void FindProperties()
        {
            PlayerOnFocused = serializedObject.FindProperty("m_PlayerOnFocused");
            PlayerOnFocusedBy = serializedObject.FindProperty("m_PlayerOnFocusedBy");
            PlayerOnUnFocusedBy = serializedObject.FindProperty("m_PlayerOnUnfocusedBy");
            PlayerOnPickedFailed = serializedObject.FindProperty("m_PlayerOnPickedFailed");
            PlayerOnPrePicked = serializedObject.FindProperty("m_PlayerOnPrePicked");
            PlayerOnPicked = serializedObject.FindProperty("m_PlayerOnPicked");
            PlayerOnPreDropped = serializedObject.FindProperty("m_PlayerOnPreDropped");
            PlayerOnDropped = serializedObject.FindProperty("m_PlayerOnDropped"); 

            EditorTabs = serializedObject.FindProperty("EditorTabs"); 
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            FindProperties();

            int Selection = EditorTabs.intValue;

            if (Selection == 1) DrawPlayerEvents();
            
            serializedObject.ApplyModifiedProperties();  
        }

        protected virtual void DrawPlayerEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MalbersEditor.DrawHeader("Player Events");

                EditorGUILayout.PropertyField(PlayerOnFocused);
                EditorGUILayout.PropertyField(PlayerOnFocusedBy);
                EditorGUILayout.PropertyField(PlayerOnUnFocusedBy);
                if ((target as Pickable).PickDelay > 0 || (target as Pickable).m_ByAnimation.Value)
                    EditorGUILayout.PropertyField(PlayerOnPrePicked, new GUIContent("On Pre-Picked By"));
                EditorGUILayout.PropertyField(PlayerOnPicked, new GUIContent("On Picked By"));
                if ((target as Pickable).DropDelay > 0 || (target as Pickable).m_ByAnimation.Value)
                    EditorGUILayout.PropertyField(PlayerOnPreDropped, new GUIContent("On Pre-Dropped By"));
                EditorGUILayout.PropertyField(PlayerOnDropped, new GUIContent("On Dropped By"));

                EditorGUILayout.PropertyField(PlayerOnPickedFailed);
            }
        }
    }
#endif
}