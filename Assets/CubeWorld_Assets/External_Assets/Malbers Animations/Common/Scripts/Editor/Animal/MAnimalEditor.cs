#if UNITY_EDITOR
using MalbersAnimations.Scriptables;
using MalbersAnimations.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(MAnimal))]
    public class MAnimalEditor : Editor
    {
        public readonly string version = "Animal Controller [v1.4.9]";

        public static GUIStyle StyleGray => MTools.Style(new Color(0.5f, 0.5f, 0.5f, 0.3f));
        public static GUIStyle StyleBlue => MTools.Style(new Color(0, 0.5f, 1f, 0.3f));

        //private GUIContent plus, minus;

        private List<Type> StatesType = new();
        private ReorderableList Reo_List_States;
        private ReorderableList Reo_List_Modes;
        private ReorderableList Reo_List_Stances;
        private ReorderableList Reo_List_Speeds;

        private readonly Dictionary<string, ReorderableList> Reo_Abilities = new();

        private readonly Dictionary<string, Editor> State_Editor = new();


        private int SpeedTabs = 0;
        private int SelectedSpeed = -1;

        #region Serialized Properties

        SerializedProperty
            S_State_List, S_PivotsList, Height, S_Mode_List, UseMainCameraDirection,

            Editor_Tabs1, Editor_Tabs2, Runtime_Tabs1, Runtime_Tabs2, GlobalRootMotion,


            StartWithMode,
            OnEnterExitStances, OnEnterExitStates, OnEnterExitSpeeds,
            RB, Anim,// NoParent,
            m_Vertical, m_VerticalRaw, m_Horizontal, m_StateFloat, m_ModeStatus, m_State, m_StateStatus, m_StateExitStatus,
            m_LastState, m_Mode, m_Grounded, m_Movement, m_Random, m_ModePower,
            m_SpeedMultiplier, m_UpDown, m_DeltaUpDown, m_StateOn, m_StateProfile, m_Sprint, m_ModeOn,// m_StanceOn,
            currentStance, defaultStance, Stances_List,
            m_Stance, m_LastStance, m_Slope, m_Type, m_StateTime, m_TargetAngle, m_StrafeAnim,
            lockInput, lockMovement, Rotator, AlignCycle, animalType, kinematicTimeline,
            RayCastRadius, MainCamera, sleep, m_gravityTime, m_ClampGravitySpeed, RootBone,

             m_CanStrafe, Aimer, m_strafe, OnStrafe, OnFreeMovement, OnGroundChangesGravity,
            m_StrafeNormalize,  /*FallForward, */m_StrafeLerp, OrientToGround,

            MainCollider, colliders,

            alwaysForward, AnimatorSpeed, m_TimeMultiplier,
            OnMovementLocked, OnMovementDetected, //OnMaxSlopeReached,
            OnInputLocked, OnSprintEnabled, OnGrounded, OnStanceChange, OnStateChange, OnStateProfile, OnModeStart, OnModeEnd,
            OnTeleport, OnPreTeleport,
            OnSpeedChange, OnAnimationChange, GroundLayer, AlignPosLerp, AlignPosDelta, AlignRotDelta,
            AlignRotLerp, m_gravity, m_gravityPower, useCameraUp, ground_Changes_Gravity,
             useSprintGlobal, SmoothVertical,
            TurnMultiplier, TurnLimit, InPlaceDamp,
            Player, OverrideStartState, CloneStates, S_Speed_List, UseCameraInput,// TerrainSlopeLimit, 
            SlopeLimit, SlideThreshold, SlideAmount, SlideDamp,
             //maxAngleSlope,  deepSlope,

             states_C,
            LockUpDownMovement, LockHorizontalMovement, LockForwardMovement, DebreeTag;

        //EditorStuff
        SerializedProperty
             ShowStateInInspector, Ability_Tabs, Mode_Tabs1, DebugModes, DebugStances, DebugStates,
              SelectedMode, SelectedStance, SelectedState, showPivots,
                Editor_EventTabs, ShowOnPlay, showModeList, showStateList
 ;
        #endregion

        MAnimal m;
        // private MonoScript script;
        private GenericMenu addMenu;
        private GUIStyle DescriptionStyle;

        private void FindSerializedProperties()
        {
            DebugModes = serializedObject.FindProperty("debugModes");
            DebugStances = serializedObject.FindProperty("debugStances");
            DebugStates = serializedObject.FindProperty("debugStates");
            UseMainCameraDirection = serializedObject.FindProperty("UseMainCameraDirection");


            MainCollider = serializedObject.FindProperty("MainCollider");
            colliders = serializedObject.FindProperty("colliders");

            //Modes
            showModeList = serializedObject.FindProperty("showModeList");
            showStateList = serializedObject.FindProperty("showStateList");
            Mode_Tabs1 = serializedObject.FindProperty("Mode_Tabs1");
            Ability_Tabs = serializedObject.FindProperty("Ability_Tabs");
            SelectedMode = serializedObject.FindProperty("SelectedMode");
            GlobalRootMotion = serializedObject.FindProperty("GlobalRootMotion");


            ShowOnPlay = serializedObject.FindProperty("ShowOnPlay");
            S_PivotsList = serializedObject.FindProperty("pivots");
            sleep = serializedObject.FindProperty("sleep");
            S_Mode_List = serializedObject.FindProperty("modes");

            ground_Changes_Gravity = serializedObject.FindProperty("ground_Changes_Gravity");
            Stances_List = serializedObject.FindProperty("Stances");
            states_C = serializedObject.FindProperty("states_C");


            //NoParent = serializedObject.FindProperty("NoParent");


            DebreeTag = serializedObject.FindProperty("DebrisTag");
            SelectedState = serializedObject.FindProperty("SelectedState");
            SelectedStance = serializedObject.FindProperty("SelectedStance");
            ShowStateInInspector = serializedObject.FindProperty("ShowStateInInspector");

            m_CanStrafe = serializedObject.FindProperty("m_CanStrafe");
            m_StrafeNormalize = serializedObject.FindProperty("m_StrafeNormalize");
            m_strafe = serializedObject.FindProperty("m_strafe");
            Aimer = serializedObject.FindProperty("Aimer");
            OnStrafe = serializedObject.FindProperty("OnStrafe");
            m_StrafeLerp = serializedObject.FindProperty("m_StrafeLerp");

            alwaysForward = serializedObject.FindProperty("alwaysForward");

            MainCamera = serializedObject.FindProperty("m_MainCamera");


            S_Speed_List = serializedObject.FindProperty("speedSets");

            currentStance = serializedObject.FindProperty("currentStance");
            defaultStance = serializedObject.FindProperty("defaultStance");


            RB = serializedObject.FindProperty("RB");
            Anim = serializedObject.FindProperty("Anim");

            UseCameraInput = serializedObject.FindProperty("useCameraInput");
            useCameraUp = serializedObject.FindProperty("useCameraUp");
            StartWithMode = serializedObject.FindProperty("StartWithMode");

            OnEnterExitStates = serializedObject.FindProperty("OnEnterExitStates");
            OnEnterExitStances = serializedObject.FindProperty("OnEnterExitStances");
            OnEnterExitSpeeds = serializedObject.FindProperty("OnEnterExitSpeeds");

            Height = serializedObject.FindProperty("height");
            // ModeIndexSelected = serializedObject.FindProperty("ModeIndexSelected");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            Editor_Tabs2 = serializedObject.FindProperty("Editor_Tabs2");

            Runtime_Tabs1 = serializedObject.FindProperty("Runtime_Tabs1");
            Runtime_Tabs2 = serializedObject.FindProperty("Runtime_Tabs2");

            m_Vertical = serializedObject.FindProperty("m_Vertical");
            m_VerticalRaw = serializedObject.FindProperty("m_VerticalRaw");
            // Center = serializedObject.FindProperty("Center");
            m_Horizontal = serializedObject.FindProperty("m_Horizontal");
            m_StateFloat = serializedObject.FindProperty("m_StateFloat");
            m_ModeStatus = serializedObject.FindProperty("m_ModeStatus");
            m_State = serializedObject.FindProperty("m_State");
            m_StateStatus = serializedObject.FindProperty("m_StateStatus");
            m_StateExitStatus = serializedObject.FindProperty("m_StateExitStatus");
            m_LastState = serializedObject.FindProperty("m_LastState");
            m_Mode = serializedObject.FindProperty("m_Mode");
            m_Grounded = serializedObject.FindProperty("m_Grounded");
            m_Movement = serializedObject.FindProperty("m_Movement");
            m_Random = serializedObject.FindProperty("m_Random");
            m_ModePower = serializedObject.FindProperty("m_ModePower");
            m_StrafeAnim = serializedObject.FindProperty("m_Strafe");
            m_SpeedMultiplier = serializedObject.FindProperty("m_SpeedMultiplier");

            m_UpDown = serializedObject.FindProperty("m_UpDown");
            m_DeltaUpDown = serializedObject.FindProperty("m_DeltaUpDown");

            m_StateOn = serializedObject.FindProperty("m_StateOn");
            m_StateProfile = serializedObject.FindProperty("m_StateProfile");
            //m_StanceOn = serializedObject.FindProperty("m_StanceOn");
            m_ModeOn = serializedObject.FindProperty("m_ModeOn");
            m_Sprint = serializedObject.FindProperty("m_Sprint");

            m_Stance = serializedObject.FindProperty("m_Stance");
            m_LastStance = serializedObject.FindProperty("m_LastStance");

            m_Slope = serializedObject.FindProperty("m_Slope");
            m_Type = serializedObject.FindProperty("m_Type");
            m_StateTime = serializedObject.FindProperty("m_StateTime");
            m_TargetAngle = serializedObject.FindProperty("m_DeltaAngle");
            lockInput = serializedObject.FindProperty("lockInput");
            lockMovement = serializedObject.FindProperty("lockMovement");
            Rotator = serializedObject.FindProperty("Rotator");
            animalType = serializedObject.FindProperty("animalType");
            kinematicTimeline = serializedObject.FindProperty("kinematicTimeline");
            RayCastRadius = serializedObject.FindProperty("rayCastRadius");
            AlignCycle = serializedObject.FindProperty("AlignCycle");
            AnimatorSpeed = serializedObject.FindProperty("AnimatorSpeed");
            m_TimeMultiplier = serializedObject.FindProperty("m_TimeMultiplier");
            // m_TargetHorizontal = serializedObject.FindProperty("m_TargetHorizontal");

            LockForwardMovement = serializedObject.FindProperty("lockForwardMovement");
            LockHorizontalMovement = serializedObject.FindProperty("lockHorizontalMovement");
            LockUpDownMovement = serializedObject.FindProperty("lockUpDownMovement");



            //OnMaxSlopeReached = serializedObject.FindProperty("OnMaxSlopeReached");
            OnMovementLocked = serializedObject.FindProperty("OnMovementLocked");
            OnMovementDetected = serializedObject.FindProperty("OnMovementDetected");
            OnInputLocked = serializedObject.FindProperty("OnInputLocked");
            OnSprintEnabled = serializedObject.FindProperty("OnSprintEnabled");
            OnGrounded = serializedObject.FindProperty("OnGrounded");
            OnStanceChange = serializedObject.FindProperty("OnStanceChange");
            OnStateChange = serializedObject.FindProperty("OnStateChange");
            OnStateProfile = serializedObject.FindProperty("OnStateProfile");
            OnModeStart = serializedObject.FindProperty("OnModeStart");
            OnFreeMovement = serializedObject.FindProperty("OnFreeMovement");
            OnGroundChangesGravity = serializedObject.FindProperty("OnGroundChangesGravity");

            OnModeEnd = serializedObject.FindProperty("OnModeEnd");
            OnSpeedChange = serializedObject.FindProperty("OnSpeedChange");
            OnTeleport = serializedObject.FindProperty("OnTeleport");
            OnPreTeleport = serializedObject.FindProperty("OnPreTeleport");
            OnAnimationChange = serializedObject.FindProperty("OnAnimationChange");


            showPivots = serializedObject.FindProperty("showPivots");
            // ShowpivotColor = serializedObject.FindProperty("ShowpivotColor");
            GroundLayer = serializedObject.FindProperty("groundLayer");

            //TerrainSlopeLimit = serializedObject.FindProperty("TerrainSlopeLimit");
            SlopeLimit = serializedObject.FindProperty("SlopeLimit");
            SlideThreshold = serializedObject.FindProperty("slideThreshold");
            SlideAmount = serializedObject.FindProperty("slideAmount");
            SlideDamp = serializedObject.FindProperty("slideDamp");

            //maxAngleSlope = serializedObject.FindProperty("maxAngleSlope");
            //deepSlope = serializedObject.FindProperty("m_deepSlope");


            AlignPosLerp = serializedObject.FindProperty("AlignPosLerp");
            OrientToGround = serializedObject.FindProperty("m_OrientToGround");
            AlignPosDelta = serializedObject.FindProperty("AlignPosDelta");
            AlignRotDelta = serializedObject.FindProperty("AlignRotDelta");
            AlignRotLerp = serializedObject.FindProperty("AlignRotLerp");


            m_gravity = serializedObject.FindProperty("m_gravityDir");
            m_gravityPower = serializedObject.FindProperty("m_gravityPower");
            m_gravityTime = serializedObject.FindProperty("m_gravityTime");
            m_ClampGravitySpeed = serializedObject.FindProperty("m_clampGravitySpeed");

            useSprintGlobal = serializedObject.FindProperty("useSprintGlobal");
            SmoothVertical = serializedObject.FindProperty("SmoothVertical");
            TurnMultiplier = serializedObject.FindProperty("TurnMultiplier");
            TurnLimit = serializedObject.FindProperty("TurnLimit");
            InPlaceDamp = serializedObject.FindProperty("inPlaceDamp");


            Player = serializedObject.FindProperty("isPlayer");
            OverrideStartState = serializedObject.FindProperty("OverrideStartState");
            CloneStates = serializedObject.FindProperty("CloneStates");
            RootBone = serializedObject.FindProperty("RootBone");
            Editor_EventTabs = serializedObject.FindProperty("Editor_EventTabs");

        }

        private void OnEnable()
        {
            m = (MAnimal)target;
            //  script = MonoScript.FromMonoBehaviour(target as MonoBehaviour);
            FindSerializedProperties();



            StatesType.Clear();
            StatesType = MTools.GetAllTypes<State>();

            S_State_List = serializedObject.FindProperty("states");

            Reo_List_States = new ReorderableList(serializedObject, S_State_List, true, true, true, true)
            {
                drawHeaderCallback = Draw_Header_State,
                drawElementCallback = Draw_Element_State,
                onReorderCallbackWithDetails = OnReorderCallback_States,
                //onReorderCallbackWithDetails = OnReorderCallback_States_Details,
                onAddCallback = OnAddCallback_State,
                onRemoveCallback = OnRemove_State,
                onSelectCallback = Selected_State,
            };

            Reo_List_Modes = new ReorderableList(serializedObject, S_Mode_List, true, true, true, true)
            {
                drawElementCallback = Draw_Element_Modes,
                drawHeaderCallback = Draw_Header_Modes,
                onAddCallback = OnAdd_Modes,
                onRemoveCallback = OnRemoveCallback_Mode,
                onSelectCallback = Selected_Mode,
                onReorderCallback = (list) => { ModeArray_Popup(); }
            };

            Reo_List_Speeds = new ReorderableList(serializedObject, S_Speed_List, true, true, true, true)
            {
                drawElementCallback = Draw_Element_Speed,
                drawHeaderCallback = Draw_Header_Speed,
                onAddCallback = OnAddCallback_Speeds,
                onRemoveCallback = OnRemoveCallback_Speeds,

                onSelectCallback = (list) =>
                {
                    SelectedSpeed = list.index;
                }

            };

            Reordable_Stances();

            Reo_List_States.index = SelectedState.intValue;
            Reo_List_Modes.index = SelectedMode.intValue;

            UpdateCacheState();

            StateArray_Popup();
            ModeArray_Popup();
        }

        private void CheckGuiStyles()
        {
            if (DescriptionStyle == null)
            {
                DescriptionStyle = new GUIStyle(MTools.StyleGray)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    stretchWidth = true
                };
                DescriptionStyle.normal.textColor = EditorStyles.boldLabel.normal.textColor;
            }


            popupStyle ??= new(GUI.skin.GetStyle("PaneOptions"))
            {
                imagePosition = ImagePosition.ImageOnly
            };
        }

        private void Selected_Mode(ReorderableList list)
        {
            SelectedMode.intValue = list.index;
            // SelectedAbility = -1;
            ModeArray_Popup();
            StateArray_Popup();
        }

        private void ModeArray_Popup()
        {
            ModePopupList = new string[S_Mode_List.arraySize];

            for (int i = 0; i < ModePopupList.Length; i++)
            {
                ModePopupList[i] = m.modes[i].ID != null ? m.modes[i].ID.name : "<EMPTY>";
            }
        }

        private void StateArray_Popup()
        {
            StatePopupList = new string[S_State_List.arraySize];

            for (int i = 0; i < StatePopupList.Length; i++)
            {
                StatePopupList[i] = m.states[i].ID != null ? m.states[i].ID.name : "<EMPTY>";
            }
        }

        private string[] ModePopupList;
        private string[] StatePopupList;


        // int SelectedAbility;

        private readonly string[] tab1 = new string[] { "General", "States", "Modes", "Stances" };
        private readonly string[] tab2 = new string[] { "Advanced", "Speeds", "Events", "Debug" };

        private readonly string[] DebugTab1 = new string[] { "Data", "State", "Ground", "Speeds" };
        private readonly string[] DebugTab2 = new string[] { "Input", "Mode", "Forces", "Movement" };

        private GUIContent _icon_Show;
        private GUIStyle icon_EYE;
        public GUIStyle Icon_EYE
        {
            get
            {
                icon_EYE ??= new GUIStyle(StyleBlue);
                return icon_EYE;
            }
        }

        public GUIContent Icon_Show
        {
            get
            {
                if (_icon_Show == null)
                {
                    _icon_Show = EditorGUIUtility.IconContent("d_ViewToolOrbit", "Enable/Disable");
                    _icon_Show.tooltip = "Hide Animal Inspector on PlayMode. This will increase the speed of the game if the animal is selected ";
                }

                return _icon_Show;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            CheckGuiStyles();
            var descri = version;

            if (Application.isPlaying)
            {
                if (m.Sleep)
                {
                    descri += "      [[SLEEP]]";
                }
                else if (m.InTimeline)
                {
                    descri += "      [[IN TIMELINE]]";
                }
                else if (m.LockInput && m.LockMovement)
                {
                    descri += "      [[LOCKED]]";
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                MalbersEditor.DrawDescription(descri);

                var currentGUIColor = GUI.color;
                GUI.color = ShowOnPlay.boolValue ? (GUI.color + Color.white) * 2 : (GUI.color + Color.black) / 1.65f;

                ShowOnPlay.boolValue = GUILayout.Toggle(ShowOnPlay.boolValue, Icon_Show, Icon_EYE,
                      GUILayout.Width(25), GUILayout.Height(22));

                GUI.color = currentGUIColor;

            }


            if (!ShowOnPlay.boolValue && Application.isPlaying)
            {
                EditorGUILayout.HelpBox("The Inpector is hidden in Play Mode to improve performance. Use the [Eye] icon to show it again", MessageType.Info);
            }
            else
            {
                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, tab1);
                if (Editor_Tabs1.intValue != 4) Editor_Tabs2.intValue = 4;

                Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, tab2);
                if (Editor_Tabs2.intValue != 4) Editor_Tabs1.intValue = 4;

                //First Tabs
                int Selection = Editor_Tabs1.intValue;



                switch (Selection)
                {
                    case 0: ShowGeneral(); break;
                    case 1: ShowStates(); break;
                    case 2: ShowModes(); break;
                    case 3: ShowStances(); break;
                    default: break;
                }




                //2nd Tabs
                Selection = Editor_Tabs2.intValue;

                switch (Selection)
                {
                    case 0: ShowAdvanced(); break;
                    case 1: ShowSpeeds(); break;
                    case 2: ShowEvents(); break;
                    case 3: ShowDebug(); break;
                    default: break;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ShowGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                EditorGUILayout.PropertyField(Player, G_Player);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(S_PivotsList, true);

                if (S_PivotsList.isExpanded)
                {
                    //Draw Height
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(Height);

                        if (GUILayout.Button(new GUIContent("C", "Calculate Height and Animal Center"), GUILayout.Width(26)))
                        {
                            m.SetPivots();
                            m.CalculateCenter(true);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MainCollider.isExpanded = MalbersEditor.Foldout(MainCollider.isExpanded, "Colliders");

                if (MainCollider.isExpanded)
                {
                    EditorGUILayout.PropertyField(MainCollider);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(colliders, new GUIContent("Internal Colliders"), true);
                    EditorGUI.indentLevel--;
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                UseCameraInput.isExpanded = MalbersEditor.Foldout(UseCameraInput.isExpanded, "Movement");

                if (UseCameraInput.isExpanded)
                {
                    EditorGUILayout.PropertyField(UseCameraInput, new GUIContent("Camera Input", "The Animal uses the Camera Forward Diretion to Move"));
                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(alwaysForward,
                            new GUIContent("Always Forward", "If true the animal will always go forward. useful for infinite runners"));
                    }
                    if (EditorGUI.EndChangeCheck() && Application.isPlaying && Application.isEditor)
                        m.AlwaysForward = m.AlwaysForward; //Update Always Forward Property on the Editor


                    EditorGUILayout.PropertyField(useCameraUp, new GUIContent("Use Camera Up", "Uses the Camera Up Vector to move UP or Down while flying or Swiming UnderWater. if this is false the Animal will need an UPDOWN Input to move higher or lower"));
                    EditorGUILayout.PropertyField(SmoothVertical, G_SmoothVertical);
                    EditorGUILayout.PropertyField(useSprintGlobal, G_useSprintGlobal);
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(TurnMultiplier);
                    EditorGUILayout.PropertyField(InPlaceDamp);
                    EditorGUILayout.PropertyField(TurnLimit);
                    EditorGUILayout.PropertyField(GlobalRootMotion);
                    EditorGUILayout.PropertyField(AnimatorSpeed);
                    EditorGUILayout.PropertyField(m_TimeMultiplier);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GroundLayer.isExpanded = MalbersEditor.Foldout(GroundLayer.isExpanded, "Ground");

                if (GroundLayer.isExpanded)
                {
                    EditorGUILayout.PropertyField(GroundLayer, G_GroundLayer);
                    EditorGUILayout.PropertyField(OrientToGround);
                    EditorGUILayout.PropertyField(DebreeTag);




                    //EditorGUILayout.PropertyField(TerrainSlopeLimit);
                    EditorGUILayout.PropertyField(SlopeLimit);
                    EditorGUILayout.PropertyField(SlideThreshold);
                    EditorGUILayout.PropertyField(SlideAmount);
                    EditorGUILayout.PropertyField(SlideDamp);
                    //EditorGUILayout.PropertyField(maxAngleSlope);
                    //EditorGUILayout.PropertyField(deepSlope);

                    Height.isExpanded = MalbersEditor.Foldout(Height.isExpanded, "Ground Aligment");
                    if (Height.isExpanded)
                    {
                        EditorGUILayout.PropertyField(AlignPosLerp, G_AlignPosLerp);
                        EditorGUILayout.PropertyField(AlignPosDelta, G_AlignPosDelta);
                        EditorGUILayout.PropertyField(AlignRotLerp, G_AlignRotLerp);
                        EditorGUILayout.PropertyField(AlignRotDelta, G_AlignRotDelta);
                        EditorGUILayout.PropertyField(RayCastRadius, G_RayCastRadius);
                        EditorGUILayout.PropertyField(AlignCycle);
                    }
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                m_gravity.isExpanded = MalbersEditor.Foldout(m_gravity.isExpanded, "Gravity");

                if (m_gravity.isExpanded)
                {
                    //  EditorGUILayout.LabelField("Gravity", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(m_gravity, G_gravityDirection);
                    EditorGUILayout.PropertyField(m_gravityPower, G_GravityForce);
                    EditorGUILayout.PropertyField(m_gravityTime, G_GravityCycle);
                    EditorGUILayout.PropertyField(m_ClampGravitySpeed);
                    EditorGUILayout.PropertyField(ground_Changes_Gravity, new GUIContent("Ground Changes Gravity", "The Ground will change the gravity direction, allowing the animals to move in any surface"));
                }
            }
            ShowStrafingVars();
        }

        private void Reordable_Stances()
        {
            Reo_List_Stances = new ReorderableList(serializedObject, Stances_List, true, true, true, true)
            {
                drawHeaderCallback = (rect) =>
                {
                    var r = new Rect(rect);
                    r.x += 40;
                    r.width = 90;
                    EditorGUI.LabelField(r, new GUIContent("Stances", "Stances allowed in this Controller"));

                    var activeRect = rect;
                    activeRect.width -= 20;
                    activeRect.x += 20;
                    var IDRect = new Rect(activeRect.width + 35, activeRect.y, 35, activeRect.height);

                    EditorGUI.LabelField(IDRect, new GUIContent("ID", "Mode ID:\n Numerical ID value for the Mode"));
                },

                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    if (Stances_List.arraySize <= index) return;

                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        var ModeProperty = Stances_List.GetArrayElementAtIndex(index);
                        var ID = ModeProperty.FindPropertyRelative("ID");

                        var Active = ModeProperty.FindPropertyRelative("enabled");
                        var ConstValue = Active.FindPropertyRelative("ConstantValue");
                        var VarValue = Active.FindPropertyRelative("Variable");
                        var useConstant = Active.FindPropertyRelative("UseConstant").boolValue;
                        BoolVar variable = VarValue.objectReferenceValue as BoolVar;

                        var rectan = new Rect(rect);
                        rectan.width -= 20;
                        rectan.x += 20;
                        rectan.y -= 2;

                        var ActiveRect = new Rect(rect.x, rect.y - 2, 20, rect.height);
                        var IDRect = new Rect(rect.x + 40, rect.y, rect.width - 70, EditorGUIUtility.singleLineHeight);
                        var Rect_Label = new Rect(rect.width - 80, rect.y, 60, EditorGUIUtility.singleLineHeight);

                        if (Application.isPlaying) IDRect.width -= 100f;

                        if (useConstant)
                        {
                            ConstValue.boolValue = EditorGUI.Toggle(ActiveRect, GUIContent.none, ConstValue.boolValue);

                            if (variable != null)
                            {
                                variable.Value = ConstValue.boolValue;
                                EditorUtility.SetDirty(variable);
                            }
                        }
                        else
                        {
                            if (variable != null)
                            {
                                variable.Value = EditorGUI.Toggle(ActiveRect, GUIContent.none, variable.Value);
                                ConstValue.boolValue = variable.Value;
                            }
                            else
                            {
                                ConstValue.boolValue = EditorGUI.Toggle(ActiveRect, GUIContent.none, ConstValue.boolValue);
                            }
                        }

                        var oldColor = GUI.contentColor;

                        if (Application.isPlaying)
                        {
                            if (m.Stances[index].Active) GUI.contentColor = Color.yellow;
                            else if (m.Stances[index].Persistent) GUI.contentColor = Color.red + Color.white;
                            else if (m.Stances[index].DisableTemp) GUI.contentColor = Color.white;
                        }


                        var st_label = "";

                        var stanceElement = m.Stances[index];

                        if (Application.isPlaying)
                        {
                            if (stanceElement.Active) st_label = "[Active]";
                            if (stanceElement.Persistent) st_label = "[Persis]";
                            if (stanceElement.Queued) st_label = "[Queued]";
                            if (stanceElement.DisableTemp) st_label = "[Disabled]";
                        }

                        if (Application.isPlaying)
                        {
                            var TestBRect = new Rect(rect.width - 20, rect.y, 38, EditorGUIUtility.singleLineHeight);

                            var color = GUI.color;
                            GUI.color = MTools.MGreen * 1.5f;
                            if (GUI.Button(TestBRect, "Test"))
                            {
                                m.Stance_Activate(ID.objectReferenceValue as StanceID);
                            }
                            GUI.color = color;
                        }



                        var dC = GUI.contentColor;

                        if (!Application.isPlaying && isFocused) GUI.contentColor = new Color(3f, 0.7f, 0.5f);


                        var dbC = GUI.backgroundColor;
                        GUI.backgroundColor = isActive ? MTools.MOrange : dbC;

                        EditorGUI.PropertyField(IDRect, ID, GUIContent.none);

                        EditorGUI.LabelField(Rect_Label, st_label);

                        GUI.contentColor = Application.isPlaying ? oldColor : dC;
                        GUI.backgroundColor = dbC;

                        var style = new GUIStyle(EditorStyles.boldLabel)
                        { alignment = TextAnchor.UpperRight };

                        if (stanceElement.ID != null)
                        {
                            var IDVal = new Rect(rect.width + 10, rectan.y + 3, 35, rectan.height);
                            EditorGUI.LabelField(IDVal, stanceElement.ID.ID.ToString(), style);
                        }

                        if (cc.changed)
                        {
                            Undo.RecordObject(target, "MAnimal Inspector");
                            EditorUtility.SetDirty(target);
                        }
                    }
                },

                onAddCallback = (list) =>
                {
                    if (m.Stances == null) m.Stances = new List<Stance>();

                    var newStance = new Stance();
                    m.Stances.Add(newStance);
                    EditorUtility.SetDirty(m);

                },

                onSelectCallback = (list) =>
                { SelectedStance.intValue = list.index; }
            };
        }

        #region States Stuff

        private void ShowStates()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(OverrideStartState, G_OverrideStartState);
                    CloneStates.boolValue = GUILayout.Toggle(CloneStates.boolValue, G_CloneStates, EditorStyles.miniButton, GUILayout.Width(85));
                    MalbersEditor.DrawDebugIcon(DebugStates, MTools.MBlue);
                }
                //  EditorGUI.indentLevel--;

                if (!CloneStates.boolValue)
                {
                    EditorGUILayout.HelpBox("Disable Clone States only when you are setting values and debugging while playing. ", MessageType.Warning);
                }


                using (new GUILayout.HorizontalScope())
                {
                    var Head = " States";

                    if (m.states != null && m.states.Count > 0 && SelectedState.intValue != -1 && SelectedState.intValue < m.states.Count)
                    {
                        var s = m.states[SelectedState.intValue];
                        //  Head += $" [{s.GetType().Name}]";

                        if (s != null && s.ID != null)
                        {
                            Head += $"  ID: [{s.ID.ID}]";
                            Head += $"  Tag: [{s.ID.name}]";
                        }
                    }


                    showStateList.boolValue = MalbersEditor.Foldout(showStateList.boolValue, Head);

                }

                if (showStateList.boolValue)
                {
                    Reo_List_States.DoLayoutList();        //Paint the Reordable List 
                    DropAreaGUIStates();

                }
                EditorGUILayout.Space();

                Reo_List_States.index = SelectedState.intValue;
                var index = SelectedState.intValue;

                UpdateCacheState();

                if (index != -1 && Reo_List_States.serializedProperty.arraySize > index)
                {
                    var element = Reo_List_States.serializedProperty.GetArrayElementAtIndex(index);

                    var StateObj = m.states[index];

                    if (element != null & StateObj != null)
                    {
                        bool showStateEditor = false;

                        using (new GUILayout.HorizontalScope())
                        {
                            if (m.MainCollider && StateObj.OverrideCapsule &&
                                GUILayout.Button(new GUIContent("CC", "Copy Main Capsule values to the Override Capsule values of the state "), GUILayout.Width(30)))
                            {
                                StateObj.newCapsule = new(m.MainCollider);
                                EditorUtility.SetDirty(StateObj);
                            }
                            if (!showStateList.boolValue)
                            {
                                if (StatePopupList.Length != m.states.Count) StateArray_Popup();
                                SelectedState.intValue = EditorGUILayout.Popup(SelectedState.intValue, StatePopupList, popupStyle, GUILayout.Width(20));
                            }

                            showStateEditor = MalbersEditor.Foldout(ShowStateInInspector, $"ID [{StateObj.ID.ID}] ");
                        }


                        GUILayout.Space(-20);
                        EditorGUIUtility.labelWidth = 110;

                        using (new EditorGUI.DisabledGroupScope(true))
                            EditorGUILayout.ObjectField(new GUIContent("    "), StateObj, typeof(StateID), false, GUILayout.MinWidth(50));

                        EditorGUIUtility.labelWidth = 0;




                        //Show the inspector in the Animal Controller
                        if (showStateEditor)
                        {
                            if (element.objectReferenceValue != null)
                            {
                                // MMDrawnPropertiesEditor.MMDrawnProperties(element.objectReferenceValue);
                                var key = element.propertyPath;

                                if (State_Editor.TryGetValue(key, out Editor editor))
                                {
                                    editor = State_Editor[key];
                                }
                                else
                                {
                                    Editor.CreateCachedEditor(element.objectReferenceValue, null, ref editor);
                                    State_Editor.Add(key, editor);
                                }
                                editor.OnInspectorGUI();
                                editor.serializedObject.ApplyModifiedProperties();

                                //if (Application.isPlaying)
                                //     Repaint();
                            }
                        }
                    }
                }
            }
        }

        #region DrawStates 
        //-------------------------STATES-----------------------------------------------------------
        private void Draw_Header_State(Rect rect)
        {
            var r = new Rect(rect);
            r.x += 13;
            r.width -= 60;

            var Head = "    States";

            if (m.states != null && m.states.Count > 0 && SelectedState.intValue != -1 && SelectedState.intValue < m.states.Count)
            {
                var s = m.states[SelectedState.intValue];
                //  Head += $" [{s.GetType().Name}]";

                if (s != null && s.ID != null)
                {
                    Head += $"  ID: [{s.ID.ID}]";
                    Head += $"  Tag: [{s.ID.name}]";
                }
            }

            EditorGUI.LabelField(r, new GUIContent(Head, "States are the core logic the Animals can do [Double clic to modify them]"), EditorStyles.boldLabel);

            Rect R_2 = new(rect.width - 8, rect.y, 60, EditorGUIUtility.singleLineHeight - 3);
            EditorGUI.LabelField(R_2, new GUIContent("Priority", "Priority of the States, Higher value -> Higher priority"));
        }

        private void Selected_State(ReorderableList list)
        {
            SelectedState.intValue = list.index;

            var stateProperty = S_State_List.GetArrayElementAtIndex(list.index);

            //Update the Local State ID also
            states_C.GetArrayElementAtIndex(list.index).FindPropertyRelative("state").objectReferenceValue = stateProperty.objectReferenceValue;
        }

        private void Draw_Element_State(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 1;
            rect.height += 2;
            if (S_State_List.arraySize <= index) return;

            var stateProperty = S_State_List.GetArrayElementAtIndex(index);

            var activeRect = new Rect(rect);
            activeRect.width -= 20;
            activeRect.x += 20;

            var ActiveRect = new Rect(rect.x - 2, rect.y - 3, 20, activeRect.height);
            var StateRect = new Rect(activeRect.x - 5, activeRect.y, activeRect.width - 30, activeRect.height - 5);
            var PriorityRect = new Rect(activeRect.width + 45, activeRect.y, 25, activeRect.height - 2);

            if (Application.isPlaying) StateRect.width = activeRect.width / 2f + 10;


            State state = stateProperty.objectReferenceValue as State;

            // Remove the ability if it no longer exists.
            if (state == null)
            {
                EditorGUI.ObjectField(StateRect, stateProperty, GUIContent.none);
                return;
            }

            var stat_C = states_C.GetArrayElementAtIndex(index);

            var priority = stat_C.FindPropertyRelative("priority");

            var active = stat_C.FindPropertyRelative("active");
            active.boolValue = EditorGUI.Toggle(ActiveRect, GUIContent.none, Application.isPlaying ? state.Active : active.boolValue);

            //state.Active = EditorGUI.Toggle(ActiveRect, GUIContent.none, state.Active);

            var st_label = "";

            if (Application.isPlaying)
            {
                if (m.ActiveState == state)
                {
                    if (state.IsPending) st_label = "[Pending]";
                    else st_label = "[Active]";

                    if (state.IsPersistent) st_label += "[Pers]";
                }
                else if (state.IsSleepFromState) st_label = "[Sleep by State]";
                else if (state.IsSleepFromMode) st_label = "[Sleep by Mode]";
                else if (state.IsSleepFromStance) st_label = "[Sleep by Stance]";
                else if (state.OnActiveQueue) st_label = "[Active Queue]";
                else if (state.OnQueue) st_label = "[Queued]";
                else if (state.OnHoldByReset) st_label = "[On Hold Reset]";
            }

            var dbC = GUI.backgroundColor;
            GUI.backgroundColor = isActive ? MTools.MBlue : dbC;

            // var dC = GUI.contentColor;
            //   if (isActive) GUI.contentColor = new Color(0.7f, 0.7f, 2f);
            EditorGUI.ObjectField(StateRect, stateProperty, GUIContent.none);
            //  GUI.contentColor = dC;
            GUI.backgroundColor = dbC;
            var style = new GUIStyle(EditorStyles.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter };

            if (Application.isPlaying && m.isActiveAndEnabled && state != null)
            {
                var activestate = m.ActiveState;

                if (activestate != null)
                {
                    if (state.IsPersistent)
                    {
                        style.normal.textColor = Color.green;
                    }

                    if (state.Priority < activestate.Priority && activestate.IsPersistent)
                    {
                        style.normal.textColor = new Color(style.normal.textColor.r, style.normal.textColor.g, style.normal.textColor.b, style.normal.textColor.a / 2);
                    }
                }

                var Rect_Label = new Rect() { x = activeRect.width / 2 + 80, width = activeRect.width / 2 - 36, y = activeRect.y, height = activeRect.height };

                EditorGUI.LabelField(Rect_Label, st_label, style);
            }

            if (Application.isPlaying)
            {
                var TestBRect = new Rect(rect.width - 20, rect.y, 38, EditorGUIUtility.singleLineHeight);

                var color = GUI.color;
                GUI.color = MTools.MGreen * 1.5f;
                if (GUI.Button(TestBRect, "Test"))
                {
                    m.State_Force(state.ID);
                }
                GUI.color = color;
            }



            PriorityRect.height = 18;





            // state.Priority = EditorGUI.IntField(PriorityRect, GUIContent.none, state.Priority);
            priority.intValue = EditorGUI.IntField(PriorityRect, GUIContent.none, priority.intValue);

            activeRect = rect;
            activeRect.x += activeRect.width - 34;
            activeRect.width = 20;
        }

        private void OnReorderCallback_States(ReorderableList list, int oldIndex, int newIndex)
        {
            //Check if the Cache and States have the same size

            var OldState = m.states[oldIndex];
            var NewState = m.states[newIndex];

            if (OldState != null)
            {
                OldState.Priority = S_State_List.arraySize - newIndex;
                EditorUtility.SetDirty(OldState);
            }

            if (NewState != null)
            {
                NewState.Priority = S_State_List.arraySize - oldIndex;
                EditorUtility.SetDirty(NewState);
            }

            UpdateCacheState();

            //Do Chache stuff
            states_C.MoveArrayElement(oldIndex, newIndex);
            states_C.GetArrayElementAtIndex(oldIndex).FindPropertyRelative("priority").intValue = S_State_List.arraySize - oldIndex;
            states_C.GetArrayElementAtIndex(newIndex).FindPropertyRelative("priority").intValue = S_State_List.arraySize - newIndex;

            states_C.serializedObject.ApplyModifiedProperties();



            EditorUtility.SetDirty(target);
        }

        private void OnAddCallback_State(ReorderableList list)
        {
            addMenu = new GenericMenu();

            for (int i = 0; i < StatesType.Count; i++)
            {
                Type st = StatesType[i];

                bool founded = false;
                for (int j = 0; j < m.states.Count; j++)
                {
                    if (m.states[j].GetType() == st)
                    {
                        founded = true;
                    }
                }

                if (!founded)
                {
                    var att = st.GetCustomAttribute<AddTypeMenuAttribute>(false); //Find the correct name
                    string LabelName = att != null ? att.MenuName : st.Name;
                    addMenu.AddItem(new GUIContent(LabelName), false, () => AddState(st, st.Name));
                }
            }
            addMenu.ShowAsContext();
        }

        /// <summary> The ReordableList remove button has been pressed. Remove the selected ability.</summary>
        private void OnRemove_State(ReorderableList list)
        {
            // bool DeleteAsset = false;
            // State state = S_StateList.GetArrayElementAtIndex(list.index).objectReferenceValue as State;

            S_State_List.DeleteArrayElementAtIndex(list.index);
            states_C.DeleteArrayElementAtIndex(list.index);
            states_C.serializedObject.ApplyModifiedProperties();

            list.index -= 1;

            EditorUtility.SetDirty(m);
        }

        /// <summary>Adds a new State of the specified type.</summary>       
        private void AddState(Type selectedState, string name)
        {
            State state = (State)CreateInstance(selectedState);

            var nameS = m.name.RemoveSpecialCharacters();

            if (m.states != null && m.states.Count > 0)
            {
                var anySt = m.states[0];
                var path = AssetDatabase.GetAssetPath(anySt);

                path = System.IO.Path.GetDirectoryName(path);
                //Debug.Log("path = " + path);
                AssetDatabase.CreateAsset(state, $"{path}/{nameS} {name}.asset");
            }
            else
            {
                AssetDatabase.CreateAsset(state, $"Assets/{nameS} {name}.asset");
            }

            AssetDatabase.SaveAssets();

            // Pull all the information from the target of the serializedObject.
            S_State_List.serializedObject.Update();
            // Add a null array element to the start of the array then populate it with the object parameter.
            S_State_List.InsertArrayElementAtIndex(0);
            S_State_List.GetArrayElementAtIndex(0).objectReferenceValue = state;
            // Push all the information on the serializedObject back to the target.
            S_State_List.serializedObject.ApplyModifiedProperties();

            state.Priority = S_State_List.arraySize;  //Set the priority!! Important!

            AddState_Cache(state);
            state.SetSpeedSets(m);

            EditorUtility.SetDirty(target);
            EditorUtility.SetDirty(state);
        }

        #endregion

        /// <summary>
        /// This is used to Have local Active and Priority values
        /// </summary>
        private void UpdateCacheState()
        {
            //Use the same that is already on the states
            if (m.states_C == null || (m.states_C.Count != m.states.Count && m.states.Count > 0))
            {
                m.states_C = new List<MAnimal.StateCache>();

                foreach (var st in m.states)
                {
                    m.states_C.Add(new MAnimal.StateCache() { active = st.Active, priority = st.Priority, state = st });
                }
                EditorUtility.SetDirty(m);
                // return;

                Debug.Log($"<B>[{m.name}]</B> Local State Priority Value and Active Value Updated (AC v1.4.2c) Save the Prefab");
            }
        }
        private void AddState_Cache(State newState)
        {
            states_C.InsertArrayElementAtIndex(0);
            states_C.GetArrayElementAtIndex(0).FindPropertyRelative("state").objectReferenceValue = newState;
            states_C.GetArrayElementAtIndex(0).FindPropertyRelative("active").boolValue = true;
            states_C.GetArrayElementAtIndex(0).FindPropertyRelative("priority").intValue = newState.Priority;
            states_C.serializedObject.ApplyModifiedProperties();
        }


        public void DropAreaGUIStates()
        {
            EditorGUILayout.Space(5);

            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0f, 20, GUILayout.ExpandWidth(true));

            //var GC = GUI.backgroundColor;
            //GUI.backgroundColor = GC * 0.5f + Color.green * 0.5f;
            // GUI.backgroundColor = Color.green; ;

            var st = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };


            GUI.Box(drop_area, "> Drag created states here < ", st);
            //  GUI.backgroundColor = GC;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                    // ... change whether or not the drag *can* be performed by changing the visual mode of the cursor based on the IsDragValid function.
                    DragAndDrop.visualMode = IsDragValid() ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Rejected;
                    break;
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object is State)
                            {
                                State newState = dragged_object as State;

                                if (m.states.Contains(newState)) continue;

                                EditorUtility.SetDirty(m);

                                // Pull all the information from the target of the serializedObject.
                                S_State_List.serializedObject.Update();
                                // Add a null array element to the start of the array then populate it with the object parameter.
                                S_State_List.InsertArrayElementAtIndex(0);
                                S_State_List.GetArrayElementAtIndex(0).objectReferenceValue = newState;
                                // Push all the information on the serializedObject back to the target.
                                S_State_List.serializedObject.ApplyModifiedProperties();

                                AddState_Cache(newState);

                                Reo_List_States.index = -1;

                                EditorUtility.SetDirty(newState);
                            }
                        }
                    }
                    break;
            }
        }



        #endregion

        private static GUIContent Icon_Include;
        private static GUIContent Icon_Exclude;

        private void ShowStances()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Stances", EditorStyles.boldLabel);
                    MalbersEditor.DrawDebugIcon(DebugStances, MTools.MOrange);
                }
                EditorGUILayout.PropertyField(defaultStance, new GUIContent("Default Stance", "Default Stance ID to reset to when the animal exit an Stance"));
                EditorGUILayout.PropertyField(currentStance, new GUIContent("Current Stance", "Current Stance ID the animal is On"));

                Reo_List_Stances.index = SelectedStance.intValue;

                Reo_List_Stances.DoLayoutList();

            }
            var SnceIndex = Reo_List_Stances.index;
            if (SnceIndex != -1 && Stances_List.arraySize > 0 && SnceIndex < Stances_List.arraySize)
            {
                //EditorGUILayout.Space(-16);
                var SelectedStance = Stances_List.GetArrayElementAtIndex(SnceIndex);

                var ID = SelectedStance.FindPropertyRelative("ID").objectReferenceValue;
                var n = ID != null ? ID.name : "";

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))

                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(SelectedStance, new GUIContent($"Stance [{n}]"), false);
                    EditorGUI.indentLevel--;

                    if (SelectedStance != null && SelectedStance.isExpanded)
                    {
                        var Active = SelectedStance.FindPropertyRelative("enabled");
                        var Input = SelectedStance.FindPropertyRelative("Input");
                        var persistent = SelectedStance.FindPropertyRelative("persistent");
                        var CoolDown = SelectedStance.FindPropertyRelative("CoolDown");
                        var ExitAfter = SelectedStance.FindPropertyRelative("ExitAfter");
                        var CanStrafe = SelectedStance.FindPropertyRelative("CanStrafe");
                        var states = SelectedStance.FindPropertyRelative("states");
                        var StateQueue = SelectedStance.FindPropertyRelative("StateQueue");
                        var Include = SelectedStance.FindPropertyRelative("Include");
                        var DisableStances = SelectedStance.FindPropertyRelative("DisableStances");
                        var activeOnly = SelectedStance.FindPropertyRelative("activeOnly");
                        var OverrideCapsule = SelectedStance.FindPropertyRelative("OverrideCapsule");
                        var newCapsule = SelectedStance.FindPropertyRelative("newCapsule");
                        var MovementStrafe = SelectedStance.FindPropertyRelative("MovementStrafe");
                        var IdleStrafe = SelectedStance.FindPropertyRelative("IdleStrafe");


                        EditorGUILayout.PropertyField(Active);
                        EditorGUILayout.PropertyField(Input);
                        EditorGUILayout.PropertyField(CoolDown);
                        EditorGUILayout.PropertyField(ExitAfter);
                        EditorGUILayout.PropertyField(persistent);
                        EditorGUILayout.PropertyField(activeOnly);

                        using (new EditorGUI.DisabledGroupScope(!MainCollider.objectReferenceValue))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                EditorGUILayout.PropertyField(OverrideCapsule);
                                if (OverrideCapsule.boolValue && GUILayout.Button(new GUIContent("C", "Copy Main Capsule values"), GUILayout.Width(25)))
                                {
                                    m.Stances[SnceIndex].newCapsule = new(m.MainCollider);
                                    m.Stances[SnceIndex].newCapsule.modify = CapsuleModifier.height | CapsuleModifier.radius | CapsuleModifier.center;
                                    EditorUtility.SetDirty(m);
                                }
                            }

                            if (OverrideCapsule.boolValue)
                                EditorGUILayout.PropertyField(newCapsule);
                        }

                        var stance = m.Stances[SnceIndex];
                        var StanceName = stance.ID != null ? stance.ID.name : "-EMPTY-";

                        var inc = $"States the '{StanceName}' stance " + (Include.boolValue ? $"[can]" : "[cannot]") + " be activated.";

                        if (!stance.HasStates) inc += " [All]";


                        EditorGUILayout.PropertyField(CanStrafe);

                        if (stance.CanStrafe.Value)
                        {
                            EditorGUILayout.PropertyField(IdleStrafe);
                            EditorGUILayout.PropertyField(MovementStrafe);
                        }


                        using (new GUILayout.HorizontalScope())
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(states, new GUIContent(inc), true);
                            EditorGUI.indentLevel--;

                            if (states.arraySize > 0)
                            {
                                if (Icon_Include == null)
                                {
                                    Icon_Include = EditorGUIUtility.IconContent("d_winbtn_mac_max_h@2x");
                                    Icon_Include.tooltip = "Includes the States List for the stance activation";
                                }

                                if (Icon_Exclude == null)
                                {
                                    Icon_Exclude = EditorGUIUtility.IconContent("d_winbtn_mac_close_h@2x");
                                    Icon_Exclude.tooltip = "Excludes the States List for the stance activation";
                                }

                                var btn = Include.boolValue ? Icon_Include : Icon_Exclude;
                                Include.boolValue = GUILayout.Toggle(Include.boolValue, btn, EditorStyles.toolbarButton, GUILayout.Width(28));
                            }
                        }


                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(StateQueue, true);
                        EditorGUI.indentLevel--;

                        if (!stance.ActiveOnly)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(DisableStances, true);
                            EditorGUI.indentLevel--;
                        }


                        if (Application.isPlaying && m.HasStances)
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayout.IntField("Temp Disable", stance.DisableValue);
                                EditorGUILayout.Toggle("Active", stance.Active);
                                EditorGUILayout.Toggle("Queued", stance.Queued);
                                EditorGUILayout.Toggle("CanExit", stance.CanExit);
                                Repaint();
                            }
                        }
                    }
                }
            }

        }

        private bool IsDragValid()
        {
            // Go through all the objects being dragged...
            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                // ... and if any of them are not script assets, return that the drag is invalid.
                if (DragAndDrop.objectReferences[i].GetType().BaseType != typeof(State))
                    return false;
            }

            // If none of the dragging objects returned that the drag was invalid, return that it is valid.
            return true;
        }

        private void ShowAdvanced()
        {

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (Anim.isExpanded = MalbersEditor.Foldout(Anim.isExpanded, "References"))
                {
                    EditorGUILayout.PropertyField(Anim, new GUIContent("Animator"));
                    EditorGUILayout.PropertyField(RB, new GUIContent("RigidBody"));
                    EditorGUILayout.PropertyField(UseMainCameraDirection);

                    EditorGUILayout.PropertyField(MainCamera, new GUIContent("Main Camera"));
                    EditorGUILayout.PropertyField(Aimer);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (Rotator.isExpanded = MalbersEditor.Foldout(Rotator.isExpanded, "Free Movement"))
                {
                    EditorGUILayout.PropertyField(Rotator, G_Rotator);
                    EditorGUILayout.PropertyField(RootBone, G_RootBone);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (sleep.isExpanded = MalbersEditor.Foldout(sleep.isExpanded, "Lock Inputs"))
                {
                    EditorGUILayout.PropertyField(sleep, new GUIContent("Sleep", "Disable internally the Controller wihout disabling the component"));
                    EditorGUILayout.PropertyField(lockInput);
                    EditorGUILayout.PropertyField(lockMovement);
                    EditorGUILayout.PropertyField(LockForwardMovement, new GUIContent("Lock Forward"));
                    EditorGUILayout.PropertyField(LockHorizontalMovement, new GUIContent("Lock Horizontal"));
                    EditorGUILayout.PropertyField(LockUpDownMovement, new GUIContent("Lock UpDown"));
                }
            }
            ShowAnimParam();

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                animalType.isExpanded = MalbersEditor.Foldout(animalType.isExpanded, "Extras");

                if (animalType.isExpanded)
                {
                    // EditorGUILayout.PropertyField(NoParent);
                    EditorGUILayout.PropertyField(animalType, G_animalType);
                    EditorGUILayout.PropertyField(kinematicTimeline);
                }
            }
        }

        private void ShowAnimParam()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var v_float = UnityEngine.AnimatorControllerParameterType.Float;
                var v_int = UnityEngine.AnimatorControllerParameterType.Int;
                var v_bool = UnityEngine.AnimatorControllerParameterType.Bool;
                var v_trigger = UnityEngine.AnimatorControllerParameterType.Trigger;
                var anim = m.Anim;

                using (new GUILayout.HorizontalScope())
                {

                    m_Vertical.isExpanded = MalbersEditor.Foldout(m_Vertical.isExpanded, "Required Animator Parameters");

                    if (m_Vertical.isExpanded)
                    {
                        if (GUILayout.Button(new GUIContent("*", "Check Required Parameters"), GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            MalbersEditor.CheckAnimParameter(anim, m_StateOn.stringValue, v_trigger);
                            MalbersEditor.CheckAnimParameter(anim, m_ModeOn.stringValue, v_trigger);
                            MalbersEditor.CheckAnimParameter(anim, m_Vertical.stringValue, v_float);

                            MalbersEditor.CheckAnimParameter(anim, m_Horizontal.stringValue, v_float);
                            MalbersEditor.CheckAnimParameter(anim, m_State.stringValue, v_int);
                            MalbersEditor.CheckAnimParameter(anim, m_LastState.stringValue, v_int);
                            MalbersEditor.CheckAnimParameter(anim, m_StateStatus.stringValue, v_int);
                            MalbersEditor.CheckAnimParameter(anim, m_StateFloat.stringValue, v_float);
                            MalbersEditor.CheckAnimParameter(anim, m_Mode.stringValue, v_int);
                            MalbersEditor.CheckAnimParameter(anim, m_ModeStatus.stringValue, v_int);
                            MalbersEditor.CheckAnimParameter(anim, m_Grounded.stringValue, v_bool);
                            MalbersEditor.CheckAnimParameter(anim, m_Movement.stringValue, v_bool);
                            MalbersEditor.CheckAnimParameter(anim, m_SpeedMultiplier.stringValue, v_float);
                        }
                    }
                }

                if (m_Vertical.isExpanded)
                {
                    MalbersEditor.DisplayParam(anim, m_StateOn, v_trigger);
                    MalbersEditor.DisplayParam(anim, m_ModeOn, v_trigger);
                    EditorGUILayout.Space();
                    MalbersEditor.DisplayParam(anim, m_Vertical, v_float);
                    MalbersEditor.DisplayParam(anim, m_Horizontal, v_float);
                    EditorGUILayout.Space();
                    MalbersEditor.DisplayParam(anim, m_State, v_int);
                    MalbersEditor.DisplayParam(anim, m_LastState, v_int);
                    MalbersEditor.DisplayParam(anim, m_StateStatus, v_int);
                    MalbersEditor.DisplayParam(anim, m_StateFloat, v_float);
                    EditorGUILayout.Space();
                    MalbersEditor.DisplayParam(anim, m_Mode, v_int);
                    MalbersEditor.DisplayParam(anim, m_ModeStatus, v_int);
                    EditorGUILayout.Space();
                    MalbersEditor.DisplayParam(anim, m_Grounded, v_bool);
                    MalbersEditor.DisplayParam(anim, m_Movement, v_bool);
                    MalbersEditor.DisplayParam(anim, m_SpeedMultiplier, v_float);
                }

                m_UpDown.isExpanded = MalbersEditor.Foldout(m_UpDown.isExpanded, "Optional Animator Parameters");

                if (m_UpDown.isExpanded)
                {
                    MalbersEditor.DisplayParam(anim, m_UpDown, v_float);
                    MalbersEditor.DisplayParam(anim, m_DeltaUpDown, v_float);
                    MalbersEditor.DisplayParam(anim, m_VerticalRaw, v_float);
                    MalbersEditor.DisplayParam(anim, m_TargetAngle, v_float);
                    MalbersEditor.DisplayParam(anim, m_Sprint, v_bool);
                    EditorGUILayout.Space();

                    MalbersEditor.DisplayParam(anim, m_StateProfile, v_int);
                    MalbersEditor.DisplayParam(anim, m_StateExitStatus, v_int);
                    MalbersEditor.DisplayParam(anim, m_StateTime, v_float);
                    EditorGUILayout.Space();

                    MalbersEditor.DisplayParam(anim, m_Stance, v_int);
                    MalbersEditor.DisplayParam(anim, m_LastStance, v_int);

                    EditorGUILayout.Space();

                    MalbersEditor.DisplayParam(anim, m_ModePower, v_float);
                    EditorGUILayout.Space();

                    MalbersEditor.DisplayParam(anim, m_Slope, v_float);
                    MalbersEditor.DisplayParam(anim, m_Random, v_int);
                    MalbersEditor.DisplayParam(anim, m_StrafeAnim, v_bool);
                    //   DisplayParam(m_TargetHorizontal, v_float);
                    EditorGUILayout.Space();
                    MalbersEditor.DisplayParam(anim, m_Type, v_int);
                }
            }
        }

        private void ShowEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);

                Editor_EventTabs.intValue = GUILayout.Toolbar(Editor_EventTabs.intValue,
                    new string[] { "Movement", "State", "Stance", "Modes", "Speeds", "Extras" }, EditorStyles.toolbarButton);

                switch (Editor_EventTabs.intValue)
                {
                    case 0: //Movement
                        EditorGUILayout.PropertyField(OnSprintEnabled, new GUIContent("On Sprint"));
                        EditorGUILayout.PropertyField(OnMovementDetected);
                        EditorGUILayout.PropertyField(OnFreeMovement);
                        EditorGUILayout.PropertyField(OnGrounded);
                        EditorGUILayout.Space();

                        if (m.CanStrafe)
                        {
                            EditorGUILayout.PropertyField(OnStrafe);
                        }
                        //EditorGUILayout.PropertyField(OnMaxSlopeReached);
                        EditorGUILayout.PropertyField(OnPreTeleport);
                        EditorGUILayout.PropertyField(OnTeleport);
                        EditorGUILayout.PropertyField(OnGroundChangesGravity);
                        EditorGUILayout.Space();
                        break;
                    case 1: //States
                        EditorGUILayout.PropertyField(OnStateChange);
                        EditorGUILayout.PropertyField(OnStateProfile);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(OnEnterExitStates);
                        EditorGUI.indentLevel--;
                        break;
                    case 2: //Stances
                        EditorGUILayout.PropertyField(OnStanceChange);
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(OnEnterExitStances);
                        EditorGUI.indentLevel--;
                        break;
                    case 3: //Modes
                        EditorGUILayout.PropertyField(OnModeStart);
                        EditorGUILayout.PropertyField(OnModeEnd);
                        EditorGUILayout.Space();

                        for (int i = 0; i < S_Mode_List.arraySize; i++)
                        {
                            var SelectedMode = S_Mode_List.GetArrayElementAtIndex(i);

                            var ID = SelectedMode.FindPropertyRelative("ID").objectReferenceValue;
                            var ModeName = ID != null ? ID.name : "";


                            var expanded = SelectedMode.FindPropertyRelative("allowRotation");

                            expanded.isExpanded = MalbersEditor.Foldout(expanded.isExpanded, $"Mode [{ModeName}] Events");

                            if (expanded.isExpanded)
                            {
                                var OnEnterMode = SelectedMode.FindPropertyRelative("OnEnterMode");
                                var OnExitMode = SelectedMode.FindPropertyRelative("OnExitMode");
                                EditorGUILayout.PropertyField(OnEnterMode, new GUIContent($"On [{ModeName}] Enter "));
                                EditorGUILayout.PropertyField(OnExitMode, new GUIContent($"On [{ModeName}] Exit"));
                                EditorGUILayout.PropertyField(expanded, new GUIContent($"On [{ModeName}] Active Ability Index changed"));
                            }
                        }


                        break;
                    case 4:
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(OnEnterExitSpeeds);
                        EditorGUI.indentLevel--;
                        EditorGUILayout.PropertyField(OnSpeedChange);
                        break;

                    case 5:
                        EditorGUILayout.PropertyField(OnMovementLocked);
                        EditorGUILayout.PropertyField(OnInputLocked);
                        EditorGUILayout.Space();

                        EditorGUILayout.PropertyField(OnAnimationChange);
                        break;
                    default:
                        break;
                }

                EditorGUI.indentLevel--;
            }
        }
        public static void DrawDebugButton(SerializedProperty property, GUIContent name, Color Highlight)
        {
            var currentGUIColor = GUI.color;
            GUI.color = property.boolValue ? Highlight : currentGUIColor;
            property.boolValue = GUILayout.Toggle(property.boolValue, name, EditorStyles.miniButton);
            GUI.color = currentGUIColor;
        }

        #region Debug Stuff

        private void ShowDebug()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var Deb = serializedObject.FindProperty("debugStates");
                var DebG = serializedObject.FindProperty("debugGizmos");

                using (new GUILayout.HorizontalScope())
                {
                    var DebColor = Color.red + Color.white;

                    DrawDebugButton(Deb, new GUIContent(" States", "Activate debbuging on the States"), DebColor);
                    DrawDebugButton(DebugModes, new GUIContent(" Modes", "Activate debbuging on the Modes"), DebColor);
                    DrawDebugButton(DebugStances, new GUIContent(" Stances", "Activate debbuging on the Stances"), DebColor);
                    DrawDebugButton(DebG, new GUIContent(" Gizmos", "Show States and Modes Gizmos"), DebColor);
                    DrawDebugButton(showPivots, new GUIContent(" Pivots", "Show Animal Pivos"), DebColor);
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("RUNTIME DATA", EditorStyles.boldLabel);

                    if (Application.isPlaying)
                    {

                        EditorGUIUtility.labelWidth = 120;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowOnGUIData"));
                        EditorGUIUtility.labelWidth = 00;
                    }
                }
                // var TypeStyle = new GUIStyle(EditorStyles.objectField);

                Runtime_Tabs1.intValue = GUILayout.Toolbar(Runtime_Tabs1.intValue, DebugTab1, EditorStyles.toolbarButton);
                if (Runtime_Tabs1.intValue != 4) Runtime_Tabs2.intValue = 4;

                Runtime_Tabs2.intValue = GUILayout.Toolbar(Runtime_Tabs2.intValue, DebugTab2, EditorStyles.toolbarButton);
                if (Runtime_Tabs2.intValue != 4) Runtime_Tabs1.intValue = 4;

            }


            if (Application.isPlaying)
            {

                using (new EditorGUI.DisabledGroupScope(true))
                {

                    switch (Runtime_Tabs1.intValue)
                    {
                        case 0:
                            DrawDebugData();
                            break;
                        case 1: //State
                            DrawStateData();
                            break;

                        case 2: //Ground
                            DebugGroundData();
                            break;
                        case 3: //Speed
                            DrawSpeeds();
                            break;
                        default:
                            break;
                    }


                    switch (Runtime_Tabs2.intValue)
                    {
                        case 0: //Input
                            DebugInputData();
                            break;
                        case 1: //Mode
                            DebugModeData();
                            break;
                        case 2: //Forces
                            DebugForcesData();
                            break;
                        case 3: //Movement
                            DebugMoveData();
                            break;

                        default:
                            break;
                    }

                    Repaint();
                }
            }
        }

        private void DrawSpeeds()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"RigidBody Horizontal Speed: [{m.HorizontalSpeed:F4}]", EditorStyles.boldLabel);
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m.CurrentSpeedSet != null)
                    EditorGUILayout.LabelField
                        ($"Set: [{m.CurrentSpeedSet.name}] -  Speed: [{m.CurrentSpeedModifier.name}]. " +
                        $"Current Index: [{m.CurrentSpeedIndex}]", EditorStyles.boldLabel);
                DisplayActiveSpeed();
            }
        }

        private void DrawStateData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ObjectField($"Active State: [{m.ActiveState.ID.name}] ({m.ActiveState.ID.ID})", m.ActiveState, typeof(State), false);

                var M = m.ActiveState;
                EditorGUILayout.ToggleLeft("Is Active State", M.IsActiveState);
                StateData(M);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m.LastState)
                {
                    EditorGUILayout.ObjectField($"Last State: [{m.LastState.ID.name}] ({m.LastState.ID.ID})", m.LastState, typeof(State), false);
                    StateData(m.LastState);
                }
            }



            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ObjectField("Stance", m.Stance, typeof(StanceID), false);
                if (m.ActiveStance != null) EditorGUILayout.ToggleLeft("Stance Input", m.ActiveStance.InputValue);
            }
        }

        private void StateData(State M)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUIUtility.labelWidth = 50;
                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.ToggleLeft("Enabled", M.Active);
                    EditorGUILayout.ToggleLeft("In Core Animation", M.InCoreAnimation);
                    EditorGUILayout.ToggleLeft("Can Exit", M.CanExit);
                    EditorGUILayout.ToggleLeft("OnQueue", M.OnQueue);
                    EditorGUILayout.ToggleLeft("On Active Queue", M.OnActiveQueue);
                    EditorGUILayout.ToggleLeft("Pending", M.IsPending);
                    EditorGUILayout.ToggleLeft("Always Forward", M.AlwaysForward);
                }
                using (new GUILayout.VerticalScope())
                {
                    EditorGUILayout.ToggleLeft("Sleep From State", M.IsSleepFromState);
                    EditorGUILayout.ToggleLeft("Sleep From Mode", M.IsSleepFromMode);
                    EditorGUILayout.ToggleLeft("Sleep From Stance", M.IsSleepFromStance);
                    EditorGUILayout.ToggleLeft("Ignore Lower States", M.IgnoreLowerStates);
                    EditorGUILayout.ToggleLeft("Is Persistent", M.IsPersistent);
                    EditorGUILayout.ToggleLeft("On Hold by Reset", M.OnHoldByReset);
                    EditorGUILayout.ToggleLeft("Input Value", M.InputValue);
                }
                EditorGUIUtility.labelWidth = 0;
            }
        }

        private void DebugMoveData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.FloatField("HorizontalSpeed ", m.HorizontalSpeed);
                EditorGUILayout.Vector3Field("Horizontal Velocity ", m.HorizontalVelocity.Round(3) * m.DeltaTime);
                EditorGUILayout.Vector3Field("Inertia ", m.Inertia.Round(3));
                EditorGUILayout.Vector3Field("Inertia Speed ", m.InertiaPositionSpeed.Round(3));
                EditorGUILayout.Vector3Field("Target Speed ", m.TargetSpeed.Round(3));
                EditorGUILayout.Vector3Field("Pitch Direction", m.PitchDirection.Round(3));
                EditorGUILayout.Vector3Field("Delta Pos ", m.DeltaPos.Round(3));
                EditorGUILayout.Vector3Field("Delta RM ", m.DeltaRootMotion.Round(3));
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Vector3Field("Raw Input Axis", m.RawInputAxis.Round(3));
                EditorGUILayout.Vector3Field("Movement Direction", m.Move_Direction.Round(3));
                EditorGUILayout.Vector3Field("Movement Axis Raw", m.MovementAxisRaw.Round(3));
                EditorGUILayout.Vector3Field("Movement Axis", m.MovementAxis.Round(3));
                EditorGUILayout.Vector3Field("Movement Smooth", m.MovementAxisSmoothed.Round(3));
                EditorGUILayout.Toggle("Disable Position", m.DisablePosition);
                EditorGUILayout.Toggle("Disable Rotation", m.DisableRotation);
            }
        }
        private void DebugInputData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.FloatField("Delta Angle", m.DeltaAngle);
                EditorGUILayout.FloatField("Pitch Angle", m.PitchAngle);
                EditorGUILayout.FloatField("Bank", m.Bank);

                EditorGUILayout.Toggle("Rotate at Direction", m.Rotate_at_Direction);
                EditorGUILayout.Toggle("Move with Direction", m.UsingMoveWithDirection);
                EditorGUILayout.Toggle("Use Raw Input", m.UseRawInput);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Vector3Field("Raw Input Axis", m.RawInputAxis.Round(3));
                EditorGUILayout.Vector3Field("Raw Rotate Axis", m.RawRotateDirAxis.Round(3));
                EditorGUILayout.Vector3Field("Movement Direction", m.Move_Direction.Round(3));
                EditorGUILayout.Vector3Field("Movement Axis Raw", m.MovementAxisRaw.Round(3));
                EditorGUILayout.Vector3Field("Movement Axis", m.MovementAxis.Round(3));
                EditorGUILayout.Vector3Field("Movement Smooth", m.MovementAxisSmoothed.Round(3));

            }
        }

        private void DebugGroundData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ObjectField("Platform", m.platform, typeof(Transform), false);
                EditorGUILayout.ObjectField("Ground Changer", m.GroundChanger, typeof(GroundSpeedChanger), false);
                EditorGUILayout.FloatField("Terrain Slope", m.TerrainSlope);
                EditorGUILayout.FloatField("Main Pivot Slope", m.MainPivotSlope);
                EditorGUILayout.FloatField("Slope Normalized", m.SlopeNormalized);
                EditorGUILayout.FloatField("Slope Dir Angle", m.SlopeDirectionAngle);
                EditorGUILayout.FloatField("Slope Limit", m.SlopeLimit);
                EditorGUILayout.FloatField("Slope  Angle Difference", m.SlopeAngleDifference);
                EditorGUILayout.ToggleLeft("Deep Slope", m.DeepSlope);
                EditorGUILayout.ToggleLeft("Use Oritent To Ground", m.UseOrientToGround);
                EditorGUILayout.Vector3Field("Slope Direction", m.SlopeDirection);
                EditorGUILayout.Vector3Field("Slope Direction Sm", m.SlopeDirectionSmooth);
                EditorGUILayout.Vector3Field("Surface Normal", m.SurfaceNormal);
            }
        }

        private void DebugForcesData()
        {
            EditorGUIUtility.labelWidth = 80;
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.FloatField("Gravity Time", m.GravityTime, GUILayout.MinWidth(50));
                    EditorGUILayout.FloatField("Gravity Mult", m.GravityMultiplier, GUILayout.MinWidth(50));
                }
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.FloatField("Delta Time", m.DeltaTime, GUILayout.MinWidth(50));
                    EditorGUILayout.FloatField("Time Scale", Time.timeScale, GUILayout.MinWidth(50));
                }
                EditorGUILayout.Space();


                EditorGUIUtility.labelWidth = 120;
                EditorGUILayout.Vector3Field("Gravity Velocity", m.GravityStoredVelocity);
                EditorGUILayout.Vector3Field("Gravity Offset", m.GravityOffset);
                EditorGUILayout.FloatField("Gravity ExPower", m.GravityExtraPower, GUILayout.MinWidth(50));
            }

            EditorGUIUtility.labelWidth = 0;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Vector3Field("External Force", m.CurrentExternalForce);
                EditorGUILayout.Vector3Field("External Force Max", m.ExternalForce);
                EditorGUILayout.FloatField("External Force Acel", m.ExternalForceAcel);
                EditorGUILayout.ToggleLeft("Force Air Control ?", m.ExternalForceAirControl);
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Vector3Field("Slope Direction", m.SlopeDirection);
                EditorGUILayout.Vector3Field("Slope Dir Smooth", m.SlopeDirectionSmooth);
                EditorGUILayout.FloatField("SlopeAngle", m.SlopeDirectionAngle);
            }
        }

        private void DebugModeData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {

                    EditorGUILayout.ObjectField("Active Mode: ", m.IsPlayingMode ? m.ActiveMode.ID : null, typeof(ModeID), false);
                    EditorGUILayout.ObjectField("In Zone: ", m.InZone ? m.Zone.transform : null, typeof(Transform), false);


                    EditorGUILayout.TextField("Ability: ", (m.ActiveMode != null && m.ActiveMode.ActiveAbility != null) ?
                        "[" + m.ActiveMode.ActiveAbility.Index.Value + "]" + m.ActiveMode.ActiveAbility.Name : "");

                    EditorGUILayout.ToggleLeft("Ability Input Value  ", m.IsPlayingMode ? m.ActiveMode.ActiveAbility.InputValue : false);

                }

                EditorGUIUtility.labelWidth = 70;
                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.ToggleLeft("Input Value  ", m.IsPlayingMode ? m.ActiveMode.InputValue : false);
                        EditorGUILayout.ToggleLeft("Playing Mode", m.IsPlayingMode);
                    }
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.ToggleLeft("Preparing Mode", m.IsPreparingMode);
                        EditorGUILayout.ToggleLeft("Mode In Transition", m.ActiveMode != null && m.ActiveMode.IsInTransition);
                    }
                }

                EditorGUIUtility.labelWidth = 90;
                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if (m.LastMode != null) EditorGUILayout.IntField("Last Mode ID", m.LastMode.ID);
                        EditorGUILayout.IntField("ModeID-Ability", m.ModeAbility);
                    }

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        if (m.LastMode != null) EditorGUILayout.IntField("Last Mode Ability", m.LastMode.AbilityIndex);
                        EditorGUILayout.FloatField("Mode Time", m.ModeTime);
                    }
                }
            }
            EditorGUIUtility.labelWidth = 0;
        }

        private void DrawDebugData()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.ToggleLeft("Using Camera Input", m.UseCameraInput);
                EditorGUILayout.ObjectField("In Zone: ", m.InZone ? m.Zone.transform : null, typeof(Transform), false);
                EditorGUILayout.IntField("Current Anim Tag", m.AnimStateTag);
                EditorGUILayout.FloatField("Strafe Delta", m.StrafeDeltaValue);
            }


            EditorGUIUtility.labelWidth = 70;
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.ToggleLeft("++ [Pos]", m.UseAdditivePos);
                    EditorGUILayout.ToggleLeft("RootMotion", m.RootMotion);
                    EditorGUILayout.ToggleLeft("RootMotion Rot", m.RootMotionRotation);
                    EditorGUILayout.ToggleLeft("Orient To Ground", m.UseOrientToGround);
                    EditorGUILayout.ToggleLeft("Chest Ray", m.FrontRay);
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.ToggleLeft("++ [Rotation]", m.UseAdditiveRot);
                    EditorGUILayout.ToggleLeft("Grounded", m.Grounded);
                    EditorGUILayout.ToggleLeft("Use Custom Rot", m.UseCustomRotation);
                    EditorGUILayout.ToggleLeft("Strafe", m.Strafe);
                    EditorGUILayout.ToggleLeft("Hip Ray", m.MainRay);
                }
            }


            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.ToggleLeft("Move with Direction", m.UsingMoveWithDirection);
                    EditorGUILayout.ToggleLeft("Raw Input", m.UseRawInput);
                    EditorGUILayout.ToggleLeft("Use Sprint", m.UseSprint);
                    EditorGUILayout.ToggleLeft("Input Locked", m.LockInput);
                    EditorGUILayout.ToggleLeft("Free Move", m.FreeMovement);
                }


                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.ToggleLeft("Rotate At Direction", m.Rotate_at_Direction);
                    EditorGUILayout.ToggleLeft("Movement Detected", m.MovementDetected);
                    EditorGUILayout.ToggleLeft("Sprint", m.Sprint);
                    EditorGUILayout.ToggleLeft("Movement Locked", m.LockMovement);
                    EditorGUILayout.ToggleLeft("Use Gravity", m.UseGravity);
                }
            }

            EditorGUIUtility.labelWidth = 0;
        }

        #endregion

        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        private void ShowModes()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(StartWithMode, G_StartWithMode);
                MalbersEditor.DrawDebugIcon(DebugModes, MTools.MGreen);


                if (Application.isPlaying)
                {
                    var color = GUI.color;
                    GUI.color = MTools.MGreen * 1.5f;
                    if (GUILayout.Button(new GUIContent("Interrupt", "Interrupt Mode"), GUILayout.Width(60)))
                    {
                        m.Mode_Interrupt();
                    }
                    GUI.color = color;
                }
            }

            Reo_List_Modes.index = SelectedMode.intValue;
            var index = SelectedMode.intValue;

            //On Empty List
            if (S_Mode_List.arraySize == 0 || SelectedMode.intValue == -1)
            {
                Reo_List_Modes.DoLayoutList();        //Paint the Reordable List
            }

            if (index != -1 && m.modes.Count > 0 && index < m.modes.Count && S_Mode_List != null && S_Mode_List.arraySize > 0)
            {
                var CurrentMode = S_Mode_List.GetArrayElementAtIndex(index);

                var ID = CurrentMode.FindPropertyRelative("ID").objectReferenceValue;
                var SelectedAbilityIndexEditor = CurrentMode.FindPropertyRelative("SelectedAbilityIndexEditor");
                var ModeName = ID != null ? ID.name : "";
                var ModeID = ID != null ? (ID as ModeID).ID : -1;

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    if (targets != null && targets.Length > 1)
                    {
                        EditorGUILayout.EndVertical();
                        return; //Do not show Multiple Animals
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        showModeList.boolValue = MalbersEditor.Foldout(showModeList.boolValue, $"Modes List [{S_Mode_List.arraySize}].    Selected:[{ModeName}]     ID:[{ModeID}]");


                        //if (!showModeList.boolValue)
                        //{
                        //    SelectedMode.intValue = EditorGUILayout.Popup( SelectedMode.intValue, ModePopupList, popupStyle, GUILayout.Width(150));
                        //}
                    }

                    if (showModeList.boolValue)
                    {
                        Reo_List_Modes.DoLayoutList();        //Paint the Reordable List
                    }
                }

                if (CurrentMode != null)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        //Mode Title
                        //var contentColor = GUI.contentColor;
                        //  GUI.contentColor = Color.green;

                        using (new GUILayout.HorizontalScope())

                        {
                            if (!showModeList.boolValue)
                            {
                                SelectedMode.intValue = EditorGUILayout.Popup(SelectedMode.intValue, ModePopupList, popupStyle, GUILayout.Width(15));
                            }

                            EditorGUI.indentLevel++;
                            CurrentMode.isExpanded = GUILayout.Toggle(CurrentMode.isExpanded, new GUIContent("Mode"), EditorStyles.foldoutHeader);
                        }
                        //  GUI.contentColor = contentColor;

                        GUILayout.Space(-20);
                        var IDD = CurrentMode.FindPropertyRelative("ID");

                        EditorGUIUtility.labelWidth = 80;
                        using (new EditorGUI.DisabledGroupScope(true))
                            EditorGUILayout.ObjectField(IDD, new GUIContent("  "), GUILayout.MinWidth(50));
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.indentLevel--;


                        if (Application.isPlaying)
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                var Temporal = CurrentMode.FindPropertyRelative("TemporalActivation");
                                EditorGUILayout.PropertyField(Temporal);
                            }
                        }


                        GUILayout.Space(5);

                        if (CurrentMode.isExpanded)
                        {
                            Mode_Tabs1.intValue = GUILayout.Toolbar(Mode_Tabs1.intValue, new string[4] { "General", "Abilities", "Events", "Reactions" });

                            switch (Mode_Tabs1.intValue)
                            {
                                case 0:
                                    var Input = CurrentMode.FindPropertyRelative("Input");
                                    var active = CurrentMode.FindPropertyRelative("active");
                                    var hasCoolDown = CurrentMode.FindPropertyRelative("hasCoolDown");
                                    var CoolDown = CurrentMode.FindPropertyRelative("CoolDown");
                                    var allowRotation = CurrentMode.FindPropertyRelative("allowRotation");
                                    var m_Source = CurrentMode.FindPropertyRelative("m_Source");
                                    var allowMovement = CurrentMode.FindPropertyRelative("allowMovement");
                                    var modifier = CurrentMode.FindPropertyRelative("modifier");
                                    var ignoreLowerModes = CurrentMode.FindPropertyRelative("ignoreLowerModes");
                                    var forceIgnorePriority = CurrentMode.FindPropertyRelative("forceIgnorePriority");
                                    var EnterConditions = CurrentMode.FindPropertyRelative("EnterConditions");
                                    var InterruptConditions = CurrentMode.FindPropertyRelative("InterruptConditions");
                                    var ExitConditions = CurrentMode.FindPropertyRelative("ExitConditions");

                                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUILayout.PropertyField(Input);

                                        EditorGUILayout.PropertyField(active);
                                        EditorGUILayout.PropertyField(ignoreLowerModes,
                                            new GUIContent("Ignore Lower", "It will play this mode even if another Lower Priority Mode is playing"));
                                        EditorGUILayout.PropertyField(forceIgnorePriority);

                                        EditorGUILayout.PropertyField(hasCoolDown);
                                        if (hasCoolDown.boolValue)
                                            EditorGUILayout.PropertyField(CoolDown);
                                        EditorGUILayout.PropertyField(allowRotation, new GUIContent("Allow Rotation", "Allows rotate while is on the Mode"));
                                        EditorGUILayout.PropertyField(allowMovement, new GUIContent("Allow Movement", "Allows movement while is on the Mode"));

                                        EditorGUILayout.PropertyField(modifier, G_Modifier);
                                        EditorGUILayout.PropertyField(m_Source);
                                        EditorGUILayout.PropertyField(EnterConditions);
                                        EditorGUILayout.PropertyField(InterruptConditions);
                                        EditorGUILayout.PropertyField(ExitConditions);
                                    }

                                    if (Application.isPlaying)
                                    {
                                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                        {
                                            using (new EditorGUI.DisabledGroupScope(true))
                                            {
                                                EditorGUILayout.Toggle("Playing Mode", m.modes[index].PlayingMode);
                                                EditorGUILayout.Toggle("Input Value", m.modes[index].InputValue);
                                                EditorGUILayout.Toggle("In CoolDown", m.modes[index].InCoolDown);
                                                EditorGUILayout.IntField("Temporal Activation", m.modes[index].TemporalActivation);
                                            }
                                        }
                                    }


                                    break;
                                case 1:
                                    var AbilityIndex = CurrentMode.FindPropertyRelative("m_AbilityIndex");
                                    var DefaultIndex = CurrentMode.FindPropertyRelative("DefaultIndex");
                                    var ResetToDefault = CurrentMode.FindPropertyRelative("ResetToDefault");
                                    var Abilities = CurrentMode.FindPropertyRelative("Abilities");

                                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                                    {
                                        EditorGUIUtility.labelWidth = 70;
                                        EditorGUILayout.PropertyField(AbilityIndex, G_AbilityIndex, GUILayout.MinWidth(50));
                                        EditorGUILayout.PropertyField(DefaultIndex, G_DefaultIndex, GUILayout.MinWidth(50));
                                        EditorGUIUtility.labelWidth = 0;
                                        ResetToDefault.boolValue = GUILayout.Toggle(ResetToDefault.boolValue, G_ResetToDefault, EditorStyles.miniButton, GUILayout.Width(20));
                                    }

                                    using (new GUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField("[If Active Ability Index is -99, the mode will play a random ability]", DescriptionStyle);
                                        if (Application.isPlaying)
                                        {
                                            var color = GUI.color;
                                            GUI.color = MTools.MGreen * 1.5f;
                                            if (GUILayout.Button(new GUIContent("Interrupt", "Interrupt Mode"), GUILayout.Width(60)))
                                            {
                                                m.Mode_Interrupt();
                                            }

                                            if (GUILayout.Button(new GUIContent("Stop", "Stop Mode"), GUILayout.Width(60)))
                                            {
                                                m.Mode_Stop();
                                            }
                                            GUI.color = color;
                                        }
                                    }

                                    DrawAbilities(index, CurrentMode, Abilities);
                                    break;
                                case 2:
                                    var OnEnterMode = CurrentMode.FindPropertyRelative("OnEnterMode");
                                    var OnAbilityIndex = CurrentMode.FindPropertyRelative("OnAbilityIndex");
                                    var OnExitMode = CurrentMode.FindPropertyRelative("OnExitMode");

                                    EditorGUILayout.PropertyField(OnEnterMode, new GUIContent($"On [{ModeName}] Enter "));
                                    EditorGUILayout.PropertyField(OnExitMode, new GUIContent($"On [{ModeName}] Exit"));
                                    EditorGUILayout.PropertyField(OnAbilityIndex, new GUIContent($"On [{ModeName}] Active Ability Index changed "));
                                    break;

                                case 3:
                                    var OnEnterReaction = CurrentMode.FindPropertyRelative("OnEnterReaction");
                                    var OnExitReaction = CurrentMode.FindPropertyRelative("OnExitReaction");

                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(OnEnterReaction, new GUIContent($"On [{ModeName}] Enter Reaction"));
                                    EditorGUILayout.PropertyField(OnExitReaction, new GUIContent($"On [{ModeName}] Exit Reaction "));
                                    EditorGUI.indentLevel--;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void DrawAbilities(int ModeIndex, SerializedProperty SelectedMode, SerializedProperty Abilities)
        {
            ReorderableList Reo_AbilityList;

            string listKey = SelectedMode.propertyPath;

            var SelectedAbility = SelectedMode.FindPropertyRelative("SelectedAbilityIndexEditor");

            if (Reo_Abilities.ContainsKey(listKey))
            {
                // fetch the reorderable list in dict
                Reo_AbilityList = Reo_Abilities[listKey];
            }
            else
            {
                Reo_AbilityList = new ReorderableList(SelectedMode.serializedObject, Abilities, true, true, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;

                        var element = Abilities.GetArrayElementAtIndex(index);

                        var IndexValue = element.FindPropertyRelative("Index");
                        var name = element.FindPropertyRelative("Name");

                        var Active = element.FindPropertyRelative("active");


                        var ConstValue = Active.FindPropertyRelative("ConstantValue");
                        var VarValue = Active.FindPropertyRelative("Variable");
                        var useConstant = Active.FindPropertyRelative("UseConstant").boolValue;
                        BoolVar variable = VarValue.objectReferenceValue as BoolVar;

                        var IDRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };

                        var ActiveRect = new Rect(IDRect);
                        var NameRect = new Rect(IDRect);

                        ActiveRect.width = 20;

                        IDRect.x = rect.width / 4 * 3 + 50;
                        IDRect.width = rect.width / 4 - 12;

                        NameRect.x += 24;
                        NameRect.width = rect.width / 4 * 3 - 50;

                        if (useConstant)
                        {
                            ConstValue.boolValue = EditorGUI.Toggle(ActiveRect, GUIContent.none, ConstValue.boolValue);

                            if (variable != null)
                            {
                                variable.Value = ConstValue.boolValue;
                                EditorUtility.SetDirty(variable);
                            }
                        }
                        else
                        {
                            if (variable != null)
                            {
                                variable.Value = EditorGUI.Toggle(ActiveRect, GUIContent.none, variable.Value);
                                ConstValue.boolValue = variable.Value;
                            }
                            else
                            {
                                ConstValue.boolValue = EditorGUI.Toggle(ActiveRect, GUIContent.none, ConstValue.boolValue);
                            }
                        }

                        var dC = GUI.color;

                        if (SelectedAbility.intValue == index) GUI.color = MTools.MGreen * 2;

                        if (Application.isPlaying)
                        {
                            IDRect.width -= 30;
                        }


                        EditorGUI.PropertyField(NameRect, name, GUIContent.none);

                        EditorGUIUtility.labelWidth = 56;
                        EditorGUI.PropertyField(IDRect, IndexValue, GUIContent.none);
                        EditorGUIUtility.labelWidth = 0;
                        GUI.color = dC;


                        if (Application.isPlaying)
                        {
                            var TestBRect = new Rect(rect.width + 10, rect.y, 38, EditorGUIUtility.singleLineHeight);

                            var color = GUI.color;
                            GUI.color = MTools.MGreen * 1.5f;
                            if (GUI.Button(TestBRect, "Test"))
                            {
                                var ModeID = m.modes[ModeIndex].ID;
                                var ability = m.modes[ModeIndex].Abilities[index].Index.Value;
                                m.Mode_Activate(ModeID, ability);
                            }
                            GUI.color = color;
                        }
                    },

                    drawHeaderCallback = rect =>
                    {
                        var IDRect = new Rect(rect)
                        {
                            height = EditorGUIUtility.singleLineHeight
                        };

                        var NameRect = new Rect(IDRect);

                        IDRect.x = rect.width / 4 * 3 + 40;
                        IDRect.width = 40;

                        NameRect.x += 24;
                        NameRect.width = rect.width / 4 * 3 - 50;

                        string Selected = "None";

                        if (SelectedAbility.intValue != -1 && SelectedAbility.intValue < m.modes[ModeIndex].Abilities.Count)
                        {
                            var valu = m.modes[ModeIndex].Abilities[SelectedAbility.intValue].Index.Value;
                            var neg = valu > 0 ? 1 : -1;

                            if (m.modes[ModeIndex].ID)
                                Selected = $"{(m.modes[ModeIndex].ID.ID * 1000 + Mathf.Abs(valu)) * neg}";
                        }
                        else
                        {
                            SelectedAbility.intValue = m.modes[ModeIndex].Abilities.Count - 1;
                        }


                        EditorGUI.LabelField(NameRect, $"   Abilities    Selected → [{Selected}]");
                        EditorGUI.LabelField(IDRect, "Index");
                    },

                    onAddCallback = (list) =>
                    {
                        var index = list.count == 0 ? 0 : list.count - 1;
                        Abilities.InsertArrayElementAtIndex(list.count == 0 ? 0 : list.count - 1);
                        list.index = -1;
                    },
                    onSelectCallback = (list) =>
                    {
                        SelectedAbility.intValue = list.index;
                    }
                };

                Reo_Abilities.Add(listKey, Reo_AbilityList);  //Store it on the Editor
            }

            Reo_AbilityList.DoLayoutList();

            Reo_AbilityList.index = SelectedAbility.intValue;


            if (SelectedAbility.intValue != -1 && SelectedAbility.intValue < Abilities.arraySize)
            {
                // Debug.Log("SelectedAbility = " + SelectedAbility);
                SerializedProperty ability = Abilities.GetArrayElementAtIndex(SelectedAbility.intValue);

                if (ability != null)
                {

                    var Name = ability.FindPropertyRelative("Name");

                    var audioClip = ability.FindPropertyRelative("audioClip");
                    var audioSource = ability.FindPropertyRelative("audioSource");
                    var m_stopAudio = ability.FindPropertyRelative("m_stopAudio");
                    var ClipDelay = ability.FindPropertyRelative("ClipDelay");
                    var Status = ability.FindPropertyRelative("Status");
                    var Release = ability.FindPropertyRelative("Release");
                    var abilityTime = ability.FindPropertyRelative("abilityTime");
                    var ChargeValue = ability.FindPropertyRelative("ChargeValue");
                    var ChargeCurve = ability.FindPropertyRelative("ChargeCurve");
                    var OnEnter = ability.FindPropertyRelative("OnEnter");
                    var OnExit = ability.FindPropertyRelative("OnExit");
                    var ReactEnter = ability.FindPropertyRelative("ReactEnter");
                    var ReactExit = ability.FindPropertyRelative("ReactExit");
                    var OnCharged = ability.FindPropertyRelative("OnCharged");


                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var M = m.modes[ModeIndex];
                        if (M.ID != null && M.Abilities[SelectedAbility.intValue] != null)
                        {
                            var valu = m.modes[ModeIndex].Abilities[SelectedAbility.intValue].Index.Value;
                            var neg = valu > 0 ? 1 : -1;

                            EditorGUILayout.LabelField(new GUIContent($"[{Name.stringValue}] Combined Index → " +
                            $"[{(m.modes[ModeIndex].ID.ID * 1000 + Mathf.Abs(valu)) * neg}]",
                             "The combined index is set using this formula: (Mode_ID * 1000 + Ability_Index)\nThis is used in the Animator Controller Transition values for activating Mode Abilities"), DescriptionStyle);
                        }
                        Ability_Tabs.intValue = GUILayout.Toolbar(Ability_Tabs.intValue, new string[5] { "General", "Status", "Limits", "Audio", "Events" });

                        switch (Ability_Tabs.intValue)
                        {
                            //General
                            case 0:
                                {
                                    var active = ability.FindPropertyRelative("active");
                                    var Input = ability.FindPropertyRelative("Input");


                                    var MultiplierPosition = ability.FindPropertyRelative("MultiplierPosition");
                                    var MultiplierRotation = ability.FindPropertyRelative("MultiplierRotation");

                                    var AdditivePosition = ability.FindPropertyRelative("AdditivePosition");
                                    var AdditiveRotation = ability.FindPropertyRelative("AdditiveRotation");

                                    var CoolDown = ability.FindPropertyRelative("CoolDown");
                                    var modifier = ability.FindPropertyRelative("modifier");

                                    var IgnoreGrounded = ability.FindPropertyRelative("IgnoreGrounded");
                                    var IgnoreGravity = ability.FindPropertyRelative("IgnoreGravity");
                                    var NoYMovement = ability.FindPropertyRelative("NoYMovement");
                                    var Persistent = ability.FindPropertyRelative("Persistent");

                                    EditorGUILayout.PropertyField(active);
                                    EditorGUILayout.PropertyField(Input);

                                    EditorGUILayout.PropertyField(modifier);
                                    EditorGUILayout.PropertyField(CoolDown);



                                    EditorGUILayout.PropertyField(IgnoreGrounded);
                                    EditorGUILayout.PropertyField(IgnoreGravity);
                                    EditorGUILayout.PropertyField(NoYMovement);
                                    EditorGUILayout.PropertyField(Persistent);


                                    EditorGUILayout.PropertyField(MultiplierPosition);
                                    EditorGUILayout.PropertyField(MultiplierRotation);

                                    EditorGUILayout.PropertyField(AdditivePosition);
                                    EditorGUILayout.PropertyField(AdditiveRotation);

                                    break;
                                }
                            //Status
                            case 1:
                                {
                                    var help = "";

                                    EditorGUILayout.PropertyField(Status);

                                    switch ((AbilityStatus)Status.intValue)
                                    {
                                        case AbilityStatus.Charged:
                                            EditorGUILayout.PropertyField(abilityTime, new GUIContent("Charge Time"));
                                            var ConstValue = abilityTime.FindPropertyRelative("ConstantValue");

                                            if (ConstValue.floatValue != 0)
                                            {
                                                EditorGUILayout.PropertyField(ChargeValue);
                                                EditorGUILayout.PropertyField(ChargeCurve);
                                                EditorGUILayout.PropertyField(Release);
                                                help = "The Ability can be charged, it will be active while the Input Value is [True]. It will be stopped when the Input is released.\n" +
                                               "The ModePower animator Parameter will store the charge value ";
                                            }
                                            else
                                            {
                                                help = "The Ability will be active while the input is press down";
                                            }

                                            break;
                                        case AbilityStatus.ActiveByTime:
                                            EditorGUILayout.PropertyField(abilityTime);
                                            help = "The Ability is active during the ability time, then it will stop";
                                            break;
                                        case AbilityStatus.PlayOneTime:
                                            help = "The Ability will play once";
                                            break;
                                        case AbilityStatus.Toggle:
                                            help = "The Ability will active when the Input Value is [True].\nIt will be stopped the next time the Input Value is [True]";
                                            break;
                                        case AbilityStatus.Forever:
                                            help = "The Ability will active forever. To stop it, call:\nMAnimal.Mode_Stop()";
                                            break;
                                        default: break;
                                    }

                                    EditorGUILayout.LabelField(help, DescriptionStyle);
                                    break;
                                }
                            //Limits
                            case 2:
                                {
                                    var S_properties = ability.FindPropertyRelative("Limits");

                                    EditorGUILayout.PropertyField(S_properties, true);

                                    if (GUILayout.Button("Copy these limits to all other Abilities"))
                                    {
                                        var ModeAbilities = m.modes[ModeIndex].Abilities;
                                        var properties = ModeAbilities[SelectedAbility.intValue].Limits;

                                        foreach (var ab in ModeAbilities)
                                            ab.Limits = new ModeProperties(properties);

                                        Debug.Log("All Limits copied to all the Abilities in Mode: " + m.modes[ModeIndex].Name);
                                        EditorUtility.SetDirty(target);
                                    }
                                    break;
                                }
                            //Audio
                            case 3:
                                {
                                    EditorGUILayout.PropertyField(audioClip);
                                    EditorGUILayout.PropertyField(audioSource);
                                    EditorGUILayout.PropertyField(ClipDelay);
                                    EditorGUILayout.PropertyField(m_stopAudio);


                                    if (GUILayout.Button("Copy these audio properties to all other Abilities"))
                                    {
                                        var ModeAbilities = m.modes[ModeIndex].Abilities;
                                        var audioclip = ModeAbilities[SelectedAbility.intValue].audioClip;
                                        var audioSourcec = ModeAbilities[SelectedAbility.intValue].audioSource;
                                        var delay = ModeAbilities[SelectedAbility.intValue].ClipDelay.Value;
                                        var stopaudio = ModeAbilities[SelectedAbility.intValue].m_stopAudio;

                                        foreach (var ab in ModeAbilities)
                                        {
                                            ab.audioClip = new(audioclip);
                                            ab.audioSource = (audioSourcec);
                                            ab.ClipDelay = new(delay);
                                            ab.m_stopAudio = (stopaudio);
                                        }
                                        Debug.Log("All Limits copied to all the Abilities in Mode: " + m.modes[ModeIndex].Name);
                                        EditorUtility.SetDirty(target);
                                    }
                                    break;
                                }
                            //Events
                            case 4:
                                {
                                    EditorGUILayout.PropertyField(ReactEnter);
                                    EditorGUILayout.PropertyField(ReactExit);
                                    var ab_name = Name.stringValue;
                                    EditorGUILayout.PropertyField(OnEnter, new GUIContent($"On [{ab_name}] Enter"));
                                    EditorGUILayout.PropertyField(OnExit, new GUIContent($"On [{ab_name}] Exit"));

                                    if ((AbilityStatus)Status.intValue == AbilityStatus.Charged)
                                        EditorGUILayout.PropertyField(OnCharged, new GUIContent($"On [{ab_name}] Charged"));
                                    break;
                                }
                        }
                    }
                }
            }
        }

        private void ShowSpeeds()
        {
            MSpeedEditor.ShowSpeeds(Reo_List_Speeds, m.speedSets, SelectedSpeed, ref SpeedTabs);
            DisplayActiveSpeed();
        }

        private void DisplayActiveSpeed()
        {
            if (Application.isPlaying)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.LabelField("Active Speed Modifier", EditorStyles.boldLabel);
                    EditorGUILayout.IntField("Current Index", m.CurrentSpeedIndex);
                    EditorGUILayout.Toggle("Locked Speed", m.CurrentSpeedSet.LockSpeed);
                    EditorGUILayout.Toggle("Using Custom Speed", m.CustomSpeed);

                    EditorGUILayout.LabelField($"Current Speed Modifier: [{m.CurrentSpeedModifier.Name}]");
                    var cpM = serializedObject.FindProperty("currentSpeedModifier");
                    var cSprintSpeed = serializedObject.FindProperty("SprintSpeed");
                    cpM.isExpanded = true;
                    cSprintSpeed.isExpanded = true;

                    if (m.Sprint && !m.CustomSpeed)
                        EditorGUILayout.PropertyField(cSprintSpeed, true);
                    else
                        EditorGUILayout.PropertyField(cpM, true);

                    // EditorGUILayout.LabelField($"SprintSpeed: {m.SprintSpeed.name}", EditorStyles.boldLabel);
                }


            }
        }

        private void ShowStrafingVars()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_CanStrafe.isExpanded = MalbersEditor.Foldout(m_CanStrafe.isExpanded, "Strafing"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(m_CanStrafe, G_CanStrafe);
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (m.Aimer == null)
                            {
                                m.Aimer = m.FindComponent<Aim>();
                                if (m.Aimer == null)
                                {
                                    m.Aimer = m.gameObject.AddComponent<Aim>();
                                }
                                EditorUtility.SetDirty(m);
                            }
                        }

                        if (GUILayout.Button("?", GUILayout.Width(20)))
                        {
                            Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/strafing");
                        }
                    }

                    if (m.CanStrafe)
                    {
                        EditorGUILayout.PropertyField(m_strafe, G_Strafe);
                        EditorGUILayout.PropertyField(m_StrafeNormalize, G_StrafeNormalize);
                        EditorGUILayout.PropertyField(m_StrafeLerp, G_StrafeLerp);
                    }
                }
            }
        }

        #region Draw Speeds

        private void Draw_Header_Speed(Rect rect)
        {
            var height = EditorGUIUtility.singleLineHeight;
            var nameRec = new Rect(rect.x + 30, rect.y, rect.width / 2, height);

            EditorGUI.LabelField(nameRec, "Speed Sets");

            Rect R_1 = new Rect(rect.width + 5, rect.y, 20, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(R_1, "?"))
                Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/speeds");

        }

        private void OnRemoveCallback_Speeds(ReorderableList list)
        {
            S_Speed_List.DeleteArrayElementAtIndex(list.index);
            list.index = -1;
            SelectedSpeed = -1;
            EditorUtility.SetDirty(m);
        }

        private void OnAddCallback_Speeds(ReorderableList reo_List_Speeds)
        {
            if (m.speedSets == null) m.speedSets = new List<MSpeedSet>();

            m.speedSets.Add(new MSpeedSet());

            EditorUtility.SetDirty(m);
        }

        private void Draw_Element_Speed(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (S_Speed_List.arraySize <= index) return;

            var nameRect = new Rect(rect);
            var IndexRect = new Rect(rect);

            nameRect.y += 1;
            nameRect.x += 25;
            nameRect.height = EditorGUIUtility.singleLineHeight;
            IndexRect.height = EditorGUIUtility.singleLineHeight;
            IndexRect.width = 20f;

            Rect activeRect = new Rect(nameRect);

            var speedSet = S_Speed_List.GetArrayElementAtIndex(index);
            var nameSpeedSet = speedSet.FindPropertyRelative("name");
            nameRect.width /= 2;
            nameRect.width += 10;
            EditorGUI.LabelField(IndexRect, $"[{index}]");
            EditorGUI.PropertyField(nameRect, nameSpeedSet, GUIContent.none);

            activeRect.x = rect.width / 2 + 80;
            activeRect.width = rect.width / 2 - 40;

            if (Application.isPlaying)
            {
                if (m.speedSets[index] == m.CurrentSpeedSet)
                {
                    EditorGUI.LabelField(activeRect, "  (" + m.CurrentSpeedModifier.name + ")", EditorStyles.boldLabel);
                }
            }
        }
        #endregion



        #region DrawModes

        //-------------------------MODES-----------------------------------------------------------
        private void Draw_Header_Modes(Rect rect)
        {
            var r = new Rect(rect);
            var a = new Rect(rect) { width = 65 };
            EditorGUI.LabelField(a, new GUIContent("  Active", "Is the Mode Enable or Disable"));
            r.x += 60;
            r.width = 60;
            EditorGUI.LabelField(r, new GUIContent("Mode", "Modes are the Animations that can be played on top of the States"));

            var activeRect = rect;
            activeRect.width -= 20;
            activeRect.x += 20;
            var prioRect = new Rect(activeRect.width + 30, activeRect.y, 45, activeRect.height);
            var IDRect = new Rect(activeRect.width + 5, activeRect.y, 35, activeRect.height);

            EditorGUI.LabelField(IDRect, new GUIContent("ID", "Mode ID:\n Numerical ID value for the Mode"));
            EditorGUI.LabelField(prioRect, new GUIContent("Pri", "Priority:\n If A mode has 'Ignore Lower Modes' enabled, it will play even if a Lower Mode is Playing"));
        }
        private void Draw_Element_Modes(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            if (S_Mode_List.arraySize <= index) return;

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                var ModeProperty = S_Mode_List.GetArrayElementAtIndex(index);
                var active = ModeProperty.FindPropertyRelative("active");
                var ID = ModeProperty.FindPropertyRelative("ID");

                var rectan = new Rect(rect);
                rectan.width -= 20;
                rectan.x += 20;
                rectan.y -= 2;

                var activeRect1 = new Rect(rect.x, rect.y - 2, 20, rect.height);
                var IDRect = new Rect(rect.x + 40, rect.y, rect.width - 90, EditorGUIUtility.singleLineHeight);


                if (Application.isPlaying)
                {
                    IDRect.width -= 40;
                }

                var IDVal = new Rect(rectan.width + 9, rectan.y + 3, 35, rectan.height);

                var dC = GUI.backgroundColor;
                if (isActive) GUI.backgroundColor = MTools.MGreen;
                active.boolValue = EditorGUI.Toggle(activeRect1, GUIContent.none, active.boolValue);
                EditorGUI.PropertyField(IDRect, ID, GUIContent.none);
                GUI.backgroundColor = dC;


                if (Application.isPlaying)
                {
                    var TestBRect = new Rect(rect.width - 40 - 2, rect.y, 38, EditorGUIUtility.singleLineHeight);

                    var color = GUI.color;
                    GUI.color = MTools.MGreen * 1.5f;
                    if (GUI.Button(TestBRect, "Test"))
                    {
                        m.Mode_Activate(ID.objectReferenceValue as ModeID);
                    }
                    GUI.color = color;
                }



                var style = new GUIStyle(EditorStyles.boldLabel)
                { alignment = TextAnchor.UpperRight };

                if (m.modes[index].ID != null)
                {
                    EditorGUI.LabelField(IDVal, m.modes[index].ID.ID.ToString(), style);
                }

                var priorityRect = new Rect(rectan.width + 42, rectan.y, 25, rectan.height);

                EditorGUI.LabelField(priorityRect, "│" + (S_Mode_List.arraySize - index - 1));

                if (cc.changed)
                {
                    Undo.RecordObject(target, "Move Handles");
                }
            }
        }
        private void OnAdd_Modes(ReorderableList list)
        {
            if (m.modes == null) m.modes = new();

            Ability newAbility = new()
            {
                active = new BoolReference(true),
                Index = new IntReference(1),
                Name = "AbilityName"
            };

            var newMode = new Mode()
            {
                Abilities = new List<Ability>(1) { newAbility },
            };

            m.modes.Add(newMode);

            EditorUtility.SetDirty(m);
            ModeArray_Popup();
        }
        private void OnRemoveCallback_Mode(ReorderableList list)
        {
            // The reference value must be null in order for the element to be removed from the SerializedProperty array.
            S_Mode_List.DeleteArrayElementAtIndex(list.index);
            list.index -= 1;

            if (list.index == -1 && S_Mode_List.arraySize > 0)  //In Case you remove the first one
            {
                list.index = 0;
            }
            SelectedMode.intValue--;
            list.index = Mathf.Clamp(list.index, 0, list.index - 1);
            S_Mode_List.serializedObject.ApplyModifiedProperties();
            ModeArray_Popup();
            EditorUtility.SetDirty(m);
            GUIUtility.ExitGUI();
        }
        #endregion

        public void SetPivots()
        {
            m.Pivot_Hip = m.pivots.Find(item => item.name.ToUpper() == "HIP");
            m.Pivot_Chest = m.pivots.Find(item => item.name.ToUpper() == "CHEST");
        }

        #region GUICONTENT

        readonly GUIContent G_Rotator = new("Rotator", "Used to add extra Rotations to the Animal");
        readonly GUIContent G_RootBone = new("RootBone", "Bone to Identify the Main Root Bone of the Animal. Mainly Used for TimeLine and Flying Animals");

        readonly GUIContent G_RayCastRadius = new("RayCast Radius", "Instead of using Raycast for checking the ground beneath the animal we use SphereCast, this is the Radius of that Sphere");

        readonly GUIContent G_animalType = new("Type", "Value set on the Animator for Additive Pose Fixing");


        readonly GUIContent G_AbilityIndex = new("Active", "Active Ability Index \n(if set to -99 it will Play a Random Ability )\n(if set to 0 it wont play anything)");
        readonly GUIContent G_DefaultIndex = new("Default", "Default Ability Index to return to when exiting the mode \n(if set to -99 it will Play a Random Ability )");
        readonly GUIContent G_ResetToDefault = new("R", "Reset to Default:\nWhen Exiting the Mode\nthe Active Index will reset\nto the Default");
        readonly GUIContent G_CloneStates = new("Clone States", "Creates instances of the States so they cannot be overwritten by other animals using the same scriptable objects");

        readonly GUIContent G_GroundLayer = new("Ground Layer", "Layers the Animal considers ground");
        readonly GUIContent G_AlignPosLerp = new("Align Pos Lerp", "Smoothness value to Snap to ground while Grounded");
        readonly GUIContent G_AlignPosDelta = new("Align Pos Delta", "Smoothness Position value to Snap to ground when using a non Grounded State");
        readonly GUIContent G_AlignRotDelta = new("Align Rot Delta", "Smoothness Rotaion value to Snap to ground when using a non Grounded State");
        readonly GUIContent G_AlignRotLerp = new("Align Rot Lerp", "Smoothness value to Align to ground slopes while Grounded");

        readonly GUIContent G_Modifier = new("Modifier", "Extra Logic to give the Animal when Entering or Exiting the Modes");

        readonly GUIContent G_gravityDirection = new("Direction", "Direction of the Gravity applied to the animal");
        readonly GUIContent G_GravityForce = new("Force", "Force of the Gravity, by Default it 9.8");
        readonly GUIContent G_GravityCycle = new("Start Gravity Cycle", "Start the gravity with an extra time to push the animal down.... higher values stronger Gravity");

        readonly GUIContent G_useSprintGlobal = new("Can Sprint", "Can the Animal Sprint?");
        readonly GUIContent G_CanStrafe = new("Can Strafe", "Can the Animal Strafe?\nStrafing requires new sets of strafe animations. Make sure you have proper animations to Use this feature. Check the Help button for more Info [?]");
        readonly GUIContent G_Strafe = new("Strafe", "Activate the Strafe on the Animal.");
        readonly GUIContent G_StrafeNormalize = new("Normalize", "Normalize the value of the Strafe Angle on the Animation (-1 to 1 instead of -180 to 180)");
        readonly GUIContent G_StrafeLerp = new("Lerp", "Lerp Value to smoothly enter the  Strafe");

        readonly GUIContent G_SmoothVertical = new("Smooth Vertical", "Used for Joysticks to increase the speed by the Stick Pressure");

        readonly GUIContent G_Player = new("Player", "True if this will be your main Character Player, used for Respawing characters");
        readonly GUIContent G_OverrideStartState = new("Override Start State", "Overrides the Start State");
        readonly GUIContent G_StartWithMode =
            new("Start with Mode", "On Start .. Plays a Mode. Use the Mode ID.\nIf you want an specific Ability within the mode. Set the Mode and the Ability in the Format (Mode*1000+Ability). E.g Eat = 4002");
        #endregion

        //-------------------------STATES-----------------------------------------------------------
        void OnSceneGUI()
        {
            foreach (var pivot in m.pivots)
            {
                if (pivot.EditorModify)
                {
                    Transform t = m.transform;

                    using (var cc = new EditorGUI.ChangeCheckScope())
                    {
                        Vector3 piv = t.TransformPoint(pivot.position);
                        Vector3 NewPivPosition = Handles.PositionHandle(piv, t.rotation);
                        //   pivot.position = m.transform.InverseTransformPoint(NewPivPosition);

                        float multiplier =
                            Handles.ScaleSlider(pivot.multiplier, piv, pivot.WorldDir(t), Quaternion.identity, HandleUtility.GetHandleSize(piv), 0f);

                        if (cc.changed)
                        {
                            Undo.RecordObject(m, "Pivots");
                            pivot.position = t.InverseTransformPoint(NewPivPosition);
                            pivot.multiplier = multiplier;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
            }
        }
    }
}
#endif