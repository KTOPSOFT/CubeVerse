using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Variables and Properties</summary>
    public partial class MWeaponManager
    {
        #region Riding Only
        public bool UseWeaponsOnlyWhileRiding = true;
        public Transform IgnoreTransform { get; set; }

        public IRider Rider { get; protected set; }
        public IInputSource MInput { get; protected set; }
        #endregion

        #region Animal Controller
        [Tooltip("Reference for the Character Controller (Using Animal Controller as Main Character)")]
        public MAnimal animal;

        /// <summary> Is the Main Controller of the Weapon Manager an Animal Controller? </summary>
        public bool HasAnimal => animal != null;

        /// <summary>Is the Character Riding an animal</summary>
        public bool IsRiding { get; set; }

        /// <summary>Is the Character mounting or dismounting.. (USE THIS TO BLOCK ANY ACTION FROM THE WEAPONS)</summary>
        public bool MountingDismounting { get; set; }


        /// <summary>Store if the Animal was already Strafing</summary>
        public bool DefaultStrafing { get; set; }

        /// <summary>Is the Current weapon Action Reload?</summary>
        public bool IsReloading => Weapon.IsReloading;

        /// <summary>Is the Current weapon Action Reload?</summary>
        public bool IsAttacking => WeaponAction == Weapon_Action.Attack;

        [Tooltip("If the weapon is dropped from a holster it will be dropped from this point, relative to the WeaponManager")]
        public Transform DropPoint;

        [Tooltip("Mode ID used for Draw Unsheathe Weapons")]
        public ModeID DrawWeaponModeID;
        [Tooltip("Mode ID used for Store/Sheathe Weapons")]
        public ModeID StoreWeaponModeID;

        [Tooltip("Mode ID used for to attack with no weapons. If is Set ")]
        public ModeID UnarmedModeID;

        [Tooltip("Reference for the Combo Manager component")]
        public ComboManager comboManager;

        [Tooltip("Value to set the branch on the Combo Manager when the Main Attack is called")]
        public IntReference mainAttackBranch = new(0);

        [Tooltip("Value to set the branch on the Combo Manager when the Main Attack is called")]
        public IntReference secondAttackBranch = new(1);

        public int MainAttackBranch { get => mainAttackBranch.Value; set => mainAttackBranch.Value = value; }

        public int SecondAttackBranch { get => secondAttackBranch.Value; set => secondAttackBranch.Value = value; }


        [Tooltip("Ignore the Left and Right hand Offsets")]
        public BoolReference IgnoreHandOffset = new();

        [Tooltip("Ignore all Draw|Unsheathe animations for all weapons")]
        [SerializeField] protected BoolReference m_IgnoreDraw = new(false);   //Ignore Draw/Store Aniamtions

        [Tooltip("Ignore all Store|sheathe animations for all weapons")]
        [SerializeField] protected BoolReference m_IgnoreStore = new(false);


        //public List<StateID> LockStates;
        [Tooltip("Disable these modes when a weapon is equipped")]
        public List<ModeID> DisableModes = new();

        [Tooltip("Unequip Weapons If any of these modes are Activated")]
        public List<ModeID> ExitOnModes = new();


        [Tooltip("Disable these States when a weapon is equipped")]
        public List<StateID> DisableStates = new();

        [Tooltip("Unequip Weapons If any of these modes are Activated")]
        public List<StateID> ExitOnState = new();

        [Tooltip("Unequip Weapons If any of these modes are Activated.Ignore Store|Sheathe animations")]
        public bool ExitFast = false;


        [Tooltip("Store current active weapon to its own holster. (Disable for Assasing Creed Mode)")]
        public bool StoreSelfHolster = false;

        /// <summary>Ignore Draw|Unsheathe and Store|Sheathe Animations for the weapon</summary>
        public bool IgnoreDraw { get => m_IgnoreDraw.Value; set => m_IgnoreDraw.Value = value; }
        public bool IgnoreStore { get => m_IgnoreStore.Value; set => m_IgnoreStore.Value = value; }

        /// <summary> get the combo branch</summary>
        public int ComboBranch => comboManager.Branch;

        /// <summary>Stores if the Animal has a Mode for the weapon</summary>
        public Mode WeaponMode { get; protected set; }

        /// <summary>Stores if the Animal has a Draw weapon mode</summary>
        public Mode DrawMode { get; protected set; }
        /// <summary>Stores if the Animal has a Store weapon mode</summary>
        public Mode StoreMode { get; protected set; }
        /// <summary>Stores if the Animal has a Unarmed  mode</summary>
        public Mode UnArmedMode { get; protected set; }
        #endregion

        #region Animator System.Actions
        /// <summary>Sets a bool Parameter on the Animator using the parameter Hash</summary>
        public System.Action<int, bool> SetBoolParameter { get; set; }
        /// <summary>Sets a float Parameter on the Animator using the parameter Hash</summary>
        public System.Action<int, float> SetFloatParameter { get; set; }

        /// <summary>Sets a Integer Parameter on the Animator using the parameter Hash</summary> 
        public System.Action<int, int> SetIntParameter { get; set; }

        /// <summary>Sets a Trigger Parameter on the Animator using the parameter Hash</summary> 
        public System.Action<int> SetTriggerParameter { get; set; }
        #endregion

        #region Public Entries
        public bool UseExternal = true;                                //Use this if the weapons came from an external source

        [Tooltip("If the Weapon sent on the EquipExternal Method is a Prefab... instantiate it")]
        public bool InstantiateOnEquip = true;                         //If the weapons comes from an inventory check if they are already intantiate
        [Tooltip("Destroy the Weapon when is unequipped")]
        public bool DestroyOnUnequip = false;                         //If the weapons comes from an inventory check if they are already intantiate

        [Tooltip("Override the Weapon Layer Mask when equipped. This will be ignored when is set to none")]
        public LayerReference OverrideWeaponLayer = new(0);


        #region Holsters
        public bool UseHolsters = false;                                 //Use this if the weapons are on the Holsters
        //   public HolsterID DefaultHolster;
        public List<Holster> holsters = new();
        //public float HolsterTime = 0.2f;

        /// <summary> Used to change to the Next/Previus Holster</summary>
        public int ActiveHolsterIndex { get; set; }
        /// <summary> ID Value of the Active Holster</summary>
        public Holster ActiveHolster { get; set; }
        #endregion

        [Tooltip("Tranform Reference for the Left Hand. The weapon will be parented to this transform when is equipped")]
        [ContextMenuItem("Find Left Hand", nameof(FindLHand))]
        [RequiredField] public Transform LeftHandEquipPoint;
        [Tooltip("Tranform Reference for the Right Hand. The weapon will be parented to this transform when is equipped")]
        [ContextMenuItem("Find Right Hand", nameof(FindRHand))]
        [RequiredField] public Transform RightHandEquipPoint;


        protected virtual void FindRHand()
        {
            if (anim != null && anim.avatar.isHuman)
            {
                RightHandEquipPoint = anim.GetBoneTransform(HumanBodyBones.RightHand);
                MTools.SetDirty(this);
            }
        }
        protected virtual void FindLHand()
        {
            if (anim != null && anim.avatar.isHuman)
            {
                LeftHandEquipPoint = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                MTools.SetDirty(this);
            }
        }

        /// <summary>Path of the Combat Layer on the Resource Folder </summary>
        [SerializeField] internal string m_CombatLayerPath = "Layers/Combat2";
        /// <summary>Name of the Combat Layer </summary>
        [SerializeField] internal string m_CombatLayerName = "Upper Body (AC Weapons)";

        public bool debug;
        #endregion

        /// <summary>Reference for the Animator component</summary>
        [RequiredField, SerializeField] private Animator anim;
        public Animator Anim { get => anim; set => anim = value; }

        /// <summary>Reference for the Animator Update Mode</summary>
        public AnimatorUpdateMode DefaultAnimUpdateMode { get; set; }
        private Weapon_Action weaponAction = Weapon_Action.None;             //Which type of action is making the active weapon
        //private Weapon_Action lastWeaponAction = Weapon_Action.None;

        [Tooltip("If the weapon is on the Idle Action it will be stored after X seconds. If Zero, this feature will be ignored.")]
        public FloatReference StoreAfter = new(0);

        #region Animator Hashs

        [SerializeField, Tooltip("It sends to the Animator the Weapon ID")]
        private string m_WeaponType = "WeaponType";

        [SerializeField, Tooltip("Animator Curve name to set the Aim IK Values on the Weapons")]
        private string m_IKAim = "IKAim";
        [SerializeField, Tooltip("Animator Curve name to set the Auxiliar hand IK Values on the Weapons")]
        private string m_IKFreeHand = "IKFreeHand";

        [SerializeField, Tooltip("Sends to the Animator the Weapon Hand Value. [True -> Left Hand] [False ->Right Hand]")]
        private string m_LeftHand = "LeftHand";

        [SerializeField, Tooltip("Weapon Charge or power is the same parameter as the Animal Controller Mode Power")]
        private string m_WeaponPower = "ModePower";
        [SerializeField, Tooltip("Sends to the Animator the a trigger to activate the next Weapon Action ")]
        private string m_ModeOn = "ModeOn";
        [SerializeField, Tooltip("Sends to the Animator the Weapon Action ")]
        private string m_Mode = "Mode";

        internal int Hash_WType;
        internal int Hash_LeftHand;
        //Mode Stuff
        internal int hash_Mode;
        internal int hash_ModeOn;
        internal int Hash_WPower;
        internal int hash_ModeStatus;

        public int Hash_IKFreeHand;
        public int Hash_IKAim;
        #endregion

        #region Events
        public BoolEvent OnCombatMode = new();
        public BoolEvent OnCanAim = new();
        public GameObjectEvent OnEquipWeapon = new();
        public GameObjectEvent OnUnequipWeapon = new();
        public IntEvent OnWeaponAction = new();
        //  public GameObjectEvent OnMainAttackStart = new GameObjectEvent();
        #endregion

        #region Inputs values
        public StringReference m_AimInput = new StringReference("Aim");
        public StringReference m_ReloadInput = new StringReference("Reload");
        public StringReference m_MainAttack = new StringReference("MainAttack");
        public StringReference m_SecondAttack = new StringReference("SecondAttack");
        public StringReference m_SpecialAttack = new StringReference("SpecialAttack");
        #endregion


        [Tooltip("An previous weapon will be destroyed if a new weapon is going to use the same holster. Enable this when using Malbers Inventory")]
        public bool DestroyOnDrop;

        #region Properties

        /// <summary>Is the weapon Manager Enabled If is false everything will be ignored </summary>
        public virtual bool Active
        {
            get => enabled;
            set
            {
                if (!value)
                {
                    if (CombatMode) //Means it has a weapon equipped!!
                    {
                        Store_Weapon();
                    }
                    else
                        ResetCombat(); //If the Rider Combat was deactivated
                }
                enabled = value;
            }
        }

        /// <summary>  Same As Active  </summary>
        public virtual void SetActive(bool value) => Active = value;

        /// <summary>is there an Ability Active and the Active Weapon is Active too</summary>
        public bool WeaponIsActive => (Weapon && Weapon.Enabled && Weapon.IsEquiped) && Active && !Paused;

        //   public bool CheckRidingOnly => !UseWeaponsOnlyWhileRiding || (Rider != null && Rider.IsRiding);

        public bool Paused => Time.timeScale == 0;

        //public MRider Rider { get; set; }

        public IAim Aimer { get; set; }

        /// <summary>Cache the Default aiming Side</summary>
        public AimSide DefaultAimSide { get; set; }

        public float DeltaTime { get; set; }

        protected bool combatMode;


        /// <summary>Enable or Disable the Combat Mode (True When the animal has equipped a weapon)</summary>
        public virtual bool CombatMode
        {
            get => combatMode;
            set
            {
                combatMode = value;
                //  Debug.Log("CombatMode = " + combatMode);
                OnCombatMode.Invoke(value);
            }
        }

        protected IEnumerator C_StoreAfter()
        {
            yield return StoreAfterTime;
            Store_Weapon();
        }

        private WaitForSeconds StoreAfterTime;
        Coroutine IStoreAfter;

        public bool DrawWeapon { get; set; }
        public bool StoreWeapon { get; set; }

        #region IK Values
        /// <summary> Actual weight pass to the Animator IK</summary>
        public float IKAimWeight { get; set; }

        /// <summary> Weights for the IK Two Handed Weapon </summary>
        public float IK2HandsWeight { get; set; }
        #endregion


        public GameObject Owner => gameObject;

        [SerializeField, Tooltip("Start with an equipped Weapon")]
        private GameObjectReference startWeapon;

        /// <summary> Reference for the Start Weapon</summary>
        public virtual GameObject StartWeapon { get => startWeapon.Value; set { startWeapon.Value = value; } }

        protected MWeapon m_weapon;

        /// <summary>Current active/Equiped Weapon </summary>
        public virtual MWeapon Weapon
        {
            get => m_weapon;
            set
            {
                if (value == null)        //No Weapon
                {
                    if (m_weapon != null) //If there was a weapon before then remove the Weapon 
                        SetWeapon(false);

                    m_weapon = value;
                }
                else                        //NEW WEAPON
                {
                    if (m_weapon != null) SetWeapon(false);

                    m_weapon = value;
                    SetWeapon(true);
                    SetWeaponHand(value.IsLefttHanded);
                }
            }
        }

        /// <summary>Prepare a new and Old Weapon. False: release the old weapon. True: Listen to the new Weapon </summary>
        protected virtual void SetWeapon(bool new_Weapon)
        {
            if (new_Weapon)
            {
                m_weapon.WeaponAction += Action;
                m_weapon.OnCharged.AddListener(SetWeaponCharge);
            }
            else
            {
                m_weapon.WeaponAction -= Action;
                m_weapon.OnCharged.RemoveListener(SetWeaponCharge);
                m_weapon.IgnoreTransform = null;
            }
        }

        /// <summary>This will recieve the messages Animator Behaviors the moment the rider make an action on the weapon</summary>
        public virtual void Action(int value) => WeaponAction = (Weapon_Action)value;

        //#region Bones References 
        //public Transform RightShoulder { get; set; }
        //public Transform LeftShoulder { get; set; }
        public Transform RightHand { get => RightHandEquipPoint; set => RightHandEquipPoint = value; }
        public Transform LeftHand { get => LeftHandEquipPoint; set => LeftHandEquipPoint = value; }
        //public Transform Head { get; set; }
        //public Transform Chest { get; set; }
        //#endregion

        [Tooltip("Set the Aiming to true on the Weapon Manager")]
        public BoolReference aim = new();


        [Tooltip("Enable/Disable the Aim Feature for the weapons")]
        public BoolReference canAim = new(true);

        public bool CanAim
        {
            get => canAim.Value;
            set => CanAim_Set(value);
        }

        public virtual void CanAim_Set(bool value)
        {
            if (CanAim != value)
            {

                if (!value)
                {
                    if (Aim) Aim_Set(false);
                    OldAimValue = true;
                    canAim.Value = value;
                }
                else
                {
                    canAim.Value = value;
                    if (OldAimValue)
                        Aim_Set(true);
                }
                Debugging($"Weapon Manager Can Aim [{value}]");
            }
        }

        protected bool OldAimValue = false;


        /// <summary>Direction the Rider is Aiming</summary>
        public Vector3 AimDirection => Aimer.AimDirection;

        public LayerMask Layer { get => Aimer.Layer; set => Aimer.Layer = value; }
        public QueryTriggerInteraction TriggerInteraction { get => Aimer.TriggerInteraction; set => Aimer.TriggerInteraction = value; }

        public bool Weapon_is_RightHand => Weapon.IsRightHanded;
        public bool Weapon_is_LeftHand => !Weapon.IsRightHanded;

        protected int weaponType;             //Which Type of weapon is in the active weapon
        /// <summary>Which Type of Weapon is in the Active Weapon, this value is sent to the animator</summary>
        public virtual int WeaponType
        {
            get => weaponType;
            set
            {
                weaponType = value;

                var weapoFinalVal = (Weapon && Weapon.OverrideWeaponArmPose != null && value != 0) ? Weapon.OverrideWeaponArmPose.ID : weaponType;

                TryAnimParameter(Hash_WType, weapoFinalVal);          //Set the WeaponType in the Animator
            }
        }
        #endregion  
    }
}