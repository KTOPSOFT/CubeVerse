using MalbersAnimations.Reactions;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [DisallowMultipleComponent]
    /// <summary> Damager Receiver</summary>
    [AddComponentMenu("Malbers/Damage/MDamageable")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/mdamageable")]
    public class MDamageable : MonoBehaviour, IMDamage
    {
        [Tooltip("Animal Reaction to apply when the damage is done")]
        public Component character;

        [Tooltip("Animal Reaction to apply when the damage is done")]
        [SerializeReference, SubclassSelector]
        public Reaction reaction;

        [Tooltip("Animal Reaction when it receives a critical damage")]
        [SerializeReference, SubclassSelector]
        public Reaction criticalReaction;

        [Tooltip("Reaction sent to the Damager if it hits this Damageable")]
        [SerializeReference, SubclassSelector]
        public Reaction damagerReaction;

        [Tooltip("Type of surface the Damageable is. (Flesh, Metal, Wood,etc)")]
        public SurfaceID surface;
        public Transform Transform => transform;

        [Tooltip("The Damageable will ignore the Reaction coming from the Damager. Use this when this Damager Needs to have the Default Reaction")]
        [SerializeField] private BoolReference ignoreDamagerReaction = new();

        [Tooltip("Stats component to apply the Damage")]
        public Stats stats;

        [Tooltip("Multiplier for the Stat modifier Value. Use this to increase or decrease the final value of the Stat")]
        public FloatReference multiplier = new(1);

        [Tooltip("When Enabled the animal will rotate towards the Damage direction")]
        public BoolReference AlignToDamage = new();

        [Tooltip("Only Align to Damage when Movement is Not Detected")]
        public BoolReference OnlyOnMovementZero = new(true);

        [Tooltip("Time to align to the damage direction")]
        public FloatReference AlignTime = new(0.25f);
        [Tooltip("Aligmend curve")]
        public AnimationCurve AlignCurve = new(MTools.DefaultCurve);

        [Tooltip("Point Forward to align the animal to the Damage, It will rotate around this point")]
        public FloatReference AlignOffset = new();

        public MDamageable Root;
        public DamagerEvents events;

        public Vector3 HitDirection { get; set; }
        public Vector3 HitPosition { get; set; }
        public GameObject Damager { get; set; }
        public Collider HitCollider { get; set; }
        public ForceMode LastForceMode { get; set; }
        public SurfaceID Surface => CurrentProfile.surface;
        public GameObject Damagee => gameObject;

        public bool IgnoreDamagerReaction { get => ignoreDamagerReaction; set => ignoreDamagerReaction.Value = value; }

        public DamageData LastDamage;

        [Tooltip("Elements that affect the MDamageable")]
        public List<ElementMultiplier> elements = new();

        public MDamageableProfile Default;
        public MDamageableProfile CurrentProfile;


        [Tooltip("The Damageable can Change profiles to Change the way the Animal React to the Damage")]
        public List<MDamageableProfile> profiles = new();




        [HideInInspector] public int Editor_Tabs1;

        private ICharacterMove characterMove;

        protected void Start()
        {
            if (stats != null)
            {
                if (character == null && reaction != null)
                {
                    character = stats.GetComponent(reaction.ReactionType); //Find the character where the Stats are
                }
                else
                {
                    character = stats.transform;
                }
            }

            //Store the default values
            Default = new MDamageableProfile("Default", surface,
                reaction, criticalReaction, damagerReaction, ignoreDamagerReaction,
                multiplier, AlignToDamage, elements);

            Default.OnProfileEnter = events.OnProfileDefaultEnter;
            Default.OnProfileExit = events.OnProfileDefaultExit;
            Default.OnProfileEnter?.Invoke();

            CurrentProfile = Default;

            profiles ??= new();

            //Check if we have a Character Move in the Animal
            if (character != null)
                characterMove = character.GetComponent<ICharacterMove>();
        }

        protected void OnDisable()
        {
            StopAllCoroutines();
        }


        /// <summary> Restore the Default Damageable profile </summary>
        public virtual void Profile_Restore()
        {
            CurrentProfile.OnProfileExit?.Invoke();
            CurrentProfile = Default;
            CurrentProfile.OnProfileEnter?.Invoke();
        }

        //public virtual MDamageableProfile GetCurrentProfile() => CurrentProfile;


        public virtual void Profile_Set(string name)
        {
            if (string.IsNullOrEmpty(name) || name.ToLower() == "default")
            {
                Profile_Restore();
            }
            else
            {
                var index = profiles.FindIndex(p => p.name == name);

                if (index != -1)
                {
                    CurrentProfile.OnProfileExit?.Invoke();
                    CurrentProfile = profiles[index];
                    CurrentProfile.OnProfileEnter?.Invoke();
                }
            }
        }



        //-*********************************************************************--
        /// <summary>  Main Receive Damage Method!!! </summary>
        /// <param name="Direction">The Direction the Damage is coming from</param>
        /// <param name="Damager">Game Object doing the Damage</param>
        /// <param name="damage">Stat Modifier containing the Stat ID, what to modify and the Value to modify</param>
        /// <param name="isCritical">is the Damage Critical?</param>
        /// <param name="react">Does the Damage that is coming has a Custom Reaction? </param>
        /// <param name="customReaction">The Attacker Brings a custom Reaction to override the Default one</param>
        /// <param name="pureDamage">Pure damage means that the multipliers wont be applied</param>
        /// <param name="element"></param>
        public virtual void ReceiveDamage(Vector3 Direction, Vector3 Position, GameObject Damager, StatModifier damage,
            bool isCritical, bool react, Reaction customReaction, bool pureDamage, StatElement element)
        {
            if (!enabled) return;       //This makes the Animal Immortal.
            HitDirection = Direction;   //Store the Last Direction
            HitPosition = Position;   //Store the Last Position

            var stat = stats.Stat_Get(damage.ID);
            if (stat == null || !stat.Active || stat.IsEmpty || stat.IsImmune) return; //Do nothing if the stat is empty, null or disabled

            ReactionLogic(isCritical, react, customReaction);

            ElementMultiplier statElement = new(element, 1);

            //Apply the Element Multiplier
            if (element != null && CurrentProfile.elements.Count > 0)
            {
                statElement = CurrentProfile.elements.Find(x => element.ID == x.element.ID);

                if (statElement.multiplier != null)
                {
                    damage.Value *= statElement.multiplier;
                    events.OnElementDamage.Invoke(statElement.element.ID);
                    if (Root) Root.events.OnElementDamage.Invoke(statElement.element.ID);
                }
            }

            SetDamageable(Direction, Damager);
            if (Root) Root.SetDamageable(Direction, Damager);                     //Send the Direction and Damager to the Root 


            //Store the last damage applied to the Damageable
            LastDamage = new DamageData(Damager, gameObject, damage, isCritical, statElement);
            if (Root) Root.LastDamage = LastDamage;


            if (isCritical)
            {
                events.OnCriticalDamage.Invoke();
                if (Root) Root.events.OnCriticalDamage.Invoke();
            }

            if (!pureDamage)
                damage.Value *= CurrentProfile.multiplier;               //Apply to the Stat modifier a new Modification

            events.OnReceivingDamage.Invoke(damage.Value);
            events.OnDamager.Invoke(Damager);

            //Send the Events on the Root
            if (Root)
            {
                Root.events.OnReceivingDamage.Invoke(damage.Value);
                Root.events.OnDamager.Invoke(Damager);
            }
            damage.ModifyStat(stat);

            //Invoke if the Stat is empty..
            if (stat.IsEmpty) { events.OnStatEmpty.Invoke(stat.ID); }

            AlignmentLogic(Damager);
        }

        protected virtual void AlignmentLogic(GameObject Damager)
        {
            if (CurrentProfile.AlignToDamage.Value)
            {
                if (OnlyOnMovementZero.Value && characterMove != null && characterMove.MovementDetected) return; //Do not Align if the Animal is moving

                AlignToDamageDirection(Damager);
            }
        }

        protected virtual void ReactionLogic(bool isCritical, bool react, Reaction customReaction)
        {
            if (react)
            {
                //Custom reaction if the Attacker sends one and Ignore Damager is False
                if (customReaction != null && !IgnoreDamagerReaction)
                {
                    if (!customReaction.TryReact(character)) //if there's no valid reaction then use the default one
                    {
                        DoReaction(isCritical);
                    }
                }
                else
                {
                    DoReaction(isCritical);
                }
            }

            //Make the Damager react to the Damageable
            if (Damager) CurrentProfile.DamagerReaction?.React(Damager);
        }

        protected virtual void DoReaction(bool isCritical)
        {
            if (isCritical)
                CurrentProfile.criticalReaction?.React(character);     //if the damage is Critical then react with the critical reaction instead
            else
                CurrentProfile.reaction?.React(character);    //React Default
        }

        protected virtual void AlignToDamageDirection(GameObject Direction)
        {

            if (isActiveAndEnabled && !Direction.IsDestroyed())
            {
                StopAllCoroutines();
                StartCoroutine(MTools.AlignLookAtTransform(
                    character.transform, Direction.transform.position, AlignOffset, AlignTime.Value,
                    stats.transform.localScale.y, AlignCurve));
            }
        }

        /// <summary>  Receive Damage from external sources simplified </summary>
        /// <param name="stat"> What stat will be modified</param>
        /// <param name="amount"> value to substact to the stat</param>
        public virtual void ReceiveDamage(StatID stat, float amount)
        {
            var modifier = new StatModifier() { ID = stat, modify = StatOption.SubstractValue, Value = amount };
            ReceiveDamage(Vector3.forward, Vector3.forward, null, modifier, false, true, null, false, null);
        }

        /// <summary>  Receive Damage from external sources simplified </summary>
        /// <param name="stat"> What stat will be modified</param>
        /// <param name="amount"> value to substact to the stat</param>
        public virtual void ReceiveDamage(StatID stat, float amount, StatOption modifyStat = StatOption.SubstractValue)
        {
            var modifier = new StatModifier() { ID = stat, modify = modifyStat, Value = amount };
            ReceiveDamage(Vector3.forward, transform.position, null, modifier, false, true, null, false, null);
        }



        /// <summary>  Receive Damage from external sources simplified </summary>
        /// <param name="Direction">Where the Damage is coming from</param>
        /// <param name="Damager">Who is doing the Damage</param>
        /// <param name="modifier">What Stat will be modified</param>
        /// <param name="modifyStat">Type of modification applied to the stat</param>
        /// <param name="isCritical">is the Damage Critical?</param>
        /// <param name="react">Does Apply the Default Reaction?</param>
        /// <param name="pureDamage">if is pure Damage, do not apply the default multiplier</param>
        /// <param name="stat"> What stat will be modified</param>
        /// <param name="amount"> value to substact to the stat</param>
        public virtual void ReceiveDamage(Vector3 Direction, GameObject Damager, StatID stat, float amount, StatOption modifyStat = StatOption.SubstractValue,
             bool isCritical = false, bool react = true, Reaction customReaction = null, bool pureDamage = false, StatElement element = null)
        {
            var modifier = new StatModifier() { ID = stat, modify = modifyStat, Value = amount };
            ReceiveDamage(Direction, transform.position, Damager, modifier, isCritical, react, customReaction, pureDamage, element);
        }


        /// <summary>  Receive Damage from external sources simplified </summary>
        /// <param name="Direction">Where the Damage is coming from</param>
        /// <param name="Damager">Who is doing the Damage</param>
        /// <param name="modifier">What Stat will be modified</param>
        /// <param name="isCritical">is the Damage Critical?</param>
        /// <param name="react">Does Apply the Default Reaction?</param>
        /// <param name="pureDamage">if is pure Damage, do not apply the default multiplier</param>
        /// <param name="stat"> What stat will be modified</param>
        /// <param name="amount"> value to substact to the stat</param>
        public virtual void ReceiveDamage(Vector3 Direction, GameObject Damager, StatID stat,
            float amount, bool isCritical = false, bool react = true, Reaction customReaction = null, bool pureDamage = false)
        {
            var modifier = new StatModifier() { ID = stat, modify = StatOption.SubstractValue, Value = amount };
            ReceiveDamage(Direction, transform.position, Damager, modifier, isCritical, react, customReaction, pureDamage, null);
        }


        /// <summary>  Receive Damage from external sources simplified </summary>
        /// <param name="Direction">Where the Damage is coming from</param>
        /// <param name="Damager">Who is doing the Damage</param>
        /// <param name="modifier">What Stat will be modified</param>
        /// <param name="isCritical">is the Damage Critical?</param>
        /// <param name="react">Does Apply the Default Reaction?</param>
        /// <param name="pureDamage">if is pure Damage, do not apply the default multiplier</param>
        /// <param name="stat"> What stat will be modified</param>
        /// <param name="amount"> value to substact to the stat</param>
        public virtual void ReceiveDamage(Vector3 Direction, GameObject Damager, StatModifier damage,
        bool isCritical, bool react, Reaction customReaction, bool pureDamage) =>
         ReceiveDamage(Direction, transform.position, Damager, damage, isCritical, react, customReaction, pureDamage, null);

        /// <summary>  Fill the Local Values of the MDamageable  </summary>
        internal void SetDamageable(Vector3 Direction, GameObject Damager)
        {
            HitDirection = Direction;
            this.Damager = Damager;
        }

        [System.Serializable]
        public class DamagerEvents
        {
            public FloatEvent OnReceivingDamage = new();
            public UnityEvent OnCriticalDamage = new();
            public GameObjectEvent OnDamager = new();
            public IntEvent OnElementDamage = new();
            public IntEvent OnStatEmpty = new();
            public UnityEvent OnProfileDefaultEnter = new();
            public UnityEvent OnProfileDefaultExit = new();
        }

        public struct DamageData
        {
            /// <summary> Who is doing the damage </summary>
            public GameObject Damager;
            /// <summary>Who received the damage </summary>
            public GameObject Damagee;
            /// <summary> Stat Modifier with the updated final stat values</summary>
            public StatModifier stat;


            /// <summary> Final value who modified the Stat</summary>
            public readonly float Damage => stat.modify != StatOption.None ? stat.Value : 0f;

            /// <summary>Store if the Damage was Critical</summary>
            public bool WasCritical;

            /// <summary>Store if the damage was  </summary>
            public ElementMultiplier Element;

            public DamageData(GameObject damager, GameObject damagee, StatModifier stat, bool wasCritical, ElementMultiplier element)
            {
                Damager = damager;
                Damagee = damagee;
                this.stat = new StatModifier(stat);
                WasCritical = wasCritical;
                Element = element;
            }
        }


#if UNITY_EDITOR
        private void Reset()
        {
            // reaction = MTools.GetInstance<ModeReaction>("Damaged");

            reaction = new ModeReaction() { ID = MTools.GetInstance<ModeID>("Damage"), };

            criticalReaction = new ModeReaction() { ID = MTools.GetInstance<ModeID>("Damage"), };

            surface = MTools.GetInstance<SurfaceID>("Flesh");

            stats = this.FindComponent<Stats>();
            Root = transform.FindComponent<MDamageable>();     //Check if there's a Damageable on the Root
            if (Root == this) Root = null;


            //Add Stats if it not exist
            if (stats == null) stats = gameObject.AddComponent<Stats>();

            profiles = new List<MDamageableProfile>();
        }



        private void OnDrawGizmosSelected()
        {
            if (AlignOffset != 0)
            {
                Vector3 Offset = transform.position + AlignOffset * transform.localScale.y * transform.forward; //Use Offset

                Gizmos.color = Color.white;
                Gizmos.DrawSphere(Offset, 0.075f);
            }
        }
#endif
    }


    [System.Serializable]
    public struct MDamageableProfile
    {
        [Tooltip("Name of the Profile. This is used for Setting a New Damageable Profile. E.g. When the Animal is blocking or Parrying")]
        public string name;

        [Tooltip("Type of surface the Damageable is. (Flesh, Metal, Wood,etc)")]
        public SurfaceID surface;

        [Tooltip("Animal Reaction to apply when the damage is done")]
        [SerializeReference, SubclassSelector]
        public Reaction reaction;

        [Tooltip("Animal Reaction when it receives a critical damage")]
        [SerializeReference, SubclassSelector]
        public Reaction criticalReaction;

        [Tooltip("Animal Reaction to apply when the damage is done")]
        [SerializeReference, SubclassSelector]
        public Reaction DamagerReaction;

        [Tooltip("Multiplier for the Stat modifier Value. Use this to increase or decrease the final value of the Stat")]
        public FloatReference multiplier;

        [Tooltip("When Enabled the animal will rotate towards the Damage direction")]
        public BoolReference AlignToDamage;

        public BoolReference ignoreDamagerReaction;

        [Tooltip("Elements that affect the MDamageable")]
        public List<ElementMultiplier> elements;

        public UnityEvent OnProfileEnter;
        public UnityEvent OnProfileExit;


        public MDamageableProfile(string Name, SurfaceID surface,
            Reaction reaction, Reaction criticalReaction, Reaction DamagerReaction, BoolReference ignoreDamagerReaction,
            FloatReference multiplier, BoolReference AlignToDamage, List<ElementMultiplier> elements)
        {
            this.name = Name;
            this.surface = surface;
            this.reaction = reaction;
            this.DamagerReaction = DamagerReaction;
            this.multiplier = multiplier;
            this.criticalReaction = criticalReaction;
            this.AlignToDamage = AlignToDamage;
            this.elements = elements;
            this.ignoreDamagerReaction = ignoreDamagerReaction;
            OnProfileEnter = new();
            OnProfileExit = new();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MDamageable))]
    public class MDamageableEditor : Editor
    {
        SerializedProperty reaction, damagerReaction, criticalReaction, surface,
            stats,
            multiplier, ignoreDamagerReaction, events, Root, OnlyOnMovementZero,
            // OnProfileDefaultEnter, OnProfileDefaultExit,
            AlignTime, AlignCurve, AlignToDamage, AlignOffset,
            Editor_Tabs1, elements, profiles;
        MDamageable M;

        protected string[] Tabs1 = new string[] { "General", "Profiles", "Events" };


        GUIContent plus;

        private void OnEnable()
        {
            M = (MDamageable)target;

            reaction = serializedObject.FindProperty("reaction");
            criticalReaction = serializedObject.FindProperty("criticalReaction");
            damagerReaction = serializedObject.FindProperty("damagerReaction");
            stats = serializedObject.FindProperty("stats");
            multiplier = serializedObject.FindProperty("multiplier");

            events = serializedObject.FindProperty("events");
            //OnProfileDefaultEnter = serializedObject.FindProperty("OnProfileDefaultEnter");
            //OnProfileDefaultExit = serializedObject.FindProperty("OnProfileDefaultExit");


            Root = serializedObject.FindProperty("Root");
            OnlyOnMovementZero = serializedObject.FindProperty("OnlyOnMovementZero");

            AlignToDamage = serializedObject.FindProperty("AlignToDamage");
            AlignCurve = serializedObject.FindProperty("AlignCurve");
            AlignTime = serializedObject.FindProperty("AlignTime");
            AlignOffset = serializedObject.FindProperty("AlignOffset");


            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            elements = serializedObject.FindProperty("elements");
            profiles = serializedObject.FindProperty("profiles");
            surface = serializedObject.FindProperty("surface");
            ignoreDamagerReaction = serializedObject.FindProperty("ignoreDamagerReaction");

            if (plus == null) plus = UnityEditor.EditorGUIUtility.IconContent("d_Toolbar Plus");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Connects the Damage received to the Stat and Animal components");


            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            switch (Editor_Tabs1.intValue)
            {
                case 0: DrawGeneral(); break;
                case 1: DrawProfiles(); break;
                case 2: DrawEvents(); break;
                default: break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProfiles()
        {
            EditorGUILayout.PropertyField(profiles, true);
        }

        private void DrawGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(stats);

                if (M.transform.parent != null)
                    EditorGUILayout.PropertyField(Root);

                EditorGUILayout.PropertyField(surface);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(reaction, new GUIContent("Default Reaction"));
                EditorGUILayout.PropertyField(criticalReaction);
                EditorGUILayout.PropertyField(damagerReaction);
                EditorGUI.indentLevel--;
                EditorGUILayout.PropertyField(ignoreDamagerReaction);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(multiplier);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(elements);
                EditorGUI.indentLevel--;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(AlignToDamage);

                if (M.AlignToDamage.Value)
                {
                    EditorGUILayout.PropertyField(OnlyOnMovementZero);

                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(AlignTime);
                        EditorGUILayout.PropertyField(AlignCurve, GUIContent.none, GUILayout.MaxWidth(75));
                    }
                    EditorGUILayout.PropertyField(AlignOffset);
                }
            }
        }


        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(events, true);
                EditorGUI.indentLevel--;

            }
        }
    }
#endif
}