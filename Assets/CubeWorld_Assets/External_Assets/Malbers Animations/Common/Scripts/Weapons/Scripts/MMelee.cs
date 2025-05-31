using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Utilities;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/Melee Weapon")]
    public class MMelee : MWeapon
    {
        [RequiredField] public Collider meleeTrigger;
        [Tooltip("Do not interact with Static Objects")]
        public bool ignoreStaticObjects = true;
        public BoolEvent OnCauseDamage = new();
        public Color DebugColor = new(1, 0.25f, 0, 0.5f);

        public bool UseCameraSide;
        public bool InvertCameraSide;

        [Tooltip("What Abilities to apply to the meleee weapons if they are not using any Combo")]
        public int[] GroundAttackAbilities;

        [Tooltip("What Abilities to apply to the meleee weapons if they are not using any Combo")]
        public int[] RidingAttackAbilities;

        protected bool canCauseDamage;                      //The moment in the Animation the weapon can cause Damage 
        public override bool CanCauseDamage
        {
            get => canCauseDamage;
            set
            {
                Debugging($"Can cause Damage [{value}]", this);
                canCauseDamage = value;
                if (Proxy) Proxy.Active = value;
                meleeTrigger.enabled = value;         //Enable/Disable the Trigger

                if (CanCauseDamage && AttackDirection)
                {
                    if (C_Direction != null) { StopCoroutine(C_Direction); C_Direction = null; }
                    StartCoroutine(C_Direction = I_CalculateDirection(meleeTrigger));
                }
            }
        }

        protected TriggerProxy Proxy { get; private set; }

        /// <summary>Damager from the Attack Triger Behaviour</summary>
        public override void ActivateDamager(int value, int profile)
        {
            // Debug.Log($"{profile}");

            if (value == 0)
            {
                CanCauseDamage = false;
                OnCauseDamage.Invoke(CanCauseDamage);

                if (CurrentProfileIndex != 0)
                {
                    DefaultProfile.Modify(this);
                    CurrentProfileIndex = 0; //Update the Profile Index
                    OnProfileChanged.Invoke(CurrentProfileIndex);
                    Debugging($"Setting Default Profile", this);
                }
            }
            else if (value == -1 || value == Index)
            {
                base.ActivateDamager(value, profile);
                CanCauseDamage = true;
                OnCauseDamage.Invoke(CanCauseDamage);
            }
        }

        private void Awake()
        {
            if (animator)
                defaultAnimatorSpeed = animator.speed;

            Initialize();

            CanCauseDamage = false;
        }


        public override void Initialize()
        {
            base.Initialize();
            FindTrigger();
        }

        void OnEnable()
        {
            if (Proxy)
            {
                Proxy.EnterTriggerInteraction += AttackTriggerEnter;
                // proxy.ExitTriggerInteraction += AttackTriggerExit;
            }

        }

        /// <summary>Disable Listeners </summary>
        void OnDisable()
        {
            if (Proxy != null)
            {
                Proxy.EnterTriggerInteraction -= AttackTriggerEnter;
                //proxy.ExitTriggerInteraction -= AttackTriggerExit;
            }
        }

        //void AttackTriggerExit(GameObject root, Collider other)
        //{
        //    //???
        //}


        #region Main Attack 
        internal override void MainAttack_Start(IMWeaponOwner RC)
        {
            base.MainAttack_Start(RC);

            if (CanAttack)
            {
                WeaponAction.Invoke((int)Weapon_Action.Attack);
            }
        }




        /// <summary>Set when the Current Attack is Active and Holding ... So reset the Attack</summary>
        internal override void Attack_Charge(IMWeaponOwner RC, float time)
        {
            if (Automatic && CanAttack && CanCharge && Rate > 0 && Input)
            {
                MainAttack_Start(RC);
            }
        }

        #endregion
        void AttackTriggerEnter(GameObject root, Collider other)
        {
            //Debug.Log("AttackTriggerEnter = " + other);
            if (IsInvalid(other)) return;                                               //Check Layers and Don't hit yourself
            if (other.transform.root == IgnoreTransform) return;                        //Check an Extra transform that you cannot hit...e.g your mount
            if (ignoreStaticObjects && other.transform.gameObject.isStatic) return;     //Ignore Static Objects


            if (MissAttack()) return; //Ignore the Attack if the Animal is Missing the Attack 


            var damagee = other.GetComponentInParent<IMDamage>();                      //Get the Animal on the Other collider

            if (!AttackDirection)
                Direction = Owner.transform.forward;

            var center = meleeTrigger.bounds.center;

            Debugging($"Hit [{other.name}]", this);

            TryInteract(other.gameObject);                                              //Get the interactable on the Other collider
            TryPhysics(other.attachedRigidbody, other, center, Force);       //If the other has a riggid body and it can be pushed
            TryStopAnimator();
            TryHitEffect(other, meleeTrigger.bounds.center, damagee);

            var Damage = new StatModifier(statModifier)
            { Value = Mathf.Lerp(MinDamage, MaxDamage, ChargedNormalized) };            //Do the Damage depending the charge

            if (damagee != null) { damagee.HitCollider = other; }
            TryDamage(damagee, Damage); //if the other does'nt have the Damagable Interface dont send the Damagable stuff 

            //Store the Last Collider that the animal hit
        }



        public override void ResetWeapon()
        {
            if (meleeTrigger)
            {

                meleeTrigger.enabled = false;
                Proxy.Active = false;
            }
            base.ResetWeapon();
        }

        private void FindTrigger()
        {
            if (meleeTrigger == null) meleeTrigger = GetComponent<Collider>();

            if (meleeTrigger)
            {
                Proxy = TriggerProxy.CheckTriggerProxy(meleeTrigger, Layer, TriggerInteraction, Owner.transform);

                meleeTrigger.enabled = false;
                Proxy.Active = meleeTrigger.enabled;
                Proxy.EnterTriggerInteraction = null; //Clear all of them in start
            }
            else
            {
                Debug.LogError($"Weapon [{name}] needs a collider. Please add one. Disabling Weapon", this);
                enabled = false;
            }
        }

        #region Gizmos

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) return;

            if (meleeTrigger != null)
                DrawTriggers(meleeTrigger.transform, meleeTrigger, DebugColor, false);
        }

        void OnDrawGizmosSelected()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) return;

            if (!Application.isPlaying)
                if (meleeTrigger != null)
                    DrawTriggers(meleeTrigger.transform, meleeTrigger, DebugColor, true);
        }


        protected override void Reset()
        {
            base.Reset();

            weaponType = MTools.GetInstance<WeaponID>("Melee");
            m_rate.Value = 0.5f;
            m_Automatic.Value = true;
        }
