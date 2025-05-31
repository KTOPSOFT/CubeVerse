using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Reactions;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Effect Manager")]
    public class EffectManager : MonoBehaviour, IAnimatorListener
    {
        [RequiredField, Tooltip("Root Gameobject of the Hierarchy")]
        public Transform Owner;

        public List<Effect> Effects;

        public int SelectedEffect = -1;
        [SerializeField] private bool debug;
        [SerializeField] private bool editOffset;

        public Color debugColor = new(1, 0.6f, 0, 0.333f);

        private Effect Pin_Effect;

        private void Awake()
        {
            foreach (var e in Effects)
            {
                e.Initialize();
            }
        }

        private void OnDisable()
        {
            Stop_Effects(Effects);  //Stop all effects if the Effect Manager is disabled
        }


        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }


        public virtual void Effect_Pin(string name)
        {
            Pin_Effect = Effects.Find(effect => effect.Name == name && effect.active == true);
        }


        public virtual void Effect_Pin(int ID)
        {
            Pin_Effect = Effects.Find(effect => effect.ID == ID && effect.active == true);
        }


        public virtual void Effect_Pin_Root(Transform root)
        {
            Pin_Effect.root = root;
        }

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void PlayEffect(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);

            if (effects != null)
                foreach (var effect in effects) Play(effect);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(string name)
        {
            var effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);

            Stop_Effects(effects);
        }

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void StopEffect(int ID) => Effect_Stop(ID);

        /// <summary>Plays an Effect using its ID value</summary>
        public virtual void Effect_Play(int ID) => PlayEffect(ID);
        public virtual void EffectPlay(int ID) => PlayEffect(ID);
        public virtual void Effect_Play(string name) => PlayEffect(name);
        public virtual void EffectPlay(string name) => PlayEffect(name);

        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(int ID)
        {
            var effects = Effects.FindAll(effect => effect.ID == ID && effect.active == true);

            Stop_Effects(effects);
        }

        private void Stop_Effects(List<Effect> effects)
        {
            if (effects != null)
            {
                foreach (var e in effects)
                {
                    StopEffect(e, e.Instance);
                }
            }
        }

        public virtual void StopEffect(Effect e, GameObject instance)
        {
            //Stop the Effect only if is playing
            if (e.IsPlaying)
            {
                e.OnStopReaction?.React(Owner);
                e.OnStop.Invoke();

                e.IsPlaying = false;

                if (e.effect != null)
                {
                    if (!e.effect.IsPrefab())
                    {
                        if (e.disableOnStop)
                            instance?.SetActive(false);
                    }
                    else
                        Destroy(instance);

                }

                if (debug)
                    Debug.Log($"<B>{Owner.name}</B> Effect Stop: <B>[{e.Name}]</B>", (instance != null ? instance : this));
            }
        }


        /// <summary>Stops an Effect using its ID value</summary>
        public virtual void Effect_Stop(string name)
        {
            var effects = Effects.FindAll(effect => effect.Name == name && effect.active == true);
            Stop_Effects(effects);
        }


        protected virtual void Play(Effect e)
        {
            //e.Modifier?.PreStart(e);        //Execute the Method PreStart Effect if it has a modifier

            if (e.effect != null && e.IsPlaying) return; //Do not play a effect that is already playing

            //Delay an action
            this.Delay_Action(e.delay,
                () =>
                {
                    e.IsPlaying = true;

                    //Play Audio
                    if (!e.Clip.NullOrEmpty() && e.audioSource != null)
                    {
                        if (e.audioSource.isPlaying) e.audioSource.Stop();

                        e.Clip.Play(e.audioSource);
                    }

                    if (e.effect != null)
                    {
                        if (e.effect.IsPrefab())                        //If instantiate is active (meaning is a prefab)
                        {
                            e.Instance = Instantiate(e.effect);         //Instantiate!
                            e.Instance.SetActive(false);

                            e.Instance.transform.localScale *= e.scale;
                        }
                        else
                        {
                            e.Instance = e.effect;                     //Use the effect as the gameobject
                        }

                        if (Owner == null) Owner = transform.root;
                        if (e.Owner == null) e.Owner = Owner;  //Save in all effects that the owner of the effects is this transform


                        if (e.Instance)
                        {
                            //Apply Offsets
                            if (e.isChild)
                            {
                                e.Instance.transform.parent = e.root;
                                e.Offset.RestoreTransform(e.Instance.transform); //Restore the Offset
                            }
                            else
                            {
                                //Move to the root position
                                if (e.useRootPosition) e.Instance.transform.position = e.root.position;

                                //Orient to the root rotation
                                if (e.useRootRotation) e.Instance.transform.rotation = e.root.rotation;
                            }


                            e.Instance.SetActive(true);


                            if (e.effect.IsPrefab()) //get the trailrenderer and particle system from the Instance instead of the prefab
                            {
                                e.IsTrailRenderer = e.Instance.FindComponent<TrailRenderer>();
                                e.IsParticleSystem = e.Instance.FindComponent<ParticleSystem>();
                            }

                            if (e.IsTrailRenderer) e.IsTrailRenderer.Clear();
                            if (e.IsParticleSystem) e.IsParticleSystem.Play();
                        }
                    }

                    if (e.life > 0)
                    {
                        this.Delay_Action(e.life, () => StopEffect(e, e.Instance));
                    }

                    if (debug)
                        Debug.Log($"<B>{Owner.name}</B> Effect Play: <B>[{e.Name}]</B>", (e.Instance != null ? e.Instance : this));

                    e.OnPlay.Invoke();                 //Invoke the Play Event
                    e.OnPlayReaction?.React(Owner);    //Play the Reaction
                }
            );
        }


        /// <summary>IAnimatorListener function </summary>
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);

        //─────────────────────────────────CALLBACKS METHODS───────────────────────────────────────────────────────────────────

        /// <summary>Disables all effects using their name </summary>
        public virtual void Effect_Disable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }

        /// <summary> Disables all effects using their ID</summary>
        public virtual void Effect_Disable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = false;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }

        /// <summary>Enable all effects using their name</summary>
        public virtual void Effect_Enable(string name)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.Name.ToUpper() == name.ToUpper());

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the name: " + name + " was found");
            }
        }


        /// <summary> Enable all effects using their ID</summary>
        public virtual void Effect_Enable(int ID)
        {
            List<Effect> effects = Effects.FindAll(effect => effect.ID == ID);

            if (effects != null)
            {
                foreach (var e in effects) e.active = true;
            }
            else
            {
                Debug.LogWarning("No effect with the ID: " + ID + " was found");
            }
        }



