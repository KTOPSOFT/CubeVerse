using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using MalbersAnimations.Utilities;


#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif

namespace MalbersAnimations.IK
{
    [DisallowMultipleComponent]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/ik/ikmanager")]
    [AddComponentMenu("Malbers/IK/IK Manager")]
    public class IKManager : MonoBehaviour, IIKSource
    {
        [RequiredField] public Animator animator;

        [Range(0f, 1f), Tooltip("Global weight for the All IK Profiles")]
        public float Weight = 1;

        public List<IKSet> sets = new();
        private HashSet<int> animatorHashParams;
        public Transform Owner => transform;

        /// <summary> Store the Selected Tab in the inspector</summary>
        [HideInInspector, SerializeField] private int EditorTabs;
        [HideInInspector, SerializeField] internal int SelectedSet;

        // private ICharacterAction characterAction;

        private void Awake()
        {
            animator = animator == null ? GetComponent<Animator>() : animator;

            //Store all the FLOAT animParameters of the Animator
            animatorHashParams = new();
            foreach (var parameter in animator.parameters)
            {
                if (parameter.type == UnityEngine.AnimatorControllerParameterType.Float)
                    animatorHashParams.Add(parameter.nameHash);
            }

            foreach (var set in sets)
            {
                set.Initialize(animator, animatorHashParams);
                set.Owner = this; //Set the Owner of the IKSet
            }

        }

        private void OnEnable()
        {
            animatePhysics = animator.updateMode == AnimatorUpdateMode.Fixed;

            if (animatePhysics)
                StartCoroutine(SolveFixedUpdateIK());

            //initialize every IK Set
            foreach (var set in sets)
                set.OnEnable(animator, animatorHashParams);
        }

        private bool animatePhysics;

        private void OnDisable()
        {
            foreach (var set in sets)
            {
                set.OnDisable(animator, animatorHashParams);
            }


            StopAllCoroutines();
        }
        private IEnumerator SolveFixedUpdateIK()
        {
            var wait = new WaitForFixedUpdate();

            while (true)
            {
                yield return wait;

                foreach (var set in sets)
                {
                    if (animatePhysics)
                    {
                        set.CacheValues(animator);
                        set.LateUpdate(animator, Weight, Time.fixedDeltaTime);
                    }
                }
            }
        }
        private void LateUpdate()
        {
            foreach (var set in sets)
            {
                if (!animatePhysics)
                {
                    set.CacheValues(animator);
                    set.LateUpdate(animator, Weight, Time.deltaTime);
                }
            }
        }

        private void OnAnimatorIK()
        {
            foreach (var set in sets)
                set.OnAnimatorIK(animator, Weight, animator.updateMode == AnimatorUpdateMode.Normal ? Time.deltaTime : Time.fixedDeltaTime);
        }

        /// <summary>Activate or deactivate a Set.</summary>
        /// <param name="set"> name of the set</param>
        /// <param name="value">enable: true disable: false</param>
        public void Set_Enable(string set, bool value)
        {
            if (!enabled) return;

            var sets = set.Split(',');

            foreach (var s in sets)
            {
                var NewSet = FindSet(s);
                NewSet?.Enable(value);
            }
        }

        /// <summary>Finds a set by its name and Activates it</summary>
        public void Set_Enable(string set) => Set_Enable(set, true);

        /// <summary>Finds a set by its name and deactivates it</summary>
        public void Set_Disable(string set) => Set_Enable(set, false);

        /// <summary>Finds a set by its name and Activates it</summary>
        public void Set_Weight_1(string set) => Set_Enable(set, false);
        /// <summary>Finds a set by its name and deactivates it</summary>
        public void Set_Weight_0(string set) => Set_Enable(set, false);

        public void Set_Weight(string set, bool value)
        {
            var sets = set.Split(',');
            foreach (var s in sets)
            {
                var NewSet = FindSet(s);
                NewSet?.SetWeight(value);
            }
        }

        /// <summary> Sets a new Target to a IK Set given the set name, and the new index and target transform value </summary>

