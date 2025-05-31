using MalbersAnimations.Events;
using MalbersAnimations.Weapons;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

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
    [AddComponentMenu("Malbers/Multiplayer/Net Weapon Manager")]
    public class NetMWeaponManager : MWeaponManager
    {
        [HideInInspector] public bool HolsterFromInv = false;
        public bool isController => m_netAnimal != null ? m_netAnimal.isController : false;
        public bool isPlayerOwned => m_netAnimal == null ? false : (m_netAnimal is PlayerInstance playerInstance && playerInstance.isPlayerOwned);
        public System.Action<int> SetActiveHolster { get; set; } = delegate { };
        public System.Action<float> WeaponChargeSync { get; set; } = delegate { };
        public System.Action<IDs, int> OnAmmoUpdate { get; set; } = delegate { };
        public event System.Action<IDs, int> AmmoConsummed;
        protected AnimalInstance m_netAnimal;
        protected EventSync m_eventSync;
        public override MWeapon Weapon
        {
            get => m_weapon;
            set
            {
                base.Weapon = value;
                WeaponChanged(m_weapon);
            }
        }

        protected override void Awake()
        {
            m_netAnimal = GetComponent<AnimalInstance>();
            m_eventSync = gameObject.FindComponent<EventSync>();
            base.Awake();

            OnCombatMode.AddListener((data) => { if(isPlayerOwned) m_PlayerOnCombatMode.Invoke(data); } );
            OnCanAim.AddListener((data) => { if(isPlayerOwned) m_PlayerOnCanAim.Invoke(data); } );
            OnEquipWeapon.AddListener((data) => { if(isPlayerOwned) m_PlayerOnEquipWeapon.Invoke(data); } );
            OnUnequipWeapon.AddListener((data) => { if(isPlayerOwned) m_PlayerOnUnequipWeapon.Invoke(data); } );
            OnWeaponAction.AddListener((data) => { if(isPlayerOwned) m_PlayerOnWeaponAction.Invoke(data); } );

            m_eventSync.NetEventSync += RecivedEventSync;

            if(UseExternal){
                // Force these as they need to be world Objects
                InstantiateOnEquip = true;
                DestroyOnUnequip = true;
                DestroyOnDrop = true;
            }
            
            if(UseHolsters){
                SetActiveHolster += NetActiveHolster;
            }

            WeaponChargeSync += SetWeaponCharge;
        }
        protected virtual void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.WeaponManager || target != NetEventSource.ActiveWeapon) return;
            if(isController) return;

            if(target == NetEventSource.ActiveWeapon){
                if(Weapon == null) return;

                Weapon.GetComponent<WeaponUtility>().RecivedEventSync(data, index);
            }else{
                switch(index)
                {
                    case 0: OnCombatMode.Invoke((bool)data); break;
                    case 1: OnCanAim.Invoke((bool)data); break;
                    case 2: OnWeaponAction.Invoke((int)data); break;
                }
            }
        }

        public override void SetWeaponParent(MWeapon weapon, Transform target)
        {
#if AC_FISHNET
            NetworkBehaviour netObj_Weapon = weapon.GetComponent<NetworkBehaviour>();
            if(netObj_Weapon != null){
                if(target == null){
                    netObj_Weapon.NetworkObject.UnsetParent();
                    return;
                }
                netObj_Weapon.NetworkObject.SetParent(target.GetComponent<NetworkBehaviour>());
            }else{
                base.SetWeaponParent(weapon, target);
            }
#else
            base.SetWeaponParent(weapon, target);
#endif
        }

        public override void SetWeaponCharge(float Charge)
        {
            if(HasAnimal && !isController)return;

            if(isController) m_netAnimal.AttackCharge = Charge;

            base.SetWeaponCharge(Charge);
        }
        public virtual void ShooterReload(IDs ammoType, int chamberSize)
        {
            if(ammoType is StatID && chamberSize > 0){
                if(!m_netAnimal.isServer) return;
                    // Need to send this Damage to Server??
                    Debug.Log("Working on Stats as Ammo");
                return;
            }
            if(AmmoConsummed == null){
                OnAmmoUpdate?.Invoke(ammoType, -1);
            }else{
                AmmoConsummed.Invoke(ammoType, chamberSize);
            }
        }
        public virtual void CheckActiveWeapon(bool equip = false)
        {
            if(Weapon == null && m_netAnimal.ActiveWeapon != -1){
                if(UseHolsters){
                    NetWeaponUpdate(m_netAnimal.ActiveWeapon);
                    if(equip) Equip_Weapon();
                }
                if(UseExternal){
                    //Need more Checks here
                    SetActiveHolster?.Invoke(m_netAnimal.ActiveWeapon);
                }
            }
        }
        
        public override void Holster_SetActive(int ID)
        {
            if(!UseHolsters) return;
            //Debug.Log("Holster Called: "+ ID);
            if(isController) SetActiveHolster?.Invoke(ID);
            base.Holster_SetActive(ID);
        }

        protected void WeaponChanged(MWeapon weapon)
		{
            if(!isController) return;
			int holster = (weapon != null)? weapon.HolsterID : -1;
            if(m_netAnimal != null) m_netAnimal.ActiveWeapon = holster;
		}
        public override void Equip_External(MWeapon Next_Weapon)
        {
            //Debug.Log("Wepon Called");
            if(isController){
                SetActiveHolster?.Invoke(Next_Weapon.HolsterID);
                base.Equip_External(Next_Weapon);
                return;
            }

            if(Weapon == null){
                TryInstantiateWeapon(Next_Weapon);
                if (Weapon.IgnoreDraw || IgnoreDraw) Draw_Weapon();
            }else{
                if(Weapon.IgnoreStore || IgnoreStore){
                    Store_Weapon();
                }else{
                    if(Weapon.Equals(Next_Weapon)) return;
                    StartCoroutine(NetSwapWeaponsInventory(Next_Weapon));
                    return;
                }
                TryInstantiateWeapon(Next_Weapon);
                if (Weapon.IgnoreDraw || IgnoreDraw) Draw_Weapon();
            }
        }

        private IEnumerator NetSwapWeaponsInventory(MWeapon Next_Weapon)
		{
			while(Weapon != null) yield return null;
			
			TryInstantiateWeapon(Next_Weapon);
            if (Weapon.IgnoreDraw || IgnoreDraw) Draw_Weapon();
		}
        
        private void NetActiveHolster(int holster)
        {
            if(isController) return;
            
            //Debug.Log(gameObject.name+" HolsterSet - A:"+ActiveHolster.Index+" N:"+holster);
            if(Weapon == null){
                NetWeaponUpdate(holster);
            }else{
                if(Weapon.IgnoreStore || IgnoreStore){
                    Store_Weapon();
                    NetWeaponUpdate(holster);
                    return;
                }
                if(holster != ActiveHolster.Index) StartCoroutine(NetSwapWeaponsHolster(holster));
            }
        }
        private IEnumerator NetSwapWeaponsHolster(int HolstertoSwap)
		{
			while(Weapon != null) yield return null;
			
			NetWeaponUpdate(HolstertoSwap);
		}
		private void NetWeaponUpdate(int holster)
		{ 
            Holster_SetActive(holster);
			if(ActiveHolster.Weapon == null) return;
            if(Weapon == ActiveHolster.Weapon) return;
			Weapon = ActiveHolster.Weapon;
            //if(!ignoreDraw && (Weapon.IgnoreDraw || IgnoreDraw)) Draw_Weapon();
		}

        #region RPC/CMD FROM WEAPON UTILITY
        public void SyncProjectile(Vector3 velocity, Vector3 position, Vector3 forward, float charge, int seed = 0)
        {
            if(!isController) return;
            m_netAnimal.SyncProjectile(velocity, position, forward, charge, seed);
        }
        #endregion
        #region Events
        [SerializeField] protected BoolEvent m_PlayerOnCombatMode;
        [SerializeField] protected BoolEvent m_PlayerOnCanAim;
        [SerializeField] protected GameObjectEvent m_PlayerOnEquipWeapon;
        [SerializeField] protected GameObjectEvent m_PlayerOnUnequipWeapon;
        [SerializeField] protected IntEvent m_PlayerOnWeaponAction;
		[SerializeField] protected UnityEvent<MWeapon>[] m_PlayerOnWeaponHolsted;

        public void Net_WeaponHolsted(int target)
		{
			if(target >= m_PlayerOnWeaponHolsted.Length) return;
			if(isPlayerOwned) m_PlayerOnWeaponHolsted[target].Invoke(holsters[target].Weapon);
		}
		#endregion
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(NetMWeaponManager))]
    public class NetMWeaponManagerEditor : MWeaponManagerEditor
    {
        private SerializedProperty
            //NetSetWeapon, 
            Editor_Tabs1,Editor_Tabs2,
            PlayerEquipWeapon, PlayerUnequipWeapon, PlayerCombatMode, PlayerCanAim, PlayerWeaponAction, PlayerWeaponHolsted;
        
        protected void FindProperties()
        {
            PlayerCombatMode = serializedObject.FindProperty("m_PlayerOnCombatMode");
            PlayerCanAim = serializedObject.FindProperty("m_PlayerOnCanAim");
            PlayerEquipWeapon = serializedObject.FindProperty("m_PlayerOnEquipWeapon");
            PlayerUnequipWeapon = serializedObject.FindProperty("m_PlayerOnUnequipWeapon");
            PlayerWeaponAction = serializedObject.FindProperty("m_PlayerOnWeaponAction");
            PlayerWeaponHolsted = serializedObject.FindProperty("m_PlayerOnWeaponHolsted");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            FindProperties();

            int Selection = Editor_Tabs1.intValue;

            if (Selection == 3) DrawPlayerEvents();

            serializedObject.ApplyModifiedProperties();  
        }

        protected virtual void DrawPlayerEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MalbersEditor.DrawHeader("Player Events");

                EditorGUILayout.PropertyField(PlayerCombatMode);
                EditorGUILayout.PropertyField(PlayerCanAim);
                EditorGUILayout.PropertyField(PlayerEquipWeapon);
                EditorGUILayout.PropertyField(PlayerUnequipWeapon);
                //  EditorGUILayout.PropertyField(OnMainAttackStart);
                EditorGUILayout.PropertyField(PlayerWeaponAction);
                EditorGUILayout.PropertyField(PlayerWeaponHolsted);
            }
            
        }

    }
    #endif
}