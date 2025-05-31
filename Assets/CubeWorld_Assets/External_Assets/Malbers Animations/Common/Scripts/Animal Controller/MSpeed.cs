using MalbersAnimations.Scriptables;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public class MSpeedSet : IComparable, IComparer
    {
        [Tooltip("Name of the Speed Set")]
        public string name;

        [Tooltip("Which Speed the Set will start, This value is the Index for the Speed Modifier List, Starting the first index with (1) instead of (0)")]
        public IntReference StartVerticalIndex;
        [Tooltip("Set the Top Index when Increasing the Speed using SpeedUP")]
        public IntReference TopIndex;

        [Tooltip("Index Value of the Sprint Speed")]
        public IntReference m_SprintIndex = new(10);

        [Tooltip("When the Speed is locked this will be the value s")]
        public IntReference m_LockIndex = new(1);

        [Tooltip("Lock the Speed Set to Certain Value")]
        public BoolReference m_LockSpeed = new(false);


        [Tooltip("RootMotion multiplier for the speeds Position. Set it to zero to remove RootMotion movement")]
        public FloatReference m_RootMotionPos = new(1f);

        [Tooltip("RootMotion multiplier for the speeds Rotation. Set it to zero to remove RootMotion Rotation")]
        public FloatReference m_RootMotionRot = new(1f);

        [Tooltip("Backwards Speed multiplier: When going backwards the speed will be decreased by this value")]
        public FloatReference BackSpeedMult = new(0.5f);

        [Tooltip("Lerp used to Activate the FreeMovement")]
        public FloatReference PitchLerpOn = new(10f);

        [Tooltip("Lerp used to Deactivate the FreeMovement")]
        public FloatReference PitchLerpOff = new(10f);

        [Tooltip("Lerp used to for the Banking on FreeMovement")]
        public FloatReference BankLerp = new(10f);

        [Tooltip("Up Down Multiplier")]
        public FloatReference UpDownMult = new(1);


        [Tooltip("States that will use the Speed Set")]
        public List<StateID> states;
        [Tooltip("Stances that will use the Speed Set")]
        public List<StanceID> stances;

        //[Tooltip("Multiplier to slowdown the speed if the character is on a slope. E.g. Set the Value (1,0.5) on the last key to slowdown the speed while  ")]
        //public AnimationCurve SlopeMultiplier = new AnimationCurve(new Keyframe(-1, 1), new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary> List of Speed Modifiers for the Speed Set</summary>
        public List<MSpeed> Speeds;

        /// <summary>THis Speed Set has no Stances
        public bool HasStances => stances != null && stances.Count > 0;
        // public bool HasStates => states != null && states.Count > 0;

        /// <summary> Current Active Index of the SpeedSet</summary>
        public int CurrentIndex { get; set; }

        /// <summary>Locked Index of a Speed Set</summary>
        public int LockIndex { get => m_LockIndex.Value; set => m_LockIndex.Value = value; }
        public int SprintIndex { get => m_SprintIndex.Value; set => m_SprintIndex.Value = value; }

        public float RootMotionPos { get => m_RootMotionPos.Value; set => m_RootMotionPos.Value = value; }

        public float RootMotionRot { get => m_RootMotionRot.Value; set => m_RootMotionRot.Value = value; }

        /// <summary>Locked Index of a Speed Set</summary>
        public bool LockSpeed
        {
            get => m_LockSpeed.Value;
            set
            {
                m_LockSpeed.Value = value;

                if (value)
                    LockedSpeedModifier = Speeds[Mathf.Clamp(LockIndex - 1, 0, Speeds.Count - 1)]; //Extract the Lock Speed
            }
        }

        /// <summary>Store the current Lock Speed Modifier</summary>
        public MSpeed LockedSpeedModifier { get; set; }


        public MSpeedSet()
        {
            name = "Set Name";
            states = new List<StateID>();
            StartVerticalIndex = new(1);
            TopIndex = new(2);
            Speeds = new(1) { new MSpeed("SpeedName", 1, 4, 4) };
        }

        public MSpeed this[int index]
        {
            get => Speeds[index];
            set => Speeds[index] = value;
        }

        /// <summary>Returns a Speed given its name</summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MSpeed this[string name] => Speeds.Find(x => x.name == name);

        public bool HasStance(int stance)
        {
            if (!HasStances) return true;
            else return stances.Find(s => s.ID == stance);
        }

        public int Compare(object x, object y)
        {
            bool XHas = (x as MSpeedSet).HasStances;
            bool YHas = (y as MSpeedSet).HasStances;

            if (XHas && YHas)
                return 0;
            else if (XHas && !YHas)
                return 1;
            else return -1;
        }

        public int CompareTo(object obj)
        {
            bool XHas = (obj as MSpeedSet).HasStances;
            bool YHas = HasStances;

            if (XHas && YHas)
                return 0;
            else if (XHas && !YHas)
                return 1;
            else return -1;
        }

        public MSpeed GetSpeed(string name) => Speeds.Find(x => x.name == name);
        public int GetSpeedIndex(string name) => Speeds.FindIndex(x => x.name == name);

        internal void SwapSpeed(MSpeed NewSpeed)
        {
            //find the SpeedSet
            var speedIndex = GetSpeedIndex(NewSpeed.Name);

            if (speedIndex != -1)
            {
                Speeds[speedIndex] = NewSpeed;
            }
        }

        internal void AddSpeed(MSpeed NewSpeed)
        {
            Speeds.Add(NewSpeed);
        }
    }
    [System.Serializable]
    /// <summary>Position, Rotation and Animator Modifiers for the Animals</summary>
    public struct MSpeed
    {
        /// <summary>Default value for an MSpeed</summary>
        public static readonly MSpeed Default = new MSpeed("Default", 1, 4, 4);

        /// <summary>Name of this Speed</summary>
        public string name;

        /// <summary>Vertical Mutliplier for the Animator</summary>
        public FloatReference Vertical;

        /// <summary>Add additional speed to the transfrom</summary>
        public FloatReference position;

        /// <summary> Smoothness to change to the Transform speed, higher value more Responsiveness</summary>
        public FloatReference lerpPosition;

        /// <summary> Smoothness to change to the Animator Vertical speed, higher value more Responsiveness</summary>
        public FloatReference lerpPosAnim;


        /// <summary>Add Aditional Rotation to the Speed</summary>
        public FloatReference rotation;

        /// <summary> Smoothness to change to the Animator Vertical speed, higher value more Responsiveness</summary>
        public FloatReference lerpRotAnim;

        /// <summary>Changes the Animator Speed</summary>
        public FloatReference animator;

        /// <summary> Smoothness to change to the Animator speed, higher value more Responsiveness </summary>
        public FloatReference lerpAnimator;

        /// <summary>Strafe Stored Velocity</summary>
        public FloatReference strafeSpeed;


        /// <summary> Smoothness to change to the Rotation speed, higher value more Responsiveness </summary>
        public FloatReference lerpStrafe;

        public string Name { readonly get => name; set => name = value; }

        public MSpeed(MSpeed newSpeed)
        {
            name = newSpeed.name;

            position = newSpeed.position;
            lerpPosition = newSpeed.lerpPosition;
            lerpPosAnim = newSpeed.lerpPosAnim;

            rotation = newSpeed.rotation;
            //  lerpRotation = newSpeed.lerpRotation;
            lerpRotAnim = newSpeed.lerpRotAnim;

            animator = newSpeed.animator;
            lerpAnimator = newSpeed.lerpAnimator;
            Vertical = newSpeed.Vertical;
            strafeSpeed = newSpeed.strafeSpeed;
            strafeSpeed = newSpeed.strafeSpeed;
            lerpStrafe = newSpeed.lerpStrafe;

            //nameHash = name.GetHashCode();
        }


        public MSpeed(string name, float lerpPos, float lerpanim)
        {
            this.name = name;
            Vertical = 1;

            position = 0;
            lerpPosition = lerpPos;
            lerpPosAnim = 4;

            rotation = 0;
            strafeSpeed = 0;
            //lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;

            animator = 1;
            lerpAnimator = lerpanim;
            // nameHash = name.GetHashCode();
        }

        public MSpeed(string name, float vertical, float lerpPos, float lerpanim)
        {
            this.name = name;
            Vertical = vertical;

            position = 0;
            lerpPosition = lerpPos;
            lerpPosAnim = 4;

            rotation = 0;
            strafeSpeed = 0;
            // lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;


            animator = 1;
            lerpAnimator = lerpanim;

            // nameHash = name.GetHashCode();
        }


        public MSpeed(string name)
        {
            this.name = name;
            Vertical = 1;

            position = 0;
            lerpPosition = 4;
            lerpPosAnim = 4;


            rotation = 0;
            strafeSpeed = 0;

            // lerpRotation = 4;
            lerpRotAnim = 4;
            lerpStrafe = 4;


            animator = 1;
            lerpAnimator = 4;

            // nameHash = name.GetHashCode();
        }
    }