        public void Target_Set(string set, Transform newTarget, int index)
        {
            var sets = set.Split(',');
            foreach (var s in sets)
            {
                var NewSet = FindSet(s);
                NewSet?.SetTarget(newTarget, index);
            }
        }

        /// <summary>  Finds a IK Set given a name </summary>
        public virtual IKSet FindSet(string set) => sets.Find(x => x.Name == set);

        public void Target_Clear(string set, int index)
        {
            var NewSet = FindSet(set);
            NewSet?.ClearTarget(index);
        }

        public void Target_Clear(string set)
        {
            var sets = set.Split(',');
            foreach (var s in sets)
            {
                var NewSet = FindSet(s);
                NewSet?.ClearAllTargets();
            }
        }

        public void Target_Set(string set, Transform[] targets)
        {
            var sets = set.Split(',');

            foreach (var s in sets)
            {
                var NewSet = FindSet(s);
                NewSet?.SetTargets(targets);
            }
        }


        public void Processor_SetEnable(string set, string processor, bool value)
        {
            var NewSet = FindSet(set);
            NewSet?.Processor_SetEnable(processor, value);
        }

        private void Reset()
        {
            animator = this.FindComponent<Animator>();
        }

        private void OnValidate()
        {
            foreach (var set in sets)
            {
                if (set.aimer == null) set.aimer = this.FindComponent<Aim>();
            }
        }
        private void OnDrawGizmosSelected()
        {
            if (!enabled) return;

            if (sets != null && sets.Count > 0 && animator != null)
            {
                for (int k = 0; k < sets.Count; k++)
                {
                    var set = sets[k];

                    if (!set.active) continue;

                    //Paint the Weight Processors
                    if (set.weightProcessors != null)
                    {
                        foreach (var weightP in set.weightProcessors)
                        {
                            weightP?.OnDrawGizmos(set, animator);
                        }
                    }

                    if (set != null && SelectedSet == k && set.active && set.Processors != null)
                    {
                        for (int i = 0; i < set.Processors.Count; i++)
                        {
                            var link = set.Processors[i];
                            if (link != null && link.Active && set.SelectedIKProcessor == i)
                                link.OnDrawGizmos(set, animator, Weight);
                        }
                    }
                }
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(IKManager))]
    public class IKManagerEditor : Editor
    {
        ReorderableList Reo_Sets;

        private readonly Dictionary<string, ReorderableList> Reo_Links = new();
        List<Type> derivedTypes;

        IKManager m;
        private int result;

        SerializedProperty GlobalWeight, IKSets, animator, EditorTabs, SelectedSet
            //  , animatorparam
            ;

        private List<string> floatAnimParam;

        private void OnEnable()
        {
            m = (IKManager)target;

            animator = serializedObject.FindProperty("animator");
            IKSets = serializedObject.FindProperty("sets");
            EditorTabs = serializedObject.FindProperty("EditorTabs");
            GlobalWeight = serializedObject.FindProperty("Weight");
            SelectedSet = serializedObject.FindProperty("SelectedSet");
            // animatorparam = serializedObject.FindProperty("animatorparam");


            Reo_Sets = new ReorderableList(serializedObject, IKSets, true, true, true, true)
            {
                drawHeaderCallback = Draw_Header_Set,
                drawElementCallback = Draw_Element,
                onAddCallback = OnAddCallBack,
                onSelectCallback = (list) => { SelectedSet.intValue = list.index; }
            };

            SelectedSet.intValue = Reo_Sets.index;

            derivedTypes = MTools.GetAllTypes<IKProcessor>();

            FindAllFloatParameters();
        }

        private void FindAllFloatParameters()
        {
            if (animator != null && animator.objectReferenceValue != null && floatAnimParam == null)
            {
                if (m.gameObject.activeInHierarchy)
                {
                    floatAnimParam = new() { "None" };
                    result = 0;

                    var anim = m.animator;

                    if (anim == null) return;

                    for (int i = 0; i < anim.parameterCount; i++)
                    {
                        var parameter = anim.GetParameter(i);

                        if (parameter.type == UnityEngine.AnimatorControllerParameterType.Float)
                        {
                            floatAnimParam.Add(parameter.name);
                        }
                    }
                }
            }
        }

