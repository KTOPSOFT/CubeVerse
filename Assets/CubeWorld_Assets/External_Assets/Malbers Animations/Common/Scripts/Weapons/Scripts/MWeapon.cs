using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using System;
using UnityEngine.Serialization;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    public enum Weapon_Action
    {
        /// <summary>[0] No Weapon is equiped</summary>
        None = 0,
        /// <summary>[100] The Weapon is resting in the Hand</summary>
        Idle = 100,
        /// <summary>[97] The Weapon is aiming???</summary>
        Aim = 97,
        /// <summary>[101] The Weapon is firing/ or Attacking a Projectile</summary>
        Attack = 101,
        /// <summary>[96] The Character is reloading the weapon</summary>
        Reload = 96,
        /// <summary>[99] The Weapon is draw for the RIGHT Side (Hostler) </summary>
        Draw = 99,
        /// <summary>[98] The Weapon is stored to  the RIGHT Side (Hostler) </summary>
        Store = 98,
    }

    [System.Serializable] public class WeaponEvent : UnityEvent<MWeapon> { }
    [SelectionBase]
    public abstract class MWeapon : MDamager, IMWeapon, IMDamager
    {
        #region Data
        [SerializeField] protected Sprite m_UI;
        [SerializeField] protected StringReference description = new(string.Empty);
        #endregion

        //#region Damage
        //[SerializeField] protected FloatReference minDamage = new FloatReference(10);                       //Weapon minimum Damage
        //[SerializeField] protected FloatReference maxDamage = new FloatReference(20);                        //Weapon Max Damage
        //#endregion

        #region Weapon Charge
        public FloatReference chargeTime = new(0);


        [Tooltip("Value of Charge.. from zero to Max")]
        public FloatReference m_MaxCharge = new(1);
        [SerializeField] private float chargeCharMultiplier = 1;
        public AnimationCurve ChargeCurve = new(new Keyframe[] { new(0, 0), new(1, 1) });
        #endregion

        #region Riding/Ground Values
        [Tooltip("Use a Arm Pose when the weapon is equipped while Grounded. It will keep playing in loop an animation for an arm while the weapon is equipped")]
        public bool GroundArmPose = true;
        [Tooltip("Override WeaponType value in the animator")]
        [SerializeField] private WeaponID groundHandID;
        public WeaponID OverrideWeaponArmPose => groundHandID;


        [Tooltip("Use a Arm Pose when the weapon is equipped while Riding. It will keep playing in loop an animation for an arm while the weapon is equipped")]
        public bool RidingArmPose = true;


        [Tooltip("Is the weapon using Combo while grounded?.  Leave empty to not use combos!")]
        public ModeID GroundCombo;
        [Tooltip("Is the weapon using Combo while Riding?. Leave empty to not use combos!")]
        public ModeID RidingCombo;
        #endregion


        /// <summary>Continue Attacking using the Rate of the Weapon </summary>
        public bool Automatic { get => m_Automatic.Value; set => m_Automatic.Value = value; }

        /// <summary> The call of attack was from automatic </summary>
        public bool AttackFromAutomatic { get; set; }

        /// <summary>Does the weapon has Ammo in the Chamber?</summary>
        public virtual bool HasAmmo => false;


        /// <summary>Ignore Draw Weapon Animations</summary>
        public bool IgnoreDraw { get => m_IgnoreDraw.Value; set => m_IgnoreDraw.Value = value; }
        /// <summary>Ignore Store Weapon Animations</summary>
        public bool IgnoreStore { get => m_IgnoreStore.Value; set => m_IgnoreStore.Value = value; }

        /// <summary>Holster Transform Slot Index for the Weapon</summary>
        public int HolsterSlot { get => m_holsterIndex; set => m_holsterIndex = value; }

        ///// <summary> Sets if the weapon is stored in a holster or not  </summary>
        //public bool InHolster { get; set; }

        //public Sprite UISprite { get => m_UI; set => m_UI = value; }

        #region Aiming
        [SerializeField, RequiredField] private Transform m_AimOrigin;
        public virtual Transform AimOrigin { get => m_AimOrigin; set => m_AimOrigin = value; }
        public Vector3 AimOriginPos => AimOrigin.position;

        [SerializeField] private AimSide m_AimSide;
        #endregion

        #region Weapon Properties
        [SerializeField] protected WeaponID weaponType;




        [Tooltip("Identifier for the Holster that the weapon will be stored")]
        public HolsterID holster;                                       // From which Holder you will draw the Weapon
        public HolsterID holsterAnim;                                   // From which Holder you will draw the Weapon

        [Min(0), Tooltip("A holster can have multiple Transform to be parent to. This is the Index of the Transform Slots Array")]
        [SerializeField] protected int m_holsterIndex = 0;                                  // From which Holder you will draw the Weapon
        public BoolReference rightHand = new(true);                           // With which hand you will draw the Weapon;

        [SerializeField] protected FloatReference m_rate = new(0);           //Weapon Rate
        [SerializeField] protected BoolReference m_Automatic = new(false);    //Press Fire to Contiue Attacking

        [Tooltip("Ignore Draw Animations for the weapon")]
        [SerializeField] protected BoolReference m_IgnoreDraw = new(false);

        [Tooltip("Ignore Store Animations for the weapon")]
        [SerializeField] protected BoolReference m_IgnoreStore = new(false);
        #endregion

        #region ANIMAL CONTROLLER WEAPON GROUNDED

        [Tooltip("Stance Used by the Animal Controller for the Weapon")]
        public StanceID stance;
        [SerializeField, Tooltip("Enable Strafing while Aiming")]
        private BoolReference strafeOnAim = new();
        [Tooltip("When using the weapon on foot it will Try activate the Strafe on the Animal")]
        public BoolReference strafeOnEquip = new(false);
        [Tooltip("When the weapon is  unequipped enable or disable straffing")]
        public BoolReference strafeOnUnequip = new(false);
        #endregion

        #region IK
        [ExposeScriptableAsset, Tooltip("Aim IK Modification to the Character Body to Aim Properly when the Weapon is RightHanded")]
        public IKProfile AimIKRight;

        [ExposeScriptableAsset, Tooltip("Aim IK Modification to the Character Body to Aim Properly when the Weapon is LeftHanded")]
        public IKProfile AimIKLeft;

        public IKProfile AimIK => rightHand ? AimIKRight : AimIKLeft;

        [Tooltip("IK Modification to the Character Body to Aim Properly")]
        public BoolReference TwoHandIK;                              // Makes the IK for the 2Hands

        [Tooltip("Position and Rotation Reference for the secondary Hand IK Goal (Left) ")]
        [FormerlySerializedAs("IKHandPoint")]
        public TransformReference IKHandLeft = new();                       // Rotation Offset Left Hand

        [Tooltip("Position and Rotation Reference for the secondary Hand IK  (Right)  ")]
        public TransformReference IKHandRight = new();                       // Rotation Offset Left Hand

        public Transform IKHandPoint => rightHand ? IKHandLeft.Value : IKHandRight.Value;


        #endregion

        #region Offsets
        public TransformOffset HolsterOffset = new(1);
        public TransformOffset LeftHandOffset = new(1);
        public TransformOffset RightHandOffset = new(1);
        #endregion

        #region Audio
        /// <summary> Weapon Sounds</summary>
        public AudioClip[] Sounds;                          //Sounds for the weapon
        #endregion

        #region Properties
        /// <summary>Unique Weapon ID for each weapon</summary>
        public virtual int WeaponID => index;

        /// <summary>ID of the Damager (To be activated by the Animator)</summary>
        public override int Index => weaponType.ID;



        /// <summary>Can the weapon be stored, unequiped -> override with the different weapons</summary>
        public virtual bool CanUnequip => true;

        /// <summary>Holster the weapon can be draw from</summary> 
        public virtual int HolsterID => (Holster != null) ? Holster.ID : 0;

        public HolsterID Holster { get => holster; set => holster = value; }
        public WeaponID WeaponType => weaponType;

        /// <summary> ID value for the Holster, This is used on the Animator to Draw or Store the weapons </summary>
        public int HolsterAnim => holsterAnim != null ? holsterAnim.ID : holster.ID;//  { get => holsterAnim; set => holsterAnim = value; }

        /// <summary>Send to the Weapon Owner that the weapon Action Changed</summary>
        public Action<int> WeaponAction { get; set; } = delegate { };

        private bool isEquiped = false;

        /// <summary> Is the Weapon Equiped </summary>
        public virtual bool IsEquiped
        {
            get => isEquiped;
            set
            {
                isEquiped = value;
                Debugging($"Equiped [{value}]", this, "green");  //Debug

                if (isEquiped && Owner)
                {
                    OnEquiped.Invoke(Owner.transform);

                    MaxCharged = false;
                    IsAttacking = false;
                }
                else
                {
                    OnUnequiped.Invoke(Owner ? Owner.transform : null);

                    //Reset the Owner
                    Owner = null;
                    CurrentOwner = null;
                    IsReloading = false;
                }
            }
        }

        /// <summary>Is the Weapon Charging?</summary>
        public virtual bool IsCharging// { get; set; }
        {
            get => isCharging;
            set
            {
                isCharging = value;
                //Debug.Log("isCharging = " + isCharging);   
            }
        }
        private bool isCharging;

        /// <summary>Is the Weapon used while riding?</summary>
        public virtual bool IsRiding { get; set; }


        /// <summary>Is the Playing a Reloading animation</summary>
        public virtual bool IsReloading  // { get; set; }
        {
            get => isReloading;
            set
            {
                isReloading = value;
                //  Debug.Log("isReloading = " + isReloading);   
            }
        }
        private bool isReloading;


        /// <summary>Can the Weapon Attack? Uses the Weapon Rate to evaluate if the weapon can Attack Again (Works for Melee and Shotable weapons)</summary>
        public virtual bool CanAttack
        {
            get => canAttack;
            set
            {
                canAttack = value;

                if (!canAttack)
                {
                    if (Rate > 0)
                        this.Delay_Action(Rate, () => { canAttack = true; });
                    else
                        canAttack = true; //Restore Can Attack if the weapon has no Rate
                }

            }
        }
        protected bool canAttack;

        /// <summary>Check if the Weapon is aiming so it plays the Aim Animation </summary>
        public virtual void CheckAim()
        {
            //   Debug.Log("CHECK AIMIMING");

            WeaponAction?.Invoke(IsAiming ? (int)Weapon_Action.Aim : (int)Weapon_Action.Idle);
            IsAttacking = false;
        }


        /// <summary>Is the Weapon Attacking...Is Playing the Attack Animation if it has one.</summary>
        public virtual bool IsAttacking { get; set; }
        //{
        //    get => isAttacking;
        //    set
        //    {
        //        isAttacking = value;
        //        Debug.Log("isAttacking = " + isAttacking);

        //    }
        //}
        //protected bool isAttacking;

        /// <summary>Main Attack Input Value. Also Means the Main Attack has Started. I use this to know if the weapon is holding the Attack Down</summary>
        public virtual bool Input//{ get; set; }
        {
            get => m_MainInput;
            set
            {
                if (m_MainInput != value)
                    Debugging($"Input → [{value}]", this);
                m_MainInput = value;
            }
        }
        bool m_MainInput;

        ///// <summary>Second Attack Input Value. Also Means the Secondary Attack has Started</summary>
        //public virtual bool SecondInput { get; set; }

        public string Description { get => description.Value; set => description.Value = value; }


        protected bool isAiming;
        /// <summary>Is the Weapon Aiming?</summary>
        public virtual bool IsAiming
        {
            get => isAiming;
            set
            {
                isAiming = value;
                OnAiming.Invoke(isAiming);

                //  if (!value) ResetCharge();
            }
        }

        /// <summary>Side of the Camera to use when using the Weapon</summary>
        public AimSide AimSide { get => m_AimSide; set => m_AimSide = value; }

        /// <summary>Can the weapon Aim?? Overrie with shootables</summary>
        public virtual bool CanAim => false;

        public float MinDamage => statModifier.MinValue.Value;
        public float MaxDamage => statModifier.MaxValue.Value;

        /// <summary>Time needed to fully charge the weapon</summary>
        public float ChargeTime { get => chargeTime.Value; set => chargeTime.Value = value; }
        public float MaxCharge { get => m_MaxCharge.Value; set => m_MaxCharge.Value = value; }

        /// <summary>Can the Weapon be Charged? Meaning Charge time is greater than 0</summary>
        public virtual bool CanCharge => ChargeTime > 0;

        /// <summary>Is the weapon at max charge</summary>
        public bool MaxCharged { get; internal set; }

        /// <summary> Charge multiplier to Apply to the Character Charge Value (For the Animator Parameter)  </summary>
        public float ChargeCharMultiplier { get => chargeCharMultiplier; set => chargeCharMultiplier = value; }

        /// <summary>Elapsed Time since the Charge Weapon Started</summary>
        public float ChargeCurrentTime { get; set; }
        //{
        //    get => m_ChargeCurrentTime;
        //    set
        //    {
        //        m_ChargeCurrentTime = value;
        //        Debug.Log("m_ChargeCurrentTime: "+value);
        //    }
        //}
        //float m_ChargeCurrentTime;

        /// <summary>Is the weapon used on the Right hand(True) or left hand (False)</summary>
        public bool IsRightHanded => rightHand.Value;
        public bool IsLefttHanded => !IsRightHanded;



        /// <summary>Weapon Rate</summary>
        public float Rate { get => m_rate.Value; set => m_rate.Value = value; }

        ///// <summary>Is the weapon fully charged?</summary>
        //public bool IsCharged => ChargeCurrentTime >= ChargeTime;

        /// <summary>Normalized Value for the Charge. if ChargeTime == 0 then Random Value between [0-1]</summary>
        public float ChargedNormalized => CanCharge ? ChargeCurve.Evaluate(Charging) : UnityEngine.Random.Range(0f, 1f);

        public float Charging => Mathf.Clamp01(ChargeCurrentTime / ChargeTime);

        /// <summary>Current charge the weapon has </summary>
        public float CurrentCharge { get; set; }

        /// <summary> Normalized Value of the Charge </summary>
        public float Power => Mathf.Lerp(MinForce, MaxForce, ChargedNormalized);

        /// <summary>Enable or Disable the weapon to "block it"</summary>
        public override bool Enabled
        {
            get => enabled;
            set
            {
                m_Active.Value = enabled = value;

                Debugging($"Active [{value}]", this);

                //If the weapon is Disabled change the Weapon to Idle (if it Was Aiming or Shooting or Something like that
                if (!value && IsEquiped) WeaponAction.Invoke((int)Weapon_Action.Idle);
            }
        }

        public IMWeaponOwner CurrentOwner { get; set; }

        /// <summary>  Enable Strafing on Aiming  </summary>
        public bool StrafeOnAim { get => strafeOnAim.Value; set => strafeOnAim.Value = value; }
        public bool StrafeOnEquip { get => strafeOnEquip.Value; set => strafeOnEquip.Value = value; }
        public bool StrafeOnUnequip { get => strafeOnUnequip.Value; set => strafeOnUnequip.Value = value; }

        /// <summary> The Free Hand is Free[True] or used[false]</summary>
        public bool FreeHand { get; set; }
        public ICollectable IsCollectable { get; private set; }
        #endregion

        #region Events
        public TransformEvent OnEquiped = new();
        public TransformEvent OnUnequiped = new();
        /// <summary>Invoked when the weapon is Charging  (Returns a Normalized Value) </summary>
        public FloatEvent OnCharged = new();
        public UnityEvent OnMaxCharged = new();
        public FloatEvent OnChargedFinished = new();
        public BoolEvent OnAiming = new();

        public UnityEvent OnUseFreeHand = new();
        public UnityEvent OnReleaseFreeHand = new();
        #endregion


        /// <summary>Returns True if the Weapons has the same ID</summary>
        public override bool Equals(object a)
        {
            if (a is IMWeapon)
                return WeaponID == (a as IMWeapon).WeaponID;

            return false;
        }
        public override int GetHashCode() => base.GetHashCode();

        #region WeaponActions
        /// <summary>Set the Primary Attack </summary>
        internal virtual void MainAttack_Start(IMWeaponOwner RC)
        {
            Input = true;
            ResetCharge();
        }

        /// <summary>Set when the Current Attack is Active and Holding ... So reset the Attack</summary>
        internal abstract void Attack_Charge(IMWeaponOwner RC, float time);




        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        internal virtual void MainAttack_Released(IMWeaponOwner RC)
        {
            // Debugging($"Main Attack Released", this);
            Input = false;
            ResetCharge();
        }

        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        internal virtual void SecondAttack_Released(IMWeaponOwner RC)
        {
            //   Debugging($"Second Attack Released", this);
            Input = false;
            ResetCharge();
        }


        /// <summary>Unequip the weapon from the owner </summary>

        public void Unequip()
        {
            CurrentOwner?.UnEquip();
        }


        /// <summary> Reload Weapon </summary>
        internal virtual void Reload(IMWeaponOwner RC) { }

        /// <summary>Called on the Late Update of the Rider Combat Script </summary>
        internal virtual void Weapon_LateUpdate(IMWeaponOwner RC) { }

        /// <summary> Reload Weapon </summary>
        public virtual bool TryReload() => false;

        #endregion


        #region ABILITY SYSTEM 
        /// <summary>Prepare weapon to be equipped with all the necesary component to activate on the Weapons Owner (SAME AS START ABILITY)</summary>
        public virtual bool Equip(IMWeaponOwner _char)
        {
            if (gameObject.IsPrefab()) return false; //Means is still a prefab
            if (!Enabled) { Debugging("The weapon is Disable. It cannot be equipped", this); return false; }

            CurrentOwner = _char;
            Owner = CurrentOwner.Owner;
            IsEquiped = true;
            CanAttack = true;
            ChargeCurrentTime = 0;
            IgnoreTransform = _char.IgnoreTransform;
            // Enabled = true;
            gameObject.SetActive(true);

            animator = _char.Anim; //Set the Animator;

            DisablePhysics();

            Debugging($"Weapon [Prepared]", this);

            return true;
        }


        /// <summary> Attack Trigger Behaviour </summary>
        public virtual void ActivateDamager(int value, int prof)
        {
            DoDamage(true, prof);
        }

        /// <summary>Charge the Weapon using time.deltatime</summary>
        public virtual void Charge(float time)
        {
            if (CanCharge)
            {
                ChargeCurrentTime += time;
                IsCharging = true;

                CurrentCharge = MaxCharge * ChargeCurve.Evaluate(Charging);

                if (Charging == 1 && !MaxCharged)
                {
                    MaxCharged = true; //Meaning the weapon has charged to the max
                    OnMaxCharged.Invoke(); //Max Charged Invoked
                }
                OnCharged.Invoke(CurrentCharge); //Charge Normalized
            }
            else
            {
                ReleaseCharge(); //Means the weapon does not need charging
            }
        }

        /// <summary > Do all Release Logic with your Free Hand (E.g. Bow Grab Know on the String) </summary>
        public virtual void FreeHandRelease()
        {
            OnReleaseFreeHand.Invoke();
            FreeHand = true;
        }

        /// <summary > Do all Grab Logic with your Free Hand (E.g. Bow Grab Know on the String) </summary>
        public virtual void FreeHandUse()
        {
            OnUseFreeHand.Invoke();
            FreeHand = false;
        }


        /// <summary>Reset the Charge of the weapon</summary>
        public virtual void ResetCharge()
        {
            if (CanCharge)
            {
                ChargeCurrentTime = 0;
                IsCharging = false;
                CurrentCharge = 0;
                OnCharged.Invoke(0);
                MaxCharged = false;
                Debugging($"Weapon [Charge Reseted]", this);
            }
        }

        /// <summary>Set when the Primary Attack is Released (BOW) </summary>
        public virtual void ReleaseCharge()
        {
            Debug.Log("RELEASE CHARGE");

            WeaponAction.Invoke((int)Weapon_Action.Attack);
            ResetCharge();
        }
        #endregion

        /// <summary>Resets all the Weapon Properties</summary>
        public virtual void ResetWeapon()
        {
            Owner = null;
            CurrentOwner = null;
            IsEquiped = false;
            IsAiming = false;
            animator = null;
            IgnoreTransform = null;
            ResetCharge();

            Debugging($"Weapon [Reseted]", this);
        }

        public virtual void Initialize()
        {
            isEquiped = false;
            if (Owner == null) Owner = transform.root.gameObject;

            CheckAudioSource();

            IsCollectable = GetComponent<ICollectable>(); //Cache if the weapon is a collectable

            if (holsterAnim == null) holsterAnim = holster;


            SetDefaultProfile();
        }



        /// <summary> Apply the Correct offset to the weapon</summary>
        public virtual void ApplyOffset()
        {
            if (IsRightHanded)
                RightHandOffset.SetOffset(transform);
            else
                LeftHandOffset.SetOffset(transform);
        }

        /// <summary> Set the Weapon RigidBody to Kinematic and Disable the Colliders</summary>
        public void DisablePhysics()
        {
            IsCollectable?.OnPickDisablePhysics();
        }

        /// CallBack from the RiderCombat Layer in the Animator to reproduce a sound on the weapon
        public virtual void PlaySound(int ID)
        {
            if (ID < Sounds.Length && Sounds[ID] != null)
            {
                var newSound = Sounds[ID];
                PlaySound(newSound);
            }
        }

        //internal void AnimalModeEnd(int modeID, int ablility)
        //{ }

        /// <summary> Called when Weapon has been stored </summary>
        internal virtual void StoringWeapon() { }


        [ContextMenu("Set Hand Offset Values")]
        private void CopyTransformToOffsets()
        {
            if (IsRightHanded)
                RightHandOffset = new TransformOffset(transform);
            else
                LeftHandOffset = new TransformOffset(transform);
        }


        /// <summary>This is used to listen the Animator asociated to this gameObject </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            index = UnityEngine.Random.Range(100000, 999999);

            m_audio = GetComponent<AudioSource>(); //Gets the Weapon Source

            if (!m_audio) m_audio = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            m_audio.spatialBlend = 1;

            holster = MTools.GetInstance<HolsterID>("Back Holster 1");
        }