#if UNITY_EDITOR
    public static class MSpeedEditor
    {
        public static void ShowSpeeds(ReorderableList list, List<MSpeedSet> set, int OldSelectedSpeed, ref int SpeedTabs)
        {
            // using (new GUILayout.VerticalScope())
            {
                EditorGUILayout.HelpBox("For [In Place] animations <Not Root Motion>, Increase [Position] and [Rotation] values for each Speed Set", MessageType.Info);
                list.DoLayoutList();        //Paint the Reordable List speeds 

                list.index = OldSelectedSpeed;

                if (list.index >= 0 && list.index < list.serializedProperty.arraySize)
                {
                    var SelectedSpeed = list.serializedProperty.GetArrayElementAtIndex(list.index); //?!??!

                    if (SelectedSpeed != null)
                    {
                        var Speeds = SelectedSpeed.FindPropertyRelative("Speeds");
                        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(SelectedSpeed, false);
                            EditorGUI.indentLevel--;

                            if (SelectedSpeed.isExpanded)
                            {
                                EditorGUILayout.LabelField("Speed Index Values", EditorStyles.boldLabel);


                                var StarSpeed = set[list.index].StartVerticalIndex.Value;
                                var GCC = GUI.contentColor;

                                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    using (new EditorGUI.DisabledGroupScope(true))
                                    {
                                        for (int i = 0; i < Speeds.arraySize; i++)
                                        {

                                            var speedN = Speeds.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;

                                            if (StarSpeed - 1 == i)
                                            {
                                                GUI.contentColor = Color.yellow;
                                                speedN += " [Start]";
                                            }
                                            EditorGUILayout.FloatField(speedN, 1 + i);
                                            GUI.contentColor = GCC;
                                        }
                                    }
                                }

                                SpeedTabs = GUILayout.Toolbar(SpeedTabs, new string[2] { "General", "Speeds" });

                                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    if (SpeedTabs == 0)
                                    {

                                        var states = SelectedSpeed.FindPropertyRelative("states");
                                        var stances = SelectedSpeed.FindPropertyRelative("stances");
                                        var StartVerticalSpeed = SelectedSpeed.FindPropertyRelative("StartVerticalIndex");
                                        var TopIndex = SelectedSpeed.FindPropertyRelative("TopIndex");
                                        var BackSpeedMult = SelectedSpeed.FindPropertyRelative("BackSpeedMult");


                                        var PitchLerpOn = SelectedSpeed.FindPropertyRelative("PitchLerpOn");
                                        var PitchLerpOff = SelectedSpeed.FindPropertyRelative("PitchLerpOff");
                                        var BankLerp = SelectedSpeed.FindPropertyRelative("BankLerp");
                                        var UpDownMult = SelectedSpeed.FindPropertyRelative("UpDownMult");
                                        var m_LockSpeed = SelectedSpeed.FindPropertyRelative("m_LockSpeed");
                                        var m_SprintIndex = SelectedSpeed.FindPropertyRelative("m_SprintIndex");

                                        var m_LockIndex = SelectedSpeed.FindPropertyRelative("m_LockIndex");
                                        var m_RootMotionPos = SelectedSpeed.FindPropertyRelative("m_RootMotionPos");
                                        var m_RootMotionRot = SelectedSpeed.FindPropertyRelative("m_RootMotionRot");

                                        StartVerticalSpeed.isExpanded = MalbersEditor.Foldout(StartVerticalSpeed.isExpanded, "Indexes");
                                        if (StartVerticalSpeed.isExpanded)
                                        {
                                            EditorGUILayout.PropertyField(StartVerticalSpeed, new GUIContent("Start Index", StartVerticalSpeed.tooltip));
                                            EditorGUILayout.PropertyField(TopIndex);
                                            EditorGUILayout.PropertyField(m_SprintIndex);
                                            EditorGUILayout.PropertyField(BackSpeedMult, new GUIContent("Back Speed Mult", BackSpeedMult.tooltip));
                                        }

                                        m_RootMotionPos.isExpanded = MalbersEditor.Foldout(m_RootMotionPos.isExpanded, "RootMotion");
                                        if (m_RootMotionPos.isExpanded)
                                        {
                                            EditorGUILayout.PropertyField(m_RootMotionPos);
                                            EditorGUILayout.PropertyField(m_RootMotionRot);
                                        }


                                        m_LockSpeed.isExpanded = MalbersEditor.Foldout(m_LockSpeed.isExpanded, "Lock Speed");
                                        if (m_LockSpeed.isExpanded)
                                        {
                                            EditorGUILayout.PropertyField(m_LockSpeed);
                                            EditorGUILayout.PropertyField(m_LockIndex);
                                        }

                                        PitchLerpOn.isExpanded = MalbersEditor.Foldout(PitchLerpOn.isExpanded, "Free Movement Lerp Values");

                                        if (PitchLerpOn.isExpanded)
                                        {
                                            EditorGUILayout.PropertyField(PitchLerpOn);
                                            EditorGUILayout.PropertyField(PitchLerpOff);
                                            EditorGUILayout.PropertyField(BankLerp);
                                            EditorGUILayout.PropertyField(UpDownMult);
                                        }


                                        BankLerp.isExpanded = MalbersEditor.Foldout(BankLerp.isExpanded, "Limits");

                                        if (BankLerp.isExpanded)
                                        {
                                            // EditorGUILayout.Space();
                                            EditorGUI.indentLevel++;
                                            EditorGUI.indentLevel++;
                                            EditorGUILayout.PropertyField(states, new GUIContent("States", "States that will activate these Speeds"), true);
                                            EditorGUILayout.PropertyField(stances, new GUIContent("Stances", "Stances that will activate these Speeds"), true);
                                            EditorGUI.indentLevel--;
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                    else
                                    {
                                        // EditorGUILayout.Space();
                                        EditorGUI.indentLevel++;
                                        EditorGUILayout.PropertyField(Speeds, new GUIContent("Speeds", "Speeds for this speed Set"), true);
                                        EditorGUI.indentLevel--;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //DisplayActiveSpeed();
        }
    }
#endif
}