        private void OnAddCallBack(ReorderableList list)
        {
            m.sets ??= new();

            m.sets.Add(
                new IKSet()
                {
                    // m_name = $"",
                    name = new Scriptables.StringReference($"newIK Set {list.count}") { UseConstant = true },

                    Targets = new Scriptables.TransformReference[1],
                    EnableTime = 0.25f,
                    DisableTime = 0.25f
                }
                );

            IKSets.serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(m);
        }
        private void Draw_Header_Set(Rect rect)
        {
            var r = new Rect(rect);
            var WeightRect = new Rect(rect) { x = r.width - 30, width = 65 };
            var a = new Rect(rect) { width = 65 };
            EditorGUI.LabelField(a, new GUIContent("Active", "Enable Disable the IK Set"));
            r.x += 60;
            r.width = 60;
            EditorGUI.LabelField(r, new GUIContent("IK Set", "Name of the IK Profile "));
            EditorGUI.LabelField(WeightRect, new GUIContent("Weight", "Modes are the Animations that can be played on top of the States"));
        }

        private void Draw_Element(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            if (IKSets.arraySize <= index) return;

            var ikSet = IKSets.GetArrayElementAtIndex(index);
            var active = ikSet.FindPropertyRelative("active");
            //var name = ikSet.FindPropertyRelative("m_name");
            var Name = ikSet.FindPropertyRelative("name");
            var weight = ikSet.FindPropertyRelative("weight");

            //if (name.stringValue != string.Empty)
            //{
            //    var constant = Name.FindPropertyRelative("ConstantValue");
            //    constant.stringValue = name.stringValue;

            //    name.stringValue = string.Empty;
            //    serializedObject.ApplyModifiedProperties();
            //    // Debug.Log("Transfer Done");
            //}

            var height = EditorGUIUtility.singleLineHeight;

            var activeRect = new Rect(rect.x, rect.y, 20, height);
            var NameRect = new Rect(rect.x + 35, rect.y, rect.width * 0.7f - 35, height);
            var weightRect = new Rect(rect.width - rect.width * 0.3f + 25, rect.y, rect.width * 0.3f + 12f, height);

            EditorGUIUtility.labelWidth = 30;

            var dC = GUI.color;
            if (index == SelectedSet.intValue)
                GUI.color = Color.yellow;


            active.boolValue = EditorGUI.Toggle(activeRect, GUIContent.none, active.boolValue);
            EditorGUI.PropertyField(NameRect, Name, GUIContent.none);
            EditorGUI.PropertyField(weightRect, weight, new GUIContent(" "));
            EditorGUIUtility.labelWidth = 0;

            GUI.color = dC;
        }

        private static string[] EditorLabel = new string[] { "IK Processors", "Weight Processors", "Events" };


        /// <summary> Cached style to use to draw the popup button. </summary>
        private GUIStyle popupStyle;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            popupStyle ??= new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
            {
                imagePosition = ImagePosition.ImageOnly,
                margin = new RectOffset(0, 0, 3, 0)
            };

            MalbersEditor.DrawDescription("Manage all IK logic for all components");


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(animator);
                EditorGUILayout.PropertyField(GlobalWeight);
            }

            Reo_Sets.DoLayoutList();

            var index = SelectedSet.intValue = SelectedSet.intValue;

