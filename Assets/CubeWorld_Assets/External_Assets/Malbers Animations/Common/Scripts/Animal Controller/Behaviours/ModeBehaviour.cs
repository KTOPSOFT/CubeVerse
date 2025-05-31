using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{

    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/modes#mode-behaviour")]
    [AddComponentMenu("Malbers/Mode Behavior")]
    public class ModeBehaviour : StateMachineBehaviour
    {
        public ModeID ModeID;

        [Tooltip("Calls 'Animation Tag Enter' on the Modes")]
        public bool EnterMode = true;
        [Tooltip("Calls 'Animation Tag Exit' on the Modes")]
        public bool ExitMode = true;

        [Tooltip("Calls 'OnModeUpdate' on the Modes")]
        public bool OnModeUpdate = true;

        [Tooltip("Next Ability to do on the Mode.If is set to -1, The Exit On Ability Logic will be ignored.\n" +
            "Used this when you need an ability to finish on another Ability.\n" +
            "E.g. If the wolf is in the Ability SIT, and you activate the HOWL; When HOWL finish you can play again SIT right after")]
        [Min(-1)] public int ExitAbility = -1;

        [Tooltip("True: the Animation will exit automatically after the Exit Time. No need for [EXIT] or [INTERRUPTED] transitions.\n\nImportant:\n" +
            "The Mode Layer needs a Transition to an Empty Animator State from Any State.\n" +
            "Conditions: Mode = 0 ModeStatus = 0")]
        public bool NoExitTransitions = false;

        private bool JustExit = false;

        [Range(0, 1), Tooltip("Time to Exit the Animation Automatically with no exit transitions")]
        public float ExitTime = 0.95f;

        private MAnimal animal;
        private Mode mode;
        private Ability ActiveAbility;

        public void InitializeBehaviour(MAnimal animal)
        {
            this.animal = animal;

            if (ModeID != null)
            {
                mode = animal.Mode_Get(ModeID);
            }
            else
            {
                Debug.LogWarning("There's a Mode behaviour without an ID. Please check all your Mode Animations states.");
            }
        }

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            JustExit = false; //Reset the Do Exit Var

            if (animal == null) //Find the Animal the first time
            {
                animal = animator.GetComponent<MAnimal>();
                mode = animal.Mode_Get(ModeID);
            }

            if (!animal.enabled) JustExit = true; //Skip the Mode if the Animal is Disabled

            if (ModeID == null) { Debug.LogError("Mode behaviour needs an ID"); return; }
            if (mode == null) { Debug.LogError($"There's no [{ModeID.name}] mode on your character"); return; }

            ActiveAbility = mode.ActiveAbility;
            if (animal.ModeStatus == Int_ID.Loop) return;            //Means is Looping so Skip!!!

            if (EnterMode) mode.AnimationTagEnter(stateInfo.fullPathHash);
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (JustExit) return;

            //Means is Looping to itself So Skip the Exit Mode EXTREMELY IMPORTANT
            if (animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash) return;
            if (animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash) return;

            if (ExitMode)
                mode.AnimationTagExit(ActiveAbility, ExitAbility);
        }


        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (JustExit) return;

            mode.UpdateMode = OnModeUpdate;             //Send the Update to the Animal Mode Update variable (This affect Charging Modes and Mode Modifiers)
            animal.ModeTime = stateInfo.normalizedTime; //Send the Normalized time to the Animal ModeTime variable

            ExitModeNoTransition(animator, stateInfo, layerIndex);
        }
        int JustInTransition;

        private void ExitModeNoTransition(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (ExitMode && !JustExit && NoExitTransitions && stateInfo.normalizedTime >= ExitTime)
            {
                JustInTransition++;

                if (animator.IsInTransition(layerIndex)
                    && (animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash))
                {
                    JustInTransition = 0;
                    return;
                }

                if (JustInTransition <= 1) return; //Skip the first frame weird issue (BUG)


                //Do not Exit if the Ability is Charging or is Forever
                if (ActiveAbility.Status == AbilityStatus.Charged || ActiveAbility.Status == AbilityStatus.Forever)
                {
                    if (animal.ModeStatus != -2) return; //Meaning is not in the Exit or Interrupted State
                }

                JustExit = true;

                if (animal.ActiveMode != null && ActiveAbility != animal.ActiveMode.ActiveAbility)
                {
                    if (animal.debugModes) Debug.Log("Playing different Ability ..ignore exit");
                    return;
                }

                //Debug.Log($"Mode [{ModeOwner.Name}] . Ability [{ActiveAbility.Name}] Automatic Exit on [{ExitTime}]");
                mode.AnimationTagExit(ActiveAbility, ExitAbility);
                animal.SetModeStatus(0); //Must set the Mode Status to 0 to allow the Mode to Exit Automatically
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModeBehaviour))]
    public class ModeBehaviourED : Editor
    {
        SerializedProperty EnterMode, ExitMode, ModeID, ExitAbility, NoExitTransitions, ExitTime, OnModeUpdate
            ;
        Color RequiredColor = new(1, 0.4f, 0.4f, 1);

        void OnEnable()
        {

            ModeID = serializedObject.FindProperty("ModeID");
            EnterMode = serializedObject.FindProperty("EnterMode");
            ExitMode = serializedObject.FindProperty("ExitMode");
            ExitAbility = serializedObject.FindProperty("ExitAbility");
            NoExitTransitions = serializedObject.FindProperty("NoExitTransitions");
            ExitTime = serializedObject.FindProperty("ExitTime");
            OnModeUpdate = serializedObject.FindProperty("OnModeUpdate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {


                using (new GUILayout.HorizontalScope())
                {
                    var currentGUIColor = GUI.color;
                    GUI.color = ModeID.objectReferenceValue == null ? RequiredColor : currentGUIColor;
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(ModeID, GUIContent.none, GUILayout.MinWidth(40));

                    var width = 42;
                    var OnColor = Color.green;

                    GUI.color = EnterMode.boolValue ? OnColor : currentGUIColor;

                    EnterMode.boolValue = GUILayout.Toggle(EnterMode.boolValue,
                                       new GUIContent("Enter", "Notify the Mode on the Animal that the Ability has Started"), EditorStyles.miniButton, GUILayout.Width(width));

                    GUI.color = OnModeUpdate.boolValue ? OnColor : currentGUIColor;
                    OnModeUpdate.boolValue = GUILayout.Toggle(OnModeUpdate.boolValue,
                                       new GUIContent("Update", "Notify the Mode on the Animal that is playing. It Sends Normalized time of the animation. Useful for Abilities that have charge values and Mode Modifiers"), EditorStyles.miniButton, GUILayout.Width(width + 15));

                    GUI.color = ExitMode.boolValue ? OnColor : currentGUIColor;

                    ExitMode.boolValue = GUILayout.Toggle(ExitMode.boolValue,
                                       new GUIContent("Exit", "Notify the Mode on the Animal that the Ability has ended"), EditorStyles.miniButton, GUILayout.Width(width));


                    GUI.color = currentGUIColor;

                    if (ExitMode.boolValue)
                    {
                        var exitAbility = ExitAbility.intValue;
                        var GuiColor = GUI.color;
                        if (exitAbility > 0)
                            GUI.color = Color.yellow + Color.green;
                        EditorGUIUtility.labelWidth = 65;
                        EditorGUILayout.PropertyField(ExitAbility, GUILayout.Width(100));

                        GUI.color = GuiColor;
                    }
                }
                EditorGUIUtility.labelWidth = 0;

                if (ExitMode.boolValue)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUIUtility.labelWidth = 113;
                        EditorGUILayout.PropertyField(NoExitTransitions, GUILayout.MaxWidth(140));
                        EditorGUIUtility.labelWidth = 55;
                        if (NoExitTransitions.boolValue)
                            EditorGUILayout.PropertyField(ExitTime);
                        EditorGUIUtility.labelWidth = 0;

                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}