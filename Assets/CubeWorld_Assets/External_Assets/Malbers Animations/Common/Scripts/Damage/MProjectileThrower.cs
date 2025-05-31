using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.Events;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Damage/Projectile Thrower")]

    public class MProjectileThrower : MonoBehaviour, IThrower, IAnimatorListener
    {
        /// <summary> Is Used to calculate the Trajectory and Display it as a LineRenderer </summary>
        public System.Action<bool> Predict { get; set; }

        [Tooltip("What projectile will be instantiated")]
        [SerializeField] private GameObjectReference m_Projectile = new();

        [Tooltip("The projectile will be fired on start")]
        public BoolReference FireOnStart;

        [Tooltip("Spread Angle for the Projectile")]
        public FloatReference spreadAngle = new(0);
        [Tooltip("Spread Multiplier the spread angle")]
        public Vector3Reference spreadMult = new(Vector3.one);

        [Tooltip("Ammount of projectiles to fire at the same time")]
        public FloatReference amount = new(1);

        [SerializeField] private LayerReference hitLayer = new(-1);

        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        public bool debug;



        [Tooltip("When this parameter is set it will Aim Directly to the Target")]
        [SerializeField] private TransformReference m_Target;

        [Tooltip("Transform Reference for to calculate the Thrower Aim Origin Position")]
        [SerializeField] private Transform m_AimOrigin;

        [Tooltip("Owner of the Thrower Component. By default it should be the Root GameObject")]
        [SerializeField] private GameObjectReference m_Owner;

        [Tooltip("Reference to the Aimer Component to Aim the Projectile")]
        public Aim Aimer;

        [Tooltip("if its set to False. it will use this GameObject Forward Direction")]
        public BoolReference useAimerDirection = new(true);
        [Hide("Aimer")]
        [Tooltip("Update the Thrower Target from the Aimer component")]
        public bool UpdateTargetFromAimer = false;


        [Tooltip("Multiplier value to Apply to the Projectile Stat Modifier"), FormerlySerializedAs("Multiplier")]
        public FloatReference DamageMultiplier = new(1);
        [Tooltip("Multiplier value to apply to the Projectile Scale")]
        public FloatReference ScaleMultiplier = new(1);
        [Tooltip("Multiplier value to apply to the Projectile Launch Force")]
        public FloatReference ForceMultiplier = new(1);

        [SerializeField, Tooltip("Launch force for the Projectile")]
        private float m_Force = 50f;

        //[TabGroup("Multipliers")]

        [Range(0, 90), Tooltip("Angle of the Projectile when a Target is assigned")]
        [SerializeField] private float m_angle = 45f;


        //[TabGroup("Multipliers")]
        [Tooltip("Gravity to apply to the Projectile. By default is set to Physics.gravity")]
        [SerializeField] private Vector3Reference gravity = new(Physics.gravity);

        //[TabGroup("Multipliers")]
        [Tooltip("Apply Gravity after certain distance is reached")]
        [SerializeField] private FloatReference m_AfterDistance = new(0f);


        // [Header("Projectile Pool")]
        //[TabGroup("Pooling")]
        [Tooltip("Initial number of Projectiles in Projectile Pool")]
        public IntReference m_ProjectileInitialPoolSize = new(10); // Initial size of pool
                                                                   // [TabGroup("Pooling")]
        [Tooltip("Max number of Projectiles in Projectile Pool")]
        public IntReference m_ProjectileMaxPoolSize = new(50); // Max size of pool
                                                               //  [TabGroup("Pooling")]
        [Tooltip("Start the Pool with the Initial Pool Size (GameObjects Instantiated and disabled)")]
        public BoolReference StartPoolSize = new(false);

        public UnityEvent OnFire = new UnityEvent();

        #region Properties


        public float AfterDistance { get => m_AfterDistance.Value; set => m_AfterDistance.Value = value; }
        public Vector3 Gravity { get => gravity.Value; set => gravity.Value = value; }
        public LayerMask Layer { get => hitLayer.Value; set => hitLayer.Value = value; }
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }
        public Vector3 AimOriginPos => m_AimOrigin.position;
        public Transform Target { get => m_Target.Value; set => m_Target.Value = value; }

        /// <summary> Owner of the  </summary>
        public GameObject Owner { get => m_Owner.Value; set => m_Owner.Value = value; }
        public GameObject Projectile { get => m_Projectile.Value; set => m_Projectile.Value = value; }

        /// <summary> Projectile Launch Velocity(v3) </summary>
        public Vector3 Velocity { get; set; }

        /// <summary>Force to Apply to the Projectile</summary>
        public float Power { get => m_Force * ForceMultiplier; set => m_Force = value; }

        /// <summary>Angle to throw a projectile when it has a target</summary>
        public float Angle { get => m_angle; set => m_angle = value; }

        /// <summary>Set if the Aimer Direction will be used or not</summary>
        public bool UseAimerDirection { get => useAimerDirection.Value; set => useAimerDirection.Value = value; }

        public Transform AimOrigin => m_AimOrigin;

        /// <summary> Pool - Added IObjectPool<GameObject> thisProjectilePool new parameter for Projectile_Prepare / Prepare </summary>  
        private IObjectPool<GameObject> ProjectilePool;
        /// <summary>  Pool - End </summary>  
           #endregion

        public bool CalculateTrajectory
        {
            get => m_CalculateTrajectory;
            set
            {
                m_CalculateTrajectory = value;
                Predict?.Invoke(value);
            }
        }
        private bool m_CalculateTrajectory;

        void Awake()
        {
            ProjectilePool = new ObjectPool<GameObject>
                        (CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, false,
                        m_ProjectileInitialPoolSize.Value, m_ProjectileMaxPoolSize.Value);

            if (StartPoolSize.Value) PopulatePool();
        }

        private void OnEnable()
        {
            if (Owner == null) Owner = transform.FindObjectCore().gameObject;

            if (m_AimOrigin == null)
            {
                //Set the Aim origin from the Aimer
                m_AimOrigin = Aimer ? Aimer.AimOrigin : transform;
            }

            if (FireOnStart) Fire();

            Aimer?.OnSetTarget.AddListener(AimerTarget);
        }

        private void OnDisable()
        {
            Aimer?.OnSetTarget.RemoveListener(AimerTarget);
        }

        private void AimerTarget(Transform target)
        {
            if (UpdateTargetFromAimer) Target = target;
        }

        public virtual void SetProjectile(GameObject newProjectile)
        {
            if (Projectile != newProjectile) Projectile = newProjectile;
        }

        /// <summary> Fire Proyectile </summary>
        public virtual void Fire()
        {
            if (!enabled) return;
            if (Projectile == null) return;

            OnFire.Invoke();

            for (int i = 0; i < amount; i++)
            {
                CalculateVelocity();
                //var Inst_Projectile = Instantiate(Projectile, AimOriginPos, Quaternion.identity);
                var Inst_Projectile = ProjectilePool.Get();

                Inst_Projectile.transform.localScale *= ScaleMultiplier;

                Prepare_Projectile(Inst_Projectile);

                Predict?.Invoke(false);

                if (debug) Debug.Log($"<B>[{Owner.name}]</B> is Firing a <B>[{Projectile.name}]</B> with a <B>[{Power}]</B> Force", this);
            }
        }


        private void FixedUpdate()
        {
            if (CalculateTrajectory) CalculateVelocity();
        }

        public void SetTarget(Transform target) => Target = target;
        public void ClearTarget() => Target = null;
        public void SetTarget(GameObject target) => Target = target.transform;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        void Prepare_Projectile(GameObject p)
        {
            if (spreadAngle > 0)
            {
                Quaternion spreadRotation =
                Quaternion.Euler(
                    Random.Range(-spreadAngle * spreadMult.x, spreadAngle * spreadMult.x),
                    Random.Range(-spreadAngle * spreadMult.y, spreadAngle * spreadMult.y),
                    Random.Range(-spreadAngle * spreadMult.z, spreadAngle * spreadMult.z));

                Velocity = spreadRotation * Velocity;
            }

            //Means its a Malbers Projectile ^^
            if (p.TryGetComponent<IProjectile>(out var projectile))
            {

                //Pool - Added thisProjectilePool new parameter to Prepare  </summary>  
                projectile.Prepare(Owner, Gravity, Velocity, Layer, TriggerInteraction, ProjectilePool);
                //Pool - End  
                projectile.AfterDistance = AfterDistance;
                projectile.SetDamageMultiplier(DamageMultiplier); //Apply Multiplier
                projectile.Fire();
            }
            else //Fire without the Projectile Component
            {
                if (p.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce(Velocity, ForceMode.VelocityChange);
                }
            }
        }

        public virtual void SetDamageMultiplier(float m) => DamageMultiplier = m;
        public virtual void SetScaleMultiplier(float m) => ScaleMultiplier = m;
        public virtual void SetPowerMultiplier(float m) => ForceMultiplier = m;
        public virtual void SetForceMultiplier(float m) => SetPowerMultiplier(m);

        public virtual void SetAimerDirection(float m) => useAimerDirection.Value = m > 0.5f;
        public virtual void SetAimerDirection(int m) => useAimerDirection.Value = m == 1;

        public virtual void CalculateVelocity()
        {
            if (Target)
            {
                var target_Direction = (Target.position - AimOriginPos).normalized;

                float TargetAngle = 90 - Vector3.Angle(target_Direction, -Gravity) + 0.1f; //in case the angle is smaller than the target height

                if (TargetAngle < m_angle)
                    TargetAngle = m_angle;

                Power = MTools.PowerFromAngle(AimOriginPos, Target.position, TargetAngle);
                Velocity = MTools.VelocityFromPower(AimOriginPos, Power, TargetAngle, Target.position);
            }
            else if (Aimer && useAimerDirection.Value)
            {
                Velocity = (Aimer.AimPoint - AimOriginPos).normalized * Power;
            }
            else
            {
                Velocity = transform.forward.normalized * Power;
            }

            Predict?.Invoke(true);
        }


        private void OnReturnedToPool(GameObject Go)
        {
            Go.transform.SetParent(transform, false); // or to Weapon
            Go.transform.ResetLocal();
            Go.SetActive(false); // Disable
        }

        private void OnTakeFromPool(GameObject Go)
        {
            Go.transform.SetParent(null, true); // Unparent projectile
            Go.transform.position = AimOriginPos; // Set position to AimOrigin
            Go.SetActive(true); // Enable 
        }

        private GameObject CreatePooledItem()
        {
            // This is used to return Projectile to the pool when they have stopped.
            // Create instance of Projectile
            GameObject projectileInstance = Instantiate(m_Projectile.Value, transform);
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


        // If the pool capacity is reached then any items returned will be destroyed.
        // We can control what the destroy behavior does, here we destroy the GameObject.
        private void OnDestroyPoolObject(GameObject Go)
        {
            Destroy(Go); // When pool size excides max size destroy projectile
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



        void Reset()
        {
            m_Owner = new GameObjectReference(transform.FindObjectCore().gameObject);
            m_AimOrigin = transform;
        }

#if UNITY_EDITOR && MALBERS_DEBUG

        private void OnValidate()
        {
            if (amount.Value < 1) amount.Value = 1;
        }


        void OnDrawGizmos()
        {
            if (AimOrigin)
            {
                Handles.color = Color.blue;
                Handles.ArrowHandleCap(0, m_AimOrigin.position, transform.rotation, Power * 0.01f, EventType.Repaint);
            }
        }
#endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MProjectileThrower))]
    public class MProjectileThrowerEditor : Editor
    {
        private SerializedProperty m_Projectile, m_Force, m_angle, gravity, OnFire,
            m_AfterDistance, hitLayer, triggerInteraction, m_Target, m_AimOrigin,
            m_Owner, Aimer, useAimerDirection, UpdateTargetFromAimer, DamageMultiplier,
            ScaleMultiplier, ForceMultiplier, spreadAngle, spreadMult,
            amount, FireOnStart, debug,
            m_ProjectileInitialPoolSize, m_ProjectileMaxPoolSize, StartPoolSize;

        private void OnEnable()
        {
            m_Projectile = serializedObject.FindProperty("m_Projectile");
            m_Force = serializedObject.FindProperty("m_Force");
            m_angle = serializedObject.FindProperty("m_angle");
            gravity = serializedObject.FindProperty("gravity");
            m_AfterDistance = serializedObject.FindProperty("m_AfterDistance");
            hitLayer = serializedObject.FindProperty("hitLayer");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            m_Target = serializedObject.FindProperty("m_Target");
            m_AimOrigin = serializedObject.FindProperty("m_AimOrigin");
            m_Owner = serializedObject.FindProperty("m_Owner");
            Aimer = serializedObject.FindProperty("Aimer");
            useAimerDirection = serializedObject.FindProperty("useAimerDirection");
            UpdateTargetFromAimer = serializedObject.FindProperty("UpdateTargetFromAimer");
            DamageMultiplier = serializedObject.FindProperty("DamageMultiplier");
            ScaleMultiplier = serializedObject.FindProperty("ScaleMultiplier");
            ForceMultiplier = serializedObject.FindProperty("ForceMultiplier");
            spreadAngle = serializedObject.FindProperty("spreadAngle");
            spreadMult = serializedObject.FindProperty("spreadMult");
            amount = serializedObject.FindProperty("amount");
            FireOnStart = serializedObject.FindProperty("FireOnStart");
            debug = serializedObject.FindProperty("debug");
            m_ProjectileInitialPoolSize = serializedObject.FindProperty("m_ProjectileInitialPoolSize");
            m_ProjectileMaxPoolSize = serializedObject.FindProperty("m_ProjectileMaxPoolSize");
            StartPoolSize = serializedObject.FindProperty("StartPoolSize");
            OnFire = serializedObject.FindProperty("OnFire");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            serializedObject.Update();
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Projectile.isExpanded = MalbersEditor.Foldout(m_Projectile.isExpanded, "Projectile"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_Projectile);
                        MalbersEditor.DrawDebugIcon(debug);

                        if (Application.isPlaying)
                        {
                            if (GUILayout.Button("Fire", GUILayout.Width(40)))
                            {
                                (target as MProjectileThrower).Fire();
                            }
                        }
                    }

                    EditorGUILayout.PropertyField(FireOnStart);
                    EditorGUILayout.PropertyField(spreadAngle);
                    EditorGUILayout.PropertyField(spreadMult);
                    EditorGUILayout.PropertyField(amount);
                }

            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Projectile.isExpanded = MalbersEditor.Foldout(m_Projectile.isExpanded, "Layer Interaction"))
                {
                    EditorGUILayout.PropertyField(hitLayer);
                    EditorGUILayout.PropertyField(triggerInteraction);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Target.isExpanded = MalbersEditor.Foldout(m_Target.isExpanded, "References"))
                {
                    EditorGUILayout.PropertyField(m_Target);
                    EditorGUILayout.PropertyField(m_AimOrigin);
                    EditorGUILayout.PropertyField(m_Owner);
                    EditorGUILayout.PropertyField(Aimer);
                    EditorGUILayout.PropertyField(useAimerDirection);
                    EditorGUILayout.PropertyField(UpdateTargetFromAimer);
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_Force.isExpanded = MalbersEditor.Foldout(m_Force.isExpanded, "Physics"))
                {
                    EditorGUILayout.PropertyField(m_Force);
                    EditorGUILayout.PropertyField(m_angle);
                    EditorGUILayout.PropertyField(gravity);
                    EditorGUILayout.PropertyField(m_AfterDistance);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (DamageMultiplier.isExpanded = MalbersEditor.Foldout(DamageMultiplier.isExpanded, "Multipliers"))
                {
                    EditorGUILayout.PropertyField(DamageMultiplier);
                    EditorGUILayout.PropertyField(ScaleMultiplier);
                    EditorGUILayout.PropertyField(ForceMultiplier);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (OnFire.isExpanded = MalbersEditor.Foldout(OnFire.isExpanded, "Events"))
                {
                    EditorGUILayout.PropertyField(OnFire);
                }
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (StartPoolSize.isExpanded = MalbersEditor.Foldout(StartPoolSize.isExpanded, "Pooling"))
                {
                    EditorGUILayout.PropertyField(m_ProjectileInitialPoolSize);
                    EditorGUILayout.PropertyField(m_ProjectileMaxPoolSize);
                    EditorGUILayout.PropertyField(StartPoolSize);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}