            if (Reo_Sets.count > 0 && index > -1 && index < Reo_Sets.count)
            {
                var selectedSet = IKSets.GetArrayElementAtIndex(index);
                var Name = m.sets[index].name.Value;

                //EditorGUILayout.Space(-18);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(selectedSet, new GUIContent($"[{Name} - IK Set]"), false);
                    EditorGUI.indentLevel--;

                    //using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    //{
                    //var defaultGuiColor = GUI.contentColor;
                    //GUI.contentColor = Color.yellow;
                    //selectedSet.isExpanded = MalbersEditor.Foldout(selectedSet.isExpanded, $"[[{selectedSet.displayName}] IK Set]");
                    //GUI.contentColor = defaultGuiColor;

                    var Target = selectedSet.FindPropertyRelative("Targets");

                    var processorsAmount = m.sets[index].Processors.Count;
                    var weigthAmmount = m.sets[index].weightProcessors.Count;
                    EditorLabel = new string[] { $"IK Set [{processorsAmount}]", $"Weight Processors [{weigthAmmount}]", "Events" };

                    if (selectedSet.isExpanded)
                    {
                        EditorTabs.intValue = GUILayout.Toolbar(EditorTabs.intValue, EditorLabel);

                        if (EditorTabs.intValue == 0)
                        {
                            DrawFinalWeight(index);

                            var EnableTime = selectedSet.FindPropertyRelative("EnableTime");
                            EnableTime.isExpanded = MalbersEditor.Foldout(EnableTime.isExpanded, $"IK Set General Properties");

                            if (EnableTime.isExpanded)
                            {

                                var InvertAnimParameter = selectedSet.FindPropertyRelative("InvertAnimParameter");
                                var DisableTime = selectedSet.FindPropertyRelative("DisableTime");
                                var aimer = selectedSet.FindPropertyRelative("aimer");
                                var EnterLerp = selectedSet.FindPropertyRelative("EnterLerp");
                                var ExitLerp = selectedSet.FindPropertyRelative("ExitLerp");

                                // var Name = selectedSet.FindPropertyRelative("name");
                                //  EditorGUILayout.PropertyField(Name);

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.PropertyField(EnableTime);

                                    if (EnableTime.floatValue > 0)

                                        EditorGUILayout.PropertyField(EnterLerp, GUIContent.none, GUILayout.MaxWidth(50), GUILayout.MinWidth(5));
                                }

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.PropertyField(DisableTime);
                                    if (DisableTime.floatValue > 0)
                                        EditorGUILayout.PropertyField(ExitLerp, GUIContent.none, GUILayout.MaxWidth(50), GUILayout.MinWidth(5));
                                }

                                EditorGUILayout.PropertyField(aimer);
                            }

                            var ClearTargetsOnDisable = selectedSet.FindPropertyRelative("ClearTargetsOnDisable");
                            var IKProcesors = selectedSet.FindPropertyRelative("IKProcesors");

                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(Target);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.PropertyField(ClearTargetsOnDisable);
                            DrawProfile(selectedSet, IKProcesors);
                        }
                        else if (EditorTabs.intValue == 1)
                        {
                            var weights = selectedSet.FindPropertyRelative("weightProcessors");
                            var LerpWeight = selectedSet.FindPropertyRelative("LerpWeight");

                            DrawFinalWeight(index);

                            EditorGUILayout.PropertyField(LerpWeight);

                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(weights);
                            EditorGUILayout.PropertyField(Target);
                            EditorGUI.indentLevel--;
                        }
                        else if (EditorTabs.intValue == 2)
                        {
                            var OnWeightChanged = selectedSet.FindPropertyRelative("OnWeightChanged");
                            var OnSetEnable = selectedSet.FindPropertyRelative("OnSetEnable");
                            var OnSetDisable = selectedSet.FindPropertyRelative("OnSetDisable");

                            EditorGUILayout.PropertyField(OnWeightChanged);
                            EditorGUILayout.PropertyField(OnSetEnable);
                            EditorGUILayout.PropertyField(OnSetDisable);
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            //base.OnInspectorGUI();

        }

        private void DrawFinalWeight(int index)
        {
            if (Application.isPlaying)
            {
                var guiColor = GUI.color;
                GUI.color = Color.yellow;
                using (new EditorGUI.DisabledGroupScope(true))
                    EditorGUILayout.FloatField("Final Weight", m.sets[index].FinalWeight);
                GUI.color = guiColor;

                Repaint();
            }
        }