#if UNITY_EDITOR
        private void Reset()
        {
            Owner = transform.root;
        }

        private void OnValidate()
        {
            if (Owner == null) Owner = transform.root;

            foreach (var e in Effects)
            {
                if (e.root == null) e.root = transform; //Make sure the owner is not null
            }
        }

        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener = gameObject.FindComponent<MEventListener>();

            if (listener == null) listener = gameObject.AddComponent<MEventListener>();
            if (listener.Events == null) listener.Events = new List<MEventItemListener>();

            MEvent effectEnable = MTools.GetInstance<MEvent>("Effect Enable");
            MEvent effectDisable = MTools.GetInstance<MEvent>("Effect Disable");

            if (listener.Events.Find(item => item.Event == effectEnable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectEnable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Enable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Enable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Enable</B> Added to the Event Listeners");
            }

            if (listener.Events.Find(item => item.Event == effectDisable) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = effectDisable,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, Effect_Disable);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, Effect_Disable);
                listener.Events.Add(item);

                Debug.Log("<B>Effect Disable</B> Added to the Event Listeners");
            }

            UnityEditor.EditorUtility.SetDirty(listener);
        }

        [SerializeField] private Mesh meshGizmo;
        [SerializeField] private Transform meshOwner;
        public void FindSelectedEffectMesh()
        {
            meshGizmo = null;
            meshOwner = transform;

            var Effect = Effects[SelectedEffect]; //Cache the selected Effect

            if (Effect.effect == null) return;

            var staticMesh = Effect.effect.GetComponentInChildren<MeshFilter>();

            if (staticMesh != null)
            {
                meshGizmo = staticMesh.sharedMesh;
                meshOwner = staticMesh.transform;
            }
            else
            {
                var skinnedMesh = Effect.effect.GetComponentInChildren<SkinnedMeshRenderer>();
                if (skinnedMesh != null)
                {
                    meshGizmo = skinnedMesh.sharedMesh;
                    meshOwner = skinnedMesh.transform;
                }
            }
            MTools.SetDirty(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) return;
            if (Selection.gameObjects.Length > 0 && Selection.gameObjects[0] != this.gameObject) return;

            // if (Application.isPlaying) return;
            if (meshOwner == null) meshOwner = transform;

            if (Effects != null)
            {
                SelectedEffect = Mathf.Clamp(SelectedEffect, 0, Effects.Count - 1);

                var Effect = Effects[SelectedEffect]; //Cache the selected Effect

                if (Effect != null)
                {
                    Gizmos.color = debugColor;

                    meshOwner.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);
                    var scale = meshOwner.localScale;

                    if (Effect.isChild)
                    {
                        position = Effect.root.TransformPoint(Effect.Offset.Position + meshOwner.localPosition);
                        rotation = Effect.root.rotation * Quaternion.Euler(Effect.Offset.Rotation) * meshOwner.rotation;
                        // rotation = meshOwner.rotation;
                        scale.Scale(Effect.Offset.Scale);
                    }
                    else
                    {
                        if (Effect.effect != null && !Effect.effect.IsPrefab())
                        {
                            position = meshOwner.position;
                            if (!Effect.useRootRotation) rotation = Effect.root.rotation * meshOwner.localRotation;
                            scale.Scale(Effect.effect.transform.localScale);
                        }
                        else
                        {
                            rotation = Effect.root.rotation * Quaternion.Euler(Effect.Offset.Rotation) * meshOwner.localRotation;
                        }
                    }

                    if (meshGizmo != null)
                    {
                        Gizmos.DrawWireMesh(meshGizmo, position, rotation, scale);
                    }
                    else
                    {
                        Matrix4x4 currentMatrix = Matrix4x4.TRS(position, rotation, scale);
                        Gizmos.matrix = currentMatrix;

                        Gizmos.DrawSphere(Vector3.zero, 0.2f);
                        Gizmos.DrawWireSphere(Vector3.zero, 0.2f);
                    }
                }

            }
        }
