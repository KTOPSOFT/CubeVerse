using UnityEngine;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    [AddComponentMenu("Malbers/Multiplayer/Interaction/Net Pick Up - Drop")]
    public class NetPickUp : MPickUp
    {        
        protected AnimalInstance m_netAnimal;
        protected EventSync m_eventSync;

        public bool isController => m_netAnimal != null ? m_netAnimal.isController : false;
        public bool isPlayerOwned => m_netAnimal == null ? false : (m_netAnimal is PlayerInstance playerInstance && playerInstance.isPlayerOwned);

        protected override void Awake()
        {
            m_netAnimal = GetComponentInParent<AnimalInstance>();
            m_eventSync = gameObject.FindComponent<EventSync>();

            base.Awake();

            CanPickUp.AddListener((data) => { if(isPlayerOwned) m_PlayerCanPickUp.Invoke(data); } );
            OnItemPicked.AddListener((data) => { if(isPlayerOwned) m_PlayerOnItemPicked.Invoke(data); } );
            OnItemDrop.AddListener((data) => { if(isPlayerOwned) m_PlayerOnItemDrop.Invoke(data); } );
            OnFocusedItem.AddListener((data) => { if(isPlayerOwned) m_PlayerOnFocusedItem.Invoke(data); } );
            OnPicking.AddListener((data) => { if(isPlayerOwned) m_PlayerOnPicking.Invoke(data); } );
            OnDropping.AddListener((data) => { if(isPlayerOwned) m_PlayerOnDropping.Invoke(data); } );

            m_eventSync.NetEventSync += RecivedEventSync;
            
        }

        public override void TryDrop()
        {
            if(!isActiveAndEnabled || !isController) return;
            
            base.TryDrop();
        }
        
        public override void TryPickUp()
        {
            if(!isActiveAndEnabled || !isController) return;

            if (FocusedItem && FocusedItem is NetPickable netFocused)
                if(!netFocused.TryPickup(character.gameObject)) return;

            base.TryPickUp();
        }
        public override void DropItem()
        {
            if(!isActiveAndEnabled || !isController) return;

            base.DropItem();
        }

        protected override void ParentItemToHolster()
        {
            if(Item is NetPickable netItem){
            
                var Holder = this.Holder;
                var PosOffset = this.PosOffset;
                var RotOffset = this.RotOffset;

                //Use extra holders 
                if (netItem.holder > -1 && netItem.holder < extraHolders.Count)
                {
                    Holder = extraHolders[netItem.holder].transform;
                    PosOffset = extraHolders[netItem.holder].position;
                    RotOffset = extraHolders[netItem.holder].rotation;
                }

                if (Holder)
                {             
                    var localScale = netItem.transform.localScale;

                    netItem.SetParentScale(Holder, localScale);
                    netItem.transform.localPosition = PosOffset;       //Offset the Position
                    netItem.transform.localEulerAngles = RotOffset;    //Offset the Rotation
                }
            }else{
                base.ParentItemToHolster();
            }
        }

        #region Events
        [SerializeField] protected BoolEvent m_PlayerCanPickUp;
        [SerializeField] protected GameObjectEvent m_PlayerOnItemPicked;
        [SerializeField] protected GameObjectEvent m_PlayerOnItemDrop;
        [SerializeField] protected GameObjectEvent m_PlayerOnFocusedItem;
        [SerializeField] protected IntEvent m_PlayerOnPicking;
        [SerializeField] protected IntEvent m_PlayerOnDropping;

        protected virtual void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.PickUp) return;
            if(!isController) return;
            switch(index)
            {
                case 0: CanPickUp.Invoke((bool)data); break;
                case 1: OnItemPicked.Invoke((GameObject)data); break;
                case 2: OnItemDrop.Invoke((GameObject)data); break;
                case 3: OnFocusedItem.Invoke((GameObject)data); break;
                case 4: OnPicking.Invoke((int)data); break;
                case 5: OnDropping.Invoke((int)data); break;
            }
        }
        
        public void Net_CanPickUp(bool target) => m_eventSync?.EventBroadcast(target, 0, NetEventSource.PickUp);
        public void Net_OnItemPicked(GameObject target) => m_eventSync?.EventBroadcast(target, 1, NetEventSource.PickUp);
        public void Net_OnItemDrop(GameObject target) => m_eventSync?.EventBroadcast(target, 2, NetEventSource.PickUp);
        public void Net_OnFocusedItem(GameObject target) => m_eventSync?.EventBroadcast(target, 3, NetEventSource.PickUp);
        public void Net_OnPicking(int target) => m_eventSync?.EventBroadcast(target, 4, NetEventSource.PickUp);
        public void Net_OnDropping(int target) => m_eventSync?.EventBroadcast(target, 5, NetEventSource.PickUp);
        
		#endregion

    }
    #region INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(NetPickUp)), CanEditMultipleObjects]
    public class NetPickUpEditor : MPickUpEditor
    {
        private SerializedProperty
            PlayerCanPickUp, PlayerOnItemPicked, PlayerOnItemDrop, PlayerOnFocusedItem, PlayerOnPicking, PlayerOnDropping,
            Editor_Tabs1
        ;
        
        protected void FindProperties()
        {
            PlayerCanPickUp = serializedObject.FindProperty("m_PlayerCanPickUp");
            PlayerOnItemPicked = serializedObject.FindProperty("m_PlayerOnItemPicked");
            PlayerOnItemDrop = serializedObject.FindProperty("m_PlayerOnItemDrop");
            PlayerOnFocusedItem = serializedObject.FindProperty("m_PlayerOnFocusedItem");
            PlayerOnPicking = serializedObject.FindProperty("m_PlayerOnPicking");
            PlayerOnDropping = serializedObject.FindProperty("m_PlayerOnDropping");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            FindProperties();

            int Selection = Editor_Tabs1.intValue;

            if (Selection == 1) DrawPlayerEvents();
            
            serializedObject.ApplyModifiedProperties();  
        }

        protected virtual void DrawPlayerEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MalbersEditor.DrawHeader("Player Events");

                EditorGUILayout.PropertyField(PlayerCanPickUp);
                EditorGUILayout.PropertyField(PlayerOnItemPicked);
                EditorGUILayout.PropertyField(PlayerOnItemDrop);
                EditorGUILayout.PropertyField(PlayerOnFocusedItem);
                EditorGUILayout.PropertyField(PlayerOnPicking);
                EditorGUILayout.PropertyField(PlayerOnDropping);
            }
        }
    }
    
#endif
    #endregion
}