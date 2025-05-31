using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Variables/Int Comparer")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class IntComparer : IntVarListener
    {
        public List<AdvancedIntegerEvent> compare = new();

        /// <summary>Set the first value on the comparer </summary>
        public int SetCompareFirstValue { get => compare[0].Value.Value; set => compare[0].Value.Value = value; }

        public IntEvent OnValueChanged = new();

        /// <summary>Pin a Comparer</summary>
        private AdvancedIntegerEvent Pin;

        public override int Value
        {
            set
            {
                base.Value = value;
                if (Auto)
                {
                    OnValueChanged.Invoke(value);
                    Compare();
                }
            }
        }


        public int this[int index]
        {
            get => compare[index].Value.Value;
            set => compare[index].Value.Value = value;
        }

        public void Pin_Comparer(int index) => Pin = compare[index];
        public void Pin_Comparer_SetValue(int value) => Pin?.SetValue(value);
        public void Pin_Comparer_SetValue(float value) => Pin?.SetValue((int)value);
        public void Pin_Comparer_SetValue(IntVar value) => Pin?.SetValue(value.Value);
        public void Pin_Comparer_SetValue(IDs value) => Pin?.SetValue(value.ID);

        public void AddWithBool(bool value) => SetValue(value ? Value + 1 : Value - 1);


        void OnEnable()
        {
            if (value.Variable && Auto)
            {
                value.Variable.OnValueChanged += Compare;
                value.Variable.OnValueChanged += Invoke;
            }

            Raise.Invoke(Value);
        }

        void OnDisable()
        {
            if (value.Variable && Auto)
            {
                value.Variable.OnValueChanged -= Compare;
                value.Variable.OnValueChanged -= Invoke;
            }
        }

        private void Reset()
        {
            compare = new()
              {
                  new() { Value = new(0), active = true, comparer = ComparerInt.Equal, name = "Compare"},
              };
        }
        public void Value_Add(int value) => Value += value;
        public void Value_Substract(int value) => Value -= value;
        public void Value_Multiply(int value) => Value *= value;
        public void Value_Divide(int value) => Value /= value;

        /// <summary>Compares the Int parameter on this Component and if the condition is made then the event will be invoked</summary>
        public virtual void Compare()
        {
            if (!enabled) return;

            foreach (var item in compare)
                item.ExecuteAdvanceIntegerEvent(value);
        }


        /// <summary>Compares an given int Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(int value)
        {
            if (!enabled) return;
            foreach (var item in compare)
                item.ExecuteAdvanceIntegerEvent(value);
        }

        /// <summary>Compares an given intVar Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(IntVar value)
        {
            if (!enabled) return;
            foreach (var item in compare)
                item.ExecuteAdvanceIntegerEvent(value.Value);
        }

        public void Index_Disable(int index) => compare[index].active = false;
        public void Index_Enable(int index) => compare[index].active = true;
    }


    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(IntComparer))]
    public class IntCompareEditor : VarListenerEditor
    {
        private UnityEditor.SerializedProperty compare, OnValueChanged;
        private UnityEditorInternal.ReorderableList reo_compare;

        private void OnEnable()
        {
            base.SetEnable();

            compare = serializedObject.FindProperty("compare");
            OnValueChanged = serializedObject.FindProperty("OnValueChanged");

            reo_compare = new UnityEditorInternal.ReorderableList(serializedObject, compare, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = compare.GetArrayElementAtIndex(index);
                    var name = element.FindPropertyRelative("name");
                    var comparer = element.FindPropertyRelative("comparer");
                    var Value = element.FindPropertyRelative("Value");
                    var active = element.FindPropertyRelative("active");


                    rect.y += 1;
                    var height = UnityEditor.EditorGUIUtility.singleLineHeight;
                    var split = rect.width / 3;
                    var p = 5;

                    var rectActveName = new Rect(rect.x, rect.y, 20, height);
                    var rectName = new Rect(rect.x + 20, rect.y, (split + p - 2) * 1.2f - 20, height);
                    var rectComparer = new Rect(rect.x + (split + p) * 1.2f, rect.y, (split - p) * 0.75f, height);
                    var rectValue = new Rect(rect.x + split * 2 + p + 15, rect.y, split - p - 10, height);


                    var def = GUI.color;

                    if (isActive) GUI.color = Color.yellow;
                    if (!active.boolValue) GUI.color = Color.gray;

                    UnityEditor.EditorGUI.PropertyField(rectActveName, active, GUIContent.none);
                    UnityEditor.EditorGUI.PropertyField(rectName, name, GUIContent.none);
                    UnityEditor.EditorGUI.PropertyField(rectComparer, comparer, GUIContent.none);
                    UnityEditor.EditorGUI.PropertyField(rectValue, Value, GUIContent.none);
                    GUI.color = def;
                },

                drawHeaderCallback = (rect) =>
                {
                    rect.y += 1;
                    var height = UnityEditor.EditorGUIUtility.singleLineHeight;
                    var split = rect.width / 3;
                    var p = (split * 0.3f);
                    var rectName = new Rect(rect.x, rect.y, split + p - 2, height);
                    var rectComparer = new Rect(rect.x + split + p, rect.y, split - p + 15, height);
                    var rectValue = new Rect(rect.x + split * 2 + p + 5, rect.y, split - p, height);
                    var DebugRect = new Rect(rect.width, rect.y - 1, 25, height + 2);

                    UnityEditor.EditorGUI.LabelField(rectName, "Active   Name");
                    UnityEditor.EditorGUI.LabelField(rectComparer, " Compare");
                    UnityEditor.EditorGUI.LabelField(rectValue, " Value");
                }
            };
        }

        protected override void DrawElemets()
        {
            reo_compare.DoLayoutList();

            int SelectedItem = reo_compare.index;
            if (SelectedItem != -1 && SelectedItem < reo_compare.count)
            {
                var element = compare.GetArrayElementAtIndex(SelectedItem);
                if (element != null)
                {
                    UnityEditor.EditorGUILayout.Space(-20);




                    if (styleDesc == null)
                        styleDesc = new GUIStyle(MTools.StyleGray)
                        {
                            fontSize = 14,
                            fontStyle = FontStyle.Normal,
                            alignment = TextAnchor.MiddleLeft,
                            stretchWidth = true
                        };

                    styleDesc.normal.textColor = UnityEditor.EditorStyles.boldLabel.normal.textColor;

                    var description = element.FindPropertyRelative("description");
                    var UpdateAfterCompare = element.FindPropertyRelative("UpdateAfterCompare");

                    UnityEditor.EditorGUILayout.LabelField("Description", UnityEditor.EditorStyles.boldLabel);
                    description.stringValue = UnityEditor.EditorGUILayout.TextArea(description.stringValue, styleDesc);
                    using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
                    {
                        var Response = element.FindPropertyRelative("Response");
                        var name = element.FindPropertyRelative("name").stringValue;
                        if (UpdateAfterCompare != null) UnityEditor.EditorGUILayout.PropertyField(UpdateAfterCompare);
                        UnityEditor.EditorGUILayout.PropertyField(Response, new GUIContent("Response: [" + name + "]   "));
                        ExtraEvents(element);
                    }
                }
            }

            if (OnValueChanged != null)
                UnityEditor.EditorGUILayout.PropertyField(OnValueChanged);


        }

        protected virtual void ExtraEvents(UnityEditor.SerializedProperty element)
        { }
    }
#endif
}