        private void DrawProfile(SerializedProperty selectedSet, SerializedProperty link)
        {
            ReorderableList ReoLink;
            string listKey = selectedSet.propertyPath;

            var SelectedIKProcessor = selectedSet.FindPropertyRelative("SelectedIKProcessor");


            if (Reo_Links.ContainsKey(listKey))
            {
                // fetch the reorderable list in dict
                ReoLink = Reo_Links[listKey];
            }
            else
            {
                ReoLink = new ReorderableList(selectedSet.serializedObject, link, true, true, true, true)
                {
                    drawElementCallback = (rect, index, isActive, isFocused) =>
                    {
                        rect.y += 2;

                        var element = link.GetArrayElementAtIndex(index);
                        if (element.managedReferenceValue == null) return;

                        var active = element.FindPropertyRelative("Active");
                        //var weight = element.FindPropertyRelative("weight");

                        //var IndexValue = element.FindPropertyRelative("Index");
                        var name = element.FindPropertyRelative("name");
                        var Weight = element.FindPropertyRelative("Weight");
                        var TargetIndex = element.FindPropertyRelative("TargetIndex");

                        var IDRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight };


                        var height = EditorGUIUtility.singleLineHeight;

                        var activeRect = new Rect(rect.x, rect.y, 20, height);
                        var IndexRect = new Rect(rect.x + 20, rect.y, 35, height);
                        var NameRect = new Rect(rect.x + 60, rect.y, rect.width * 0.7f - 60, height);
                        var weightRect = new Rect(rect.width - rect.width * 0.3f + 25, rect.y, rect.width * 0.3f + 12f, height);


                        var dC = GUI.contentColor;

                        if (SelectedIKProcessor.intValue == index) GUI.contentColor = Color.yellow;

                        EditorGUIUtility.labelWidth = 30;
                        active.boolValue = EditorGUI.Toggle(activeRect, GUIContent.none, active.boolValue);
                        EditorGUI.PropertyField(NameRect, name, GUIContent.none);
                        EditorGUI.PropertyField(IndexRect, TargetIndex, GUIContent.none);
                        EditorGUI.PropertyField(weightRect, Weight, new GUIContent(" "));

                        GUI.contentColor = dC;

                        EditorGUIUtility.labelWidth = 0;
                    },

                    drawHeaderCallback = rect =>
                    {
                        var IDRect = new Rect(rect) { height = EditorGUIUtility.singleLineHeight, width = 60 };

                        EditorGUI.LabelField(IDRect, new GUIContent(" Target [I]", "Target Index from the <Targets> array. Set it to -1 if the Processor does not need any Target"));

                        var height = EditorGUIUtility.singleLineHeight;

                        var nameRect = new Rect(IDRect.x + 75, rect.y, 80, height);
                        var WeightRect = new Rect(rect) { x = rect.width - 30, width = 65 };
                        var button = new Rect(WeightRect.x - 55, WeightRect.y, 50, height);

                        EditorGUI.LabelField(nameRect, "IK Processor");
                        EditorGUI.LabelField(WeightRect, "Weight");

                        var defaultGuiColor = GUI.color;

                        GUI.color = Color.green;

                        if (GUI.Button(button, "Verify"))
                        {
                            m.sets[SelectedSet.intValue].Verify(m.animator);
                        }

                        GUI.color = defaultGuiColor;
                    },

                    onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                    {
                        var menu = new GenericMenu();

                        foreach (var type in derivedTypes)
                        {
                            var att = type.GetCustomAttribute<AddTypeMenuAttribute>(false); //Find the correct name
                            string LabelName = att != null ? att.MenuName : type.Name;

                            menu.AddItem(new GUIContent(LabelName), false, (x) => AddNewItem(x, list, selectedSet), Activator.CreateInstance(type));
                        }
                        menu.ShowAsContext();
                    },

                    onSelectCallback = (list) => { SelectedIKProcessor.intValue = list.index; }
                };

                Reo_Links.Add(listKey, ReoLink);  //Store it on the Editor
            }

            ReoLink.DoLayoutList();
            var index = ReoLink.index = SelectedIKProcessor.intValue;

