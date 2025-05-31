using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Variables/Transform Comparer")]
    public class TransformComparer : VarListener
    {
        public enum TransformCondition { Null, Equal, ChildOf, ParentOf, Name }

        public TransformReference value;
        public TransformCondition Condition;
        public TransformReference compareTo;
        public StringReference T_Name;

        //[Tooltip("Invokes the current value on Enable")]
        //public bool InvokeOnEnable = true;

        public UnityEvent Then = new();
        public UnityEvent Else = new();


        void OnEnable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged += Invoke;
            if (compareTo.Variable != null) compareTo.Variable.OnValueChanged += Invoke;

            if (InvokeOnEnable) Invoke(value.Value);
        }

        void OnDisable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged -= Invoke;
            if (compareTo.Variable != null) compareTo.Variable.OnValueChanged -= Invoke;
        }

        /// <summary> Used to use turn Objects to True or false </summary>
        public virtual void Invoke(Transform value)
        {
            switch (Condition)
            {
                case TransformCondition.Null:
                    Response(value == null);
                    Debbuging($"Value is Null ? [{value == null}]");
                    break;
                case TransformCondition.Equal:
                    Response(value == compareTo.Value);
                    Debbuging($"{value} == {compareTo.Value} -> [{value == compareTo.Value}]");
                    break;
                case TransformCondition.ChildOf:
                    if (value) Response(value.IsChildOf(compareTo.Value));
                    break;
                case TransformCondition.ParentOf:
                    if (compareTo.Value) Response(compareTo.Value.IsChildOf(value));
                    break;
                case TransformCondition.Name:
                    if (value) Response(value.name.Contains(T_Name));
                    Debbuging($"Name is Equal to {value}");
                    break;
                default:
                    break;
            }
        }


        public virtual void Invoke() => Invoke(value.Value);

        public void SetValue(Component target) => SetTarget(target);

        public void SetTarget(Component target)
        {
            value.Value = target ? target.transform : null;
            Invoke();
        }

        private void Debbuging(string log)
        {
            if (debug) Debug.Log($"{name}: <B>{log}</B>",this);
        }


        public void SetValue(GameObject target) => SetTarget(target);
        public void SetTarget(GameObject target)
        {
            value.Value = target ? target.transform: null;
            Invoke();
        }

        public void SetCompareTo(Component target)
        {
            compareTo.Value = target ? target.transform : null;
            Invoke();
        }

        public void SetCompareTo(GameObject target)
        {
            compareTo.Value = target ? target.transform : null;
            Invoke();
        }

        public void ClearValue() => ClearTarget();

        public void ClearTarget()
        {
            if (value.Value != null)    
            {
                value.Value = null;
                Invoke();
            }
        }

        public void ClearComparteTo()
        {
            if (compareTo.Value != null)
            {
                compareTo.Value = null;
                Invoke();
            }
        }

        private void Response(bool value)
        {
            if (value) 
                Then.Invoke();
            else 
                Else.Invoke();
        }
    }

#if UNITY_EDITOR 
    //INSPECTOR
    [UnityEditor.CustomEditor(typeof(TransformComparer)), UnityEditor.CanEditMultipleObjects]
    public class TransformComparerEditor : UnityEditor.Editor
    {
        private UnityEditor.SerializedProperty debug, value, Then, Else, 
            Condition, compareTo, T_Name, Description, ShowDescription, InvokeOnEnable;
        protected GUIStyle style, styleDesc;

        void OnEnable()
        {
            
            value = serializedObject.FindProperty("value");
            debug = serializedObject.FindProperty("debug");
            Then = serializedObject.FindProperty("Then");
            Else = serializedObject.FindProperty("Else");
            Condition = serializedObject.FindProperty("Condition");
            InvokeOnEnable = serializedObject.FindProperty("InvokeOnEnable");
            compareTo = serializedObject.FindProperty("compareTo");
            T_Name = serializedObject.FindProperty("T_Name");

            Description = serializedObject.FindProperty("Description");
            ShowDescription = serializedObject.FindProperty("ShowDescription");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            if (ShowDescription.boolValue)
            {
                if (ShowDescription.boolValue)
                {
                    if (style == null)
                    {
                        style = new GUIStyle(MTools.StyleBlue)
                        {
                            fontSize = 12,
                            fontStyle = FontStyle.Bold,
                            alignment = TextAnchor.MiddleLeft,
                            stretchWidth = true
                        };

                        style.normal.textColor = EditorStyles.boldLabel.normal.textColor;
                    }

                    Description.stringValue = EditorGUILayout.TextArea(Description.stringValue, style);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
              
                EditorGUILayout.PropertyField(value);
                InvokeOnEnable.boolValue = GUILayout.Toggle(InvokeOnEnable.boolValue, new GUIContent("E", "Invoke on Enable"), EditorStyles.miniButton, GUILayout.Width(20));
                MalbersEditor.DrawDebugIcon(debug);
            }

           EditorGUILayout.PropertyField(Condition);

            if (Condition.intValue != 0 && Condition.intValue != 4)
            {
                EditorGUILayout.PropertyField(compareTo);
            }

            if (Condition.intValue == 4)
               EditorGUILayout.PropertyField(T_Name, new GUIContent("Transform Name"));

           EditorGUILayout.PropertyField(Then);
           EditorGUILayout.PropertyField(Else);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