#endif


        [HideInInspector] public int Editor_Tabs1;
        [HideInInspector] public int Editor_Tabs2;
    }


    #region INSPECTOR
#if UNITY_EDITOR
    public abstract class MWeaponEditor : MDamagerEd
    {
        protected string SoundHelp;

        protected GUIStyle DescSTyle;

        protected SerializedProperty
            Sounds, audioSource, weaponType, rightHand,
            ChargeTime, m_MaxCharge,
            chargeCharMultiplier, MaxChargeDamage, m_AimOrigin, m_UI,
            m_AimSide, OnCharged, OnMaxCharged,
            OnUnequiped, OnEquiped,  /*OnPlaced, minDamage, maxDamage,*/ holster, holsterAnim, IKProfile,

            AimIKRight, AimIKLeft, Rate, TwoHandIK, IKHandLeft, IKHandRight, //HandIKLerp,

            mode, stance, strafeOnAim, strafeOnEquip, strafeOnUnequip,
            RidingArmPose, GroundArmPose, groundHandID, //WeaponDirection,
                                                        // RidingCombo, GroundCombo,
                                                        // rotationOffsetIKHand, positionOffsetIKHand,
            OnAiming, m_Automatic, ChargeCurve, HolsterOffset, OnUseFreeHand, OnReleaseFreeHand, m_holsterIndex,
            description, Editor_Tabs2, Editor_Tabs1,
            LeftHandOffset, RightHandOffset,
            // rotationOffsetR, positionOffsetR, rotationOffsetL, positionOffsetL, scaleOffsetR, scaleOffsetL,
            m_IgnoreDraw, m_IgnoreStore;

        // bool offsets = true;

        protected string WeaponTab = "Weapon";

        protected string[] Tabs1 = new string[] { "General", "Damage", "IK", "Extras" };
        protected string[] Tabs2 = new string[] { "Weapon", "D Profiles", "Sounds", "Events" };
        protected MWeapon M;

        protected virtual void SetOnEnable()
        {
            M = (MWeapon)target;
            FindBaseProperties();

            Tabs2[0] = WeaponTab;

            Sounds = serializedObject.FindProperty("Sounds");
            m_UI = serializedObject.FindProperty("m_UI");
            audioSource = serializedObject.FindProperty("m_audio");
            weaponType = serializedObject.FindProperty("weaponType");
            HolsterOffset = serializedObject.FindProperty("HolsterOffset");


            description = serializedObject.FindProperty("description");
            strafeOnEquip = serializedObject.FindProperty("strafeOnEquip");
            strafeOnUnequip = serializedObject.FindProperty("strafeOnUnequip");
            m_Automatic = serializedObject.FindProperty("m_Automatic");
            m_AimOrigin = serializedObject.FindProperty("m_AimOrigin");
            chargeCharMultiplier = serializedObject.FindProperty("chargeCharMultiplier");
            m_MaxCharge = serializedObject.FindProperty("m_MaxCharge");

            rightHand = serializedObject.FindProperty("rightHand");
            //minDamage = serializedObject.FindProperty("minDamage");
            //maxDamage = serializedObject.FindProperty("maxDamage");
            m_IgnoreDraw = serializedObject.FindProperty("m_IgnoreDraw");
            m_IgnoreStore = serializedObject.FindProperty("m_IgnoreStore");

            IKProfile = serializedObject.FindProperty("IKProfile");
            AimIKRight = serializedObject.FindProperty("AimIKRight");
            AimIKLeft = serializedObject.FindProperty("AimIKLeft");


            OnCharged = serializedObject.FindProperty("OnCharged");
            OnMaxCharged = serializedObject.FindProperty("OnMaxCharged");
            OnUnequiped = serializedObject.FindProperty("OnUnequiped");
            OnEquiped = serializedObject.FindProperty("OnEquiped");
            OnAiming = serializedObject.FindProperty("OnAiming");
            OnReleaseFreeHand = serializedObject.FindProperty("OnReleaseFreeHand");
            OnUseFreeHand = serializedObject.FindProperty("OnUseFreeHand");

            holster = serializedObject.FindProperty("holster");
            holsterAnim = serializedObject.FindProperty("holsterAnim");



            LeftHandOffset = serializedObject.FindProperty("LeftHandOffset");
            RightHandOffset = serializedObject.FindProperty("RightHandOffset");


            GroundArmPose = serializedObject.FindProperty("GroundArmPose");
            RidingArmPose = serializedObject.FindProperty("RidingArmPose");
            groundHandID = serializedObject.FindProperty("groundHandID");
            //  WeaponDirection = serializedObject.FindProperty("WeaponDirection");

            m_holsterIndex = serializedObject.FindProperty("m_holsterIndex");


            ChargeTime = serializedObject.FindProperty("chargeTime");
            MaxChargeDamage = serializedObject.FindProperty("MaxChargeDamage");
            ChargeCurve = serializedObject.FindProperty("ChargeCurve");


            m_AimSide = serializedObject.FindProperty("m_AimSide");
            Rate = serializedObject.FindProperty("m_rate");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            TwoHandIK = serializedObject.FindProperty("TwoHandIK");

            IKHandLeft = serializedObject.FindProperty("IKHandLeft");
            IKHandRight = serializedObject.FindProperty("IKHandRight");

            //   HandIKLerp = serializedObject.FindProperty("HandIKLerp");
            //rotationOffsetIKHand = serializedObject.FindProperty("rotationOffsetIKHand");
            //positionOffsetIKHand = serializedObject.FindProperty("positionOffsetIKHand");

            mode = serializedObject.FindProperty("mode");
            stance = serializedObject.FindProperty("stance");
            strafeOnAim = serializedObject.FindProperty("strafeOnAim");
        }

        protected virtual void WeaponInspector(bool showAim = true)
        {
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue != Tabs1.Length) Editor_Tabs2.intValue = Tabs2.Length;

            Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, Tabs2);
            if (Editor_Tabs2.intValue != Tabs2.Length) Editor_Tabs1.intValue = Tabs1.Length;


            //First Tabs
            int Selection = Editor_Tabs1.intValue;

            switch (Selection)
            {
                case 0: DrawWeapon(showAim); break;
                case 1: DrawDamage(); DrawStatModifiers(); break;
                case 2: DrawIK(); break;
                case 3: DrawExtras(); break;
                default: break;
            }

            Selection = Editor_Tabs2.intValue;

            switch (Selection)
            {
                case 0: DrawAdvancedWeapon(); break;
                case 1: DrawProfiles(); break;
                case 2: DrawSound(); break;
                case 3: DrawEvents(); break;
                default: break;
            }
        }

        protected virtual void DrawExtras()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                minForce.isExpanded = MalbersEditor.Foldout(minForce.isExpanded, "Physics Force");

                if (minForce.isExpanded)
                {
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min Force", "Minimun Force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max Force", "Maximun Force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(forceMode);
                    EditorGUILayout.PropertyField(AttackDirection);
                }
            }

            DrawMisc();

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                EditorGUILayout.PropertyField(description);

        }

        protected virtual void DrawDamageProfiles()
        {

        }

        protected virtual void DrawDamage()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Rate.isExpanded = MalbersEditor.Foldout(Rate.isExpanded, "Rate and Charge");

                if (Rate.isExpanded)
                {
                    EditorGUILayout.PropertyField(Rate, new GUIContent("Rate", "Time(Delay) between attacks"));
                    EditorGUILayout.PropertyField(m_Automatic, new GUIContent("Automatic", "Continues Attacking if the Main Attack Input is pressed"));

                    var disable = (M is MShootable) && (M as MShootable).releaseProjectile == MShootable.Release_Projectile.OnAttackStart;

                    using (new EditorGUI.DisabledGroupScope(disable))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(ChargeTime, new GUIContent("Charge Time", "Weapons can be Charged|Hold before releasing the Attack."));
                            if (M.ChargeTime > 0) EditorGUILayout.PropertyField(ChargeCurve, GUIContent.none, GUILayout.Width(80));
                        }

                        if (M.ChargeTime > 0)
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.PropertyField(m_MaxCharge);

                                EditorGUIUtility.labelWidth = 30;
                                EditorGUILayout.PropertyField(chargeCharMultiplier,
                                    new GUIContent("CM", "Charge multiplier to Apply to the Character Charge Value (For the Animator Parameter) "), GUILayout.Width(80));
                                EditorGUIUtility.labelWidth = 0;
                            }

                        }
                    }

                    //if (M.ChargeTime == 0)
                    //    EditorGUILayout.HelpBox("When [Charge Time] is 0 the 'Charge Weapon' logic will be ignored", MessageType.Warning);
                }
            }
        }

        protected void DrawStatModifiers()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(statModifier);
                EditorGUILayout.PropertyField(pureDamage);

            }
            DrawCriticalDamage();

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawElement();
            }
        }

        protected virtual void DrawWeapon(bool showAim = true)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                m_Active.isExpanded = Foldout(m_Active.isExpanded, "General");
                if (m_Active.isExpanded)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_Active);
                        MalbersEditor.DrawDebugIcon(debug);
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(index, new GUIContent("Index", "Unique Weapon ID for each weapon"), GUILayout.MinWidth(1));

                        if (GUILayout.Button("Generate", EditorStyles.miniButton, GUILayout.MaxWidth(70)))
                            index.intValue = UnityEngine.Random.Range(100000, 999999);

                    }

                    CheckWeaponID();
                }
                hitLayer.isExpanded = Foldout(hitLayer.isExpanded, "Layer");
                if (hitLayer.isExpanded)
                {
                    EditorGUILayout.PropertyField(hitLayer);
                    EditorGUILayout.PropertyField(triggerInteraction);
                    EditorGUILayout.PropertyField(AttackDirection);

                    EditorGUILayout.PropertyField(dontHitOwner, new GUIContent("Don't hit Owner"));
                    if (M.dontHitOwner.Value) EditorGUILayout.PropertyField(owner);
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                weaponType.isExpanded = Foldout(weaponType.isExpanded, "Weapon Data");
                if (weaponType.isExpanded)
                {
                    var ID = weaponType.objectReferenceValue != null ? weaponType.objectReferenceValue as WeaponID : null;
                    var color = GUI.color;
                    if (weaponType.objectReferenceValue == null) GUI.color = new Color(1, 0.4f, 0.4f, 1);

                    EditorGUILayout.PropertyField(weaponType,
                        new GUIContent($"Type: {(ID != null ? ID.ID.ToString() : "-")} ",
                        "Gets the Weapon Type ID, Used on the Animator to Play the Matching animation for the weapon. It also is the Mode used on the Character"));

                    GUI.color = color;



                    EditorGUILayout.PropertyField(groundHandID);
                    EditorGUILayout.PropertyField(GroundArmPose);
                    EditorGUILayout.PropertyField(RidingArmPose);
                    // EditorGUILayout.PropertyField(WeaponDirection);

                    // ModeAbilities();
                }


                holster.isExpanded = Foldout(holster.isExpanded, "Holsters");

                if (holster.isExpanded)
                {
                    EditorGUILayout.PropertyField(holster);
                    EditorGUILayout.PropertyField(holsterAnim, new GUIContent("Holster Anim?",
                        "Instead of using the Holster as the Animation ID, use a different Animation value for Draw/Store"));
                    EditorGUILayout.PropertyField(m_holsterIndex);
                    EditorGUILayout.PropertyField(m_IgnoreDraw);
                    EditorGUILayout.PropertyField(m_IgnoreStore);
                }

                stance.isExpanded = Foldout(stance.isExpanded, "Animal Controller");

                if (stance.isExpanded)
                {
                    EditorGUILayout.PropertyField(stance);
                    EditorGUILayout.PropertyField(strafeOnAim);
                    EditorGUILayout.PropertyField(strafeOnEquip);
                    EditorGUILayout.PropertyField(strafeOnUnequip);

                    //EditorGUILayout.PropertyField(GroundCombo);
                    //EditorGUILayout.PropertyField(RidingCombo);
                }
            }

            if (showAim)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(m_AimSide, new GUIContent("Aim Side", "Side of the Character the Weapon will aim when Aim is true"));

                    if (m_AimSide.intValue != 0)
                        EditorGUILayout.PropertyField(m_AimOrigin, new GUIContent("Aim Origin", "Point where the Aiming will be Calculated.\nAlso for Shootable weapons the point where the Projectiles will come out"));
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                rightHand.isExpanded = Foldout(rightHand.isExpanded, "Hand & Holster");
                if (rightHand.isExpanded)
                {
                    if (DescSTyle == null) DescSTyle = MalbersEditor.DescriptionStyle;

                    EditorGUILayout.LabelField($"The Weapon is {(M.IsRightHanded ? "[Right]" : "[Left]")} handed", DescSTyle);
                    EditorGUILayout.PropertyField(rightHand);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(M.IsRightHanded ? RightHandOffset : LeftHandOffset, true);
                    EditorGUI.indentLevel--;

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(HolsterOffset, true);
                    EditorGUI.indentLevel--;
                }
            }

            // DrawProfiles();
        }

        protected virtual void ModeAbilities()
        {
        }

        protected virtual void DrawIK()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (DescSTyle == null) DescSTyle = MalbersEditor.DescriptionStyle;

                EditorGUILayout.LabelField($"The Weapon is {(M.IsRightHanded ? "[Right]" : "[Left]")} handed", DescSTyle);
                EditorGUILayout.PropertyField(rightHand);



                using (new EditorGUI.DisabledGroupScope(M.IsRightHanded))
                    EditorGUILayout.PropertyField(AimIKLeft);
                using (new EditorGUI.DisabledGroupScope(!M.IsRightHanded))
                    EditorGUILayout.PropertyField(AimIKRight);

                //EditorGUILayout.PropertyField(M.IsRightHanded ? AimIKRight : AimIKLeft);



                EditorGUILayout.PropertyField(TwoHandIK);
                if (M.TwoHandIK.Value)
                {
                    EditorGUILayout.LabelField($"The {(M.IsRightHanded ? "[Left]" : "[Right]")} hand is the auxiliar hand", MalbersEditor.DescriptionStyle);

                    using (new EditorGUI.DisabledGroupScope(!M.IsRightHanded))
                        EditorGUILayout.PropertyField(IKHandLeft);
                    using (new EditorGUI.DisabledGroupScope(M.IsRightHanded))
                        EditorGUILayout.PropertyField(IKHandRight);

                }
            }
        }

        protected virtual void DrawAdvancedWeapon() { }

        protected virtual void DrawSound()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(audioSource, new GUIContent("Audio Source", "Audio Source for the weapons"));
                EditorGUI.indentLevel++;
                UpdateSoundHelp();
                EditorGUILayout.PropertyField(Sounds, new GUIContent("Sounds", "Sounds Played by the weapon"), true);
                EditorGUI.indentLevel--;
                EditorGUILayout.HelpBox(SoundHelp, MessageType.None);
            }
        }

        protected override void DrawCustomEvents()
        {
            // EditorGUILayout.PropertyField(OnPlaced, new GUIContent("On Placed [In Holster or Invectory]    "));
            EditorGUILayout.PropertyField(OnEquiped, new GUIContent("On Equiped by (Owner)"));
            EditorGUILayout.PropertyField(OnUnequiped, new GUIContent("On Unequiped by (Owner)"));
            EditorGUILayout.PropertyField(OnCharged, new GUIContent("On Charged Weapon"));
            EditorGUILayout.PropertyField(OnMaxCharged, new GUIContent("On Max Charged Weapon"));
            EditorGUILayout.PropertyField(OnAiming, new GUIContent("On Aiming"));
            EditorGUILayout.PropertyField(OnUseFreeHand);
            EditorGUILayout.PropertyField(OnReleaseFreeHand);
            ChildWeaponEvents();
        }

        protected virtual string CustomEventsHelp() { return ""; }
        protected virtual void ChildWeaponEvents() { }
        protected virtual void UpdateSoundHelp() { }

        protected void CheckWeaponID()
        {
            if (index.intValue == 0)
                EditorGUILayout.HelpBox("Weapon ID needs cant be Zero, Please Set an ID number ", MessageType.Warning);
        }
    }
#endif

    #endregion
}