            if (index != -1 && index < link.arraySize)
            {
                // Debug.Log("SelectedAbility = " + SelectedAbility);
                SerializedProperty ikProcessor = link.GetArrayElementAtIndex(index);

                if (ikProcessor != null)
                {
                    EditorGUILayout.Space(-16);
                    EditorGUILayout.LabelField($"[{ikProcessor.managedReferenceValue.GetType().Name}]", EditorStyles.boldLabel);

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var TargetIndex = ikProcessor.FindPropertyRelative("TargetIndex");

                        var targets = m.sets[SelectedSet.intValue].Targets;
                        var length = targets.Length;
                        var Tindex = TargetIndex.intValue;
                        var RequireTarget = m.sets[SelectedSet.intValue].IKProcesors[index].RequireTargets;

                        string CurrentTarget = Tindex >= 0 && Tindex < length && targets[Tindex].Value ? targets[Tindex].Value.name : "Empty";

                        CurrentTarget = $" [Target: {CurrentTarget}]";

                        if (Tindex == -1 || !RequireTarget) CurrentTarget = "  [No Target Needed]";


                        if (RequireTarget && Tindex >= 0)
                        {
                            if (Tindex >= length)
                                EditorGUILayout.HelpBox($"The Target Index [{Tindex}] greater than the Set Targets Array [{length}]", MessageType.Warning);
                            else if (targets[Tindex].Value == null)
                                EditorGUILayout.HelpBox($"The Target Index [{Tindex}] is Empty. Make sure to set the value in the Editor or at Runtime", MessageType.Warning);
                        }

                        EditorGUILayout.PropertyField(ikProcessor, new GUIContent(ikProcessor.displayName + CurrentTarget), true);


                        if (ikProcessor.isExpanded)
                        {
                            if (animator.objectReferenceValue != null)
                            {
                                FindAllFloatParameters();

                                var AnimParameter = ikProcessor.FindPropertyRelative("AnimParameter");
                                var AnimParameterHash = ikProcessor.FindPropertyRelative("AnimParameterHash");

                                using (new GUILayout.HorizontalScope())
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(AnimParameter, new GUIContent("Anim Param [IKProcessor]", "Local Anim Parameter to apply to a specific IK Processor. E.g Use the Anim Curve for the Left Hand and another anim curve for the Right Hand"));

                                    EditorGUI.indentLevel--;
                                    EditorGUI.indentLevel--;
                                    if (m.gameObject.activeInHierarchy)
                                    {
                                        using (var cc = new EditorGUI.ChangeCheckScope())
                                        {
                                            result = EditorGUILayout.Popup(result, floatAnimParam.ToArray(), popupStyle, GUILayout.Width(12));

                                            if (cc.changed)
                                            {
                                                //Update the Name using the Animator Float Parameters
                                                Undo.RecordObject(target, "Set Anim Parameter");
                                                AnimParameter.stringValue = result == 0 ? string.Empty : floatAnimParam[result];
                                                //Update the Hash
                                                AnimParameterHash.intValue = result == 0 ? 0 : Animator.StringToHash(AnimParameter.stringValue);
                                                serializedObject.ApplyModifiedProperties();
                                                serializedObject.Update();
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }

        void AddNewItem(object target, ReorderableList list, SerializedProperty property)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            IKProcessor link = (IKProcessor)target;
            link.name = target.GetType().Name;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.managedReferenceValue = target;
            element.isExpanded = true;

            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            if (m.sets != null && m.sets.Count > 0 && m.animator != null)
            {
                for (int k = 0; k < m.sets.Count; k++)
                {
                    var set = m.sets[k];

                    if (set != null && m.SelectedSet == k && set.active && set.Processors != null)
                    {
                        for (int i = 0; i < set.Processors.Count; i++)
                        {
                            var link = set.Processors[i];

                            if (link != null && link.Active && set.SelectedIKProcessor == i)
                                link.OnSceneGUI(set, m.animator, target, i);
                        }
                    }
                }
            }
        }
    }
#endif
}

