using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace MalbersAnimations.Conditions
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/conditions")]
    [AddComponentMenu("Malbers/Interactions/Old Conditions (OBSOLETE) Use Conditions2"), DisallowMultipleComponent]
    public class MConditions : MonoBehaviour
    {
        [Tooltip("Evaluate the conditions on Enable")]
        public bool EvaluateOnEnable = false;
        public bool EvaluateOnDisable = false;

        [SerializeReference]
        public List<MCondition> conditions;

        public UnityEvent Then = new();
        public UnityEvent Else = new();

        public MCondition Pinned;

        public bool debug;

#pragma warning disable 414
        [HideInInspector, SerializeField] private int SelectedState = -1;
        [HideInInspector, SerializeField] private bool showResponse = true;
#pragma warning restore 414


        private void OnEnable()
        {
            if (EvaluateOnEnable) TryEvaluate();
        }

        private void OnDisable()
        {
            if (EvaluateOnDisable) TryEvaluate();
        }

        /// <summary> Set the Target of the conditions depending of the Object Type</summary>
        public virtual void SetTarget(UnityEngine.Object target)
        {
            foreach (var c in conditions)
                c.SetTarget(target);
        }

        public virtual void Pin_SetTarget(UnityEngine.Object target) => Pinned?.SetTarget(target);
        public virtual void Pin_Condition(int Index) => Pinned = conditions[Index];

        public void Evaluate() => TryEvaluate();

        public void Evaluate(UnityEngine.Object target)
        {
            SetTarget(target);
            TryEvaluate();
        }

        /// <summary> Evaluate all conditions when  </summary>
        /// <param name="value"></param>
        public void Evaluate_OnTrue(bool value)
        {
            if (value) TryEvaluate();
        }
        public void Evaluate_OnFalse(bool value)
        {
            if (!value) TryEvaluate();
        }

        public void Evaluate_OnInt(int value)
        {
            if (value > 0) TryEvaluate();
        }

        [ContextMenu("Show Conditions")]
        private void ShowAllConditions()
        {
            var conditions = GetComponents<MCondition>();
            foreach (var item in conditions)
            {
                item.hideFlags = HideFlags.None;
            }
        }

        [ContextMenu("Hide Conditions")]
        private void HideAllConditions()
        {
            var conditions = GetComponents<MCondition>();

            foreach (var item in conditions)
            {
                item.hideFlags = HideFlags.HideInInspector;
            }
        }

        public bool TryEvaluate()
        {
            if (conditions != null && conditions.Count > 0)
            {
                var c = conditions[0];
                bool result = c.Evaluate(); //Get the first one

                Debuggin(c, result);

                for (int i = 1; i < conditions.Count; i++)
                {
                    c = conditions[i];
                    var nextResult = c.Evaluate();

                    Debuggin(c, nextResult);
                    result = c.OrAnd ? (result || nextResult) : (result && nextResult);
                }
                if (result)
                    Then.Invoke();
                else
                    Else.Invoke();


                if (debug) Debug.Log($"[{name}] → Conditions Result → <B><color={(result ? "green" : "red")}>[{result}] </color></B>", this);

                return result;
            }
            return false;
        }
        public void InvokeThen() => Then.Invoke();
        public void InvokeElse() => Else.Invoke();

        private void Debuggin(MCondition c, bool result)
        {
            if (debug) Debug.Log($"[{name}] →  Cond: <B>[{c.GetType().Name}] {(c.invert ? "[!]" : " ")}  → <color={(result ? "green" : "red")}>[{result}] </color></B>.", this);
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MConditions))]
    public class MConditionsEditor : Editor
    {
        SerializedObject so;
        MConditions M;
        SerializedProperty conditions, Then, Else, SelectedState, showResponse, EvaluateOnEnable, EvaluateOnDisable,
            debug;

        private List<Type> StatesType = new();
        private ReorderableList Reo_List_States;


        private void OnEnable()
        {
            so = serializedObject;
            M = (MConditions)target;
            conditions = so.FindProperty("conditions");
            Then = so.FindProperty("Then");
            Then = so.FindProperty("Then");
            Else = so.FindProperty("Else");
            debug = so.FindProperty("debug");
            SelectedState = so.FindProperty("SelectedState");
            showResponse = so.FindProperty("showResponse");
            EvaluateOnEnable = so.FindProperty("EvaluateOnEnable");
            EvaluateOnDisable = so.FindProperty("EvaluateOnDisable");

            Reo_List_States = new ReorderableList(serializedObject, conditions, true, true, true, true)
            {
                drawHeaderCallback = Draw_Header_State,
                drawElementCallback = Draw_Element_State,
                //onReorderCallbackWithDetails = OnReorderCallback_States_Details,
                onAddCallback = OnAddCallback_State,
                onRemoveCallback = OnRemove_Condition,
                onSelectCallback = Selected_Cond,
            };


            StatesType.Clear();
            StatesType = MTools.GetAllTypes<MCondition>();

            Reo_List_States.index = SelectedState.intValue;
        }
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription($"Global Conditions. Call MConditions.Evaluate() to invoke the response");

            so.Update();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Evaluate All"))
                {
                    M.TryEvaluate();
                }
            }

            Reo_List_States.DoLayoutList();

            var index = Mathf.Clamp(Reo_List_States.index, -1, Reo_List_States.count);

            if (index != -1)
            {
                var element = conditions.GetArrayElementAtIndex(index);

                if (element != null)
                {
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            var dC = GUI.backgroundColor;
                            GUI.backgroundColor = MTools.MGreen;

                            EditorGUI.indentLevel++;
                            element.isExpanded = GUILayout.Toggle(element.isExpanded,
                                $"Condition [{index}] : [{M.conditions[index].Name}] ",
                                EditorStyles.foldoutHeader, GUILayout.MinWidth(40));
                            EditorGUI.indentLevel--;

                            GUI.backgroundColor = dC;



                            SerializedObject elementSo = new(element.objectReferenceValue);
                            var invert = elementSo.FindProperty("invert");

                            elementSo.Update();
                            GUI.color = invert.boolValue ? Color.red : dC;
                            invert.boolValue = GUILayout.Toggle(invert.boolValue, new GUIContent("NOT", "Inverts the result of the condition"),
                               EditorStyles.miniButton, GUILayout.Width(38));
                            GUI.color = dC;
                            elementSo.ApplyModifiedProperties();
                        }

                        if (element.isExpanded)
                        {
                            MTools.DrawObjectReferenceInspector(element);

                        }
                    }
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {

                        showResponse.boolValue = MalbersEditor.Foldout(showResponse.boolValue, "Response (Then-Else)");

                        if (showResponse.boolValue)
                        {
                            EditorGUILayout.PropertyField(Then);
                            EditorGUILayout.PropertyField(Else);
                        }
                    }
                }
            }

            so.ApplyModifiedProperties();
        }


        private void Draw_Header_State(Rect rect)
        {
            var r = new Rect(rect);
            r.x += 6;
            r.width = 300;

            EditorGUI.LabelField(r, new GUIContent("       Conditions", "Description or Name for the Condition"), EditorStyles.boldLabel);

            Rect R_2 = new(rect.width + 4, rect.y - 1, 25, EditorGUIUtility.singleLineHeight - 3);

            Rect R_3 = new(rect.width - 76, rect.y - 1, 78, EditorGUIUtility.singleLineHeight);
            Rect R_4 = new(rect.width - 155, rect.y - 1, 78, EditorGUIUtility.singleLineHeight);

            MalbersEditor.DrawDebugIcon(R_2, debug);

            // var ON = EvaluateOnEnable.boolValue;
            // var currentGUIColor = GUI.color;
            //GUI.color = ON ? Color.green : currentGUIColor;
            EvaluateOnEnable.boolValue = GUI.Toggle(R_4, EvaluateOnEnable.boolValue, _OnEnableG, EditorStyles.miniButton);
            EvaluateOnDisable.boolValue = GUI.Toggle(R_3, EvaluateOnDisable.boolValue, _OnDisableG, EditorStyles.miniButton);
            // GUI.color = currentGUIColor;
        }


        private GUIContent _OnEnableG = new("On Enable", "Evaluate all conditions on Enable");
        private GUIContent _OnDisableG = new("On Disable", "Evaluate all conditions on Disable");

        private void Draw_Element_State(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = conditions.GetArrayElementAtIndex(index);

            if (element != null && element.objectReferenceValue != null)
            {
                SerializedObject elementSo = new(element.objectReferenceValue);

                elementSo.Update();

                var name = elementSo.FindProperty("Name");
                var OrAnd = elementSo.FindProperty("OrAnd");
                // var invert = elementSo.FindProperty("invert");

                var AndORWidth = 40;
                // var InvertWidth = 45;

                var elRect = new Rect(rect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    y = rect.y + 2,
                    x = rect.x + AndORWidth + 2,
                    width = rect.width - AndORWidth - 2 - 2,
                };


                var AndOrRect = new Rect(rect)
                {
                    height = EditorGUIUtility.singleLineHeight,
                    y = rect.y + 2,
                    width = AndORWidth,
                };

                var dC = GUI.color;
                if (index != 0)
                {
                    GUI.color = OrAnd.boolValue ? Color.cyan : Color.green;
                    OrAnd.boolValue = GUI.Toggle(AndOrRect, OrAnd.boolValue, new GUIContent(OrAnd.boolValue ? "OR" : "AND"), EditorStyles.miniButton);
                    GUI.color = dC;
                }

                var DC = GUI.color;
                GUI.color = index == SelectedState.intValue ? Color.green : DC;
                EditorGUI.PropertyField(elRect, name, GUIContent.none);
                GUI.color = DC;

                elementSo.ApplyModifiedProperties();
            }
        }

        void OnDisable()
        {
            if (target == null)
            {
                foreach (var item in M.conditions)
                {
                    DestroyImmediate(item); //Destroy all Monobehaviours added when the Main script is destroyed
                }
            }
        }

        private void OnAddCallback_State(ReorderableList list)
        {
            var addMenu = new GenericMenu();

            for (int i = 0; i < StatesType.Count; i++)
            {
                Type st = StatesType[i];

                var att = st.GetCustomAttribute<AddTypeMenuAttribute>(false); //Find the correct name

                string LabelName = att != null ? att.MenuName : st.Name;

                addMenu.AddItem(new GUIContent(LabelName), false, () => AddCondition(st));
            }

            addMenu.ShowAsContext();
        }

        private void AddCondition(Type st)
        {
            MCondition cond = M.gameObject.AddComponent(st) as MCondition;

            cond.hideFlags = HideFlags.HideInInspector;

            conditions.serializedObject.Update();

            var ind = Mathf.Clamp(conditions.arraySize, 0, conditions.arraySize);

            cond.Name += $"({st.Name})";

            conditions.InsertArrayElementAtIndex(ind);
            conditions.GetArrayElementAtIndex(ind).objectReferenceValue = cond;
            conditions.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        /// <summary> The ReordableList remove button has been pressed. Remove the selected ability.</summary>
        private void OnRemove_Condition(ReorderableList list)
        {
            var objRef = conditions.GetArrayElementAtIndex(list.index).objectReferenceValue;

            if (objRef != null)
            {
                var state = objRef as MCondition;
                DestroyImmediate(state);
            }
            conditions.DeleteArrayElementAtIndex(list.index);

            list.index -= 1;
            SelectedState.intValue = list.index;
            EditorUtility.SetDirty(target);
        }
        private void Selected_Cond(ReorderableList list) => SelectedState.intValue = list.index;
        protected virtual void ShowConditionEditor(SerializedObject serializedObject)
        {
            var skip = 1;
            var property = serializedObject.GetIterator();
            property.NextVisible(true);

            for (int i = 0; i < skip; i++)
                property.NextVisible(false);

            do
            {
                EditorGUILayout.PropertyField(property, true);
            } while (property.NextVisible(false));

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif

}