#endif
        #endregion
    }
    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MMelee))]
    public class MMeleeEditor : MWeaponEditor
    {
        SerializedProperty meleeCollider, OnCauseDamage, UseCameraSide, InvertCameraSide,
            GroundCombo, GroundAttackAbilities, RidingCombo,
            RidingAttackAbilities, ignoreStaticObjects;

        void OnEnable()
        {
            WeaponTab = "Melee";
            SetOnEnable();
            meleeCollider = serializedObject.FindProperty("meleeTrigger");
            ignoreStaticObjects = serializedObject.FindProperty("ignoreStaticObjects");
            OnCauseDamage = serializedObject.FindProperty("OnCauseDamage");
            InvertCameraSide = serializedObject.FindProperty("InvertCameraSide");
            UseCameraSide = serializedObject.FindProperty("UseCameraSide");
            //  Attacks = serializedObject.FindProperty("Attacks");
            RidingAttackAbilities = serializedObject.FindProperty("RidingAttackAbilities");
            GroundAttackAbilities = serializedObject.FindProperty("GroundAttackAbilities");
            GroundCombo = serializedObject.FindProperty("GroundCombo");
            RidingCombo = serializedObject.FindProperty("RidingCombo");
            description = serializedObject.FindProperty("description");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDescription("Melee Weapon Properties");
            if (meleeCollider.objectReferenceValue == null)
                EditorGUILayout.HelpBox("Weapon needs a collider. Check [Melee] Tab", MessageType.Error);

            WeaponInspector();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void UpdateSoundHelp()
        {
            SoundHelp = "0:Draw   1:Store   2:Swing   3:Hit \n (Leave 3 Empty, add SoundByMaterial and Invoke 'PlayMaterialSound' for custom Hit sounds)";
        }

        protected override void ChildWeaponEvents()
        {
            EditorGUILayout.PropertyField(OnCauseDamage, new GUIContent("On AttackTrigger Active"));
        }

        protected override void DrawAdvancedWeapon()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(ignoreStaticObjects);
                EditorGUILayout.PropertyField(meleeCollider,
                    new GUIContent("Melee Trigger", "Gets the reference of where is the Melee Collider of this weapon (Not Always is in the same gameobject level)"));
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Ground Attacks", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(GroundCombo);

                if (GroundCombo.objectReferenceValue == null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(GroundAttackAbilities);
                    EditorGUI.indentLevel--;
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Riding Attacks", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(RidingCombo);

                if (RidingCombo.objectReferenceValue == null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(RidingAttackAbilities);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(UseCameraSide, new GUIContent("Use Camera Side", "The Attacks are Activated by the Main Attack and It uses the Side of the Camera to Attack on the Right or the Left side of the Mount"));

                    if (UseCameraSide.boolValue)
                        EditorGUILayout.PropertyField(InvertCameraSide, new GUIContent("Invert Camera Side", "Inverts the camera side value"));
                }
            }
        }
    }
#endif
    #endregion
}