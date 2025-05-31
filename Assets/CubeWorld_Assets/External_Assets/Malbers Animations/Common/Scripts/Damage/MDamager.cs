using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using MalbersAnimations.Reactions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary> Core Class to cause damage to the stats</summary>
    // [AddComponentMenu("Malbers/Damage/Damager")]
    public abstract class MDamager : MonoBehaviour, IMDamager, IInteractor
    {
        #region Public Variables
        [SerializeField, Tooltip("Index of the Damager, You can have multiple swords ... this identifies if a sword is different from another")]
        protected int index = 1;

        /// <summary>Enable/Disable the Damager</summary>
        [SerializeField, Tooltip("Enable/Disable the Damager")]
        protected BoolReference m_Active = new(true);


        [Tooltip("Hit Layer to interact with Objects"), ContextMenuItem("Get Layer from Root", "GetLayerFromRoot")]
        public LayerReference m_hitLayer = new(-1);

        [Tooltip("Search only Tags")]
        public Tag[] Tags;

        public virtual bool CanCauseDamage { get; set; }


        [Tooltip("True: the Attack Direction is calculated using the movement.\nFalse: The Attact Direction is the Character Forward Direction")]
        public bool AttackDirection = false;

        [SerializeField, Tooltip("What to do with Triggers")]
        protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;


        [SerializeField, Tooltip("Owner. usually the Character Owns the Damager")]
        [ContextMenuItem("Find Owner", "Find_Owner")]
        protected GameObject owner;

        [SerializeField, Tooltip("Default Hit Effect. This Gameobject will be enabled on Impact, if its a Prefab it will be instantiated")]
        internal GameObjectReference m_HitEffect;


        [Tooltip("Custom Hit Effects if the Damageable has a Surface ID")]
        public List<EffectType> hitEffects = new();

        [Tooltip("Default Audio Clip to play when the Damager hit something")]
        public AudioClipReference hitSound;

        public GameObject HitEffect { get => m_HitEffect.Value; set => m_HitEffect.Value = value; }

        [Tooltip("The Effect will be destroyed If is a Prefab. after this time has elapsed. If this value is zero, the effect will not be destroyed")]
        [Min(0)] public float DestroyHitEffect;

        [Tooltip("Dont Hit any objects on the Owner's hierarchy")]
        public BoolReference dontHitOwner = new(true);

        [Tooltip("Don't use the Default Reaction of the Damageable Component")]
        [SerializeReference, SubclassSelector]
        public Reaction CustomReaction;

        [Tooltip("Type of element damage the Damager can do")]
        public StatElement element;

        [Tooltip("Interactor ID to enable with who interactable the Interactor can react")]
        public IntReference interactorID = new(0);

        /// <summary> Extra Transform to Ignore Damage. E.g. The Mount Animal</summary>
        public virtual Transform IgnoreTransform { get; set; }

        [Tooltip("Damager can activate interactables")]
        public BoolReference interact = new(true);

        [Tooltip("Damager allows the Damagee to apply an animal reaction")]
        public BoolReference react = new(true);

        [Tooltip("If true the Damage Receiver will not apply its Default Multiplier")]
        public BoolReference pureDamage = new(false);

        [Tooltip("Stat to modify on the Damagee")]
        [ContextMenuItem("Set Default Damage", "Set_DefaultDamage")]
        public StatModifier statModifier = new();


        /// <summary>Miss Change (0 - 1)</summary>
        [SerializeField, Tooltip("Miss Chance (0 - 1)\n1 means it will always Miss")]
        protected FloatReference m_MissChance = new(0);

        /// <summary>Critical Change (0 - 1)</summary>
        [SerializeField, Tooltip("Critical Change (0 - 1)\n1 means it will be always critical")]
        protected FloatReference m_cChance = new(0);

        /// <summary>If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier</summary>
        [SerializeField, Tooltip("If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier")]
        protected FloatReference cMultiplier = new(2);

        [SerializeField, Tooltip("MAX Force to Apply to RigidBodies when the Damager hit them")]
        protected FloatReference m_Force = new(50f);

        [SerializeField, Tooltip("MIN Force to Apply to RigidBodies when the Damager hit them")]
        protected FloatReference minForce = new(20f);        //Weapon min Force to push rigid bodies;

        [Tooltip("Force mode to apply to the Object that the Damager Hits")]
        public ForceMode forceMode = ForceMode.VelocityChange;


        [Tooltip("Stores the Direction of the Attack. Used to apply the Force and to know the Direction of the Hit from the Damager")]
        protected Vector3 Direction { get; set; }
        //{
        //    get => direction;
        //    set {
        //        direction = value;
        //      //  Debug.Log($"direction -> {direction}");
        //    }
        //}
        //Vector3 direction;
        /// <summary>Point of of Contact</summary>
        public Vector3 HitPosition { get; protected set; }

        /// <summary>Rotation of the Point of contact (Normal)</summary>
        public Quaternion HitRotation { get; protected set; }

        public TransformEvent OnHit = new();
        public Vector3Event OnHitPosition = new();
        public IntEvent OnHitInteractable = new();
        public IntEvent OnProfileChanged = new();
        public UnityEvent OnAttackMissed = new();

        //[Tooltip("When the Attack Trigger Touches a valid collider, it will stop the animator to give an extra effect")]
        //public BoolReference StopAnimator = new BoolReference(false);
        [Tooltip("If there's an Animator Controller it will be stopped")]
        [ContextMenuItem("Find Animator", "Find_Animator")]
        [ContextMenuItem("Clear Animator", "Clear_Animator")]
        public Animator animator;
        [Tooltip("Value of the Animator Speed when its stopped")]
        public FloatReference AnimatorSpeed = new(0.05f);
        [Tooltip("Time the Animator will be stopped. If its zero, stopping the animator is ignored")]
        public FloatReference AnimatorStopTime = new(0.1f);

        [Tooltip("Profiles to change the values of a Damager")]
        public List<DamagerProfile> Profiles;


        /// <summary>Multiplier Damage applied to the Damager stat Modifier </summary>
        private float DamageMultiplier = 1;
        public virtual void SetDamageMultiplier(float multiplier) => DamageMultiplier = multiplier;

        /// <summary>  Current Profile Index for the Damager </summary>
        protected int CurrentProfileIndex = 0;

        public DamagerProfile DefaultProfile;


        /// <summary>Damageee that can be Damaged</summary>
        protected IMDamage damagee;

        #endregion

        #region Properties
        /// <summary>Owner of the Damager</summary>
        public virtual GameObject Owner { get => owner; set => owner = value; }

        /// <summary>Force of the Damager</summary>
        public virtual float Force => Mathf.Lerp(MinForce, MaxForce, UnityEngine.Random.Range(0f, 1f));

        /// <summary>Minimun Force the Weapon can do to a Rigid Body</summary>
        public virtual float MinForce { get => minForce.Value; set => minForce.Value = value; }

        /// <summary>Maximun Force the Weapon can do to a Rigid Body</summary>
        public virtual float MaxForce { get => m_Force.Value; set => m_Force.Value = value; }

        public LayerMask Layer { get => m_hitLayer.Value; set => m_hitLayer.Value = value; }
        public QueryTriggerInteraction TriggerInteraction { get => triggerInteraction; set => triggerInteraction = value; }


        /// <summary>Does the hit was Critical</summary>
        public bool IsCritical { get; set; }
        public bool debug;

        public AudioSource m_audio;
        protected bool playingSound;

        protected void PlaySound(AudioClip newSound)
        {
            if (m_audio && !playingSound && gameObject.activeInHierarchy)
            {
                m_audio.playOnAwake = false;
                playingSound = true;

                //HACK FOR THE SOUND
                // this.Delay_Action(2, () =>
                {
                    m_audio.clip = newSound;
                    m_audio.Play();
                    // Debug.Log($"newSound: {newSound.name}");
                    playingSound = false;
                }
                //  );
            }
        }


        protected virtual bool MissAttack()
        {
            var MissedAttack = m_MissChance >= UnityEngine.Random.value;  //Calculate if is critical  
            if (MissedAttack) OnAttackMissed.Invoke();
            return MissedAttack;
        }

        protected void CheckAudioSource()
        {
            if (!m_audio) m_audio = gameObject.FindComponent<AudioSource>(); //Gets the Weapon Source
            if (!m_audio) m_audio = gameObject.AddComponent<AudioSource>(); //Create an AudioSourse if theres no Audio Source on the weapon
            m_audio.spatialBlend = 1;
        }

        /// <summary>If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier</summary>
        public float CriticalMultiplier { get => cMultiplier.Value; set => cMultiplier.Value = value; }

        /// <summary>>Critical Change (0 - 1)</summary>
        public float CriticalChance { get => m_cChance.Value; set => m_cChance.Value = value; }


        /// <summary>>Miss Change (0 - 1)</summary>
        public float MissChance { get => m_MissChance.Value; set => m_MissChance.Value = value; }

        /// <summary>>Index of the Damager</summary>
        public virtual int Index => index;
        public virtual int ID => interactorID.Value;

        /// <summary>  Set/Get the Damager Active  </summary>
        public virtual bool Enabled
        {
            get => m_Active.Value;
            set => m_Active.Value = enabled = value;
        }


        protected IEnumerator C_Direction;
        protected IEnumerator I_CalculateDirection(Collider Trigger)
        {
            var lastPos = Trigger.bounds.center;

            var debColor = (Color.red + Color.yellow) / 2;
            debColor.a = 0.7f;


            while (CanCauseDamage)
            {
                // Debug.Log("CANCAUSE");
                var newDir = (Trigger.bounds.center - lastPos).normalized;                    //Calculate the direction of the attack
                if (newDir != Vector3.zero) Direction = newDir;
                //Direction = Owner.transform.forward;
                lastPos = Trigger.bounds.center;
                if (debug) MDebug.Draw_Arrow(Trigger.bounds.center, Direction, debColor, .5f);
                yield return null;
            }
        }

        #endregion

        /// <summary>  The Damagee does not have all the conditions to apply the Damage  </summary>
        public virtual bool IsInvalid(Collider damagee)
        {
            if (Tags != null && Tags.Length > 0)
            {
                if (!damagee.gameObject.HasMalbersTagInParent(Tags)) return true;
            }

            if (damagee.isTrigger && TriggerInteraction == QueryTriggerInteraction.Ignore) return true;    //just collapse when is a collider what we are hitting
            if (!MTools.Layer_in_LayerMask(damagee.gameObject.layer, Layer)) { return true; }        //Just hit what is on the HitMask Layer
            if (dontHitOwner && Owner != null && damagee.transform.IsChildOf(Owner.transform)) { return true; }   //Dont hit yourself!
                                                                                                                  // if (damagee.gameObject.isStatic) return true;
            return false;
        }


        /// <summary>  Applies the Damage to the Game object  </summary>
        /// <returns>is False if the other gameobject didn't had a IMDamage component attached</returns>
        protected virtual bool TryDamage(IMDamage damagee, StatModifier stat)
        {
            if (damagee != null)
            {
                damagee.LastForceMode = forceMode; //Save the last force mode applied

                if (!stat.IsNull)
                {
                    var criticalStat = CheckCriticalCheckMultiplier(stat);
                    damagee.ReceiveDamage(Direction, HitPosition, Owner, criticalStat, IsCritical, react.Value, CustomReaction, pureDamage.Value, element);
                    Debugging($"Do Damage to [{damagee.Damagee.name}]", damagee.Damagee);
                    return true;
                }
            }
            return false;
        }

        protected void TryHitEffect(Collider col, Vector3 DamageCenter, IMDamage damagee)
        {
            if (col is MeshCollider && !(col as MeshCollider).convex) return; //Do not hit NonConvex Collider
            if (col is TerrainCollider) return; //Do not hit  a Terrain Collider

            HitPosition = col.ClosestPoint(DamageCenter); //Find the closest point on the Collider hitted 
            HitRotation = Quaternion.FromToRotation(Vector3.up, col.bounds.center - DamageCenter);
            OnHitPosition.Invoke(HitPosition);

            if (debug) MDebug.DrawWireSphere(HitPosition, Color.red, 0.175f, 1);

            var HitEffect = this.HitEffect;
            var hitSound = this.hitSound;

            //Find Hit Effects and Sounds
            if (damagee != null && hitEffects != null && hitEffects.Count > 0)
            {
                var eff = hitEffects.Find(x => x.surface == damagee.Surface);

                if (eff != null)
                {
                    if (eff.effect.Value != null) HitEffect = eff.effect.Value;//Use the Effect from the List
                    if (eff.sound != null) hitSound = eff.sound; //use the sound form the list
                }
            }

            if (HitEffect != null)
            {
                if (HitEffect.IsPrefab())
                {
                    var instance = Instantiate(HitEffect, HitPosition, HitRotation);
                    // instance.transform.parent = col.transform;
                    //Reset the gameobject visibility 

                    CheckHitEffect(instance);

                    if (DestroyHitEffect > 0) Destroy(instance, DestroyHitEffect);
                }
                else
                {
                    HitEffect.transform.parent = null;
                    HitEffect.transform.SetPositionAndRotation(HitPosition, HitRotation);
                    CheckHitEffect(HitEffect);
                }
            }

            if (m_audio != null)
                PlaySound(hitSound.Value);

            OnHit.Invoke(col.transform);
        }

        //Check if the Hit Effect has a MDamager so pass the Layer and Owner (E.g. Explosions)
        protected void CheckHitEffect(GameObject hit)
        {
            //Check if the Hit Effect has a MDamager so pass the Layer and Owner (E.g. Explosions)
            var isDamager = hit.GetComponent<MDamager>();
            if (isDamager)
            {
                isDamager.Owner = Owner;
                isDamager.Layer = Layer;
                isDamager.TriggerInteraction = TriggerInteraction;
            }

            if (!hit.IsPrefab())
            {
                hit.SetActive(false);
                hit.SetActive(true);
            }
        }

        protected virtual bool TryDamage(GameObject other, StatModifier stat) => TryDamage(other.FindInterface<IMDamage>(), stat);

        /// <summary>  Activates the Damager in case the Damager uses a Trigger</summary>
        /// <param name="value"> Value used on a Child class to enable or disable the Damager</param>
        /// <param name="profileIndex">The profile needed for the Damage</param>
        /// <param name="multiplier"></param>
        public virtual void DoDamage(bool value, int profileIndex)
        {
            if (Profiles == null) return; //Do nothing if the profiles are null

            //Debug.Log($"profileIndex: {profileIndex}, current {CurrentProfileIndex}",this);

            if (profileIndex != CurrentProfileIndex)
            {
                if (profileIndex == 0)
                {
                    DefaultProfile.Modify(this);
                    CurrentProfileIndex = 0; //Update the Profile Index
                    OnProfileChanged.Invoke(CurrentProfileIndex);
                    Debugging($"Setting Default Profile", this);
                }
                else if (profileIndex <= Profiles.Count)
                {
                    Profiles[profileIndex - 1].Modify(this);
                    CurrentProfileIndex = profileIndex; //Update the Profile Index
                    OnProfileChanged.Invoke(CurrentProfileIndex);
                    Debugging($"Setting Profile {CurrentProfileIndex}", this);
                }
            }
        }

        protected void TryStopAnimator()
        {
            if (animator != null && C_StopAnim == null)
            {
                C_StopAnim = C_StopAnimator();
                StartCoroutine(C_StopAnim);
            }
        }

        protected IEnumerator C_StopAnim;
        protected float defaultAnimatorSpeed = 1;

        protected IEnumerator C_StopAnimator()
        {
            animator.speed = AnimatorSpeed;
            yield return new WaitForSeconds(AnimatorStopTime.Value);

            if (animator)
                animator.speed = defaultAnimatorSpeed;

            C_StopAnim = null;
        }

        /// <summary>Damager can Activate Interactables </summary>
        protected bool TryInteract(GameObject damagee)
        {
            if (interact)
            {
                var interactable = damagee.FindInterface<IInteractable>();
                if (interactable != null && interactable.Active)
                {
                    return Interact(interactable);              //if we have an Local Interactor then use it instead of this Damager
                }
            }
            return false;
        }


        public void Focus(IInteractable item)
        {
            if (item.Active) //Ignore One Disable Interactors
            {
                item.CurrentInteractor = this;
                item.Focused = true;
                if (item.Auto) Interact(item); //Interact if the interacter is on Auto
            }
        }

        public void UnFocus(IInteractable item)
        {
            if (item != null)
            {
                item.CurrentInteractor = this;
                item.Focused = false;
                item.CurrentInteractor = null;
            }
        }

        /// <summary> Interact locally  </summary>
        public virtual bool Interact(IInteractable interactable)
        {
            if (interactable != null)
            {
                Debugging($"Interact with <B>[{interactable.Owner.name}]</B>", interactable.Owner);
                if (interactable.Interact(this))
                {
                    OnHitInteractable.Invoke(interactable.Index);
                    return true;
                }
                return false;
            }
            return false;
        }


        /// <summary> Restart method from Interactor </summary>
        public virtual void Restart() { }

        /// <summary>Apply Physics to the Damageee </summary>
        protected virtual bool TryPhysics(Rigidbody rb, Collider col, Vector3 Origin, float force)
        {
            if (rb && force > 0)
            {
                // if (debug) MDebug.Draw_Arrow(Origin, Direction, Color.red,0.25f,0.1f);

                this.Direction *= force;

                // Debug.Log($"TryPhysics {Direction * force}  {force}");

                if (col) //When using collider
                {
                    var HitPoint = col.ClosestPoint(Origin);
                    rb.AddForceAtPosition(Direction, HitPoint, forceMode);

                    if (debug)
                    {
                        MDebug.DrawWireSphere(HitPoint, Color.red, 0.1f, 2f);
                        MDebug.Draw_Arrow(HitPoint, Direction, Color.red, 2f);
                    }
                }
                else
                    rb.AddForce(Direction, forceMode);

                Debugging($"Apply Force to [{rb.name}]", this);


                return true;
            }
            return false;
        }

        public virtual void SetOwner(GameObject owner) => Owner = owner;
        public virtual void SetOwner(Transform owner) => Owner = owner.gameObject;

        /// <summary>  Prepare the modifier value to change it if is critical  </summary>
        protected virtual StatModifier CheckCriticalCheckMultiplier(StatModifier mod)
        {
            IsCritical = m_cChance > Random.value;  //Calculate if is critical

            var modifier = new StatModifier(mod);

            if (IsCritical && CriticalChance > 0)
            {
                modifier.Value = mod.Value * CriticalMultiplier;        //apply the Critical Damage and Animation Multiplier
            }

            modifier.MinValue.Value *= DamageMultiplier;
            modifier.MaxValue.Value *= DamageMultiplier;

            DamageMultiplier = 1; //Reset the Damage Multiplier always!!!

            return modifier;
        }

        protected void Find_Owner()
        {
            if (Owner == null)
                Owner = transform.root.gameObject;

            MTools.SetDirty(this);
        }

        protected void Find_Animator()
        {
            if (animator == null) animator = gameObject.FindComponent<Animator>();
            MTools.SetDirty(this);
        }

        protected void Clear_Animator()
        {
            animator = null;
            MTools.SetDirty(this);
        }

        public virtual void Stat_SetMaxValue(float value) => statModifier.MaxValue = value;
        public virtual void Stat_SetMinValue(float value) => statModifier.MinValue = value;

        /// <summary> Cache all the default values of a Damager  </summary>
        protected virtual void SetDefaultProfile()
        {
            DefaultProfile = GetProfile();
        }

        internal virtual DamagerProfile GetProfile()
        {
            var newProfile = new DamagerProfile()
            {
                Name = "Default",
                CustomReaction = this.CustomReaction,
                element = this.element,
                forceMode = this.forceMode,
                interact = new BoolReference(this.interact),
                react = new BoolReference(this.react),
                interactorID = new IntReference(this.interactorID),
                maxForce = new FloatReference(this.m_Force),
                minForce = new FloatReference(this.minForce),

                m_cChance = new FloatReference(this.m_cChance),
                cMultiplier = new FloatReference(this.cMultiplier),

                m_HitEffect = this.m_HitEffect,
                pureDamage = new BoolReference(this.pureDamage),
                statModifier = new StatModifier(this.statModifier),
                modify = (DamagerProfile.DamageProfileModif)(-1)//Set to modify everything
            };

            return newProfile;
        }

        /// <summary> Set the values of a Damager from a profile </summary>
        protected virtual void SetProfile(DamagerProfile newProfile) => newProfile.Modify(this);


#if UNITY_EDITOR
        protected virtual void Reset()
        {
            statModifier = new StatModifier()
            {
                ID = MTools.GetInstance<StatID>("Health"),
                modify = StatOption.SubstractValue,
                MaxValue = new FloatReference(10),
                MinValue = new FloatReference(10)
            };

            m_hitLayer.Variable = MTools.GetInstance<LayerVar>("Hit Layer");
            m_hitLayer.UseConstant = false;

            var core = transform.GetComponentInParent<IObjectCore>();

            owner = core != null ? core.transform.gameObject : transform.root.gameObject;
        }



        public static void DrawTriggers(Transform transform, Collider Trigger, Color DebugColor, bool selected = false)
        {
#if MALBERS_DEBUG
            if (Trigger == null) return;

            Gizmos.color = DebugColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            var DColorFlat = new Color(DebugColor.r, DebugColor.g, DebugColor.b, 1f);

            if (selected) DColorFlat = Color.yellow;

            if (Trigger is BoxCollider)
            {
                BoxCollider _C = Trigger as BoxCollider;

                var pos = _C.center;
                var sca = _C.size;


                if (!Application.isPlaying || Application.isPlaying && Trigger.enabled)
                {
                    Gizmos.DrawCube(pos, sca);


                    Gizmos.color = DColorFlat;
                    Gizmos.DrawWireCube(pos, sca);
                }

            }
            else if (Trigger is SphereCollider)
            {
                SphereCollider _C = Trigger as SphereCollider;

                if (!Application.isPlaying || Application.isPlaying && Trigger.enabled)
                {
                    Gizmos.DrawSphere(_C.center, _C.radius);
                    Gizmos.color = DColorFlat;
                    Gizmos.DrawWireSphere(_C.center, _C.radius);
                }
            }
#endif
        }

#endif

        public void Debugging(string value, Object obj, string m_color = "yellow")
        {
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"<B>[{name}]</B> → <color={m_color}> <B>[{value}]</B> </color>", obj);
#endif
        }
    }

    [System.Serializable]
    public struct DamagerProfile
    {
        public enum DamageProfileModif
        {
            Damage = 1,
            Interact = 2,
            Reaction = 4,
            CriticalChance = 8,
            Force = 16,
            ElementalDamage = 32,
            MissChance = 64
        }

        [Tooltip("Name of the Profile")]
        public string Name;

        [Flag]
        public DamageProfileModif modify;

        [Tooltip("Damager can activate interactables")]
        public BoolReference interact;
        [Tooltip("Interactor ID to enable with who interactable the Interactor can react")]
        public IntReference interactorID;

        [Tooltip("Damager allows the Damagee to apply an animal reaction")]
        public BoolReference react;

        [Tooltip("If true the Damage Receiver will not apply its Default Multiplier")]
        public BoolReference pureDamage;

        [Tooltip("Stat to modify on the Damagee")]
        [ContextMenuItem("Set Default Damage", "Set_DefaultDamage")]
        public StatModifier statModifier;

        [Tooltip("Critical Change (0 - 1)\n1 means it will be always critical")]
        public FloatReference m_cChance;

        [Tooltip("Miss Chance (0 - 1)\n1 means it will always Miss")]
        public FloatReference m_MissChance;

        [Tooltip("If the Damage is critical, the Stat modifier value will be multiplied by the Critical Multiplier")]
        public FloatReference cMultiplier;

        [SerializeField, Tooltip("MAX Force to Apply to RigidBodies when the Damager hit them")]
        public FloatReference maxForce;

        [SerializeField, Tooltip("MIN Force to Apply to RigidBodies when the Damager hit them")]
        public FloatReference minForce;

        [Tooltip("Force mode to apply to the Object that the Damager Hits")]
        public ForceMode forceMode;

        [Tooltip("This Gameobject will be enabled on Impact, if its a Prefab it will be instantiated")]
        public GameObjectReference m_HitEffect;

        [Tooltip("Don't use the Default Reaction of the Damageable Component")]
        [SerializeReference, SubclassSelector]
        public Reaction CustomReaction;

        [Tooltip("Type of element damage the Damager can do")]
        public StatElement element;


        public void Modify(MDamager damager)
        {
            if ((int)modify == 0) return; //Means that the animal have no modification

            if (Modify(DamageProfileModif.Damage))
            {
                damager.statModifier = new StatModifier(statModifier);
                damager.pureDamage = pureDamage;
            }

            if (Modify(DamageProfileModif.Interact))
            {
                damager.interact = interact;
                damager.interactorID = interactorID;
            }
            if (Modify(DamageProfileModif.Reaction))
            {
                damager.react = react;
                damager.CustomReaction = CustomReaction;
            }

            if (Modify(DamageProfileModif.CriticalChance))
            {
                damager.CriticalChance = m_cChance;
                damager.CriticalMultiplier = cMultiplier.Value;
            }
            if (Modify(DamageProfileModif.Force))
            {
                damager.MaxForce = maxForce;
                damager.MinForce = minForce;
            }

            if (Modify(DamageProfileModif.ElementalDamage))
            {
                damager.element = element;
            }
        }

        bool Modify(DamageProfileModif modifier) => ((modify & modifier) == modifier);

    }


    ///--------------------------------INSPECTOR-------------------
    ///
