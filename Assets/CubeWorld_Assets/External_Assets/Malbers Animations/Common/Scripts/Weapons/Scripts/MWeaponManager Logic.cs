using MalbersAnimations.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    /// LOGIC
    ///──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
    public partial class MWeaponManager
    {
        /// <summary>Ignores the Draw and Store Animations</summary>
       // public bool SmoothEquip;

        /// <summary>Get all the Animator Parameters the Animal Controller has</summary>
        private List<int> animatorHashParams;
        /// <summary> Cache if a weapon was equipped when the Weapon Manager was disabled </summary>
        protected MWeapon WeaponEquippedOnDisable;

        /// <summary> Store the value sent to the Animator </summary>
        public int WeaponAnimAction { get; set; }

        private bool ExitByState;
        public bool ExitByMode;

        /// <summary> The weapon was deactivated by an Animation itcannot be activated again until someoneActivates them </summary>
        private bool ExitByAnim { get; set; }

        /// <summary> Returns the Normalized Angle Around the Y Axis (from -180 to 180) regarding the Target position</summary>
        public float HorizontalAngle => Aimer.HorizontalAngle;

        #region INITIALIZE



        protected virtual void Awake()
        {
            if (Anim == null)
                Anim = this.FindComponent<Animator>();                                       //Get the Animator 

            Aimer = this.FindInterface<IAim>();
            Rider = this.FindInterface<IRider>();
            MInput = this.FindInterface<IInputSource>();

            DefaultAnimUpdateMode = Anim.updateMode;

            DefaultAimSide = Aimer.AimSide;

            StoreAfterTime = new WaitForSeconds(StoreAfter.Value);

            GetHashIDs();

            if (LeftHandEquipPoint == null && RightHandEquipPoint == null && anim.isHuman)
            {
                LeftHandEquipPoint = Anim.GetBoneTransform(HumanBodyBones.LeftHand);
                RightHandEquipPoint = Anim.GetBoneTransform(HumanBodyBones.RightHand);
            }

            if (RightHandEquipPoint == null) Debug.LogWarning($"[{name}] - Right Hand Transform is Missing", gameObject);
            if (LeftHandEquipPoint == null) Debug.LogWarning($"[{name}] - Left Hand Transform is Missing", gameObject);

            if (UseHolsters) ActiveHolster = holsters[0]; //Set the Default Holster to be the first one.

            PrepareAnimalController();
        }


        public virtual void Restart()
        {
            OnDisable();
            OnEnable();
        }

        protected virtual void OnEnable()
        {
            //Connect to the animator
            SetBoolParameter += SetAnimParameter;
            SetIntParameter += SetAnimParameter;
            SetFloatParameter += SetAnimParameter;
            SetTriggerParameter += SetAnimParameter;

            if (HasAnimal)
            {
                animal.OnModeStart.AddListener(AnimalModeStart);
                animal.OnModeEnd.AddListener(AnimalModeEnd);
                animal.OnStateActivate.AddListener(AnimalStateActivate);
                DefaultStrafing = animal.Strafe;
                //animal.OnStrafe.AddListener(CheckStrafing);
            }


            if (Rider != null)
            {
                Rider.RiderStatus += GetRiderStatus;        //Get the notifications from the Rider
                IsRiding = Rider.IsRiding;
                MountingDismounting = Rider.IsMounting || Rider.IsDismounting;
            }

            if (MInput != null) ConnectInput(MInput, true);                //Connect the inputs from the Input Source


            ResetWeaponManager();

            if (WeaponEquippedOnDisable != null)
            {
                Equip_Fast(WeaponEquippedOnDisable);
            }


            if (canAim.Variable != null && !canAim.UseConstant) canAim.Variable.OnValueChanged += CanAim_Set; //Listen to the CanAim Variable if is not constant

        }


        protected virtual void OnDisable()
        {
            WeaponEquippedOnDisable = Weapon;

            if (CombatMode) UnEquip_Fast();

            if (HasAnimal)
            {
                animal.OnModeStart.RemoveListener(AnimalModeStart);
                animal.OnModeEnd.RemoveListener(AnimalModeEnd);
                animal.OnStateActivate.RemoveListener(AnimalStateActivate);
                // animal.OnStrafe.RemoveListener(CheckStrafing);
                // if (CombatMode) animal.Mode_Interrupt();
            }


            if (Rider != null) Rider.RiderStatus -= GetRiderStatus;    //Disconnect the notifications from the Rider
            if (MInput != null) ConnectInput(MInput, false);           //Disconnect the inputs from the Input Source

            //Disconnect from the Animator
            SetBoolParameter -= SetAnimParameter;
            SetIntParameter -= SetAnimParameter;
            SetFloatParameter -= SetAnimParameter;
            SetTriggerParameter -= SetAnimParameter;

            StopAllCoroutines();
            IStoreAfter = null;

            Debugging("Weapon Manager Disabled");

            if (canAim.Variable != null && !canAim.UseConstant) canAim.Variable.OnValueChanged += CanAim_Set; //Listen to the CanAim Variable if is not constant
        }



        /// <summary>Sets IsInCombatMode=false, ActiveAbility=null,WeaponType=None and Resets the Aim Mode. DOES **NOT** RESET THE ACTION TO NONE
        /// This one is Used Internally... since the Action will be set by the Store and Unequip Weapons</summary>
        public virtual void ResetCombat()
        {
            WeaponType = 0;
            Weapon?.ResetWeapon();
            WeaponAction = Weapon_Action.None;

            Aim_Set(false);

            CombatMode = false;
            Aim = false;
            OnCanAim.Invoke(false);
            ExitAim();
            // UnEquip_Fast();
            Debugging($"Reset Combat");
        }

        public virtual void ResetWeaponManager()
        {
            if (UseHolsters) PrepareHolsters(); //Prepare the holster if we are using holsers

            // SmoothEquip = true;

            if (startWeapon.Value != null)
            {
                if (!startWeapon.Value.TryGetComponent<MWeapon>(out var StartWComponent))
                {
                    Debug.LogWarning("The Start Weapon does not contain a MWeapon Component. Equiping weapon on start will be ignored.");
                    return;
                }

                if (StartWComponent.gameObject.IsPrefab())
                {
                    Weapon = GameObject.Instantiate(StartWComponent);
                    Weapon.name = Weapon.name.Replace("(Clone)", "");

                    Debugging($"[Start Weapon Instantiated - {Weapon.name}]", "orange");
                    //  Debug.Log($"[Start Weapon Instantiated - {Weapon.name}]");

                }
                else
                {
                    Debugging("[Start Weapon Equiped]", "orange");
                    Weapon = StartWComponent;
                }

                if (Weapon)
                {
                    this.Delay_Action(() =>
                    {
                        Holster_SetActive(Weapon.HolsterID); //Set the Active Holster the Weapon One

                        if (ActiveHolster != null)
                            Holster_AddWeapon(ActiveHolster, Weapon);

                        Equip_Fast();
                        Weapon.IsCollectable?.Pick();
                        AutoStoreWeapon();
                    });
                }
            }
            else
            {
                //Set to unarmed Combo ID
                comboManager?.SetActiveCombo(UnarmedModeID);
            }
        }
        protected virtual void GetHashIDs()
        {
            animatorHashParams = new List<int>();

            foreach (var parameter in Anim.parameters)
            {
                animatorHashParams.Add(parameter.nameHash);
            }

            Hash_LeftHand = TryGetAnimParameter(m_LeftHand);
            Hash_IKAim = TryGetAnimParameter(m_IKAim);              //Get Aim IK
            Hash_IKFreeHand = TryGetAnimParameter(m_IKFreeHand);    //Get Free/Unused Hand IK    


            Hash_WType = TryGetAnimParameter(m_WeaponType);

            //MODE STUFFS (WITHOUT ANIMAL)
            hash_Mode = TryGetAnimParameter(m_Mode);
            hash_ModeOn = TryGetAnimParameter(m_ModeOn);
            Hash_WPower = TryGetAnimParameter(m_WeaponPower);

            //Hash_WAction = TryGetAnimParameter(m_WeaponAction);
        }
        protected virtual void PrepareAnimalController()
        {
            if (HasAnimal)  //Get all the Modes the animal may have 
            {
                DrawMode = animal.Mode_Get(DrawWeaponModeID);
                StoreMode = animal.Mode_Get(StoreWeaponModeID);
                UnArmedMode = animal.Mode_Get(UnarmedModeID);
                animal.IsPreparingMode = false;
            }
            else comboManager = null; //Safe remove the Combo Manager too if there's no animal
        }



        #endregion

        ///// <summary>  store if the animal was strafing </summary>
        //protected virtual void CheckStrafing(bool value) => DefaultStrafing = value;

        #region UPDATE FIXED UPDATE
        void FixedUpdate()
        {
            WeaponCharged(Time.fixedDeltaTime);
        }


        #endregion

        #region Rider
        /// <summary>  Gets notify when the Rider Mount Dismount the Horse </summary>
        protected virtual void GetRiderStatus(RiderAction status)
        {
            var newRiding = status == RiderAction.EndMount;
            MountingDismounting = status == RiderAction.StartMount || status == RiderAction.StartDismount;

            //RECHECK EVERYTHING SO THE WEAPONS CHANGE VALUE TO RIDING
            if (IsRiding != newRiding)
            {
                IsRiding = newRiding;

                Debugging($"Is Riding: {IsRiding}");

                if (CombatMode)
                {
                    comboManager?.SetActiveCombo(IsRiding ? Weapon.RidingCombo : Weapon.GroundCombo);
                    CheckReinHandsEquip();
                }
            }

            if (MountingDismounting)
            {
                Aim_Set(false);
            }

            if (CombatMode)
            {
                if (status == RiderAction.StartMount || status == RiderAction.EndMount) //If is Mounting or Is Riding??
                {
                    WeaponType = Weapon.RidingArmPose ? Weapon.WeaponType : 0;
                }
                else if (status == RiderAction.EndDismount) //If it has finished dismounting (GROUNDED)!!!
                {
                    WeaponType = Weapon.GroundArmPose ? Weapon.WeaponType : 0;  //Set the Weapon Type On Ground
                    comboManager?.SetActiveCombo(Weapon.GroundCombo);
                    SetWeaponStance();

                    if (Aim && Weapon.StrafeOnAim) animal.Strafe = true;        //Restore the Strafing 
                }


            }

            if (Weapon)
                Weapon.Owner = IsRiding ? Rider.Mount : gameObject; //Make sure the Horse is inlcuded on the Do not Hit owner whhen its riding
        }
        #endregion

        #region Reins
        private void CheckReinHandsEquip()
        {
            if (Rider != null && Weapon != null)
            {
                if (Weapon.IsRightHanded) Rider.ReinRightHand(false);
                else Rider.ReinLeftHand(false);
            }
        }

        public void GrabReinsBothHands()
        {
            if (Rider != null)
            {
                Rider.ReinLeftHand(true);
                Rider.ReinRightHand(true);
            }
        }
        public void ReleaseReinsFromHands()
        {
            if (Rider != null)
            {
                Rider.ReinLeftHand(false);
                Rider.ReinRightHand(false);
            }
        }


        /// <summary>Use the Free Hand on the Weapon</summary>
        public void FreeHandUse()
        {
            Weapon?.FreeHandUse();
            ReleaseReinsFromHands();
        }

        /// <summary>Release the Free Hand </summary>
        public void FreeHandRelease()
        {
            Weapon?.FreeHandRelease();
            CheckReinHandsEquip();
        }
        #endregion

        #region IK Weapons

        protected void LateUpdate()
        {
            if (/*CombatMode && */WeaponIsActive)
            {
                Weapon.Weapon_LateUpdate(this);         //If there's an Active Ability do the Late Ability thingy
                                                        //  if (Anim.isHuman) Do_2Hands_IK();
            }
        }
        protected void OnAnimatorIK()
        {
            if (!Anim.isHuman) return; //this only works for Humans
            if (MountingDismounting) return; //Do not do any IK while mounting Dismounting

            if (CombatMode && WeaponIsActive)
            {
                Do_Aim_IK();
                Do_2Hands_IK();
            }
        }

        protected virtual void Do_Aim_IK()
        {
            if (Weapon.AimIK)
            {
                if (Hash_IKAim != 0) IKAimWeight = Anim.GetFloat(Hash_IKAim);
                if (IKAimWeight != 0) Weapon.AimIK.ApplyOffsets(Anim, Aimer.AimOrigin.position, AimDirection, IKAimWeight);
            }
        }

        protected virtual void Do_2Hands_IK()
        {
            //REMEMBER TO SET THE WEAPON IK THAT IS NOT WORKING WHEN DRAWING A WEAPON
            if (Weapon.TwoHandIK && Weapon.IKHandPoint)
            {
                if (Hash_IKAim != 0) IK2HandsWeight = Anim.GetFloat(Hash_IKFreeHand);

                if (IK2HandsWeight != 0)
                {
                    var ikGoal = !Weapon.IsRightHanded ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;

                    Anim.SetIKPosition(ikGoal, Weapon.IKHandPoint.position);
                    Anim.SetIKPositionWeight(ikGoal, IK2HandsWeight);

                    Anim.SetIKRotation(ikGoal, Weapon.IKHandPoint.rotation);
                    Anim.SetIKRotationWeight(ikGoal, IK2HandsWeight);
                }
            }
        }
        #endregion

        #region AIMING
        public bool AimingSide => Aimer.AimingSide;


        /// <summary>Is the Character Aiming?</summary>
        public virtual bool Aim
        {
            protected set
            {
                // if (Weapon) Debug.Log($"WeaponIsActive: {WeaponIsActive}, Weapon.CanAim: {Weapon.CanAim} : WeaponAction{WeaponAction}");

                // if (!WeaponIsActive) return; //Do nothing if the weapon is not active
                if (Weapon && !Weapon.CanAim) return;  //Do nothing if the weapon cannot aim
                if (WeaponAction == Weapon_Action.Store) return;  //Do nothing if the weapon is being stored
                if (!CanAim) return; //Do nothing if the weaponManager cannot aim


                if (aim != value)
                {
                    aim.Value = value; //Do Store the Value of the Aiming

                    //Let know the Rider is Aiming. So if is using Straigth Spine, it stops. (REVIEW)!!!!!!!!!***
                    if (Rider != null) Rider.IsAiming = value;

                    SetAimLogic(value);
                }
            }
            get => aim.Value;
        }

        public virtual void Aim_Set(bool value) => Aim = value;

        public virtual void SetAimLogic(bool value)
        {
            aim.Value = value; //Do Store the Value of the Aiming

            //Let know the Rider is Aiming. So if is using Straigth Spine, it stops. (REVIEW)!!!!!!!!!***
            if (Rider != null) Rider.IsAiming = value;

            Debugging($"Aim → [{value}]", "gray");

            if (Weapon) Weapon.IsAiming = value;    //Update the Aim Value on the Weapon to the active weapon  that the Rider is/isn't aiming

            if (aim)
            {
                if (Weapon)
                {
                    Aimer.AimSide = Weapon.AimSide;         //Send to the Aimer the Corret Side.
                                                            //Enable Strafing if the Weapon Need if the animal was not strafing and the weapon need it
                    if (!DefaultStrafing && HasAnimal && Weapon.StrafeOnAim)
                    {
                        animal.Strafe = true;
                        DefaultStrafing = false;
                    }
                }

                //DO NOT AIM IF THE ANIMAL IS DODGING or doing a high priority mode
                if (HasAnimal && WeaponMode != null && animal.IsPlayingMode && animal.ActiveMode.Priority > WeaponMode.Priority)
                { return; }


                //if We are not Reloading then we can set the Action Aim
                if (WeaponAction == Weapon_Action.Reload)
                {
                    ReloadInterrupt();
                }


                if (Weapon is MShootable shot)
                {
                    //If the weapon is a shootable and has Auto Reload and the ammo in chamber is empty
                    if (shot.AutoReload)
                    {
                        // WeaponAction = Weapon_Action.Reload;
                        Aimer.Active = true;
                        if (shot.TryReload())
                            return;
                    }
                }


                WeaponAction = Weapon_Action.Aim;
                Aimer.Active = true;                                //Activate the Aimer (Invoke the Side Events)
            }
            else
            {
                //if We are not Reloading then we can set the Action Aim
                if (WeaponAction != Weapon_Action.Reload)
                {
                    WeaponAction = CombatMode ? Weapon_Action.Idle : Weapon_Action.None;
                }

                ExitAim();
            }
        }

        /// <summary>This will recieve the messages Animator Behaviors the moment the rider make an action on the weapon</summary>
        public virtual void CheckAim()
        {
            if (WeaponAction == Weapon_Action.Reload) return; //Do not go to aim if the weapon is reloading???

            // Debug.Log($"CHECK AIM {Aim} .... ");

            WeaponAction = Aim ? (Weapon_Action.Aim) : CombatMode ? Weapon_Action.Idle : Weapon_Action.None;
        }

        /// <summary>Exit the Aiming Logic </summary>
        public virtual void ExitAim()
        {
            //Disable Straffing
            if (HasAnimal && Weapon && Weapon.StrafeOnAim && !DefaultStrafing && !ExitByMode)
                animal.Strafe = false;

            this.MInput?.ResetInput(m_AimInput.Value); //Reset Input for Toggle

            Aimer.ExitAim();
        }



        #endregion

        #region Weapon Action Stuff

        /// <summary>  DO NOT Interrupt Higher Priority Modes (Check if the Animal is Playing a Higher Priority Mode)  </summary>  
        protected virtual bool HigherPriorityMode => WeaponMode != null && animal.IsPlayingMode && animal.ActiveMode.Priority > WeaponMode.Priority;

        protected bool JustChangedAction;
        /// <summary>Which Action is currently using the RiderCombat. See WeaponActions Enum for more detail</summary>
        public virtual Weapon_Action WeaponAction
        {
            get => weaponAction;
            set
            {
                //var OldAction = weaponAction;

                //Do it only when the value is different , Do not inlcude the Attack, since you can override an attack with another attack
                //if (weaponAction != value || value == Weapon_Action.Attack)
                {
                    weaponAction = value;
                    Debugging($"[Weapon Action] -> [{value}] - [{(int)value}]", "yellow");

                    JustChangedAction = true;
                    this.Delay_Action(() => JustChangedAction = false); //reset it the next frame

                    switch (weaponAction)
                    {
                        case Weapon_Action.None:
                            GrabReinsBothHands();
                            break;
                        case Weapon_Action.Idle:
                            DoIdleWeaponAnims();
                            AutoStoreWeapon();
                            break;
                        case Weapon_Action.Attack:
                            DoWeaponAttackAnims();
                            break;
                        case Weapon_Action.Draw:
                            TryDrawWeaponAnims();
                            break;
                        case Weapon_Action.Store:
                            TryStoreWeaponAnims();
                            break;
                        case Weapon_Action.Aim:
                            if (WeaponIsActive) Weapon.IsAiming = true;
                            // if (OldAction != value) //Do different Aim Animations if the old action was NOT AIMIN
                            { DoAimAnimations(); }
                            break;
                        case Weapon_Action.Reload:
                            DoReloadAnimations();
                            break;
                        default:
                            break;
                    }

                    OnWeaponAction.Invoke((int)weaponAction);

                    if (StoreAfter.Value > 0 && enabled && gameObject.activeInHierarchy)
                    {
                        if (IStoreAfter != null) StopCoroutine(IStoreAfter);
                        if (weaponAction == Weapon_Action.Idle)
                            IStoreAfter = StartCoroutine(C_StoreAfter());
                    }

                    if (weaponAction == Weapon_Action.None) GrabReinsBothHands(); //THIS?!?!?!?!?!?!? Maybe is for the horse riding animations
                }
            }
        }


        /// <summary>  RECHECK THIS I BELIEVE I NEED TO DO MORE ?!?! </summary>
        protected virtual void DoIdleWeaponAnims()
        {
            //NOT NEEDED
            // Weapon.IsReloading = false; //Reset The Reloading since is on the Idle

            if (!HasAnimal) //If we are not using an animal ?
            {
                CustomWeaponAction(0, 0);
            }
            else if (WeaponMode != null)
            {
                if (Weapon)
                {
                    if (WeaponMode == animal.ActiveMode || animal.ModeAbility != 0)
                        animal.Mode_Stop(true);
                    WeaponMode.InputValue = false; //Make sure the Input value is set to false
                }
            }
        }

        //Remember to check While Riding
        protected virtual void DoAimAnimations()
        {
            if (CombatMode && Weapon.CanAim)
            {
                if (HasAnimal)
                {
                    if (HigherPriorityMode) return; //Avoid Forcing a new Mode if the Animal is Rolling or Dodging.... Doing a Higher Mode.
                    WeaponMode.ForceActivate((int)Weapon_Action.Aim);
                }
                else
                {
                    CustomWeaponAction(Weapon.WeaponType.ID, (int)Weapon_Action.Aim);
                }
                //Weapon.IsAiming = true;
            }
        }

        protected virtual void DoReloadAnimations()
        {
            if (HigherPriorityMode) return; //Avoid Forcing a new Mode if the Animal is Rolling or Dodging.... Doing a Higher Mode.

            if (!Weapon.IsReloading)
            {
                if (HasAnimal)
                {
                    WeaponMode.ForceActivate((int)Weapon_Action.Reload); //Play Reload Animation
                }
                else
                {
                    CustomWeaponAction(Weapon.WeaponType.ID, (int)Weapon_Action.Reload);
                }
            }
        }


        /// <summary> Try Activate the Holster  DRAW Animation for the Weapon </summary>
        public virtual void TryDrawWeaponAnims()
        {
            if (HasAnimal)
            {
                if (DrawMode != null && DrawMode.Active)
                    DrawMode.ForceActivate(Weapon.HolsterAnim);
                else
                    Equip_Fast();
            }
            else
            {
                CustomWeaponAction((int)Weapon_Action.Draw, Weapon.HolsterAnim);
            }
        }
        /// <summary> Try Activate the Holster STORE Animation for the Weapon </summary>
        public virtual void TryStoreWeaponAnims()
        {
            if (HasAnimal)
            {
                StoreWeapon = true;   //Meaning the weapon called the store animations

                if (StoreMode != null && StoreMode.Active && Weapon != null)
                    StoreMode.ForceActivate(Weapon.HolsterAnim);
                else
                    UnEquip_Fast();
            }
            else
            {
                CustomWeaponAction((int)Weapon_Action.Store, Weapon.HolsterAnim);
            }
        }

        protected virtual void CustomWeaponAction(int mode, int value)
        {
            WeaponAnimAction = mode * 1000 + value;
            SetTriggerParameter?.Invoke(hash_ModeOn); //Set Directly the Mode to 0
            SetIntParameter?.Invoke(hash_Mode, WeaponAnimAction); //Set Directly the Mode to 0
        }

        public virtual void SetWeaponCharge(float Charge)
        {
            var RealCharge = Charge * Weapon.ChargeCharMultiplier;

            if (HasAnimal)
            { animal.Mode_SetPower(RealCharge); }
            else
            {
                SetFloatParameter?.Invoke(Hash_WPower, RealCharge);
            }
        }


        /// <summary> If Auto Store weapon is enabled... do it</summary>
        protected virtual void AutoStoreWeapon()
        {

            if (StoreAfter <= 0 || !gameObject.activeInHierarchy) return; //Ignore is Store After is 0

            if (IStoreAfter != null)
            {
                StopCoroutine(IStoreAfter);
                IStoreAfter = StartCoroutine(C_StoreAfter()); //Start Coroutine Store After
            }
        }

        protected virtual void DoWeaponAttackAnims()
        {
            if (Weapon is MMelee WeaponMelee)
            {
                //Check if the weapon can play a Combo 
                if (comboManager && comboManager.ActiveCombo != null)
                {
                    if (comboManager.TryPlay())
                    {
                        Debugging($"[Melee Attack] → [{Weapon.name} <AC>]. Combo[{comboManager.ActiveCombo.Name}] Branch: [{comboManager.Branch}]", "orange");
                        Weapon.CanAttack = true; //Reset the Attack Rate... don't use Default Attack Rate
                    }
                    //else
                    //{
                    //    Debugging($"[Melee Attack] → [{Weapon.name} <AC>] <COMBO FAILED>", "red");
                    //    //Action((int)Weapon_Action.Idle); //Combo Failed
                    //}
                }
                else
                {
                    // Debug.Log("Weapon.CanAttack = " + Weapon.CanAttack);

                    if (Weapon.CanAttack) //Check Weapon Rate
                    {
                        if (WeaponMelee.RidingAttackAbilities == null)
                        { Debug.LogWarning($"The Weapon {Weapon.name} does not have Riding Attack Abilities", this); return; }

                        if (WeaponMelee.GroundAttackAbilities == null)
                        { Debug.LogWarning($"The Weapon {Weapon.name} does not have Riding Attack Abilities", this); return; }

                        var random = UnityEngine.Random.Range(0,
                               IsRiding ? WeaponMelee.RidingAttackAbilities.Length : WeaponMelee.GroundAttackAbilities.Length);

                        random = IsRiding ? WeaponMelee.RidingAttackAbilities[random] : WeaponMelee.GroundAttackAbilities[random];

                        if (HasAnimal)
                        {
                            if (IsRiding)
                            {
                                if (WeaponMelee.UseCameraSide)
                                {
                                    random *= Aimer.AimingSide ? -1 : 1;
                                    if (WeaponMelee.InvertCameraSide) random *= -1;
                                }
                            }

                            if (WeaponMode.ForceActivate(random))
                            {
                                Debugging($"[Melee Attack] → [{Weapon.name} <AC>] <NO Combo>", "orange");
                                Weapon.CanAttack = false;
                                // WeaponMode.InputValue = false; //Needs to reset the weapon Input Mode (For Charging Modes)
                            }
                            else
                            {
                                Action((int)Weapon_Action.Idle); //Mode Failed
                                Debugging($"[Melee Attack] → [{Weapon.name} <AC>] <MODE FAILED>", "gray");
                            }
                        }
                        else
                        {
                            CustomWeaponAction(weaponType, random);
                        }
                    }
                }
            }
            else if (Weapon is MShootable shoot)
            {
                if (HasAnimal)
                {
                    //DO the Weapon Attack Animation
                    if (shoot.HasFireAnim.Value) WeaponMode.ForceActivate((int)Weapon_Action.Attack);

                    Debugging($"[Fire Projectile] [AC] → [{Weapon.name}]", "orange");
                }
                else
                {
                    CustomWeaponAction(Weapon.WeaponType, (int)Weapon_Action.Attack);
                }
            }
        }


        protected virtual void AnimalStateActivate(int state)
        {
            if (CombatMode)
            {
                //Store& unequip Weapon if the animal is on any of these states.
                if (ExitOnState.Contains(animal.ActiveStateID))
                {
                    ExitByState = true;

                    if (ExitFast)
                        UnEquip_Fast();
                    else
                        Store_Weapon();
                }
                else
                {
                    ExitByState = false;
                }
            }

            if (ExitByState && UseHolsters && !ExitOnState.Contains(animal.ActiveStateID))
            {
                Weapon = ActiveHolster.Weapon;  //Get the new Weapon from the Holster

                if (ExitFast)
                    Equip_Fast();
                else
                    Draw_Weapon();

                ExitByState = false;
            }
        }

        public void ExitByAnimation(bool value)
        {
            if (CombatMode && value)
            {
                if (Aim) //Do it only when the character is actually aiming
                {
                    ExitByAnim = true;

                    if (UseHolsters)
                    {
                        UnEquip_Fast();
                    }
                    else
                    {
                        Weapon.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                if (ExitByAnim)
                {
                    ExitByAnim = false;

                    if (UseHolsters)
                    {
                        Weapon = ActiveHolster.Weapon;  //Get the new Weapon from the Holster
                        Equip_Fast();
                    }
                    else
                    {
                        if (Weapon != null)
                            Weapon.gameObject.SetActive(true);
                    }
                }
            }
        }

        protected virtual void AnimalModeStart(int ModeID, int ablility)
        {
            if (CombatMode)
            {
                //Store& unequip Weapon if the animal is on any of these states.
                if (ExitOnModes.Contains(animal.ActiveMode.ID))
                {
                    ExitByMode = true;

                    if (UseHolsters)
                    {
                        UnEquip_Fast();
                    }
                    else
                    {
                        Weapon.gameObject.SetActive(false);
                    }
                }
                else
                {
                    ExitByMode = false;
                }
            }
        }


        /// <summary>Listen to the Animal Making Modes</summary>
        protected virtual void AnimalModeEnd(int ModeID, int ablility)
        {
            if (animal.IsPreparingMode) return; //Do not change if we are already Doing a Mode
            if (JustChangedAction) return;      //Do not change back to Aim until attack finishes
            if (WeaponMode == null) return;  //Do nothing if the Weapon Mode is null

            // if (Weapon) Weapon.AnimalModeEnd(ModeID, ablility);

            if (ExitByMode/* && !ExitOnModes.Exists(x => x.ID == ModeID)*/)
            {
                if (UseHolsters)
                {
                    Weapon = ActiveHolster.Weapon;  //Get the new Weapon from the Holster
                    Equip_Fast();

                }
                else
                {
                    Weapon.gameObject.SetActive(true);
                }

                if (animal.IsPlayingMode && animal.ActiveMode != WeaponMode) //Meaning is other Mode not the Weapon Mode so check if we were Aiming?
                    CheckAim();

                ExitByMode = false;
            }

            if (WeaponMode.ID == ModeID)
            {
                // Debug.Log("SAME WEAPON MODE");
                //Do Nothing
            }
            else
            {
                // Debug.Log("ANOTHER MODE");
                CheckAim();
            }

            if (!animal.IsPlayingMode) //Make if the weapon is not on a mode so return to the default.
                CheckAim();
        }


        #endregion

        #region Draw Store Equip Unequip Weapons 
        public virtual void Equip_Fast()
        {
            // SmoothEquip = false;
            Equip_Weapon();
        }

        /// <summary>Equip Weapon from holster or from Inventory  (Called by the Animator)</summary>
        public virtual void Equip_Weapon()
        {
            //DO NOT Equip is the Active state does not allow it
            if (HasAnimal && ExitOnState.Contains(animal.ActiveStateID)) return;

            if (!Active) return;
            if (Weapon == null) return;
            //if (Weapon.gameObject.IsPrefab()) return;   //Means the Weapon is a prefab and is not instantiated yet (MAKE A WAIT COROUTINE????)

            if (!Weapon.Enabled)
            {
                Debugging("The weapon is Disabled. It cannot be equipped");
                return;
            }

            Weapon.StopAllCoroutines(); //Important! do not leave any pending works!!

            DrawWeapon = false;

            Debugging($"EQUIP → [{Weapon.name}] T:{Time.time:F2}", "orange");

            Equip_Weapon_Data_Ground_Riding();
            EquipWeapon_AnimalController();

            CombatMode = true;

            Weapon.Equip(this);
            OnEquipWeapon.Invoke(Weapon.gameObject);                    //Let everybody know that the weapon is equipped

            //Override the Weapon Layer if is NOT set to none
            if (OverrideWeaponLayer != 0)
            {
                // Debug.Log("override!!!!",Weapon);
                Weapon.m_hitLayer = OverrideWeaponLayer;
            }

            //Auto Aiming HACK!!
            if (Weapon is MShootable && (Weapon as MShootable).aimAction == MShootable.AimingAction.Automatic)
                Aim_Set(true);

            CheckAim();

            OnCanAim.Invoke(Weapon.CanAim);

            Weapon.PlaySound(WSound.Equip);                             //Play Equip Sound

            CheckReinHandsEquip();
            ParentWeapon();

            if (UseHolsters)                                                             //If Use holster Means that the weapons are on the holster
            {
                var Offset = Weapon.IsRightHanded ? Weapon.RightHandOffset : Weapon.LeftHandOffset; //Store the HandOffset

                if (IgnoreHandOffset.Value)
                {
                    Offset = new TransformOffset(0)
                    {
                        Scale = Weapon.transform.localScale
                    };
                }

                //Local position when is Parent to the weapon
                Weapon.transform.SetLocalTransform(Offset.Position, Offset.Rotation, Offset.Scale);
                //  SmoothEquip = true;
            }
            else //if (UseExternal)                           //If Use Inventory means that the weapons are on the inventory
            {
                //Apply the Offset Hand Value to the 
                if (!IgnoreHandOffset.Value)
                {
                    Weapon.ApplyOffset();
                }
                else
                {
                    Weapon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                }
            }

            Weapon.gameObject.SetActive(true);            //Set the Game Object Instance Active    


            if (IsRiding && Rider != null)
            {
                Weapon.Owner = Rider.Mount; //Make sure the Horse is inlcuded on the Do not Hit owner
            }
        }

        private void CheckCoroutines()
        {
            if (C_SmoothEquip != null)
            {
                // Debug.Log("StopUNe");
                StopCoroutine(C_SmoothEquip);
                Weapon.transform.SetLocalTransform(Weapon.HolsterOffset); //We are going to equip so restore the weapon on the Holster
            }

            if (C_SmoothUneEquip != null)
            {
                // Debug.Log("StopEq");
                StopCoroutine(C_SmoothUneEquip);
                //Weapon.transform.SetLocalTransform(Offset.Position, Offset.Rotation, Offset.Scale); //We are going to Unequip so restore the weapon on the Hand
            }
        }

        private IEnumerator C_SmoothEquip;
        private IEnumerator C_SmoothUneEquip;


        /// <summary>Unequip Weapon from holster or from Inventory (Called by the Animator)</summary>
        public virtual void Unequip_Weapon()
        {
            ResetCombat();
            if (Weapon == null) return;
            Debugging($"UNEQUIP → [{Weapon.name}] T:{Time.time:F2}", "orange");  //Debug

            StoreWeapon = false;
            //Has_IKAim = false;
            IKAimWeight = 0;
            WeaponType = 0;                                                  //Set the weapon ID to None (For the correct Animations)
            OnUnequipWeapon.Invoke(Weapon.gameObject);                      //Let the rider know that the weapon has been unequiped.

            if (UseHolsters)                                                //If Use holster Parent the ActiveMWeapon the the holster
            {
                if (Weapon.Holster != null) //Meaning the weapon has a holster
                {
                    SetWeaponParent(Weapon, ActiveHolster.GetSlot(Weapon.HolsterSlot)); //Parent the weapon to the holster

                    Weapon.transform.SetLocalTransform(Weapon.HolsterOffset); //Set the Holster Offset Option
                }
                //  SmoothEquip = true;
            }
            else// if (UseExternal)
            {
                if (DestroyOnUnequip)
                    Destroy(Weapon.gameObject);
                //else
                //    Weapon.gameObject.SetActive(false);
            }

            UnequipWeapon_AnimalController();

            Weapon = null;     //IMPORTANT

            WeaponAction = Weapon_Action.None;
        }


        /// <summary>  Set the proper values for the Weapon while is grounded or Riding  </summary>
        protected virtual void Equip_Weapon_Data_Ground_Riding()
        {
            if (!IsRiding) //GROUNDED
            {
                WeaponType = Weapon.GroundArmPose ? Weapon.WeaponType : 0;
                comboManager?.SetActiveCombo(Weapon.GroundCombo);
            }
            else //RIDING
            {
                WeaponType = Weapon.RidingArmPose ? Weapon.WeaponType : 0;
                comboManager?.SetActiveCombo(Weapon.RidingCombo);
            }
        }

        protected virtual void EquipWeapon_AnimalController()
        {
            if (HasAnimal)
            {
                SetWeaponStance();

                WeaponMode = animal.Mode_Get(Weapon.WeaponType);               //Cache the Weapon Mode, if it has one

                //Disable all the modes included on the Disable Mode list
                EnableModesAC(false);

                if (WeaponMode != null)
                {
                    WeaponMode.SetActive(true); //Activate the Mode
                }
                else
                {
                    Debug.LogWarning("The Animal Controller does not have a mode for the Equipped Weapon!!");
                    Weapon.Enabled = false; //Disable the weapon... it cannot be used!!!
                }

                if (Weapon.StrafeOnEquip) animal.Strafe = true;
            }
        }

        public virtual void UnequipWeapon_AnimalController()
        {
            if (HasAnimal)
            {
                if (Weapon.stance != null /*&& !IsRiding*/)
                {
                    animal.Stance_RestoreDefault(); //Reset the Default Stance (Remove the Combat)
                    animal.Stance_Reset(); //Reset Stance if the animal was using a stance for the weapon
                }


                if (comboManager)  //Set to unarmed Combo ID
                    comboManager.SetActiveCombo(UnarmedModeID);


                //Enable Back all the modes included on the Disable Mode list
                EnableModesAC(true);

                foreach (var m in DisableModes)
                {
                    animal.Mode_Enable(m);
                }


                if (WeaponMode != null)
                {
                    //Important! the weapon before unequipping was playing a mode.. E.g. Aiming you need to stop it!!!
                    if (WeaponMode.PlayingMode)
                    {
                        animal.Mode_Stop();
                    }

                    //   if (WasStrafing)

                    animal.Strafe = Weapon.StrafeOnUnequip;

                    WeaponMode.SetActive(false); //Disable Weapon Mode
                    WeaponMode = null;
                }

            }
        }

        private void SetWeaponStance()
        {
            if (Weapon.stance)
            {
                animal?.Stance_Set(Weapon.stance);           //Set the Stance to use on the Animal Controller
                animal.Stance_SetDefault(Weapon.stance);
            }
        }

        private void EnableModesAC(bool enable)
        {
            foreach (var m in DisableModes)
            {
                if (enable)
                    animal.Mode_Enable_Temporal(m);
                else
                    animal.Mode_Disable_Temporal(m);
            }
        }

        public void UnEquip() => UnEquip_Fast();

        public virtual void UnEquip_Fast()
        {
            //SmoothEquip = false; //Skip the Smooth Equipment.
            Unequip_Weapon();
        }


        /// <summary> Parents the Weapon to the Correct Hand</summary>
        public virtual void ParentWeapon()
        {
            if (Weapon.IsRightHanded && RightHandEquipPoint)  //Parent to the Right Hand Equip Point
            {
                SetWeaponParent(Weapon, RightHandEquipPoint);
            }
            else if (LeftHandEquipPoint)
            {
                SetWeaponParent(Weapon, LeftHandEquipPoint);
            }
        }

        public virtual void SetWeaponParent(MWeapon weapon, Transform parent) => weapon.transform.parent = parent;


        /// <summary> Draw (Set the Correct Parameters to play Draw Weapon Animation) </summary>
        public virtual void Draw_Weapon()
        {
            if (!Active) return;

            //DO NOT Equip is the Active state does not allow it
            if (HasAnimal && ExitOnState.Contains(animal.ActiveStateID)) return;

            DrawWeapon = true;

            ExitAim(); //DO NOT AIM WHEN DRAWING WEAPONS

            //If is using External Equip
            if (UseExternal)
            {
                //Set the Current holster to the weapon asigned holster (THE WEAPON IS ALREADY SET)
                // if (Weapon != null) Holster_SetActive(Weapon.HolsterID);
            }
            else //if (UseHolsters) 
                Weapon = ActiveHolster.Weapon;  //Get the new Weapon from the Holster


            if (Weapon)
            {
                if (Weapon.IgnoreDraw || IgnoreDraw)
                {
                    Equip_Fast();
                    return;
                }

                WeaponAction = Weapon_Action.Draw;

                CheckReinHandsEquip();

                Debugging($"Draw → {(Weapon.IsRightHanded ? "Right Hand" : "Left Hand")} → [{Weapon.Holster.name} → {Weapon.name}]", "yellow");  //Debug
            }
        }


        /// <summary>Store (Set the Correct Parameters to play Store Weapon Animation) </summary>
        public virtual void Store_Weapon()
        {
            if (Weapon == null) return;                    //Skip if there's no Active Weapon or is not inCombatMode, meaning there's an active weapon
            if (!Weapon.CanUnequip) return;                //Skip if there's no Active Weapon or is not inCombatMode, meaning there's an active weapon
            //if (WeaponAction != Weapon_Action.Idle) return; //Do not store if we are not finishing storing

            ExitAim();

            Weapon.StopAllCoroutines(); //Important! do not leave any pending works!!
            FreeHandRelease(); //Release the Hand

            if (Weapon.IgnoreStore || IgnoreStore)
            {
                UnEquip_Fast();
                return;
            }

            StoreWeapon = true;

            WeaponAction = Weapon_Action.Store;                 //Set the  Weapon Action to Store Weapons 

            Weapon.StoringWeapon();

            Debugging($"[Store → {(Weapon.IsRightHanded ? "Right Hand" : "Left Hand")}] → [{Weapon.Holster.name}] → [{Weapon.name}]", "cyan");  //Debug
        }

        /// <summary> Activate the Damager of a Weapon. E.g. the Attack Trigger of a Melee Weapon    </summary>
        public virtual void ActivateDamager(int value, int prof)
        {
            if (Weapon) Weapon.ActivateDamager(value, prof);
        }

        private int LastAttackTriggerHash;
        public virtual void DamagerAnimationStart(int hash)
        {
            LastAttackTriggerHash = hash;
        }

        public virtual void DamagerAnimationEnd(int hash)//??????
        {
            //Go to Idle because we are finishing in the same animations. if not is doing another attack before finishing the one that we have
            if (!HasAnimal && LastAttackTriggerHash == hash)
            {
                WeaponAction = Weapon_Action.Idle;
                //Debug.Log("WEAPONMANAGER TO IDLE");
            }
        }

        /// <summary> Is called to swap weapons</summary>
        private IEnumerator SwapWeaponsHolster(int HolstertoSwap)
        {
            if (Weapon)
            {
                Store_Weapon();

                while (WeaponAction == Weapon_Action.Aim) yield return null;    // Wait for the weapon is Unequiped Before it can Draw Another
                while (WeaponAction == Weapon_Action.Store) yield return null;    // Wait for the weapon is Unequiped Before it can Draw Another
            }

            Holster_SetActive(HolstertoSwap);
            Draw_Weapon();                                  //Set the parameters so draw a weapon
            yield return null;
        }
        #endregion
    }
}