#endif
    }

    [System.Serializable]
    public class Effect
    {
        public string Name = "EffectName";
        public int ID;
        public bool active = true;
        [RequiredField] public Transform root;

        public bool isChild;
        public bool disableOnStop = true;
        public bool useRootPosition = true;
        public bool useRootRotation = true;
        /// <summary>
        /// Prefab or GameObject to Instantiate
        /// </summary>
        public GameObject effect;
        public TransformOffset Offset = new(1);
        public AudioSource audioSource;
        public AudioClipReference Clip;

        /// <summary>Life of the Effect</summary>
        [Min(0)] public float life = 10f;

        /// <summary>Delay Time to execute the effect after is called.</summary>
        [Min(0)] public float delay;
        public float scale = 1f;

        ///// <summary>Scriptable Object to Modify anything you want before, during or after the effect is invoked</summary>
        //public EffectModifier Modifier;


        [SerializeReference, SubclassSelector]
        public Reaction OnPlayReaction;
        [SerializeReference, SubclassSelector]
        public Reaction OnStopReaction;

        public UnityEvent OnPlay;
        public UnityEvent OnStop;

        /// <summary>Returns the Owner of the Effect </summary>
        public Transform Owner { get; set; }

        /// <summary>  The Effect is playing. Use this to skip double playing the same effect /summary>
        public bool IsPlaying { get; set; }

        /// <summary>Returns the Instance of the Effect Prefab </summary>
        public GameObject Instance { get => instance; set => instance = value; }

        public TrailRenderer IsTrailRenderer { get; set; }
        public ParticleSystem IsParticleSystem { get; set; }

        [System.NonSerialized]
        private GameObject instance;

        internal void Initialize()
        {
            if (effect != null && !effect.IsPrefab()) //Store if the effect its not a prefab
            {
                effect.gameObject.SetActive(false); //Deactivate at start
                IsTrailRenderer = effect.FindComponent<TrailRenderer>();
                IsParticleSystem = effect.FindComponent<ParticleSystem>();
            }
        }
    }

    /// ---------------------------------------------------

    #region INSPECTOR
