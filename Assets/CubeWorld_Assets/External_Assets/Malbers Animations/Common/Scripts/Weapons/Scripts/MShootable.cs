using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System.Collections;
using UnityEngine.Pool;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Shootable")]
    public class MShootable : MWeapon, IShootableWeapon, IThrower
    {
        #region Enums
        public enum Release_Projectile
        {
            Never,
            OnAttackStart,
            OnAttackReleased,
        }
        public enum Equip_Projectile
        {
            ByAnimation = 1,
            OnAim = 2,
            OnAttackStart = 4,
            OnAttackReleased = 8,
            AfterReload = 16,
            OnProjectileReleased = 32,
            OnEquip = 64,
        }
        public enum AimingAction { Manual, Automatic, Ignore }
        #endregion 

        #region Variables
        //public enum Cancel_Aim { ReleaseProjectile, ResetWeapon }

        [Tooltip("When the projectile will be released")]
        public Release_Projectile releaseProjectile = Release_Projectile.OnAttackStart;

        [Flag, Tooltip("When the projectile will be released")]
        public Equip_Projectile equipProjectile = Equip_Projectile.OnAim | Equip_Projectile.ByAnimation;


        [Tooltip("How the weapon will handle aiming")]
        public AimingAction aimAction = AimingAction.Manual;

        [Tooltip("Delay to release the projectile after the Attack is Played. E.g. the Trhow animation is played but the projectile will be released a seconds after")]
        public FloatReference releaseDelay = new();

        /////<summary> When Aiming is Cancel what the Weapon should do? </summary>
        //[Tooltip("When the Projectile is Release?")]
        //public Cancel_Aim CancelAim = Cancel_Aim.ResetWeapon;

        [Tooltip("The projectile will be released by the Animator using Weapon message behavior")]
        public BoolReference releaseByAnimation = new(false);


        [Tooltip("Projectile prefab the weapon fires")]
        public GameObjectReference m_Projectile = new();                                //Arrow to Release Prefab


        [Tooltip("Spread Angle for the Projectile")]
        public FloatReference spreadAngle = new(0);
        [Tooltip("Spread Multiplier the spread angle")]
        public Vector3Reference spreadMult = new(Vector3.one);

        //[Tooltip("Amount of projectiles to fire at the same time")]
        //public FloatReference amount = new(1);

        /// <summary>  Pool </summary>        
        [Tooltip("Initial number of Projectiles in Projectile Pool")]
        public IntReference m_ProjectileInitialPoolSize = new(10); // Initial size of pool
        [Tooltip("Max number of Projectiles in Projectile Pool")]
        public IntReference m_ProjectileMaxPoolSize = new(1000); // Max size of pool

        // Define Projectile Pool
        protected IObjectPool<GameObject> m_ProjectilePool; // This is pool that is used

        // Set public IObjectPool that returns Projectile Pool or creates one if not existing
        public virtual IObjectPool<GameObject> ProjectilePool // this is returning m_ProjectilePool and create it if that is null
        {
            get
            {
                if (m_ProjectilePool == null)
                {
                    m_ProjectilePool = new ObjectPool<GameObject>
                        (CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false,
                        m_ProjectileInitialPoolSize.Value, m_ProjectileMaxPoolSize.Value);
                }

                return m_ProjectilePool;
            }
        }

        protected virtual GameObject CreatePooledItem()
        {
            // This is used to return Projectile to the pool when they have stopped.
            // Create instance of Projectile
            GameObject projectileInstance = Instantiate(m_Projectile.Value);
            projectileInstance.SetActive(false); // Disable it
            projectileInstance.name = projectileInstance.name.Replace("(Clone)", "(Pool)"); // Remove (Clone) from the name

            // NOTE - Implemented as property in IProjectile and Prepare_Projectile but also in MProjectile and Prepare
            // Add ReturnToPool to Projectile - NOT NEEDED, IMPLEMENTED INTO PROJECTILE
            //ProjectileReturnToPool returnToPool = projectileInstance.AddComponent<ProjectileReturnToPool>();
            // Define which Pool is for this Projectile
            //returnToPool.thisProjectilePool = ProjectilePool; - NOT NEEDED, IMPLEMENTED INTO PROJECTILE

            // Return prefab to be added to Pool
            return projectileInstance;
        }

        /// <summary> Called when an item is returned to the pool using Release Used by Destroy from Projectile and ProjectileReturnToPool component </summary>
        void OnReturnedToPool(GameObject Go)
        {
            if (m_ProjectileParent != null) // Check parent 
            {
                Go.transform.SetParent(m_ProjectileParent, false); // Set as Child to Projectile Parent
            }
            else
            {
                Go.transform.SetParent(this.gameObject.transform, false); // or to Weapon
            }
            Go.transform.ResetLocal(); //Restore the position
            Go.SetActive(false);       // Disable, Hide it!
                                       // Debug.Log("Projectile returned to pool, left: " + ProjectilePool.CountInactive);
        }

        // Called when an item is taken from the pool using Get 
        // like ProjectilePool.Get()
        void OnTakeFromPool(GameObject Go)
        {
            Go.transform.SetParent(null, true); // Unparent projectile
            Go.SetActive(true); // Activate 

            //  Debug.Log("Projectile taken from pool, left: " + ProjectilePool.CountInactive);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        void OnDestroyPoolObject(GameObject Go)
        {
            if (Application.isPlaying)
                Destroy(Go); // When pool size excides max size destroy projectile
            //Debug.Log("Projectile taken from pool, left: " + ProjectilePool.CountInactive);
        }

        protected virtual void PopulatePool()
        {
            if (m_ProjectileInitialPoolSize > 0)
            {
                for (int i = 0; i < m_ProjectileInitialPoolSize; i++)
                {
                    ProjectilePool.Release(CreatePooledItem());
                }
            }
        }

        /// <summary>  If the projectile value has changed then reset the pool </summary>
        public virtual void OnProjectileChanged(GameObject newProjectile)
        {
            if (newProjectile != m_Projectile)
            {
                //Reset pool
                ProjectilePool.Clear();
                PopulatePool();
            }
        }
        /// <summary> End Pool  </summary>

        [Tooltip("Parent of the Projectile")]
        public Transform m_ProjectileParent;
        public Vector3Reference gravity = new(Physics.gravity);

        public BoolReference UseAimAngle = new(false);

        [Tooltip("Does the weapon has Fire Animation? if not then does not require to exit Aim Animation")]
        public BoolReference HasFireAnim = new(true);

        public FloatReference m_AimAngle = new(0);

        /// <summary>This Curve is for Limiting the Bow Animations while the Character is on weird/hard Positions</summary>
        [MinMaxRange(-180, 180)]
        [Tooltip("Value to limit firing projectiles when the Character is on weird or dificult Positions. E.g. Firing Arrows on impossible angles")]
        public RangedFloat AimLimit = new(-180, 180);

        /// <summary>Can the Weapon be Charged? Meaning Charge time is greater than 0</summary>
        public override bool CanCharge => releaseProjectile == Release_Projectile.OnAttackReleased && ChargeTime > 0;

        //Make sure the weapon can unequip on aim
        public override bool CanUnequip => !IsAiming || UnequipOnAim;

        #region Reload and Ammo
        [Tooltip("Ignore Completely the Reload Logic")]
        public BoolReference noReload = new(false);

        /// <summary>Is the Playing a Reloading animation</summary>
        public IntReference m_Ammo = new(30);                              //Total of Ammo for this weapon
        public IntReference m_AmmoInChamber = new(1);                      //Total of Ammo in the Chamber
        public IntReference m_ChamberSize = new(1);                        //Max Capacity of the Ammo in once hit

        [Tooltip("Time needed to complete the reload of a weapon. Put a new projectile(s) the chamber ")]
        public FloatReference m_ReloadTime = new(1.5f);                    //Press Fire one or continues 

        [Tooltip("The weapon will Reload the weapon right after the last projectile of the chamber has been released")]
        public BoolReference m_AutoReload = new(false);                    //Press Fire one or continues 

        [Tooltip("Delay time to auto-Reload right after the weapon has no ammo in chamber and the last projectile has been released")]
        public FloatReference m_AutoReloadTime = new(0.25f);               //Press Fire one or continues 

        [Tooltip("If the Weapon have reload animation then Play it")]
        public BoolReference HasReloadAnim = new(false);
        #endregion

        #endregion

        #region Events
        public GameObjectEvent OnLoadProjectile = new();
        public GameObjectEvent OnFireProjectile = new();
        public UnityEvent OnReloadStart = new();
        public UnityEvent OnReload = new();
        #endregion

        #region Properties
        public override bool CanAttack
        {
            get => canAttack;
            set
            {
                if (Rate <= 0) { canAttack = true; return; } //Restore Can Attack if the weapon has no Rate

                canAttack = value;
                // Debug.Log($"CAN ATTACK {value}");
                if (!canAttack)
                {
                    this.Stop_Action(iCanAttack); //IMPORTANT!! STOP THE COROUTINE

                    iCanAttack = this.Delay_Action(Rate, () =>
                    {
                        CanAttack = true;
                        //Check if we were aiming after the attack cooldown finish
                        if (!IsReloading && !InAutoReloadTime) CheckAim();
                    });

                }
            }
        }
        private IEnumerator iCanAttack;

        public virtual GameObject Projectile
        {
            get => m_Projectile.Value;
            set
            {
                //Verify the if the projectile value has changed
                if (value != m_Projectile.Value && m_Projectile.UseConstant)
                {
                    OnProjectileChanged(value);
                }

                m_Projectile.Value = value;
            }
        }




        /// <summary> Pool - Added 2 new int values as InitialPoolSize and MaxPoolSize </summary> 
        public virtual int ProjectileInitialPoolSize { get => m_ProjectileInitialPoolSize.Value; set => m_ProjectileInitialPoolSize.Value = value; }
        public virtual int ProjectileMaxPoolSize { get => m_ProjectileMaxPoolSize.Value; set => m_ProjectileMaxPoolSize.Value = value; }
        /// <summary>  Pool - End  </summary> 

        public virtual float AutoReloadTime { get => m_AutoReloadTime.Value; set => m_AutoReloadTime.Value = value; }

        /// <summary> Delay to release the projectile after the Attack is Played  </summary>
        public virtual float ReleaseDelay { get => releaseDelay.Value; set => releaseDelay.Value = value; }
        private IEnumerator iReleaseDelay;


        /// <summary>The projectile will be released by the animator </summary>
        public virtual bool ReleaseByAnimation { get => releaseByAnimation.Value; set => releaseByAnimation.Value = value; }

        /// <summary>Ignore Reload Logic</summary>
        public virtual bool NoReload { get => noReload.Value; set => noReload.Value = value; }
        public virtual bool HasReload => !NoReload;


        /// <summary>The weapon is playing autoreload Time</summary>
        public virtual bool InAutoReloadTime { get; protected set; }

        /// <summary> Projectile Instance to launch from the weapon</summary>
        public virtual GameObject ProjectileInstance { get; set; }

        public virtual bool ProjectileEquipped => ProjectileInstance != null;

        /// <summary> Projectile Interface Reference</summary>
        public MProjectile MProjectile { get; set; }
        public Transform ProjectileParent => m_ProjectileParent;

        //public bool InstantiateProjectileOfFire = true;

        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }

        /// <summary> Adds a Throw Angle to the Aimer Direction </summary>
        public float AimAngle { get => m_AimAngle.Value; set => m_AimAngle.Value = value; }
        public Vector3 Velocity { get; set; }
        public System.Action<bool> Predict { get; set; }

        /// <summary> Total Ammo of the Weapon</summary>
        public int TotalAmmo { get => m_Ammo.Value; set => m_Ammo.Value = value; }

        public int AmmoInChamber
        {
            get => m_AmmoInChamber.Value;
            set
            {
                // Debug.Log("AmmoInChamber: " + value);
                m_AmmoInChamber.Value = value;
            }
        }

        /// <summary>When the Ammo in Chamber gets to Zero it will reload Automatically</summary>
        public bool AutoReload { get => m_AutoReload.Value; set => m_AutoReload.Value = value; }

        public int ChamberSize { get => m_ChamberSize.Value; set => m_ChamberSize.Value = value; }

        public override bool HasAmmo => TotalAmmo == -1 || AmmoInChamber > 0 || NoReload;

        /// <summary>Aim IK Weight</summary>
        public float AimWeight { get; private set; }

        /// <summary>With Aim Limit?</summary>
        public bool CanShootWithAimLimit { get; private set; }

        [Tooltip("The Shootable can be stored if is aiming. ")]
        public BoolReference m_UnequipOnAim = new(false);
        public bool UnequipOnAim { get => m_UnequipOnAim.Value; set => m_UnequipOnAim.Value = value; }


        public override bool IsEquiped
        {
            get => base.IsEquiped;
            set
            {
                base.IsEquiped = value;

                if (value)
                {
                    if ((equipProjectile & Equip_Projectile.OnEquip) == Equip_Projectile.OnEquip)
                        EquipProjectile();

                    //Make sure its a projectile in the chamber!!
                    if (AutoReload)
                    {
                        TryReload();
                    }
                }
                else
                {
                    DestroyProjectileInstance(); //If by AnyChange the Projectile is live Destroy it!!
                }
            }
        }

        public override bool IsAiming
        {
            set
            {
                base.IsAiming = value;

                if (value)
                {
                    if ((equipProjectile & Equip_Projectile.OnAim) == Equip_Projectile.OnAim)
                        EquipProjectile();
                }
                else
                {
                    //ResetCharge(); Only Reset charge 
                }
            }
        }


        #endregion


        [SerializeField, Tooltip("Apply Gravity after certain distance is reached")]
        private FloatReference m_AfterDistance = new(0f);
        public float AfterDistance { get => m_AfterDistance.Value; set => m_AfterDistance.Value = value; }

        public override bool CanAim => true;

        ///// <summary>  Set the total Ammo (Refill when you got some ammo)  </summary>
        //public void SetTotalAmmo(int value)
        //{
        //    if (AutoReload) TryReload();
        //}

        protected virtual void Awake()
        {
            Initialize();

            if (AimOrigin == null) AimOrigin = transform;

            if (ChamberSize < 0) ChamberSize = 1; //Bug Fix
            if (ReleaseDelay < 0) releaseDelay = 0; //Clamp to zero

            /// <summary>
            /// Pool - Fill Pool with gameObjects
            /// </summary> 
            // We need to populate Pool manualy because of Unity
            // New ObjectPool does have min value but it does not populate pool
            //PopulatePool();
            //Debug.Log("ProjectilePool has instances: " + ProjectilePool.CountInactive);

            /// <summary> Pool - End </summary>

        }


        private void OnEnable()
        {
            if (m_Projectile.Variable != null)
                m_Projectile.Variable.OnValueChanged += OnProjectileChanged;
        }

        private void OnDisable()
        {
            if (m_Projectile.Variable != null)
                m_Projectile.Variable.OnValueChanged -= OnProjectileChanged;
        }

        internal override void MainAttack_Start(IMWeaponOwner RC)
        {
            Input = true;

            base.MainAttack_Start(RC);

            if (IsReloading || RC.StoreWeapon)
            {
                return; //Do not fire if is reloading
            }

            // Calculate is there's an Imposible range to shoot 
            CanShootWithAimLimit = (AimLimit.IsInRange(RC.HorizontalAngle));

            if (!RC.Aim && aimAction == AimingAction.Automatic)
            {
                //Force aiming to charge the weapon
                RC.Aim_Set(true);
            }

            //If can Aim but is not aiming or is not on limits skip
            if (aimAction != AimingAction.Ignore && !IsAiming || !CanShootWithAimLimit)
            {
                return;
            }

            if (CanAttack) //and the Rider is not on any reload animation
            {
                if (HasAmmo)                                                                  //If there's Ammo on the chamber
                {
                    //Equip the projectile if its set to intantiate on start
                    if ((equipProjectile & Equip_Projectile.OnAttackStart) == Equip_Projectile.OnAttackStart)
                        EquipProjectile();

                    //Means the Weapon does not need to Charge  so Release the Projectile First!
                    if (releaseProjectile == Release_Projectile.OnAttackStart)
                    {
                        Debugging($"<color=white> Weapon <b>[Fire Projectile] On Start </b></color>", this);  //Debug

                        FireAnim_ReleaseProjectile();
                    }
                }
                else
                {
                    PlaySound(WSound.Empty);                   //Play Empty Sound Which is stored in the 4 Slot  
                    Debugging("<color=red> <b>[Empty Ammo]</b> </color>", this);
                }
            }
            else
            {
                // Debug.Log("CANNOT ATTACK");
            }
        }

        internal override void MainAttack_Released(IMWeaponOwner RC)
        {
            Input = false;

            Debugging($"Main Attack Released", this);


            AttackFromAutomatic = false;

            //Check No Aiming values (NEW)
            if (aimAction != AimingAction.Ignore && !IsAiming || !CanShootWithAimLimit) return;

            if (!CanAttack) return; //Do nothing if the Rate is on ????

            if (releaseProjectile == Release_Projectile.OnAttackReleased)
            {
                if (HasAmmo)   //If we are not firing any arrow then try to Attack with the bow
                {
                    //Equip the Projectile if its equipped on attack released
                    if ((equipProjectile & Equip_Projectile.OnAttackReleased) == Equip_Projectile.OnAttackReleased)
                        EquipProjectile();

                    FireAnim_ReleaseProjectile();
                }
            }

            // ResetCharge();
        }


        private void FireAnim_ReleaseProjectile()
        {
            //Play the Fire Animation on the Character IF the Weapon can be charged!!
            if (HasFireAnim.Value)
                WeaponAction?.Invoke((int)Weapon_Action.Attack);


            //Fire the projectile on Attack Start after a delay time if is not released by the Animator
            if (!ReleaseByAnimation)
                this.Delay_Action(ref iReleaseDelay, ReleaseDelay, () => ReleaseProjectile());

            CanAttack = false;
            IsAttacking = true;
        }



        //IEnumerator I_AutoRelease;

        /// <summary> Charge the Weapon!! </summary>
        internal override void Attack_Charge(IMWeaponOwner RC, float time)
        {
            if (Input) //The Input For Charging is Down
            {
                if (CanAttack && !IsAttacking)
                {
                    if (aimAction == AimingAction.Manual && !IsAiming) return; //Do not charge it the aiming is manual

                    if (HasAmmo && CanCharge)  //Is the Weapon ready?? we Have projectiles and we can Charge
                    {
                        //CalculateAimLimit(RC);
                        if (!CanShootWithAimLimit)
                        {
                            ResetCharge();
                            return;
                        }

                        if (!IsCharging)            //If Attack is pressed Start Bending for more Strength the Bow
                        {
                            IsCharging = true;
                            ChargeCurrentTime = 0;
                            Predict?.Invoke(true);
                            PlaySound(WSound.Charge); //Play the Charge Sound
                            Debugging("[Charge: 0]", this);
                        }
                        else    //If Attack is pressed Continue Bending the Bow for more Strength the Bow
                        {
                            if (!IsAttacking) Charge(time);
                        }
                    }


                    //If is automatic then continue attacking ◘◘◘TEST THIS!!!!!
                    if (Automatic && Rate > 0 && !IsReloading && HasAmmo)
                    {
                        if (releaseProjectile == Release_Projectile.OnAttackStart)
                        {
                            Debugging($"[**Automatic Fire** Attack Start]", this);
                            ChargeCurrentTime = ChargeTime; //Make full charge!!

                            MainAttack_Start(RC);
                        }
                        else if (releaseProjectile == Release_Projectile.OnAttackReleased && MaxCharged)
                        {
                            Debugging($"[**Automatic Fire** Attack Released]", this);

                            if (HasAmmo)   //If we are not firing any arrow then try to Attack with the bow
                            {
                                //Equip the Projectile if its equipped on attack released
                                if ((equipProjectile & Equip_Projectile.OnAttackReleased) == Equip_Projectile.OnAttackReleased)
                                    EquipProjectile();

                                FireAnim_ReleaseProjectile();
                            }

                            // ResetCharge();
                            Input = true;
                        }

                        return;
                    }

                }
                //else
                //{
                //    Predict?.Invoke(false);
                //}
            }
        }

        public virtual void ReduceAmmo(int amount)
        {
            AmmoInChamber -= amount;

            Debugging($"[Ammo: Reduced <b>-({amount})</b> ,Total<b>({TotalAmmo})</b>, In Chamber<b>({AmmoInChamber})</b>]", this);

            if (AmmoInChamber <= 0 && AutoReload)
            {
                if (!HasReloadAnim)
                    IsReloading = true; //otherwise it will not reproduce the reload animations ??

                InAutoReloadTime = true;
                this.Delay_Action(AutoReloadTime,
                   () =>
                   {
                       TryReload();
                       InAutoReloadTime = false;
                       //IsReloading = false;
                   });
            }
        }

        internal override void Weapon_LateUpdate(IMWeaponOwner RC)
        {
            CanShootWithAimLimit = (AimLimit.IsInRange(RC.HorizontalAngle)); // Calculate is there's an Imposible range to shoot 
        }

        public override void ResetCharge()
        {
            base.ResetCharge();
            Predict?.Invoke(false);
            Velocity = Vector3.zero; //Reset Velocity
        }

        public override void Charge(float time)
        {
            //No Charge while the projectile is release on Start ??? is this neeeded?
            if (releaseProjectile == Release_Projectile.OnAttackStart) return;

            if (!MaxCharged) base.Charge(time);
            CalculateVelocity();
            Predict?.Invoke(true);
        }


        /// <summary> Create an arrow ready to shooot CALLED BY THE ANIMATOR </summary>
        public virtual void EquipProjectile()
        {
            if (!HasAmmo) return;                                           //means there's no Ammo so no equipping!

            if (ProjectileInstance == null) //Means there's no projectile equipped!
            {
                var Pos = ProjectileParent ? ProjectileParent.position : AimOriginPos;
                var Rot = ProjectileParent ? ProjectileParent.rotation : AimOrigin.rotation;

                //If the Projectile is a prefab
                if (Projectile.IsPrefab())
                {
                    //Instantiate the Arrow in the Parent Object of the Shooteable Weapon
                    // ProjectileInstance = Instantiate(Projectile, Pos, Rot, ProjectileParent);
                    //Debugging($"◘ [Projectile Instantiated] [{ProjectileInstance.name}] ", ProjectileInstance);

                    ///<summary>  Pool - Get Projectile from Pool and reset its position and rotation </summary>
                    ProjectileInstance = ProjectilePool.Get();

                    ProjectileInstance.transform.SetPositionAndRotation(Pos, Rot);
                    ProjectileInstance.transform.parent = ProjectileParent;
                    ///<summary>  Pool - End </summary>
                }
                else
                {
                    //ProjectileInstance = Projectile;

                    ///<summary>  Pool - Get Projectile from Pool </summary>
                    ProjectileInstance = ProjectilePool.Get();
                    ///<summary>  Pool - End  </summary>
                }

                if (ProjectileInstance.TryGetComponent<MProjectile>(out var projectile))
                {
                    MProjectile = projectile; //Safe in a variable

                    ProjectileInstance.transform.Translate(MProjectile.PosOffset, Space.Self);   //Translate in the offset of the arrow to put it on the hand
                    ProjectileInstance.transform.Rotate(MProjectile.RotOffset, Space.Self);      //Rotate in the offset of the arrow to put it on the hand

                    //ProjectileInstance.transform.localScale = (projectile.ScaleOffset);       //Scale in the offset of the arrow to put it on the hand

                    MProjectile.transform.localScale = Vector3.one; //Reset the Scale of the projetile


                    //Use Weapon Effects on the projectiles
                    if (MProjectile.hitEffects == null || MProjectile.hitEffects.Count == 0)
                    { MProjectile.hitEffects = hitEffects; }
                    if (MProjectile.HitEffect == null)
                    { MProjectile.HitEffect = HitEffect; }
                    if (MProjectile.hitSound == null || MProjectile.hitSound.Value == null)
                    { MProjectile.hitSound = hitSound; }
                }

                if (ProjectileInstance.TryGetComponent<Rigidbody>(out var projectile_RB))
                {
                    projectile_RB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    projectile_RB.isKinematic = true;
                }


                //Disable projectile collider
                if (ProjectileInstance.TryGetComponent<Collider>(out var projectile_Col))
                {
                    projectile_Col.enabled = false;
                }



                OnLoadProjectile.Invoke(ProjectileInstance);

                // ProjectIsReleased = false;

                Debugging($"◘ [Projectile Equiped] [{ProjectileInstance.name}] ", ProjectileInstance);
            }
            else
            {
                Debugging($"◘ [Projectile Already Equipped] Skip", ProjectileInstance, "gray");
            }
        }

        /// <summary> Set a new projectile</summary>
        /// <param name="projectile"></param>
        public virtual void SetProjectile(GameObject projectile) => Projectile = projectile;

        public virtual void Fire()
        {
            ReleaseProjectile();
        }

        public virtual void ReleaseProjectile()
        {
            if (!gameObject.activeInHierarchy) return; //Crazy bug ??

            Predict?.Invoke(false); //Hide the Prediction Trajectory

            //CanAttack = false; //Wait the Rate to attack again
            //Debug.Log("Release Projectile!!!!!!!!");

            if (releaseProjectile == Release_Projectile.Never)
            {
                DestroyProjectileInstance();
                return;
            }

            if ((equipProjectile & Equip_Projectile.OnProjectileReleased) == Equip_Projectile.OnProjectileReleased)
                EquipProjectile();


            FireProjectile();


            if (HasReload)
            {
                //Reduce the Ammo the next frame
                this.Delay_Action(() => ReduceAmmo(1));
            }
        }

        public virtual void FireProjectile()
        {
            if (ProjectileInstance == null) return;

            ProjectileInstance.transform.parent = null; //Unparent the projectile


            if (MProjectile != null)
            {
                ProjectileInstance.transform.position = AimOrigin.position;                 //Put the Correct position to Throw the projectile IMPORTANT!!!!!

                CalculateVelocity();
                //Calculate the Spread Angle after calculate velocity
                if (spreadAngle.Value > 0)
                {
                    Quaternion spreadRotation =
                    Quaternion.Euler(
                    Random.Range(-spreadAngle * spreadMult.x, spreadAngle * spreadMult.x),
                    Random.Range(-spreadAngle * spreadMult.y, spreadAngle * spreadMult.y),
                    Random.Range(-spreadAngle * spreadMult.z, spreadAngle * spreadMult.z));
                    Velocity = spreadRotation * Velocity;
                }

                //MDebug.DrawRay(AimOrigin.position, Velocity * 100, Color.green, 0.5f);


                ProjectileInstance.transform.forward = Velocity.normalized;                 //Align the Projectile to the velocity

                ProjectileInstance.transform.Translate(MProjectile.PosOffset, Space.Self); //Translate in the offset of the arrow to put it on the hand

                /// <summary> Pool - Added ProjectilePool to Prepare  </summary> 
                MProjectile.Prepare(Owner, Gravity, Velocity, Layer, TriggerInteraction, ProjectilePool);
                /// <summary> Pool - End </summary> 

                MProjectile.AfterDistance = AfterDistance; //Set the After Distance in the Projectile

                if (HitEffect != null) MProjectile.HitEffect = HitEffect;                  //Send the Hit Effect too

                var newDamage = new StatModifier(statModifier)
                { Value = Mathf.Lerp(MinDamage, MaxDamage, ChargedNormalized) }; //Change the Damage of the Projectile

                MProjectile.PrepareDamage(newDamage, CriticalChance, CriticalMultiplier, element);

                Debugging($"◘ [Projectile Released] [{ProjectileInstance.name}]", ProjectileInstance);
                MProjectile.Fire();
            }

            OnFireProjectile.Invoke(ProjectileInstance);
            ProjectileInstance = null;
            MProjectile = null;

            // WeaponReady(false); //Tell the weapon cannot be Ready until Somebody set it ready again

            PlaySound(WSound.Fire); //Play the Release Projectile Sound

            ResetCharge(); //IMPORTANT!!! DO NOT ELIMINATE
        }

        private void CalculateVelocity()
        {
            var Direction = (CurrentOwner.Aimer.AimPoint - AimOrigin.position).normalized;

            if (UseAimAngle.Value)
            {
                var RightV = Vector3.Cross(Direction, -Gravity);
                Velocity = Quaternion.AngleAxis(AimAngle, RightV) * Direction * Power;
            }
            else
                Velocity = Direction * Power;

            // Debug.Log($"POWER {Power} ChargedNormalized {ChargedNormalized} ChargeCurrentTime{ChargeCurrentTime}");
        }


        /// <summary> Destroy the Active Arrow , used when is Stored the Weapon again and it had an arrow ready</summary>
        public virtual void DestroyProjectileInstance()
        {
            //Destroy the Projectile instance if the Projectile is not the weapon itself
            if (ProjectileInstance != null && ProjectileInstance != gameObject)
            {
                if (ProjectileInitialPoolSize > 0)
                {
                    ProjectilePool.Release(ProjectileInstance);
                    Debugging("[Return Projectile to Pool]", this);
                }
                else
                {
                    Destroy(ProjectileInstance);
                    Debugging("[Destroy Projectile Instance]", this);
                }


            }
            ProjectileInstance = null; //Clean the Projectile Instance
            MProjectile = null; //Clean Projectile interface
        }

        /// <summary> This is where I call the Animations for the Reload.. not the Actual Reloading of the weapon</summary>
        public override bool TryReload()
        {
            if (!HasReload) return false; //Do nothing if it does not have any reload logic
            if (TotalAmmo == 0) return false;                //Means the Weapon Cannot Reload
            if (ChamberSize == AmmoInChamber) return false;  //Means there's no need to Reload.. the Chamber is full!!

            // Debug.Log("Try reloaddddd 2e2");

            if (HasReloadAnim.Value)
            {
                //Check First if a reload can be made??
                if (CanReload())
                {
                    PlaySound(WSound.Reload);
                    WeaponAction.Invoke((int)Weapon_Action.Reload);

                    IsReloading = true; //Do Reload Animations
                    OnReloadStart.Invoke();
                    this.Delay_Action(m_ReloadTime.Value, () => ReloadWeapon()); //Do the actual reloading of the weapon

                    IsAttacking = false; //No Longer Attaking

                    return true;
                }
                else
                {
                    //Do Fail Reload

                    WeaponAction.Invoke((int)Weapon_Action.Idle);
                    PlaySound(WSound.Empty);
                    ReloadWeapon(); //HACK MAYBE?
                    return false;
                }
            }
            else
            {
                //IsReloading = false;


                //Auto Aim after the reload
                if (aimAction == AimingAction.Automatic)
                    WeaponAction.Invoke((int)Weapon_Action.Aim);
                else
                    WeaponAction.Invoke((int)Weapon_Action.Idle);

                return ReloadWeapon();
            }
        }

        /// <summary> Check if the Reload Animation can be done</summary>
        public bool CanReload()
        {
            //Means the Weapon Cannot Reload, there's no more ammo

            // Debug.Log($"CAN RELOAD {TotalAmmo}, ");

            if (TotalAmmo == 0)
            {
                Debugging($"X Cannot Reload. Total Ammo == 0", this);
                return false;
            }

            //Means there's no need to Reload.. the Chamber is full!!
            if (ChamberSize == AmmoInChamber)
            {
                Debugging($"X Cannot Reload. Chamber is Full. No need to Reload", this);
                return false;
            }
            if (TotalAmmo == -1)
            {
                Debugging($"Can Reload Infinite ammo.", this);
                return true;
            }


            //Ammo Needed to refill the Chamber
            int ReloadAmount = Mathf.Clamp(ChamberSize - AmmoInChamber, 0, TotalAmmo);

            //Ammo Remaining
            int AmmoLeft = TotalAmmo - ReloadAmount;

            if (AmmoLeft >= 0)
            {
                Debugging($"Can Reload", this);
                return true; //Meaning it can Reload something
            }

            Debugging($"X Cannot Reload. AmmoLeft = {AmmoLeft}", this);

            return false;
        }


        /// <summary> This can be called also by the ANIMATOR </summary>
        public bool ReloadWeapon()
        {
            if (HasReload)
            {
                //Ammo Needed to refill the Chamber
                int RefillChamber = ChamberSize - AmmoInChamber;

                if (ReloadLogic(RefillChamber))
                {
                    FinishReload();
                    return true;
                }

            }
            return false;
        }

        public bool ReloadLogic(int ReloadAmount)
        {
            if (TotalAmmo == -1) //Means that you will have Infinity Ammo
            {
                AmmoInChamber = ChamberSize;
                OnReload.Invoke();
                return true;
            }


            if ((TotalAmmo == 0) ||                                  //Means the Weapon Cannot Reload, there's no more ammo
                (ChamberSize == AmmoInChamber))                      //Means there's no need to Reload.. the Chamber is full!!
            {
                Debugging($"[Cannot Reload no more ammo left]", this);
                return false;
            }

            ReloadAmount = Mathf.Clamp(ReloadAmount, 0, ChamberSize - AmmoInChamber);

            int AmmoLeft = TotalAmmo - ReloadAmount;                           //Ammo Remaining


            if (AmmoLeft >= 0)                                                  //If is there any Ammo 
            {
                AmmoInChamber += ReloadAmount;
                TotalAmmo -= ReloadAmount;
            }
            else
            {
                AmmoInChamber += TotalAmmo;                                     //Set in the Chamber the remaining ammo  
                TotalAmmo = 0;                                                  //Empty the Total Ammo
            }

            ////Hack to use the AmmoInChamber when there is only one bullet left in the total ammo
            //if (ChamberSize <= 1 && TotalAmmo == 0)
            //{
            //    Debug.Log("---Hack to use the AmmoInChamber -- WORKING! ");
            //    AmmoInChamber = 0;
            //    return false;
            //}
            Debugging($"[Reloading Ammo!: <B>[{ReloadAmount}]]</B>", this);


            OnReload.Invoke();
            return true;
        }

        /// <summary> If finish reload but is still aiming go to the Aiming animation **CALLED BY THE ANIMATOR**</summary>
        public virtual void FinishReload()
        {
            if (!IsEquiped) return; //Make sure it cannot be reload when its not equipped.
            if (CurrentOwner.DrawWeapon || CurrentOwner.StoreWeapon) return; //Do nothing also if the weapon is being stored

            IsReloading = false;


            if (aimAction == AimingAction.Automatic && CurrentOwner.Aim)
            {
                WeaponAction?.Invoke((int)Weapon_Action.Aim);
            }
            else
            {
                CheckAim();
            }

            //Equip projectile after reload
            if ((equipProjectile & Equip_Projectile.AfterReload) == Equip_Projectile.AfterReload)
                EquipProjectile();


            Debugging("[Finish Reload]", this);
        }
    }


    #region INSPECTOR

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MShootable))]
    public class MShootableEditor : MWeaponEditor
    {
        SerializedProperty m_AmmoInChamber, m_Ammo, m_ChamberSize,
            releaseProjectile, releaseDelay, releaseByAnimation,
            equipProjectile,
            /// <summary>
            /// Pool - Added InitialPoolSize and MaxPoolSize
            /// </summary> 
            m_Projectile, m_ProjectileInitialPoolSize, m_ProjectileMaxPoolSize, AimLimit,
            /// <summary>
            /// Pool - End
            /// </summary> 
            m_AutoReload, m_AutoReloadTime, m_ReloadTime, NoReload, HasReloadAnim, m_UnequipOnAim, m_AfterDistance,
            //  InstantiateProjectileOfFire, 
            ProjectileParent, HasFireAnim, aimAction, spreadAngle, spreadMult,//amount,
                                                                              //  AimID, FireID, ReloadID,
            OnReload, OnReloadStart, OnLoadProjectile, OnFireProjectile, gravity, UseAimAngle, m_AimAngle;

        protected MShootable mShoot;

        private void OnEnable()
        {
            SetOnEnable();
            mShoot = (MShootable)target;
        }


        protected override void SetOnEnable()
        {
            WeaponTab = "Shootable";
            base.SetOnEnable();
            AimLimit = serializedObject.FindProperty("AimLimit");
            UseAimAngle = serializedObject.FindProperty("UseAimAngle");
            m_AimAngle = serializedObject.FindProperty("m_AimAngle");
            //CancelAim = serializedObject.FindProperty("CancelAim");
            HasFireAnim = serializedObject.FindProperty("HasFireAnim");
            aimAction = serializedObject.FindProperty("aimAction");

            spreadAngle = serializedObject.FindProperty("spreadAngle");
            spreadMult = serializedObject.FindProperty("spreadMult");
            //amount = serializedObject.FindProperty("amount");


            m_ReloadTime = serializedObject.FindProperty("m_ReloadTime");
            m_AutoReload = serializedObject.FindProperty("m_AutoReload");
            m_AutoReloadTime = serializedObject.FindProperty("m_AutoReloadTime");
            NoReload = serializedObject.FindProperty("noReload");
            HasReloadAnim = serializedObject.FindProperty("HasReloadAnim");
            //InstantiateProjectileOfFire = serializedObject.FindProperty("InstantiateProjectileOfFire");

            releaseProjectile = serializedObject.FindProperty("releaseProjectile");
            releaseDelay = serializedObject.FindProperty("releaseDelay");
            releaseByAnimation = serializedObject.FindProperty("releaseByAnimation");



            equipProjectile = serializedObject.FindProperty("equipProjectile");
            m_Projectile = serializedObject.FindProperty("m_Projectile");
            /// <summary>
            /// Pool - Define InitialPoolSize and MaxPoolSize
            /// </summary> 
            m_ProjectileInitialPoolSize = serializedObject.FindProperty("m_ProjectileInitialPoolSize");
            m_ProjectileMaxPoolSize = serializedObject.FindProperty("m_ProjectileMaxPoolSize");
            /// <summary>
            /// Pool - End
            /// </summary> 
            ProjectileParent = serializedObject.FindProperty("m_ProjectileParent");
            m_AfterDistance = serializedObject.FindProperty("m_AfterDistance");


            m_AmmoInChamber = serializedObject.FindProperty("m_AmmoInChamber");
            m_Ammo = serializedObject.FindProperty("m_Ammo");
            m_ChamberSize = serializedObject.FindProperty("m_ChamberSize");

            OnReload = serializedObject.FindProperty("OnReload");
            OnReloadStart = serializedObject.FindProperty("OnReloadStart");
            OnLoadProjectile = serializedObject.FindProperty("OnLoadProjectile");
            OnFireProjectile = serializedObject.FindProperty("OnFireProjectile");
            gravity = serializedObject.FindProperty("gravity");
            m_UnequipOnAim = serializedObject.FindProperty("m_UnequipOnAim");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Projectile Weapons Properties");
            WeaponInspector(false);
            serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Shoot   3:Reload   4:Empty  5:Charge";
        }

        protected override string CustomEventsHelp()
        {
            return "\n\n On Fire Gun: Invoked when the weapon is fired \n(Vector3: the Aim direction of the rider), \n\n On Hit: Invoked when the Weapon Fired and hit something \n(Transform: the gameobject that was hitted) \n\n On Aiming: Invoked when the Rider is Aiming or not \n\n On Reload: Invoked when Reload";
        }

        protected override void DrawExtras()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                minForce.isExpanded = MalbersEditor.Foldout(minForce.isExpanded, "Projectile Speed and Physics");

                if (minForce.isExpanded)
                {
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min", "Minimun Force Speed of the Projectile and force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max", "Maximun Force Speed of the Projectile and force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(forceMode);
                    EditorGUILayout.PropertyField(gravity);
                    EditorGUILayout.PropertyField(m_AfterDistance);
                }
            }
            DrawMisc();

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                EditorGUILayout.PropertyField(description);
        }

        protected override void DrawAdvancedWeapon()
        {
            if (Application.isPlaying)
                using (new EditorGUI.DisabledGroupScope(true))
                    EditorGUILayout.ObjectField("Projectile Instance", mShoot.ProjectileInstance, typeof(GameObject), false);

            DrawDamage();

            var dc = GUI.backgroundColor;
            if (releaseProjectile.intValue == (int)MShootable.Release_Projectile.OnAttackStart)
            {
                GUI.backgroundColor = new Color(0.2f * 2, 0.5f * 2, 1f * 2, 1f);
                EditorGUILayout.HelpBox("Charging will be ignored when [Release Projectile = On Attack Start]", MessageType.Warning);
            }
            else if (mShoot.chargeTime.Value <= 0)
            {
                GUI.backgroundColor = new Color(0.2f * 2, 0.5f * 2, 1f * 2, 1f);
                EditorGUILayout.HelpBox("Charging will be ignored when [Charge Time = 0]", MessageType.Warning);
            }
            GUI.backgroundColor = dc;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                m_AimOrigin.isExpanded = MalbersEditor.Foldout(m_AimOrigin.isExpanded, "Aiming");

                if (m_AimOrigin.isExpanded)
                {
                    EditorGUILayout.PropertyField(m_AimOrigin);
                    EditorGUILayout.PropertyField(aimAction);

                    if (mShoot.aimAction != MShootable.AimingAction.Ignore)
                    {
                        EditorGUILayout.PropertyField(m_AimSide);
                    }
                    EditorGUILayout.PropertyField(AimLimit);

                    EditorGUILayout.PropertyField(UseAimAngle, new GUIContent("Use Aim Angle", " Adds a Throw Angle to the Aimer Direction?"));
                    if (mShoot.UseAimAngle.Value)
                    {
                        EditorGUILayout.PropertyField(m_AimAngle, new GUIContent("Aim Angle", " Adds a Throw Angle to the Aimer Direction"));
                    }
                    EditorGUILayout.PropertyField(m_UnequipOnAim);
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                releaseProjectile.isExpanded = MalbersEditor.Foldout(releaseProjectile.isExpanded, "Projectile");

                if (releaseProjectile.isExpanded)
                {
                    EditorGUILayout.PropertyField(spreadAngle);
                    EditorGUILayout.PropertyField(spreadMult);
                    // EditorGUILayout.PropertyField(amount);


                    EditorGUILayout.PropertyField(HasFireAnim);
                    EditorGUILayout.PropertyField(equipProjectile);
                    EditorGUILayout.PropertyField(releaseProjectile);


                    EditorGUILayout.PropertyField(releaseByAnimation);

                    using (new EditorGUI.DisabledGroupScope(mShoot.ReleaseByAnimation))
                        EditorGUILayout.PropertyField(releaseDelay);



                    if (releaseProjectile.intValue != 0)
                    {
                        //EditorGUILayout.PropertyField(InstantiateProjectileOfFire,
                        //    new GUIContent("Inst Projectile on Fire", "Instanciate the Projectile when Firing the weapon." +
                        //    "\n E.g The Pistol Instantiate the projectile on Firing. The bow Instantiate the Arrow Before Firing"));
                        EditorGUILayout.PropertyField(m_Projectile);
                        /// <summary>
                        /// Pool - Define GUI for InitialPoolSize and MaxPoolSize
                        /// </summary> 
                        EditorGUILayout.PropertyField(m_ProjectileInitialPoolSize);
                        EditorGUILayout.PropertyField(m_ProjectileMaxPoolSize);
                        /// <summary>
                        /// Pool - End
                        /// </summary> 
                        EditorGUILayout.PropertyField(ProjectileParent);
                    }
                }

                minForce.isExpanded = MalbersEditor.Foldout(minForce.isExpanded, "Projectile Speed and Physics");

                if (minForce.isExpanded)
                {
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min", "Minimun Force Speed of the Projectile and force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max", "Maximun Force Speed of the Projectile and force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(forceMode);
                    EditorGUILayout.PropertyField(gravity);
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                HasReloadAnim.isExpanded = MalbersEditor.Foldout(HasReloadAnim.isExpanded, "Reload & Ammunition");

                if (HasReloadAnim.isExpanded)
                {
                    EditorGUILayout.PropertyField(NoReload);
                    if (!mShoot.noReload.Value)
                    {
                        EditorGUILayout.PropertyField(HasReloadAnim);
                        EditorGUILayout.PropertyField(m_ReloadTime);
                        EditorGUILayout.PropertyField(m_AutoReload);

                        if (mShoot.AutoReload)
                            EditorGUILayout.PropertyField(m_AutoReloadTime);

                        EditorGUILayout.PropertyField(m_ChamberSize, new GUIContent("Chamber Size", "Total of Ammo that can be shoot before reloading"));

                        if (mShoot.ChamberSize > 1)
                        {
                            EditorGUILayout.PropertyField(m_AmmoInChamber, new GUIContent("Ammo in Chamber", "Current ammo in the chamber"));
                        }
                        EditorGUILayout.PropertyField(m_Ammo, new GUIContent("Total Ammo", "Total ammo for the weapon. Set it to -1 to have infinity ammo"));

                        EditorGUILayout.HelpBox("<Total Ammo = -1> means infinite Ammo", MessageType.Info);
                    }
                }
            }
        }


        protected override void ChildWeaponEvents()
        {
            EditorGUILayout.PropertyField(OnLoadProjectile);
            EditorGUILayout.PropertyField(OnFireProjectile);
            //EditorGUILayout.PropertyField(OnAiming);
            EditorGUILayout.PropertyField(OnReloadStart);
            EditorGUILayout.PropertyField(OnReload);
        }
    }
#endif
    #endregion
}