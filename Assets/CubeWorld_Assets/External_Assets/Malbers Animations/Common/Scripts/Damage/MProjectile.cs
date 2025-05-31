using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Controller;
using UnityEngine.Pool;
using MalbersAnimations.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    public enum ProjectileRotation { None, FollowTrajectory, Random, Axis };

    /// <summary>  Pool - Added DisableOnImpact for ProjectilePool </summary>  
    public enum ImpactBehaviour { None, StickOnSurface, DestroyOnImpact, ActivateRigidBody, DisableOnImpact };
    /// <summary> Pool - End </summary>  


    [AddComponentMenu("Malbers/Damage/Projectile")]
    [SelectionBase]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/mdamager/mprojectile")]
    public class MProjectile : MDamager, IProjectile
    {
        /// <summary>  Pool - Added Tooltip to explain impactBehvioue new Option  </summary>  
        [Tooltip("Setting to Destroy On Impact \nWill not use ProjectilePool!")]
        /// <summary>  Pool - End </summary>  
        /// 
        public ImpactBehaviour impactBehaviour = ImpactBehaviour.None;
        public ProjectileRotation rotation = ProjectileRotation.None;

        [Tooltip("Rotation ammount around trajectory axis when the projectile is set to Follow Trajectory")]
        public float TrajectoryRoll = 0;

        public float Penetration = 0.1f;

        [SerializeField, Tooltip("Keep Projectile Damage and Layer Values, The Throwable-Shootable compoennt will not override the Damage Values")]
        protected BoolReference m_KeepDamageValues = new(false);

        [SerializeField, Tooltip("Gravity applied to the projectile, if gravity is zero the projectile will go straight. If the Projectile is thrown by a Projectile Thrower." +
            "It will inherit the gravity from it")]
        protected Vector3Reference gravity = new(Physics.gravity);

        [SerializeField, Tooltip("Apply Gravity after certain distance is reached")]
        private FloatReference m_AfterDistance = new(0f);
        public float AfterDistance { get => m_AfterDistance.Value; set => m_AfterDistance.Value = value; }

        [Tooltip("Life of the Projectile on the air, if it has not touch anything on this time it will destroy it self")]
        public FloatReference Life = new(10f);
        [Tooltip("Life of the Projectile After Impact. If the projectile is not destroyed on impact, then wait this time to do it. (0 -> Ignores it) ")]
        public FloatReference LifeImpact = new(0f);

        [Tooltip("Multiplier of the Force to Apply to the object the projectile impact ")]
        public FloatReference PushMultiplier = new(1);

        [Tooltip("Torque for the rotation of the projectile")]
        public FloatReference torque = new(50f);
        [Tooltip("Axis Torque for the rotation of the projectile")]
        public Vector3 torqueAxis = Vector3.up;

        [Tooltip("Offset to position the projectile when is Instantiated on the weapon. E.g. (Arrow in the Bow) ")]
        public Vector3 m_PosOffset;

        [Tooltip("Offset to rotation the projectile when is Instantiated on the weapon. E.g. (Arrow in the Bow) ")]
        public Vector3 m_RotOffset;

        [Tooltip("Offset to scale the projectile when is Instantiated on the weapon. E.g. (Arrow in the Bow) ")]
        public Vector3 m_ScaleOffset;

        [Tooltip("Use Spherecast to predict the trajectory")]
        public bool useRadius = false;
        [Tooltip("Radius of the projectile to cast a ray to find targets better")]
        public FloatReference Radius = new(0.01f);

        public UnityEvent OnFire = new();                       //Send the transform to the event

        [Tooltip("Reference for the Projectile Rigidbody")]
        public Rigidbody rb;
        [Tooltip("Reference for the Projectile collider")]
        public Collider m_collider;

        [Tooltip("Reference for the trail renderer")]
        public TrailRenderer m_trail;

        public float DragOnImpact = 1;

        protected Vector3 Prev_pos;

        #region Properties
        /// <summary>Initial Velocity (Direction * Power) </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>Has the projectile impacted with something</summary>
        public bool HasImpacted { get; set; }

        /// <summary>Do Fly Raycast</summary>
        protected bool doRayCast;

        /// <summary>Is the Projectile Flying</summary>
        public bool IsFlying { get; set; }


        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }
        public bool KeepValues { get => m_KeepDamageValues.Value; set => m_KeepDamageValues.Value = value; }
        public Vector3 TargetHitPosition { get; set; }
        public bool FollowTrajectory => rotation == ProjectileRotation.FollowTrajectory;
        public bool DestroyOnImpact => impactBehaviour == ImpactBehaviour.DestroyOnImpact;
        public bool StickOnSurface => impactBehaviour == ImpactBehaviour.StickOnSurface;

        public Vector3 PosOffset { get => m_PosOffset; set => m_PosOffset = value; }
        public Vector3 RotOffset { get => m_RotOffset; set => m_RotOffset = value; }
        // public Vector3 ScaleOffset { get => m_ScaleOffset; set => m_ScaleOffset = value; }
        #endregion


        public RayCastHitEvent OnRayCastHit = new();

        public IObjectPool<GameObject> Pool { get; set; }
        /// <summary> Pool - End  </summary>  


        [HideInInspector] public int Editor_Tabs1;


        protected virtual void Awake()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            if (!m_collider) m_collider = GetComponentInChildren<Collider>();

            m_audio = GetComponent<AudioSource>(); //Gets the Weapon Source

            if (!m_audio) m_audio = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            m_audio.spatialBlend = 1;
            m_audio.maxDistance = 50;

            //Create a Link to Stick on Surface 
            if (impactBehaviour == ImpactBehaviour.StickOnSurface)
            {
                LinkStickOnSurface = new GameObject($"Link [{name}]");
                LinkStickOnSurface.transform.SetParent(transform);
                LinkStickOnSurface.transform.ResetLocal();
            }
        }


        protected virtual void OnDisable()
        {
            StopAllCoroutines();
            CancelInvoke();
        }

        /// <summary> Initialize the Projectile main references and parameters</summary>
        protected virtual void Initialize()
        {
            HasImpacted = false;
            if (Life > 0) Invoke(nameof(DestroyProjectile), Life); //Destroy Projectile after a time
        }


        /// <summary> Prepare the Projectile for firing </summary>
        public virtual void Prepare(GameObject Owner, Vector3 Gravity,
            Vector3 ProjectileVelocity, LayerMask HitLayer, QueryTriggerInteraction triggerInteraction, IObjectPool<GameObject> thisProjectilePool)
        {
            if (!KeepValues)
            {
                this.Layer = HitLayer;
                this.TriggerInteraction = triggerInteraction;
            }

            this.Owner = Owner;
            this.Gravity = Gravity;
            this.Velocity = ProjectileVelocity;
            this.MaxForce = Velocity.magnitude;
            this.MinForce = Velocity.magnitude;
            /// <summary> Pool - Set thisProjectilePool  </summary>  
            this.Pool = thisProjectilePool;
            /// <summary> Pool - End </summary>  
            Debugging("Projectile Prepared", this);
        }

        public virtual void Fire(Vector3 ProjectileVelocity)
        {
            this.Velocity = ProjectileVelocity;
            this.MaxForce = Velocity.magnitude;
            this.MinForce = Velocity.magnitude;
            Fire();
        }

        public virtual void Fire()
        {
            Initialize();

            gameObject.SetActive(true); //Just to make sure is working  
            Enabled = true;

            if (Velocity == Vector3.zero) //Hack when the Velocity is not set
            {
                Velocity = transform.forward;
                this.MaxForce = 1;
                this.MinForce = 1;
            }

            doRayCast = true;

            if (m_collider && rb)
            {
                EnableCollider(0.1f); //Don't enable it right away so it does not collide with the thrower
                doRayCast = m_collider.isTrigger;
            }

            if (rb)
            {

                EnableRigidBody();
                rb.linearVelocity = Vector3.zero; //Reset the velocity IMPORTANT!

                if (rotation == ProjectileRotation.Random)
                {
                    rb.AddTorque(new Vector3(Random.value, Random.value, Random.value).normalized * torque, ForceMode.Acceleration);
                }
                else if (rotation == ProjectileRotation.Axis)
                {
                    rb.AddTorque(torqueAxis * torque.Value, ForceMode.Impulse);
                }
                //  Debug.Log("RIGID BODY Gravity");
                rb.AddForce(Velocity, ForceMode.VelocityChange);
            }

            StartCoroutine(FlyingProjectile()); //Trajectory movement is done here.


            OnFire.Invoke();

            //if (TryGetComponent<ICollectable>(out var pickable))
            //{
            //    pickable.Drop(); //if the Projectile is a pickable then drop it?
            //}

            Debugging("Projectile Fired", this);
        }

        public void EnableCollider(float time) => Invoke(nameof(Enable_Collider), time);

        protected virtual void Enable_Collider()
        {
            if (m_collider) m_collider.enabled = true;
        }

        /// <summary> / Destroy Projectile on Life Time End  </summary>
        protected virtual void DestroyProjectile()
        {
            //if (!HasImpacted)
            {
                HasImpacted = false;
                if (impactBehaviour == ImpactBehaviour.DestroyOnImpact)
                {
                    Debugging($"Life time elapsed [{Life}]. Destroy Projectile", null);
                    Destroy(gameObject);
                    if (LinkStickOnSurface) Destroy(LinkStickOnSurface);
                }
                /// <summary> Pool - Leave option to destroy and for eveything else use Pool  </summary>  
                else
                {
                    Debugging($"Life time elapsed [{LifeImpact.Value}].[Projectile returned to the Pool]", null);

                    //If the Projectile is sticking on the surface then remove the parent
                    if (LinkStickOnSurface)
                    {
                        transform.parent = null;
                        LinkStickOnSurface.transform.SetParent(transform);
                    }

                    if (m_trail != null) m_trail.Clear();
                    Pool?.Release(gameObject);
                }
                /// <summary>  Pool - End </summary>  
            }
        }

        public virtual void Pool_Release(GameObject gameObject) => Pool.Release(gameObject);

        public virtual GameObject Pool_Get() => Pool.Get();


        protected virtual void OnCollisionEnter(UnityEngine.Collision other)
        {
            if (rb && rb.isKinematic) return;
            if (HasImpacted) return; //Do not check new Collisions
            if (IsInvalid(other.collider)) return;
            if (!enabled) return;

            if (Prev_pos == Vector3.zero) Prev_pos = transform.position;

            ProjectileImpact(other.rigidbody, other.collider, other.contacts[0].point, (other.collider.bounds.center - m_collider.transform.position).normalized);

        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (HasImpacted) return; //Do not check new Collisions
            if (IsInvalid(other)) return;
            if (!enabled) return;

            if (Prev_pos == Vector3.zero) Prev_pos = transform.position;

            ProjectileImpact(other.attachedRigidbody, other, Prev_pos, (other.bounds.center - m_collider.transform.position).normalized);
        }


        /// <summary> Logic Applied when the projectile is flying</summary>
        protected virtual IEnumerator FlyingProjectile()
        {
            Vector3 start = transform.position;
            Prev_pos = start;
            float deltatime = Time.fixedDeltaTime;
            WaitForFixedUpdate waitForFixedUpdate = new();

            Direction = Velocity.normalized; //Start the 

            int step = 1;

            Vector3 RotationAround = Vector3.zero;
            if (rotation == ProjectileRotation.Random)
                RotationAround = new Vector3(Random.value, Random.value, Random.value).normalized;
            else if (rotation == ProjectileRotation.Axis)
                RotationAround = torqueAxis.normalized;

            float TraveledDistance = 0;
            int NoGravityStep = 0;

            while (!HasImpacted && enabled)
            {
                var time = deltatime * step;
                var Gravitytime = deltatime * (step - NoGravityStep);

                Vector3 next_pos = (start + Velocity * time) + (Gravitytime * Gravitytime * Gravity / 2);

                if (!rb)
                {
                    transform.position = Prev_pos; //If there's no Rigid body move the Projectile!!

                    if (rotation == ProjectileRotation.Random || rotation == ProjectileRotation.Axis)
                    {
                        transform.Rotate(RotationAround, torque * deltatime, Space.World);
                    }
                }
                else
                {
                    // rb.velocity = Direction;
                    rb.MovePosition(Prev_pos);
                }

                Direction = (next_pos - Prev_pos);



                Debug.DrawLine(Prev_pos, next_pos, Color.yellow);
                if (Radius > 0)
                {
                    MDebug.DrawWireSphere(Prev_pos, Color.yellow, Radius);
                    MDebug.DrawWireSphere(next_pos, Color.yellow, Radius);
                }

                //RaycastHit hit;

                var Length = Vector3.Distance(next_pos, Prev_pos);
                //if ( Physics.Linecast(Prev_pos, next_pos,  out RaycastHit hit,  Layer, triggerInteraction))
                if (Physics.SphereCast(Prev_pos, Radius, Direction, out RaycastHit hit, Length, Layer, triggerInteraction))
                {
                    if (!IsInvalid(hit.collider))
                    {
                        yield return waitForFixedUpdate;

                        OnRayCastHit.Invoke(hit);

                        ProjectileImpact(hit.rigidbody, hit.collider, hit.point, hit.normal);
                        yield break;
                    }
                }

                if (FollowTrajectory) //The Projectile will rotate towards de Direction
                {
                    transform.rotation = Quaternion.LookRotation(Direction, transform.up);

                    //Rotate around an axis while following a trajectory
                    if (TrajectoryRoll != 0)
                        transform.Rotate(Direction, TrajectoryRoll * deltatime, Space.World);
                }


                //Check if the gravity can be applied after distance
                if (TraveledDistance < AfterDistance)
                {
                    TraveledDistance += Direction.magnitude;
                    NoGravityStep++;
                }



                Prev_pos = next_pos;
                step++;

                yield return waitForFixedUpdate;
            }

            Debug.Log("exit one");
            yield return null;
        }


        public virtual void ProjectileImpact(Rigidbody targetRB, Collider collider, Vector3 HitPosition, Vector3 normal)
        {
            if (!Enabled) return;

            Debugging($"<color=yellow> <b>[Projectile Impact] </b> [{collider.name}] </color>", this);  //Debug

            HasImpacted = true;
            this.HitPosition = HitPosition; //Store the Hit position of the Projectile

            StopAllCoroutines();

            if (MissAttack())
            {
                Debugging("Destroy Projectile Missed", null);
                Destroy(gameObject);
                return;
            }





            //if there's no collider OR the projectile collider is a trigger
            if (!m_collider || m_collider.isTrigger)
            {
                DisableRigidBody();
                if (rb) rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            TryInteract(collider.gameObject);

            damagee = collider.GetComponentInParent<IMDamage>();                      //Get the Animal on the Other collider
            //Store the Last Collider that the animal hit
            if (damagee != null) { damagee.HitCollider = collider; }

            TryDamage(damagee, statModifier);


            // TryPhysics(targetRB, collider, Direction, Force);
            //Add a force to the Target RigidBody
            targetRB?.AddForceAtPosition(PushMultiplier * Velocity.magnitude * Direction.normalized, HitPosition, forceMode);

            OnHit.Invoke(collider.transform);
            OnHitPosition.Invoke(HitPosition);


            var hasAnimator = collider.gameObject.GetComponentInParent<Animator>();
            var RootBone = collider.transform;
            if (hasAnimator != null) { RootBone = hasAnimator.avatarRoot; }

            var ClosestTransform = collider.transform; //If the collider is a MeshCollider then use the same transform (To avoid errors with the ClosestPoint

            if (!collider.isTrigger &&
                collider is not MeshCollider && collider is not TerrainCollider && !collider.gameObject.isStatic && hasAnimator)
            {
                ClosestTransform = MTools.GetClosestTransform(HitPosition, RootBone, Layer);

                //Meaning it found a nearest transform
                if (ClosestTransform != collider.transform)
                {
                    var colTranform = ClosestTransform.GetComponent<Collider>();

                    if (colTranform != null && !colTranform.isTrigger && colTranform is not MeshCollider)
                    {
                        HitPosition = colTranform.ClosestPoint(HitPosition);
                    }
                    else
                    {
                        //find the closes point in the uper bone or the lower bone
                        var MainPos = ClosestTransform.position;

                        //find the parent bone
                        var parentPoint = ClosestTransform.parent != null ? ClosestTransform.parent.position : MainPos;

                        //find the child bone
                        var ChildPoint = ClosestTransform.childCount > 0 ? ClosestTransform.GetChild(0).position : MainPos;

                        var P1 = MTools.ClosestPointOnLine(HitPosition, ChildPoint, MainPos);
                        var P2 = MTools.ClosestPointOnLine(HitPosition, parentPoint, MainPos);

                        var Dist1 = Vector3.Distance(P1, MainPos);
                        var Dist2 = Vector3.Distance(P2, MainPos);

                        HitPosition = Dist1 < Dist2 ? P1 : P2;
                    }
                }
            }

            TryHitEffectProjectile(HitPosition, normal, ClosestTransform);



            switch (impactBehaviour)
            {
                case ImpactBehaviour.None:
                    transform.position = HitPosition;
                    MDebug.DrawWireSphere(HitPosition, Color.red, 0.1f, 2);
                    break;
                case ImpactBehaviour.StickOnSurface:
                    Stick_On_Surface(ClosestTransform, HitPosition);
                    break;
                case ImpactBehaviour.DestroyOnImpact:
                    Debugging("DestroyOnImpact", null);
                    Destroy(gameObject);
                    return;
                case ImpactBehaviour.ActivateRigidBody:
                    EnableRigidBody();
                    Enable_Collider();
                    if (rb) rb.linearDamping = DragOnImpact;
                    Debugging("Activate Rigid Body", null);
                    break;
                /// <summary> Pool - Added DisableOnImpact Option  </summary>  
                case ImpactBehaviour.DisableOnImpact:
                    Pool.Release(this.gameObject);
                    Debugging("DisableOnImpact [Return to the Pool]", null);
                    return;
                /// <summary>  Pool - End  </summary>  
                default:
                    break;
            }

            //In case the projectile lives in the scene it needs to be destroyed after life impact has elapsed (Destroy or sent back to the pool)
            if (LifeImpact > 0)
            {
                //Debug.Log("DO LIFE IMPACT WITH DEST");
                Invoke(nameof(DestroyProjectile), LifeImpact);
            }
            else
                Enabled = false; //Disable the projectile it has already impacted with something
        }

        protected virtual void EnableRigidBody()
        {
            if (rb)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        protected virtual void DisableRigidBody()
        {
            if (rb)
            {
                //For Kinematic!!= CollisionDetectionMode.Discrete;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        public void PrepareDamage(StatModifier modifier, float CriticalChance, float CriticalMultiplier, StatElement element)
        {
            if (!KeepValues)
            {
                statModifier = new StatModifier(modifier);
                this.CriticalChance = CriticalChance;
                this.CriticalMultiplier = CriticalMultiplier;
                this.element = element;
            }
        }
        protected virtual void Stick_On_Surface(Transform collider, Vector3 HitPosition)
        {
            Debugging($"Stick on Surface [{collider.name}]", this);
            MDebug.DrawWireSphere(HitPosition, Color.red, 0.05f);
            transform.position += transform.forward * Penetration; //Put the Projectile a bit deeper in the collider

            LinkStickOnSurface.transform.parent = null; //remove the Parent from the link stick
            transform.SetParentScaleFixer(collider, HitPosition, LinkStickOnSurface);
            DisableRigidBody();
        }

        private GameObject LinkStickOnSurface;

        protected virtual void TryHitEffectProjectile(Vector3 HitPosition, Vector3 Normal, Transform hitTransform)
        {
            var HitEffect = this.HitEffect;
            //  var hitSound = this.hitSound; Debug.Log($"hitSound {hitSound.Value.name}");

            //Find Hit Effects and Sounds
            if (damagee != null && hitEffects != null && hitEffects.Count > 0)
            {
                var eff = hitEffects.Find(x => x.surface == damagee.Surface);

                if (eff != null)
                {
                    if (eff.effect.Value != null) HitEffect = eff.effect.Value;     //Use the Effect from the List
                    if (eff.sound != null) hitSound = eff.sound;                    //use the sound form the list
                }
            }

            if (HitEffect != null)
            {
                var HitRotation = Quaternion.FromToRotation(Vector3.up, Normal);

                if (debug) MDebug.DrawWireSphere(HitPosition, Color.red, 0.05f, 1);

                Debugging($"<color=yellow> <b>[HitEffect] </b> [{HitEffect.name}] , {HitPosition} </color>", this);  //Debug

                if (HitEffect.IsPrefab())
                {
                    var instance = Instantiate(HitEffect, HitPosition, HitRotation);

                    var HasHlp = instance.transform.SetParentScaleFixer(hitTransform, HitPosition); //Fix the Scale issue


                    //Reset the gameobject visibility 
                    CheckHitEffect(instance);

                    if (DestroyHitEffect > 0)
                    {
                        Destroy(instance, DestroyHitEffect);
                        if (HasHlp) Destroy(HasHlp.gameObject, DestroyHitEffect);
                    }
                }
                else
                {
                    HitEffect.transform.SetPositionAndRotation(HitPosition, HitRotation);
                    CheckHitEffect(HitEffect);
                }
                HitEffect.SetActive(true);
            }

            if (m_audio != null)
            {
                /// <summary> Pool - Added audio for DisableOnImpact  </summary> 
                if (impactBehaviour == ImpactBehaviour.DestroyOnImpact || impactBehaviour == ImpactBehaviour.DisableOnImpact)
                /// <summary>  Pool - End  </summary> 
                {
                    if (HitEffect)
                    {
                        if (!HitEffect.TryGetComponent<AudioSource>(out var audio))
                        {
                            audio = HitEffect.AddComponent<AudioSource>();
                            audio.playOnAwake = false;
                        }

                        audio.playOnAwake = false;

                        if (audio.enabled && audio.isActiveAndEnabled && audio.gameObject.activeInHierarchy)
                        {

                            audio.clip = hitSound.Value;
                            audio.spatialBlend = 1;
                            audio.Play();
                        }
                    }
                }
                else
                {
                    PlaySound(hitSound.Value);
                }
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            rb = GetComponent<Rigidbody>();
            m_collider = GetComponentInChildren<Collider>();


            m_audio = GetComponent<AudioSource>(); //Gets the Weapon Source

            if (!m_audio) m_audio = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon

            m_audio.spatialBlend = 1;
            m_audio.maxDistance = 50;
        }

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow + Color.red;
            // Gizmos.DrawSphere(transform.position, Radius);
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
#endif


    }



    /// ----------------------------------------
    /// EDITOR
    /// ----------------------------------------

    #region Inspector


#if UNITY_EDITOR
    [CustomEditor(typeof(MProjectile))]
    public class MProjectileEditor : MDamagerEd
    {
        SerializedProperty gravity, Penetration, DragOnImpact,
            /*InstantiateOnImpact,*/ PushMultiplier, Editor_Tabs1, KeepDamageValues, Radius, m_AfterDistance, OnRayCastHit,
            Life, LifeImpact,
            OnFire, impactBehaviour, rotation, torque, torqueAxis, m_PosOffset, m_RotOffset, rb, m_collider, m_trail, TrajectoryRoll;

        protected string[] Tabs1 = new string[] { "General", "Damage", "Physics", "Events" };
        MProjectile M;

        readonly string[] rotationTooltip = new string[] {
             "No Rotation is applied to the projectile while flying",
             "The projectile will follow its trajectory while flying",
             "The projectile will inherit the rotation it had before it was fired",
             "The projectile will rotate randomly while flying",
             "The projectile will rotate around an axis (world relative)"};

        protected void OnEnable()
        {
            FindBaseProperties();
            M = (MProjectile)target;

            gravity = serializedObject.FindProperty("gravity");

            OnFire = serializedObject.FindProperty("OnFire");
            Radius = serializedObject.FindProperty("Radius");

            Life = serializedObject.FindProperty("Life");
            LifeImpact = serializedObject.FindProperty("LifeImpact");
            impactBehaviour = serializedObject.FindProperty("impactBehaviour");
            rotation = serializedObject.FindProperty("rotation");

            Penetration = serializedObject.FindProperty("Penetration");
            DragOnImpact = serializedObject.FindProperty("DragOnImpact");
            PushMultiplier = serializedObject.FindProperty("PushMultiplier");

            m_PosOffset = serializedObject.FindProperty("m_PosOffset");
            m_RotOffset = serializedObject.FindProperty("m_RotOffset");
            KeepDamageValues = serializedObject.FindProperty("m_KeepDamageValues");
            m_AfterDistance = serializedObject.FindProperty("m_AfterDistance");

            torque = serializedObject.FindProperty("torque");
            TrajectoryRoll = serializedObject.FindProperty("TrajectoryRoll");
            torqueAxis = serializedObject.FindProperty("torqueAxis");
            //  InstantiateOnImpact = serializedObject.FindProperty("InstantiateOnImpact");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            rb = serializedObject.FindProperty("rb");
            m_trail = serializedObject.FindProperty("m_trail");
            m_collider = serializedObject.FindProperty("m_collider");
            OnRayCastHit = serializedObject.FindProperty("OnRayCastHit");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescription("Logic for Projectiles. When is fired by a Thrower component, use the method Prepare() to transfer all the properties from the thrower");

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) DrawGeneral();
            else if (Selection == 1) DrawDamage();
            else if (Selection == 2) DrawExtras();
            else if (Selection == 3) DrawEvents();
            // EditorGUILayout.PropertyField(debug);
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawExtras()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawPhysics(false);
                EditorGUILayout.PropertyField(gravity);
                EditorGUILayout.PropertyField(PushMultiplier);
                EditorGUILayout.PropertyField(m_AfterDistance);
            }

            DrawMisc();
        }

        protected void DrawDamage()
        {
            EditorGUILayout.PropertyField(KeepDamageValues, new GUIContent("Keep Values"));
            if (!M.KeepValues)
            {
                EditorGUILayout.HelpBox("If the Projectile is thrown by a Throwable, the Stat will be set by the Throwable. [E.g. The Arrow will get the Damage from the bow]", MessageType.Info);
            }
            else
            {
                DrawStatModifier();
                DrawCriticalDamage();
            }

            DrawMisc();
        }

        protected override void DrawGeneral(bool drawbox = true)
        {
            base.DrawGeneral(drawbox);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Life.isExpanded = MalbersEditor.Foldout(Life.isExpanded, "Projectile Life");

                if (Life.isExpanded)
                {
                    EditorGUILayout.PropertyField(Life);
                    EditorGUILayout.PropertyField(LifeImpact);
                    EditorGUILayout.PropertyField(Radius);
                }

                m_PosOffset.isExpanded = MalbersEditor.Foldout(m_PosOffset.isExpanded, "Offsets");

                if (m_PosOffset.isExpanded)
                {
                    EditorGUILayout.PropertyField(m_PosOffset, new GUIContent("Position"));
                    EditorGUILayout.PropertyField(m_RotOffset, new GUIContent("Rotation"));
                }

                rotation.isExpanded = MalbersEditor.Foldout(rotation.isExpanded, "Rotation Behaviour");

                if (rotation.isExpanded)
                {
                    EditorGUILayout.PropertyField(rotation, new GUIContent("Rotation", rotationTooltip[rotation.intValue]));

                    var rot = (ProjectileRotation)rotation.intValue;

                    switch (rot)
                    {
                        case ProjectileRotation.None:
                            break;
                        case ProjectileRotation.FollowTrajectory:
                            EditorGUILayout.PropertyField(TrajectoryRoll);
                            break;
                        case ProjectileRotation.Random:
                            EditorGUILayout.PropertyField(torque);
                            break;
                        case ProjectileRotation.Axis:
                            EditorGUILayout.PropertyField(torque);
                            EditorGUILayout.PropertyField(torqueAxis);
                            break;
                        default:
                            break;
                    }
                }

                impactBehaviour.isExpanded = MalbersEditor.Foldout(impactBehaviour.isExpanded, "On Impact");

                if (impactBehaviour.isExpanded)
                {
                    EditorGUILayout.PropertyField(impactBehaviour);
                    if (impactBehaviour.intValue == 1)
                        EditorGUILayout.PropertyField(Penetration);
                    if (impactBehaviour.intValue == 3)
                        EditorGUILayout.PropertyField(DragOnImpact);
                }

                rb.isExpanded = MalbersEditor.Foldout(rb.isExpanded, "References");

                if (rb.isExpanded)
                {
                    EditorGUILayout.PropertyField(rb, new GUIContent("Rigid Body"));
                    EditorGUILayout.PropertyField(m_collider, new GUIContent("Collider"));
                    EditorGUILayout.PropertyField(m_trail);
                }
            }

        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.PropertyField(OnFire);
            EditorGUILayout.PropertyField(OnRayCastHit);
        }
    }
#endif

    #endregion
}