#if UNITY_EDITOR

    [CustomEditor(typeof(EffectManager))]
    public class EffectManagerEditor : Editor
    {
        private ReorderableList list;
        private SerializedProperty EffectList, Owner, SelectedEffect, DebugColor, debug, editOffset;
        private EffectManager M;

        private static GUIContent editicon;
        public static GUIContent EditIcon
        {
            get
            {
                editicon ??= new GUIContent(EditorGUIUtility.IconContent("TransformTool"))
                {
                    tooltip = "Edit Offset in ViewPort"
                };
                return editicon;
            }
        }


        private void OnEnable()
        {
            M = ((EffectManager)target);
            Owner = serializedObject.FindProperty("Owner");
            debug = serializedObject.FindProperty("debug");
            editOffset = serializedObject.FindProperty("editOffset");
            EffectList = serializedObject.FindProperty("Effects");
            SelectedEffect = serializedObject.FindProperty("SelectedEffect");
            DebugColor = serializedObject.FindProperty("debugColor");

            list = new ReorderableList(serializedObject, EffectList, true, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = HeaderCallbackDelegate,
                onAddCallback = OnAddCallBack,
                onSelectCallback = (list) =>
                {
                    SelectedEffect.intValue = list.index;
                    serializedObject.ApplyModifiedProperties();
                    M.FindSelectedEffectMesh();
                }
            };

            list.index = SelectedEffect.intValue;
            M.FindSelectedEffectMesh();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Manage all the Effects using the function (PlayEffect(int ID))");

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(Owner);
                editOffset.boolValue = GUILayout.Toggle(editOffset.boolValue, EditIcon, EditorStyles.miniButtonMid, GUILayout.Width(28), GUILayout.Height(20));
                EditorGUILayout.PropertyField(DebugColor, GUIContent.none, GUILayout.Width(50));
                MalbersEditor.DrawDebugIcon(debug);
            }


            list.DoLayoutList();

            if (list.index != -1)
            {
                Effect effect = M.Effects[list.index];

                EditorGUILayout.Space(-16);
                SerializedProperty Element = EffectList.GetArrayElementAtIndex(list.index);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(Element, new GUIContent($"[{effect.Name}]"), false);
                EditorGUI.indentLevel--;

                if (Element.isExpanded)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var eff = Element.FindPropertyRelative("effect");
                        eff.isExpanded = MalbersEditor.Foldout(eff.isExpanded, "General");
                        if (eff.isExpanded)
                        {

                            string prefabTooltip = "";

                            var is_Prefab = false;

                            if (eff.objectReferenceValue != null)
                                is_Prefab = (eff.objectReferenceValue as GameObject).IsPrefab();

                            if (eff.objectReferenceValue != null && is_Prefab)
                            {
                                prefabTooltip = "[Prefab]";
                            }
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("effect"), new GUIContent("Effect " + prefabTooltip, "The Prefab or gameobject which holds the Effect(Particles, transforms)"));

                            if (Application.isPlaying && is_Prefab)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                    EditorGUILayout.ObjectField("Effect Instance", effect.Instance, typeof(GameObject), false);
                            }
                            if (is_Prefab)
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("scale"), new GUIContent("Scale", "Scale the Prefab object"));


                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("life"), new GUIContent("Life", "Duration of the Effect. The Effect will be destroyed after the Life time has passed"));

                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("delay"), new GUIContent("Delay", "Time before playing the Effect"));


                            if (eff.objectReferenceValue != null && !(eff.objectReferenceValue as GameObject).IsPrefab())
                                EditorGUILayout.PropertyField(Element.FindPropertyRelative("disableOnStop"), new GUIContent("Disable On Stop", "if the Effect is not a prefab the gameOBject will be disabled"));

                            if (Element.FindPropertyRelative("life").floatValue <= 0)
                            {
                                EditorGUILayout.HelpBox("Life = 0  the effect will not be destroyed by this Script", MessageType.Info);
                            }
                        }
                    }


                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var audio = Element.FindPropertyRelative("audioSource");
                        audio.isExpanded = MalbersEditor.Foldout(audio.isExpanded, "Audio");

                        if (audio.isExpanded)
                        {
                            EditorGUILayout.PropertyField(audio,
                                new GUIContent("Source", "Where the audio for the Effect will be player"));
                            EditorGUILayout.PropertyField(Element.FindPropertyRelative("Clip"),
                               new GUIContent("Clip", "What audio will be played"));
                        }
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var root = Element.FindPropertyRelative("root");
                        root.isExpanded = MalbersEditor.Foldout(root.isExpanded, "Location & Orientation");

                        if (root.isExpanded)
                        {
                            EditorGUILayout.PropertyField(root, new GUIContent("Root", "Uses this transform to position the Effect"));


                            if (root.objectReferenceValue != null)
                            {
                                var isChild = Element.FindPropertyRelative("isChild");
                                var useRootPosition = Element.FindPropertyRelative("useRootPosition");
                                var useRootRotation = Element.FindPropertyRelative("useRootRotation");

                                using (new EditorGUI.DisabledGroupScope(isChild.boolValue))
                                {
                                    EditorGUILayout.PropertyField(useRootPosition, new GUIContent("Use Root Position", "Set the Effect location as the root positino."));
                                    EditorGUILayout.PropertyField(useRootRotation, new GUIContent("Use Root Rotation", "Orient the Effect using the root rotation."));
                                }

                                EditorGUILayout.PropertyField(isChild, new GUIContent("Is Child", "Set the Effect as a child of the Root transform"));

                                if (isChild.boolValue)
                                {
                                    var Offset = Element.FindPropertyRelative("Offset");
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(Offset, true);
                                    EditorGUI.indentLevel--;
                                }

                            }
                        }
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var OnPlayReaction = Element.FindPropertyRelative("OnPlayReaction");
                        var OnStopReaction = Element.FindPropertyRelative("OnStopReaction");
                        OnPlayReaction.isExpanded = MalbersEditor.Foldout(OnPlayReaction.isExpanded, "Reactions");

                        if (OnPlayReaction.isExpanded)
                        {

                            EditorGUILayout.PropertyField(OnPlayReaction);
                            EditorGUILayout.PropertyField(OnStopReaction);
                        }
                    }


                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        Owner.isExpanded = MalbersEditor.Foldout(Owner.isExpanded, "Events");

                        if (Owner.isExpanded)
                        {
                            var OnStop = Element.FindPropertyRelative("OnStop");
                            var OnPlay = Element.FindPropertyRelative("OnPlay");

                            EditorGUILayout.PropertyField(OnPlay);
                            EditorGUILayout.PropertyField(OnStop);
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        private void OnSceneGUI()
        {
            if (!editOffset.boolValue) return;

            if (M.Effects != null)
            {
                var Effect = M.Effects[SelectedEffect.intValue];

                if (Effect != null)
                {
                    if (Tools.current == Tool.Move)
                    {
                        if (Effect.isChild)
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                Vector3 piv = Effect.root.TransformPoint(Effect.Offset.Position);
                                Vector3 NewPivPosition = Handles.PositionHandle(piv, Quaternion.identity);

                                if (cc.changed)
                                {
                                    Undo.RecordObject(target, "Change Pos Offest");
                                    Effect.Offset.Position = Effect.root.InverseTransformPoint(NewPivPosition);
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        }
                        else
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                if (!Effect.effect.IsPrefab())
                                {
                                    Vector3 piv = Effect.effect.transform.position;
                                    Vector3 NewPivPosition = Handles.PositionHandle(piv, Quaternion.identity);

                                    if (cc.changed)
                                    {
                                        Undo.RecordObject(Effect.effect.transform, "Change Pos Offest");
                                        Effect.effect.transform.position = NewPivPosition;
                                        EditorUtility.SetDirty(Effect.effect.transform);
                                    }
                                }
                            }
                        }
                    }
                    else if (Tools.current == Tool.Rotate)
                    {
                        if (Effect.isChild)
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                Vector3 piv = Effect.root.TransformPoint(Effect.Offset.Position);
                                Quaternion NewPivRotation = Handles.RotationHandle(Effect.root.rotation * Quaternion.Euler(Effect.Offset.Rotation), piv);

                                if (cc.changed)
                                {
                                    Undo.RecordObject(target, "Change Rot Offest");
                                    Effect.Offset.Rotation = (Quaternion.Inverse(Effect.root.rotation) * NewPivRotation).eulerAngles;
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        }
                        else
                        {

                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                if (!Effect.effect.IsPrefab())
                                {
                                    Vector3 piv = Effect.effect.transform.position;
                                    Quaternion NewPivRotation = Handles.RotationHandle(Effect.effect.transform.rotation, piv);

                                    if (cc.changed)
                                    {
                                        Undo.RecordObject(Effect.effect.transform, "Change Rot Offest");
                                        Effect.effect.transform.rotation = NewPivRotation;
                                        EditorUtility.SetDirty(Effect.effect.transform);
                                    }
                                }
                            }
                        }
                    }
                    else if (Tools.current == Tool.Scale)
                    {
                        if (Effect.isChild)
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                Vector3 piv = Effect.root.TransformPoint(Effect.Offset.Position);
                                Vector3 NewPivScale = Handles.ScaleHandle(Effect.Offset.Scale, piv, Effect.root.rotation, HandleUtility.GetHandleSize(piv));

                                if (cc.changed)
                                {
                                    Undo.RecordObject(target, "Change Scale Offest");
                                    Effect.Offset.Scale = NewPivScale;
                                    EditorUtility.SetDirty(target);
                                }
                            }
                        }
                        else
                        {
                            using (var cc = new EditorGUI.ChangeCheckScope())
                            {
                                if (!Effect.effect.IsPrefab())
                                {
                                    Vector3 piv = Effect.effect.transform.position;
                                    Vector3 NewPivScale = Handles.ScaleHandle(Effect.effect.transform.localScale, piv, Effect.effect.transform.rotation, HandleUtility.GetHandleSize(piv));

                                    if (cc.changed)
                                    {
                                        Undo.RecordObject(Effect.effect.transform, "Change Scale Offest");
                                        Effect.effect.transform.localScale = NewPivScale;
                                        EditorUtility.SetDirty(Effect.effect.transform);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void HeaderCallbackDelegate(Rect rect)
        {
            Rect R_1 = new Rect(rect.x + 14, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 14 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(R_1, "Effect List", EditorStyles.miniLabel);
            EditorGUI.LabelField(R_2, "ID", EditorStyles.centeredGreyMiniLabel);
        }

        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = EffectList.GetArrayElementAtIndex(index);

            var e_active = element.FindPropertyRelative("active");
            var e_Name = element.FindPropertyRelative("Name");
            var e_ID = element.FindPropertyRelative("ID");

            rect.y += 2;

            Rect R_0 = new Rect(rect.x, rect.y, 15, EditorGUIUtility.singleLineHeight);
            Rect R_1 = new Rect(rect.x + 16, rect.y, (rect.width - 10) / 2, EditorGUIUtility.singleLineHeight);
            Rect R_2 = new Rect(rect.x + 16 + ((rect.width - 30) / 2), rect.y, rect.width - ((rect.width) / 2), EditorGUIUtility.singleLineHeight);

            e_active.boolValue = EditorGUI.Toggle(R_0, e_active.boolValue);
            e_Name.stringValue = EditorGUI.TextField(R_1, e_Name.stringValue, EditorStyles.label);
            e_ID.intValue = EditorGUI.IntField(R_2, e_ID.intValue);
        }

        void OnAddCallBack(ReorderableList list)
        {
            M.Effects ??= new System.Collections.Generic.List<Effect>();
            M.Effects.Add(new Effect());
        }
    }
#endif
    #endregion
}