#if UNITY_EDITOR
    [CustomEditor(typeof(MDamager)), CanEditMultipleObjects]
    public class MDamagerEd : Editor
    {
        protected MDamager MD;
        protected SerializedProperty Force, minForce, forceMode, index, Tags,
            statModifier, onhit, OnAttackMissed,
            OnHitPosition, OnHitInteractable, OnProfileChanged, dontHitOwner, owner, m_Active, debug, m_audio,
            hitLayer, triggerInteraction, AttackDirection,
            m_cChance, cMultiplier, element, pureDamage, react, CustomReaction, interact, m_MissChance,
            m_HitEffect, HitEffects, hitSound,
            interactorID, DestroyHitEffect, Profiles,
            StopAnimator, AnimatorSpeed, AnimatorStopTime, animator;


        private ReorderableList Reo_Profiles;

        private void OnEnable() => FindBaseProperties();

        protected virtual void FindBaseProperties()
        {
            MD = (MDamager)target;
            index = serializedObject.FindProperty("index");
            hitSound = serializedObject.FindProperty("hitSound");

            m_HitEffect = serializedObject.FindProperty("m_HitEffect");
            HitEffects = serializedObject.FindProperty("hitEffects");
            OnHitPosition = serializedObject.FindProperty("OnHitPosition");
            m_Active = serializedObject.FindProperty("m_Active");
            hitLayer = serializedObject.FindProperty("m_hitLayer");
            triggerInteraction = serializedObject.FindProperty("triggerInteraction");
            dontHitOwner = serializedObject.FindProperty("dontHitOwner");


            AttackDirection = serializedObject.FindProperty("AttackDirection");
            owner = serializedObject.FindProperty("owner");
            interactorID = serializedObject.FindProperty("interactorID");
            DestroyHitEffect = serializedObject.FindProperty("DestroyHitEffect");
            element = serializedObject.FindProperty("element");

            Profiles = serializedObject.FindProperty("Profiles");

            react = serializedObject.FindProperty("react");
            CustomReaction = serializedObject.FindProperty("CustomReaction");

            interact = serializedObject.FindProperty("interact");
            pureDamage = serializedObject.FindProperty("pureDamage");

            m_cChance = serializedObject.FindProperty("m_cChance");
            cMultiplier = serializedObject.FindProperty("cMultiplier");

            m_MissChance = serializedObject.FindProperty("m_MissChance");

            minForce = serializedObject.FindProperty("minForce");
            Force = serializedObject.FindProperty("m_Force");
            forceMode = serializedObject.FindProperty("forceMode");
            Tags = serializedObject.FindProperty("Tags");

            statModifier = serializedObject.FindProperty("statModifier");

            onhit = serializedObject.FindProperty("OnHit");
            OnAttackMissed = serializedObject.FindProperty("OnAttackMissed");
            OnHitInteractable = serializedObject.FindProperty("OnHitInteractable");
            OnProfileChanged = serializedObject.FindProperty("OnProfileChanged");
            debug = serializedObject.FindProperty("debug");


            StopAnimator = serializedObject.FindProperty("StopAnimator");
            animator = serializedObject.FindProperty("animator");
            AnimatorSpeed = serializedObject.FindProperty("AnimatorSpeed");
            AnimatorStopTime = serializedObject.FindProperty("AnimatorStopTime");
            m_audio = serializedObject.FindProperty("m_audio");


            Reo_Profiles = new ReorderableList(serializedObject, Profiles, true, true, true, true)
            {
                onAddCallback = (list) =>
                {
                    var count = Profiles.arraySize;
                    Profiles.InsertArrayElementAtIndex(count);
                    var element = Profiles.GetArrayElementAtIndex(count);
                    serializedObject.ApplyModifiedProperties();

                    var newPro = MD.GetProfile();
                    newPro.Name = $"New Profile [{count + 1}]";
                    MD.Profiles[count] = newPro;
                    MTools.SetDirty(target);
                },
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "[Index]      [Damager Profiles]");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var elementRect = new Rect(rect);
                    elementRect.height = EditorGUIUtility.singleLineHeight;

                    var IndexRect = new Rect(elementRect);
                    var ModifyRect = new Rect(elementRect);
                    IndexRect.width = 25;
                    elementRect.x += 30;
                    elementRect.width = rect.width * 0.6f - 30;
                    ModifyRect.width = rect.width * 0.4f - 10;
                    ModifyRect.x += elementRect.width + 40;

                    var element = Profiles.GetArrayElementAtIndex(index);
                    var name = element.FindPropertyRelative("Name");
                    var modify = element.FindPropertyRelative("modify");

                    //draw the modify ss


                    elementRect.y += 2;
                    ModifyRect.y += 2;

                    EditorGUI.LabelField(IndexRect, $"[{index + 1}]");
                    EditorGUI.PropertyField(elementRect, name, GUIContent.none);

                    EditorGUIUtility.labelWidth = 40;
                    EditorGUI.PropertyField(ModifyRect, modify);
                    EditorGUIUtility.labelWidth = 0;
                }
            };
        }



        protected void DrawProfiles()
        {
            Reo_Profiles.DoLayoutList();

            if (Reo_Profiles.index != -1 && Reo_Profiles.index < Reo_Profiles.count)
            {
                var e = Profiles.GetArrayElementAtIndex(Reo_Profiles.index);


                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(e, false);
                    EditorGUI.indentLevel--;

                    if (e.isExpanded)
                    {
                        var modify = e.FindPropertyRelative("modify");

                        var m = (DamagerProfile.DamageProfileModif)modify.intValue;

                        if (((m & DamagerProfile.DamageProfileModif.Damage) == DamagerProfile.DamageProfileModif.Damage))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("statModifier"));
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("pureDamage"));
                            }
                        }

                        if (((m & DamagerProfile.DamageProfileModif.Interact) == DamagerProfile.DamageProfileModif.Interact))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("interact"));
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("interactorID"));
                            }
                        }

                        if (((m & DamagerProfile.DamageProfileModif.Reaction) == DamagerProfile.DamageProfileModif.Reaction))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("react"));
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("CustomReaction"));
                            }
                        }

                        if (((m & DamagerProfile.DamageProfileModif.CriticalChance) == DamagerProfile.DamageProfileModif.CriticalChance))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.PropertyField(e.FindPropertyRelative("m_cChance"), new GUIContent("Chance [0-1]"), GUILayout.MinWidth(50));
                                    EditorGUIUtility.labelWidth = 47;
                                    EditorGUILayout.PropertyField(e.FindPropertyRelative("cMultiplier"), new GUIContent("Mult"), GUILayout.MinWidth(50));
                                    EditorGUIUtility.labelWidth = 0;
                                }
                            }

                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("m_MissChance"));
                            }
                        }

                        if (((m & DamagerProfile.DamageProfileModif.Force) == DamagerProfile.DamageProfileModif.Force))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("minForce"));
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("maxForce"));
                            }
                        }

                        if (((m & DamagerProfile.DamageProfileModif.ElementalDamage) == DamagerProfile.DamageProfileModif.ElementalDamage))
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.LabelField("Elemental Damage", EditorStyles.boldLabel);
                                EditorGUILayout.PropertyField(e.FindPropertyRelative("element"));
                            }
                        }
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDescription("Damager Core Logic");
            // DrawScript();
            DrawGeneral();
            DrawPhysics();
            DrawCriticalDamage();
            DrawStatModifier();
            DrawMisc();
            DrawEvents();
            serializedObject.ApplyModifiedProperties();
        }

        protected void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(onhit);
                EditorGUILayout.PropertyField(OnAttackMissed);
                EditorGUILayout.PropertyField(OnHitPosition);
                EditorGUILayout.PropertyField(OnHitInteractable);
                EditorGUILayout.PropertyField(OnProfileChanged);
                DrawCustomEvents();
            }
        }

        protected virtual void DrawCustomEvents() { }


        protected virtual void DrawMisc(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            react.isExpanded = MalbersEditor.Foldout(react.isExpanded, "Interactions");
            if (react.isExpanded)
            {
                EditorGUILayout.PropertyField(react);

                if (MD.react.Value)
                    EditorGUILayout.PropertyField(CustomReaction);

                EditorGUILayout.PropertyField(interact);

                if (MD.interact.Value)
                    EditorGUILayout.PropertyField(interactorID);
            }

            HitEffects.isExpanded = MalbersEditor.Foldout(HitEffects.isExpanded, "Effects");

            if (HitEffects.isExpanded)
            {
                var p = " [Prefab]";
                if (MD.HitEffect == null
                    || !MD.HitEffect.IsPrefab()) p = "";

                EditorGUILayout.PropertyField(m_audio, new GUIContent("Audio Source"));
                EditorGUILayout.PropertyField(hitSound);

                EditorGUILayout.PropertyField(m_HitEffect, new GUIContent(m_HitEffect.displayName + p));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(HitEffects);
                EditorGUI.indentLevel--;

                EditorGUILayout.PropertyField(DestroyHitEffect);

                if (MD.HitEffect != null)
                {
                    //if (MD.HitEffect.IsPrefab())
                    //    EditorGUILayout.PropertyField(DestroyHitEffect);

                    EditorGUILayout.HelpBox(
                        MD.HitEffect.IsPrefab() ?
                        "The Hit Effect its a Prefab. The Effect will be instantiated as a child of the hitted collider, positioned and oriented using the hit position" :
                        "The Hit Effect its a NOT a Prefab. The Effect will be positioned and oriented using the hit position. It will be enabled and disabled",
                        MessageType.Info);
                }
            }
            //  EditorGUILayout.Space();

            AnimatorStopTime.isExpanded = MalbersEditor.Foldout(AnimatorStopTime.isExpanded, "Stop Animator");

            if (AnimatorStopTime.isExpanded)
            {
                EditorGUILayout.PropertyField(AnimatorStopTime);

                if (MD.AnimatorStopTime.Value > 0)
                {
                    EditorGUILayout.PropertyField(AnimatorSpeed);
                    EditorGUILayout.PropertyField(animator);
                }
            }
            if (drawbox) EditorGUILayout.EndVertical();
        }

        protected virtual void DrawGeneral(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_Active);
                MalbersEditor.DrawDebugIcon(debug);
            }


            EditorGUILayout.PropertyField(index);
            EditorGUILayout.PropertyField(hitLayer);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(Tags);
            EditorGUI.indentLevel--;
            EditorGUILayout.PropertyField(triggerInteraction);
            EditorGUILayout.PropertyField(AttackDirection);

            EditorGUILayout.PropertyField(dontHitOwner, new GUIContent("Don't hit Owner"));
            if (MD.dontHitOwner.Value)
            {
                EditorGUILayout.PropertyField(owner);
                // Debug.Log("MD = " + MD.Owner);
            }

            if (drawbox) EditorGUILayout.EndVertical();
        }

        protected virtual void DrawPhysics(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Force.isExpanded = MalbersEditor.Foldout(Force.isExpanded, "Physics Force");

                if (Force.isExpanded)
                {
                    EditorGUILayout.PropertyField(minForce, new GUIContent("Min Force", "Minimun Force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(Force, new GUIContent("Max Force", "Maximun Force to apply to a hitted rigid body"));
                    EditorGUILayout.PropertyField(forceMode);
                }
            }
            if (drawbox) EditorGUILayout.EndVertical();
        }


        protected virtual void DrawCriticalDamage(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Critical Damage", EditorStyles.boldLabel);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(m_cChance, new GUIContent("Chance [0-1]"), GUILayout.MinWidth(50));
                EditorGUIUtility.labelWidth = 47;
                EditorGUILayout.PropertyField(cMultiplier, new GUIContent("Mult"), GUILayout.MinWidth(50));
                EditorGUIUtility.labelWidth = 0;
            }


            EditorGUILayout.PropertyField(m_MissChance);

            if (drawbox) EditorGUILayout.EndVertical();
        }


        protected virtual void DrawStatModifier(bool drawbox = true)
        {
            if (drawbox) EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(statModifier,
                new GUIContent("Stat Modifier", "Which Stat will be affected on the Object to hit after Impact"), true);
            EditorGUILayout.PropertyField(pureDamage);

            DrawElement();

            if (drawbox) EditorGUILayout.EndVertical();
        }

        protected void DrawElement()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(new GUIContent("Element", "Type of Element this weapon can inflict"), GUILayout.Width(60));
                EditorGUILayout.PropertyField(element);
            }
        }


        protected void DrawDescription(string desc) => MalbersEditor.DrawDescription(desc);


        public bool Foldout(bool prop, string name)
        {
            EditorGUI.indentLevel++;
            prop = GUILayout.Toggle(prop, name, EditorStyles.foldoutHeader);
            EditorGUI.indentLevel--;
            return prop;
        }
    }
#endif
}