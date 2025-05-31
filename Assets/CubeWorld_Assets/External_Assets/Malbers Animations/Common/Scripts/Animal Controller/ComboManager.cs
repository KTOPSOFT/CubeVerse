using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    [DefaultExecutionOrder(3000)]
    [AddComponentMenu("Malbers/Animal Controller/Combo Manager")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/combo-manager")]
    public class ComboManager : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;

        public int Branch = 0;
        public List<Combo> combos = new();

        private List<Combo> extraStateCombos;

        [Tooltip("Current Active Combo Index")]
        public IntReference ActiveComboIndex = new(0);

        /// <summary>Current Active Combo</summary>
        public Combo ActiveCombo { get; internal set; }

        [Tooltip("Disable Combo Manager if the animal is Sleep.")]
        public bool DisableOnSleep = true;

        public int ActiveComboSequenceIndex { get; internal set; }

        public ComboSequence ActiveComboSequence => ActiveCombo.CurrentSequence;

        /// <summary> Is the manager playing a combo? </summary>
        public bool PlayingCombo { get; internal set; }

        public bool debug;

        private void OnValidate()
        {
            ActiveComboIndex = Mathf.Clamp(ActiveComboIndex.Value, -1, combos.Count - 1);
        }

        private void OnEnable()
        {
            if (!animal) animal = this.FindComponent<MAnimal>();

            ActiveCombo = null; //Reset the Active Combo

            if (!animal)
            {
                Debug.LogWarning("The Combo Manager needs an Animal Component", gameObject);
            }
            else
            {
                animal.OnModeEnd.AddListener(OnModeEnd);
                animal.OnStateActivate.AddListener(OnStateEnter);


                //Save the Modes in the Combo itself
                for (int i = 0; i < combos.Count; i++)
                {
                    var combo = combos[i];
                    combo.ComboIndex = i;
                    combo.FinishTime = -combo.CoolDown;
                    combo.CachedMode = animal.Mode_Get(combo.Mode);

                    if (combo.CachedMode == null)
                        Debug.LogError($"Animal {animal.name} does not have the mode {combo.Mode.name}. Please Add it to your animal", this);
                }

                if (ActiveComboIndex >= 0)
                {
                    ActiveCombo = combos[ActiveComboIndex];
                    ActiveComboSequenceIndex = 0;
                    PlayingCombo = false;
                    RestartActiveCombo();
                }
            }
        }
        private void OnDisable()
        {
            animal.OnStateActivate.RemoveListener(OnStateEnter);
            animal.OnModeEnd.RemoveListener(OnModeEnd);

            StopAllCoroutines();
        }

        private void OnStateEnter(int stateID)
        {
            //Do nothing if we do not have a state to search
            if (DisableOnSleep
                || !enabled
                || ActiveCombo == null
                || ActiveComboIndex == -1
               ) return;

            var state = animal.ActiveStateID;

            if (extraStateCombos != null)
            {
                foreach (var combo in extraStateCombos)
                {
                    if (combo == ActiveCombo && combo.HasState(state))
                    {
                        break; //Find yourself and you were the first
                    }
                    if (combo.HasState(state))
                    {
                        SetActiveCombo(ActiveCombo.Mode); //Find the Other Mode 
                        break;
                    }
                }
            }
        }

        private void OnModeEnd(int modeID, int CurrentExitAbility)
        {
            if (PlayingCombo)
            {
                if (ActiveComboSequence == null) { Restart(); return; } //Weird bug

                if (ActiveComboSequence.Finisher)
                {
                    ActiveCombo.OnComboFinished.Invoke(ActiveComboSequenceIndex);
                    MDebug($"Combo Finished. <b>[{ActiveComboSequenceIndex}]</b> Branch:<b>[{Branch}]</b>. [Restarting]");
                    Restart();
                    ActiveCombo.FinishTime = Time.time; //cache the time the combo has finished.

                    if (ActiveCombo.DisableOnFinish.Value)
                    {
                        ActiveCombo.active = false;
                    }
                }
                //Are we exiting the Current Secuence or just the Old one??? A new Secuence is playing
                else if (CurrentExitAbility == ActiveComboSequence.Ability)
                {
                    if (!animal.IsPlayingMode) // if is no longer playing a Mode then means it was interrupted or Unfinished
                    {
                        MDebug($"Incomplete <b>[{ActiveComboSequenceIndex}]</b> Branch: <b>[{Branch}]</b>. [Restarting*]");
                        ActiveCombo.OnComboInterrupted.Invoke(ActiveComboSequenceIndex);

                        if (ActiveCombo.DisableOnInterrupt) ActiveCombo.active = false;

                        Restart();//meaning it got to the end of the combo
                        ActiveCombo.FinishTime = Time.time; //cache the time the combo has finished/Interrupted.
                    }
                }
            }
        }

        /// <summary> Changes the Active Combo on the Manager  </summary>
        public virtual void SetActiveCombo(int index)
        {
            ActiveComboIndex = index;

            RestartActiveCombo();

            if (ActiveComboIndex < 0) //minus 1 means ignore playing combos
            {
                MDebug($"Combo Manager Disabled. No combo set for activation.-1");
                selectedComboEditor = -1;
                ActiveCombo = null;
                return;
            }

            ActiveCombo = combos[ActiveComboIndex];

            MDebug($"Set Active Combo [{ActiveCombo.Name},Index: {index}]");

            //find all other combos with the same mode
            extraStateCombos = combos.FindAll(x => x.Mode == ActiveCombo.Mode);



            selectedComboEditor = ActiveComboIndex;
        }

        /// <summary> Changes the Active Combo on the Manager using the Mode ID </summary>
        public virtual void SetActiveCombo(ModeID ComboMode)
        {
            if (ComboMode == null)
            {
                SetActiveCombo(-1);  //meaning that we are disabling all combos
            }
            else
            {
                //if (ActiveCombo != null && ActiveCombo.Mode == ComboMode) { return; } //do nothing if you are already trying to activate the same combo

                PlayingCombo = false;
                RestartActiveCombo();

                int index = combos.FindIndex(x => x.Mode == ComboMode && x.HasState(animal.ActiveStateID));
                SetActiveCombo(index);
            }
        }

        public virtual void SetActiveCombo(IntVar index) => SetActiveCombo(index.Value);

        public virtual void SetActiveCombo(string ComboName)
        {
            int index = combos.FindIndex(x => x.Name == ComboName);
            SetActiveCombo(index);
        }

        public virtual void Play() => TryPlay(Branch);

        public virtual bool TryPlay() => TryPlay(Branch);
        public virtual void Play(int branch) => TryPlay(Branch = branch);

        ///// <summary>  Play Combo Sequence  <param name="branch"></param>
        //public virtual void Play(int[] branch)
        //{

        //}


        public virtual bool TryPlay(int branch)
        {
            if (!gameObject.activeInHierarchy ||
                (DisableOnSleep && animal.Sleep) ||  //if animal is sleep or
                !enabled ||     //  the component disabled
                animal.LockInput || animal.ActiveState == null ||
                ActiveComboIndex < 0 ||
                animal.InTimeline //If is a timeline do nothing
                )
            {
                MDebug($"[Failed] Animal Disabled|Lock");

                return false; //Active combo is minus 1 ... ignore playing combos
            }



            if ((DisableOnSleep && animal.Sleep) ||  //if animal is sleep or
               !enabled ||     //  the component disabled
               animal.LockInput ||
               ActiveComboIndex < 0)
            {
                MDebug($"[Failed] Animal Disabled|Lock");

                return false; //Active combo is minus 1 ... ignore playing combos
            }

            if (animal.IsPreparingMode) return true; //Bug when the buttons are played at the same time

            //Means is not Playing any mode so Restart
            //if (!animal.IsPlayingMode)
            if (!animal.IsPlayingMode)
                Restart();

            Branch = branch;

            if (ActiveCombo != null)
            {
                if (animal.IsPlayingMode && animal.ActiveMode.IgnoreLowerModes && ActiveCombo.CachedMode.Priority < animal.ActiveMode.Priority && animal.ActiveMode.InCoolDown)
                {
                    MDebug($"{ActiveCombo.Name} - [Active Mode does not allow playing the combo]");
                    return false; //Do not Play a Combo if the current mode has higher priority
                }


                if (!ActiveCombo.active)
                {
                    MDebug($"{ActiveCombo.Name} - [Disabled]");
                    return false; //Do not Play a Combo if its disabled
                }

                if (ActiveCombo.InCoolDown)
                {
                    MDebug($"{ActiveCombo.Name} - [In CoolDown]");
                    return false; //Do not Play a Combo if its in cooldown
                }
                //Debug.Log("ActiveCombo.Play = " + ActiveCombo);

                return ActiveCombo.Play(this);
            }
            return false;
        }

        public virtual void SetBranch(int branch)
        {
            Branch = branch;
        }

        public virtual void Restart()
        {
            ActiveComboSequenceIndex = 0;
            PlayingCombo = false;
            RestartActiveCombo();
            MDebug("Restart");
        }

        public virtual void Combo_Enable(int index)
        {
            if (index >= 0 && index < combos.Count)
            {
                combos[index].active = true;
            }
        }

        public virtual void Combo_Disable(int index)
        {
            if (index >= 0 && index < combos.Count)
            {
                combos[index].active = false;
            }
        }

        public virtual void Combo_Enable(string name)
        {
            var combo = combos.Find(x => x.Name == name);
            if (combo != null)
            {
                combo.active = true;
            }
        }

        public virtual void Combo_Disable(string name)
        {
            var combo = combos.Find(x => x.Name == name);

            if (combo != null)
            {
                combo.active = false;
            }
        }


        private void RestartActiveCombo()
        {
            if (ActiveCombo != null)
            {
                ActiveCombo.CurrentSequence = null;  //Clean the current combo secuence
                ActiveCombo.ActiveSequenceIndex = -1;  //Clean the current combo secuence
                foreach (var seq in ActiveCombo.Sequence)
                {
                    seq.Used = false; //Set that the secuenced is used to 
                }
            }
        }


        public void ResetCoolDown(string name)
        {
            var combo = combos.Find(x => x.Name == name);
            combo?.ResetCoolDown();
        }

        internal void MDebug(string value)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<b><color=orange>[{animal.name}] - [Combo - {(ActiveCombo != null ? ActiveCombo.Name : "NULL")}] - {value}</color></b>", this);
#endif
        }

        [HideInInspector] public int selectedComboEditor = -1;

        internal Combo GetCombo(ModeID weaponID)
        {
            return combos.Find(x => x.Mode == weaponID);
        }


        private void Reset()
        {
            animal = this.FindComponent<MAnimal>();
        }
    }

    [System.Serializable]
    public class Combo
    {
        public bool active = true;
        public ModeID Mode;
        public string Name = "Combo1";
        public Mode CachedMode;

        [Tooltip("After the Combo is Finished, With a finisher it cannot play again until the cooldown has passed")]
        public FloatReference CoolDown = new();

        [Tooltip("States the Combo can be played")]
        public List<StateID> states = new();

        [Tooltip("Disable the Combo after it has been completed to the last sequencer (Finisher)")]
        public BoolReference DisableOnFinish = new();
        [Tooltip("Disable the Combo if the sequence was not completed")]
        public BoolReference DisableOnInterrupt = new();


        public float FinishTime;

        public bool InCoolDown => CoolDown > 0 && (Time.time - FinishTime) <= CoolDown;

        public List<ComboSequence> Sequence = new();
        public ComboSequence CurrentSequence { get; internal set; }

        /// <summary> Current Index on the list to search combos. This is used to avoid searching already used Sequences on the list</summary>
        public int ActiveSequenceIndex { get; internal set; }
        //{
        //    get => atvs;
        //    internal set
        //    {
        //        atvs = value;
        //         Debug.Log($"[{Name}] - ActiveSequenceIndex [{value} ]");
        //    }
        //}
        //int atvs;

        /// <summary> Resets the Cooldown by giving a possible Finish time value to compare in the InCooldown property </summary>
        public void ResetCoolDown() => FinishTime = Time.time - CoolDown * 2;

        /// <summary>  Returns if the list state contains a given state   </summary>
        public bool HasState(StateID activeState)
        {
            return states == null || states.Count == 0 || states.Contains(activeState);
        }

        public int ComboIndex { get; internal set; }

        public IntEvent OnComboFinished = new();
        public IntEvent OnComboInterrupted = new();

        public bool Play(ComboManager M)
        {
            //Do nothing because there's a sequence waiting to be activated
            if (M.ActiveComboSequence != null && M.ActiveComboSequence.Buffer)
            {
                // Debug.Log("BUFFERING");
                return false;
            }

            var animal = M.animal;
            if (animal.IsPreparingMode) return false;

            ActiveSequenceIndex = Mathf.Clamp(ActiveSequenceIndex, 0, Sequence.Count - 1); //Clamp the Active sequence problem

            //If the Animal is not Playing a Mode means that there's no Combo Player
            if (!animal.IsPlayingMode ||
                (animal.ActiveMode != CachedMode)) //OR the Mode currently playing is different.... try to activate the Combo in the normal way
            {
                for (int i = 0; i < Sequence.Count; i++)
                {
                    var Starter = Sequence[i];

                    if (!Starter.Used && Starter.Branch == M.Branch && Starter.PreviewsAbility == 0) //Only Start with Started Abilities
                    {
                        // Debug.Log($"CachedMode {CachedMode.Name} + Starter.Ability [{Starter.Ability}]");

                        if (CachedMode.TryActivate(Starter.Ability))
                        {
                            // M.MDebug($"Success! ({CachedMode.Name}) Playing");

                            M.PlayingCombo = true;
                            PlaySequence(M, Starter);
                            ActiveSequenceIndex = i; //Finding which is the active secuence index;

                            if (CachedMode.ActiveAbility.Status == AbilityStatus.Charged)
                            {
                                CachedMode.InputValue = true; //Make sure the Input is set to true for charged modes?
                            }

                            return true;
                        }
                        else
                        {
                            M.MDebug($"Try Activate First Sequence ({CachedMode.Name}) Failed. Check Mode Conditions");
                            return false;
                        }
                    }
                }
            }
            //Check if we are playing the same mode or if the current mode has lower priority
            else //if (animal.ActiveMode == CachedMode) 
            {
                var ModeTime = animal.ModeTime;

                var sequence = Sequence[ActiveSequenceIndex];

                //If we are on a Finisher Secuence Ignore!! This will allow to finish the combo
                if (sequence.Finisher)
                {
                    //Use this to restart the combo at the end of a Finisher
                    if (sequence.Restarter && sequence.FinisherTime < (ModeTime))
                    {
                        OnComboFinished.Invoke(ActiveSequenceIndex);



                        M.MDebug($"Combo Finished -<RESTARTING>-. <b>[{ActiveSequenceIndex}]</b> Branch:<b>[{M.Branch}]</b>. [Restarting]");
                        M.Restart();

                        //RESTART CLEAN
                        for (int i = 0; i < Sequence.Count; i++)
                        {
                            var Starter = Sequence[i];

                            if (!Starter.Used && Starter.Branch == M.Branch && Starter.PreviewsAbility == 0) //Only Start with Started Abilities
                            {
                                // Debug.Log($"CachedMode {CachedMode.Name} + Starter.Ability [{Starter.Ability}]");

                                if (CachedMode.ForceActivate(Starter.Ability))
                                {
                                    // M.MDebug($"Success! ({CachedMode.Name}) Playing");

                                    M.PlayingCombo = true;
                                    PlaySequence(M, Starter);
                                    ActiveSequenceIndex = i; //Finding which is the active secuence index;
                                    return true;
                                }
                                else
                                {
                                    M.MDebug($"Try Activate First Sequence ({CachedMode.Name}) Failed. Check Mode Conditions");
                                    return false;
                                }
                            }
                        }
                    }
                    return true;
                }
                else
                    for (int i = ActiveSequenceIndex + 1; i < Sequence.Count; i++) //Search from the next one
                    {
                        var seq = Sequence[i];

                        if (!seq.Used && seq.Branch == M.Branch && seq.PreviewsAbility != 0 && seq.PreviewsAbility == CachedMode.AbilityIndex)
                        {
                            //Play the nex animation on the sequence if is not buffered
                            if (seq.Activation.IsInRange(ModeTime))
                            {
                                if (ModeTime > seq.ActivationTime)
                                {
                                    animal.Mode_ForceActivate(Mode, seq.Ability);
                                    PlaySequence(M, seq);
                                    ActiveSequenceIndex = i; //Finding which is the active secuence index;
                                }
                                else
                                {
                                    if (!seq.Buffer)
                                    {
                                        seq.Buffer = true;  //The Combo will definetely play after the Mode Time has passed

                                        M.MDebug($"Sequence <b>[{i}]</b> [**Buffering**] - Branch:<b>[{M.Branch}]</b>. [{animal.ModeTime:F2}]");

                                        //Stop the sequence if other was playing
                                        if (C_WaitBuffer != null) M.StopCoroutine(C_WaitBuffer);
                                        C_WaitBuffer = WaitForBuffer(M, seq, i);

                                        //Wait for the correct animatino time
                                        M.StartCoroutine(C_WaitBuffer);
                                    }
                                }
                            }
                            else
                            {
                                //M.MDebug($"Sequence [{ActiveSequenceIndex}]: <b>[{M.ActiveComboSequenceIndex}]</b> - Branch:<b>[{M.Branch}]</b>. " +
                                //    $"NOT IN TIME RANGE YET [{animal.ModeTime}]");
                            }
                            return true;
                        }
                        //else
                        //{
                        //    Debug.Log("IGNORE BRANCH!!!");
                        //}
                    }
            }

            //  Debug.Log("FALSE?!??!?!?");

            return false;
        }

        private void PlaySequence(ComboManager M, ComboSequence sequence)
        {
            CurrentSequence = sequence; //Store the current sequence
            CurrentSequence.Used = true;


            M.ActiveComboSequenceIndex = Mode.ID * 1000 + sequence.Ability;
            CurrentSequence.OnSequencePlay.Invoke(M.ActiveComboSequenceIndex);

            M.MDebug($"Sequence [{ActiveSequenceIndex}]: <b>[{M.ActiveComboSequenceIndex}]</b> - Branch:<b>[{M.Branch}]</b>. Time: {M.animal.ModeTime:F2}");
        }


        private IEnumerator WaitForBuffer(ComboManager M, ComboSequence seq, int Index)
        {
            yield return new WaitUntil(() => M.animal.ModeTime > seq.ActivationTime);
            seq.Buffer = false;

            M.animal.Mode_ForceActivate(Mode, seq.Ability); //Play the mode!!

            //  M.MDebug($"Sequence Buffered!" );
            // Debug.Log($"Sequence Buffered Played!" );
            PlaySequence(M, seq);
            ActiveSequenceIndex = Index; //Finding which is the active secuence index;

        }

        private IEnumerator C_WaitBuffer;

    }


    [System.Serializable]
    public class ComboSequence
    {
        [MinMaxRange(0, 1)]
        [Tooltip("Buffer Input Activation time for activating the next Sequence")]
        public RangedFloat Activation = new(0.3f, 0.6f);

        [Range(0, 1)]
        [Tooltip("Normalized time on the animation to activate the next ability if the animation reached this normalize time and the Sequence has been buffered.")]
        public float ActivationTime = 0.5f;

        [Tooltip("Ability needed to activate the next Sequence")]
        public int PreviewsAbility = 0;

        [Tooltip("Name of the Ability that will be used to activate the next Sequence")]
        public string previewAbilityName;

        /// <summary>Next Ability on the Sequence</summary>
        public int Ability = 0;

        [Tooltip("Name of the Next Ability to activate")]
        public string nextAbilityName;


        [Tooltip("Branch used on the combo sequence")]
        public int Branch = 0;

        [Tooltip("Is this Secuence a Finisher Combo?")]
        public bool Finisher;
        [Tooltip("Is the sequence a Restarter if is a finisher?")]
        public bool Restarter;
        [Tooltip("Restarter Finisher needs to pass this time to finish")]
        public float FinisherTime = 0.5f;
        public IntEvent OnSequencePlay = new();


        /// <summary> The Sequence has been used already </summary>
        public bool Used { get; set; }

        /// <summary> The Sequence is waiting for the activation time  </summary>
        public bool Buffer { get; set; }

        public void Reset()
        {
            Buffer = false;
            Used = false;
        }

    }

    #region INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(ComboManager))]

    public class ComboEditor : Editor
    {
        public static GUIStyle StyleGray => MTools.Style(new Color(0.5f, 0.5f, 0.5f, 0.3f));
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));
        public static GUIStyle StyleGreen => MTools.Style(new Color(0f, 1f, 0.5f, 0.3f));
        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        private int branch, prev, current;

        SerializedProperty Branch, combos, animal, selectedComboEditor, debug, DisableOnSleep,
            ActiveComboIndex;
        private readonly Dictionary<string, ReorderableList> SequenceReordable = new();
        private ReorderableList CombosReor;

        private ComboManager M;
        private int SelectedAbilityIndex;
        private readonly int IndexAbility;

        private string activeComboName;

        private void OnEnable()
        {
            M = (ComboManager)target;

            animal = serializedObject.FindProperty("animal");
            combos = serializedObject.FindProperty("combos");

            Branch = serializedObject.FindProperty("Branch");
            DisableOnSleep = serializedObject.FindProperty("DisableOnSleep");

            selectedComboEditor = serializedObject.FindProperty("selectedComboEditor");
            debug = serializedObject.FindProperty("debug");
            ActiveComboIndex = serializedObject.FindProperty("ActiveComboIndex");
            DrawComboList();
        }

        GUIContent con;

        private void DrawComboList()
        {
            CombosReor = new ReorderableList(serializedObject, combos, true, true, true, true)
            {
                drawHeaderCallback = (rect) =>
                {
                    float half = rect.width / 2;
                    var IDIndex = new Rect(rect.x, rect.y, 45, EditorGUIUtility.singleLineHeight);
                    var IDName = new Rect(rect.x + 45, rect.y, half - 15 - 45, EditorGUIUtility.singleLineHeight);
                    var IDRect = new Rect(rect.x + half + 10, rect.y, half - 10, EditorGUIUtility.singleLineHeight);

                    EditorGUI.LabelField(IDIndex, "Index");
                    EditorGUI.LabelField(IDName, " Name");
                    EditorGUI.LabelField(IDRect, "  Mode");
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = combos.GetArrayElementAtIndex(index);
                    var Mode = element.FindPropertyRelative("Mode");
                    var Name = element.FindPropertyRelative("Name");
                    var states = element.FindPropertyRelative("states");
                    var active = element.FindPropertyRelative("active");

                    rect.y += 2;

                    float half = rect.width / 2;

                    var IDActive = new Rect(rect.x, rect.y, 25, EditorGUIUtility.singleLineHeight);
                    var IDIndex = new Rect(rect.x + 20, rect.y, 25, EditorGUIUtility.singleLineHeight);
                    var IDName = new Rect(rect.x + 25 + 20, rect.y, half - 15 - 25 - 20, EditorGUIUtility.singleLineHeight);
                    var IDRect = new Rect(rect.x + half + 10, rect.y, half - 10, EditorGUIUtility.singleLineHeight);
                    var IDRectActive = new Rect(rect.x + half - 15 - 25 - 30, rect.y, 60, EditorGUIUtility.singleLineHeight);

                    var oldColor = GUI.color;

                    if (index == selectedComboEditor.intValue)
                        GUI.color = MTools.MBlue * 3;

                    EditorGUI.PropertyField(IDActive, active, GUIContent.none);
                    EditorGUI.LabelField(IDIndex, "(" + index.ToString() + ")");
                    EditorGUI.PropertyField(IDName, Name, GUIContent.none);

                    if (states.arraySize > 0)
                    {
                        IDRect.width -= 20;

                        var UpRect = new Rect(rect.x + rect.width - 15, rect.y, 25, EditorGUIUtility.singleLineHeight);

                        if (con == null)
                        {
                            var img_State = EditorGUIUtility.ObjectContent(CreateInstance<StateID>(), typeof(StateID)).image;

                            con = new(img_State);
                            //con = EditorGUIUtility.IconContent("d_P4_OutOfSync");
                            con.tooltip = "Combos with states limitation must be higher in the list";
                        }

                        EditorGUI.LabelField(UpRect, con);
                    }

                    if (index == M.ActiveComboIndex)
                    {
                        EditorGUI.LabelField(IDRectActive, "(Active)");
                        activeComboName = Name.stringValue;
                    }

                    EditorGUI.PropertyField(IDRect, Mode, GUIContent.none);

                    GUI.color = oldColor;
                },


                onSelectCallback = (list) => selectedComboEditor.intValue = list.index,

                onRemoveCallback = (list) =>
                {
                    // The reference value must be null in order for the element to be removed from the SerializedProperty array.
                    combos.DeleteArrayElementAtIndex(list.index);
                    list.index -= 1;

                    if (list.index == -1 && combos.arraySize > 0) list.index = 0;   //In Case you remove the first one

                    selectedComboEditor.intValue--;

                    list.index = Mathf.Clamp(list.index, 0, list.index - 1);

                    EditorUtility.SetDirty(target);
                }
            };
        }

        private void DrawSequence(int ComboIndex, SerializedProperty combo, SerializedProperty sequence)
        {
            ReorderableList Reo_AbilityList;
            string listKey = combo.propertyPath;

            if (popupStyle == null)
            {
                popupStyle = (GUI.skin.GetStyle("PaneOptions"));
                popupStyle.imagePosition = ImagePosition.ImageOnly;
            }

            if (SequenceReordable.ContainsKey(listKey))
            {
                Reo_AbilityList = SequenceReordable[listKey]; // fetch the reorderable list in dict
            }
            else
            {
                Reo_AbilityList = new ReorderableList(combo.serializedObject, sequence, true, true, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;

                        var Height = EditorGUIUtility.singleLineHeight;
                        var element = sequence.GetArrayElementAtIndex(index);

                        //var Activation = element.FindPropertyRelative("Activation");
                        var PreviewsAbility = element.FindPropertyRelative("PreviewsAbility");
                        var previewAbilityName = element.FindPropertyRelative("previewAbilityName");

                        var Ability = element.FindPropertyRelative("Ability");
                        var abilityName = element.FindPropertyRelative("nextAbilityName");

                        var Branch = element.FindPropertyRelative("Branch");

                        var Activation = element.FindPropertyRelative("Activation");
                        var finisher = element.FindPropertyRelative("Finisher");
                        var FinisherTime = element.FindPropertyRelative("FinisherTime");
                        var Restarter = element.FindPropertyRelative("Restarter");
                        var ActivationTime = element.FindPropertyRelative("ActivationTime");

                        var IDRect = new Rect(rect) { height = Height };
                        float wid = rect.width / 2;

                        var IRWidth = 30f;
                        var Sep = -10f;
                        var Offset = 40f;

                        float xx = IRWidth + Offset;

                        var HasRestarter = finisher.boolValue;

                        var IndexRect = new Rect(IDRect) { width = IRWidth };
                        var BranchRect = new Rect(IDRect) { x = xx, width = 45 };


                        var PrevARect = new Rect(IDRect) { x = 75 + xx + Sep + 5, width = wid - 15 - Sep - 20 - 45 };

                        var PrevARectName = new Rect(PrevARect);
                        PrevARectName.x += 30;
                        PrevARectName.width -= 30;

                        var AbilityRect = new Rect(IDRect) { x = wid + xx + Sep + 35, width = wid - Sep - 60 - 15 - (HasRestarter ? 30 : 0) };
                        var AbilityRectName = new Rect(AbilityRect);
                        AbilityRectName.x += 30;
                        AbilityRectName.width -= 30;

                        if (HasRestarter && Restarter.boolValue) AbilityRect.width -= 28;

                        var FinisherRect = new Rect(IDRect) { x = IDRect.width + 35, width = 20 };

                        var RestarterRect = new Rect(IDRect) { x = IDRect.width + 10, width = 20 };

                        var FinisherTimeRect = new Rect(IDRect) { x = IDRect.width - 23, width = 30 };


                        var ActivationRect = new Rect(rect) { height = Height, width = rect.width - 17 };
                        ActivationRect.y += Height + 2;
                        ActivationRect.width = (rect.width / 3) * 2;


                        //NEXT SEQUENCE TIME!!!
                        var RActivationTime = new Rect(rect) { height = Height, width = rect.width - 17 };
                        RActivationTime.y += Height + 2;
                        RActivationTime.width = (rect.width / 3);
                        RActivationTime.x = (rect.width / 3) * 2 + 50;


                        var style = new GUIStyle(EditorStyles.label);

                        if (Application.isPlaying && !M.ActiveCombo.Sequence[index].Used)
                            style.normal.textColor = Color.green; //If the Combo is not used turn the combos to Green


                        EditorGUI.LabelField(IndexRect, "(" + index.ToString() + ")", style);
                        var oldCColor = GUI.contentColor;
                        var oldColor = GUI.color;

                        if (PreviewsAbility.intValue <= 0)
                        {
                            GUI.contentColor = Color.green;
                            finisher.boolValue = false; //FINISHER COMBOS CANNOT BE STARTERS
                        }

                        if (finisher.boolValue) GUI.contentColor = Color.cyan;

                        if (Application.isPlaying && M.ActiveComboIndex == ComboIndex)
                        {
                            if (M.ActiveCombo != null)
                            {
                                var Index = M.ActiveCombo.ActiveSequenceIndex;

                                if (Index == index) //Paint Active Index
                                {
                                    GUI.contentColor =
                                    GUI.color = Color.yellow;

                                    if (M.ActiveComboSequence != null && M.ActiveComboSequence.Finisher) //Paint finisher
                                    {
                                        GUI.contentColor =
                                        GUI.color = (Color.red + Color.yellow) / 2;
                                    }
                                }
                                else if (Index > index)  //Paint Used Index
                                {
                                    GUI.contentColor =
                                    GUI.color = Color.gray;
                                }
                            }
                        }

                        EditorGUI.PropertyField(BranchRect, Branch, GUIContent.none);

                        // Calculate rect for configuration first button
                        Rect PrevbuttonRect = new(PrevARect);
                        PrevbuttonRect.yMin += popupStyle.margin.top;
                        PrevbuttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
                        PrevbuttonRect.x -= 20;
                        PrevbuttonRect.height = EditorGUIUtility.singleLineHeight;


                        // Calculate rect for configuration first button
                        Rect NextbuttonRect = new(AbilityRect);
                        NextbuttonRect.yMin += popupStyle.margin.top;
                        NextbuttonRect.width = popupStyle.fixedWidth + popupStyle.margin.right;
                        NextbuttonRect.x -= 20;
                        NextbuttonRect.height = EditorGUIUtility.singleLineHeight;


                        int result = -1;
                        var popupOptions = new List<string>();
                        var AbilitiesIndex = new List<int>();

                        popupOptions.Add("[0] Combo Starter");
                        AbilitiesIndex.Add(0);


                        if (IndexAbility == index)
                        {
                            rect.height = Height * 2;
                        }

                        if (M.animal != null)
                        {
                            var Mode = M.animal.Mode_Get(M.combos[ComboIndex].Mode);

                            if (Mode != null && Mode.Abilities != null && Mode.Abilities.Count > 0)
                            {
                                foreach (var item in Mode.Abilities)
                                {
                                    popupOptions.Add("[" + item.Index.Value + "] " + item.Name);
                                    AbilitiesIndex.Add(item.Index.Value);
                                }

                                result = EditorGUI.Popup(PrevbuttonRect, result, popupOptions.ToArray(), popupStyle);

                                if (result != -1)
                                {
                                    PreviewsAbility.intValue = AbilitiesIndex[result];

                                    if (PreviewsAbility.intValue <= 0)
                                        previewAbilityName.stringValue = "Combo Starter";
                                    else
                                        previewAbilityName.stringValue = Mode.Abilities[result - 1].Name;
                                }

                                popupOptions = new List<string>();
                                AbilitiesIndex = new();

                                foreach (var item in Mode.Abilities)
                                {
                                    popupOptions.Add("[" + item.Index.Value + "] " + item.Name);
                                    AbilitiesIndex.Add(item.Index.Value);
                                }

                                result = -1;

                                //Do NEXT ABILITY

                                result = EditorGUI.Popup(NextbuttonRect, result, popupOptions.ToArray(), popupStyle);

                                if (result != -1)
                                {
                                    Ability.intValue = AbilitiesIndex[result];
                                    abilityName.stringValue = Mode.Abilities[result].Name;
                                }

                                //Check Ability Empty

                                if (abilityName.stringValue == string.Empty)
                                {
                                    if (Ability.intValue <= 0)
                                        abilityName.stringValue = "<CANNOT BE COMBO STARTER>";

                                    var ability = Mode.GetAbility(Ability.intValue);


                                    abilityName.stringValue = ability != null ? ability.Name : "<Not Found>";
                                }

                                if (previewAbilityName.stringValue == string.Empty)
                                {
                                    if (PreviewsAbility.intValue <= 0)
                                        previewAbilityName.stringValue = "Combo Starter";
                                    else
                                    {
                                        var ability = Mode.GetAbility(Ability.intValue);
                                        previewAbilityName.stringValue = ability != null ? ability.Name : "<Not Found>";
                                    }
                                }
                            }
                        }
                        EditorGUI.PropertyField(PrevARect, PreviewsAbility, GUIContent.none);
                        EditorGUI.LabelField(PrevARectName, previewAbilityName.stringValue);

                        EditorGUI.PropertyField(AbilityRect, Ability, GUIContent.none);
                        EditorGUI.LabelField(AbilityRectName, abilityName.stringValue);

                        var old = GUI.contentColor;



                        if (PreviewsAbility.intValue > 0)
                        {
                            EditorGUI.PropertyField(FinisherRect, finisher, GUIContent.none);

                            if (finisher.boolValue)
                            {
                                GUI.contentColor =
                                GUI.color = Restarter.boolValue ? Color.green : oldCColor;
                                Restarter.boolValue = GUI.Toggle(RestarterRect, Restarter.boolValue, new GUIContent("R", "This finisher can restart the combo"), EditorStyles.miniButton);

                                GUI.contentColor = GUI.color = old;
                                // EditorGUI.PropertyField(RestarterRect, Restarter, GUIContent.none);

                                if (Restarter.boolValue)
                                {
                                    EditorGUI.PropertyField(FinisherTimeRect, FinisherTime, GUIContent.none);
                                    FinisherTime.floatValue = Mathf.Clamp01(FinisherTime.floatValue);
                                }
                            }
                        }

                        if (PreviewsAbility.intValue == 0)
                        {
                            ActivationRect.width = rect.width;
                            EditorGUI.LabelField(ActivationRect, "Sequence starter. It doesn't require an Activation Time", EditorStyles.whiteLabel);
                        }
                        else
                        {
                            EditorGUIUtility.labelWidth = 120;
                            EditorGUI.PropertyField(ActivationRect, Activation, new GUIContent($"Buffer Time [Ab:{PreviewsAbility.intValue}]"));
                            EditorGUIUtility.labelWidth = 32;
                            EditorGUI.PropertyField(RActivationTime, ActivationTime, new GUIContent("Anim"));
                            EditorGUIUtility.labelWidth = 00;
                        }
                        GUI.contentColor = GUI.color = oldCColor;



                        var r = new Rect(ActivationRect);
                        r.y += Height + 2;
                        r.x -= 20;
                        r.width += 43;
                        r.height = 1f;

                        EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.25f));


                        if (index == SelectedAbilityIndex)
                        {
                            branch = Branch.intValue;
                            prev = PreviewsAbility.intValue;
                            current = Ability.intValue;
                        }
                    },

                    drawHeaderCallback = rect =>
                    {
                        var Height = EditorGUIUtility.singleLineHeight;
                        var IDRect = new Rect(rect) { height = Height };

                        float wid = rect.width / 2;
                        var IRWidth = 30f;
                        var Sep = -10f;
                        var Offset = 40f;

                        float xx = IRWidth + Offset;

                        var IndexRect = new Rect(IDRect) { width = IRWidth + 5 };
                        var BranchRect = new Rect(IDRect) { x = xx, width = 45 };
                        var PrevARect = new Rect(IDRect) { x = 75 + xx + Sep + 5, width = wid - 15 - Sep - 20 - 45 };
                        var AbilityRect = new Rect(IDRect) { x = wid + xx + Sep + 25, width = wid - 15 - Sep - 90 };
                        var FinisherRect = new Rect(IDRect) { x = IDRect.width - 10, width = 50 };

                        EditorGUI.LabelField(IndexRect, "Index");
                        EditorGUI.LabelField(BranchRect, " Branch");
                        EditorGUI.LabelField(PrevARect,
                            new GUIContent("Required Ability", "Mode Ability [Index] on the Animal required to activate a sequence"));
                        EditorGUI.LabelField(AbilityRect,
                            new GUIContent("Play Ability", "Next Mode Ability [Index] to Play on the Animal if the Active Mode Animation is withing the Activation Range limit "));
                        EditorGUI.LabelField(FinisherRect, new GUIContent("Finisher", "Combo Finisher"));
                    },

                    elementHeightCallback = (int index) =>
                    {
                        return EditorGUIUtility.singleLineHeight * 2 + 6;
                    }

                };

                SequenceReordable.Add(listKey, Reo_AbilityList);  //Store it on the Editor
            }

            Reo_AbilityList.DoLayoutList();

            SelectedAbilityIndex = Reo_AbilityList.index;

            Reo_AbilityList.elementHeightCallback(SelectedAbilityIndex);

            if (SelectedAbilityIndex != -1)
            {
                var element = sequence.GetArrayElementAtIndex(SelectedAbilityIndex);

                var Activation = element.FindPropertyRelative("Activation");
                var OnSequencePlay = element.FindPropertyRelative("OnSequencePlay");

                var lbl = "Branch:[" + branch + "] Required Ab:[" + prev + "] Next Ab:[" + current + "]";

                //EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                //{

                //    EditorGUILayout.LabelField("Sequence Properties - " + lbl);
                //    EditorGUILayout.PropertyField(Activation, new GUIContent("Activation", "Range of the Preview Animation the Sequence can be activate"));
                //}
                // EditorGUILayout.EndVertical();
                EditorGUILayout.PropertyField(OnSequencePlay, new GUIContent("Sequence Play - " + lbl));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription
                ("Use Modes to make combos. Activate using ComboManager.Play(int Branch)\nBranches are the different Inputs Values");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(animal);

                if (animal.objectReferenceValue != null)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        using (var cc = new EditorGUI.ChangeCheckScope())
                        {
                            int oldActiveCIndex = M.ActiveComboIndex.Value;

                            EditorGUILayout.PropertyField(ActiveComboIndex, new GUIContent($"Active Combo [{activeComboName}]", "Active Combo Index"));
                            if (cc.changed && Application.isPlaying)
                            {
                                ActiveComboIndex.serializedObject.ApplyModifiedProperties();

                                M.SetActiveCombo(M.ActiveComboIndex.Value);
                            }
                        }
                        MalbersEditor.DrawDebugIcon(debug);
                        //  debug.boolValue = GUILayout.Toggle(debug.boolValue,new GUIContent("D","Debug"), EditorStyles.miniButton, GUILayout.Width(23));
                    }

                    EditorGUILayout.PropertyField(Branch, new GUIContent("Branch",
                        "Current Branch ID for the Combo Sequence, if this value change then the combo will play different sequences"));

                    EditorGUILayout.PropertyField(DisableOnSleep);
                    CombosReor.DoLayoutList();

                    CombosReor.index = selectedComboEditor.intValue;
                    int IndexCombo = CombosReor.index;

                    if (IndexCombo != -1)
                    {

                        var combo = combos.GetArrayElementAtIndex(IndexCombo);

                        if (combo != null)
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                var name = combo.FindPropertyRelative("Name");

                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(combo, new GUIContent($"[{name.stringValue}] Combo"), false);
                                EditorGUI.indentLevel--;


                                if (combo.isExpanded)
                                {
                                    var Mode = combo.FindPropertyRelative("Mode");
                                    var OnComboFinished = combo.FindPropertyRelative("OnComboFinished");
                                    var OnComboInterrupted = combo.FindPropertyRelative("OnComboInterrupted");
                                    var DisableOnInterrupt = combo.FindPropertyRelative("DisableOnInterrupt");
                                    var DisableOnFinish = combo.FindPropertyRelative("DisableOnFinish");
                                    var CoolDown = combo.FindPropertyRelative("CoolDown");
                                    var states = combo.FindPropertyRelative("states");

                                    using (new GUILayout.VerticalScope(StyleGreen))
                                    {
                                        EditorGUILayout.LabelField("Green Sequences are combo starters", EditorStyles.boldLabel);
                                    }
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(states);
                                    EditorGUI.indentLevel--;

                                    EditorGUILayout.PropertyField(CoolDown);
                                    EditorGUILayout.PropertyField(DisableOnFinish);
                                    EditorGUILayout.PropertyField(DisableOnInterrupt);



                                    EditorGUILayout.LabelField("Combo Sequence List", EditorStyles.boldLabel);
                                    var sequence = combo.FindPropertyRelative("Sequence");

                                    if (Mode.objectReferenceValue)
                                        DrawSequence(IndexCombo, combo, sequence);

                                    EditorGUILayout.PropertyField(OnComboFinished);
                                    EditorGUILayout.PropertyField(OnComboInterrupted);
                                }
                            }

                            if (debug.boolValue && Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.Toggle("Playing Combo", M.PlayingCombo);
                                    EditorGUILayout.IntField("ActiveCombo", M.ActiveComboIndex.Value);
                                    EditorGUILayout.IntField("ActiveComboSequence", M.ActiveComboSequenceIndex);
                                }
                            }
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
    #endregion
}