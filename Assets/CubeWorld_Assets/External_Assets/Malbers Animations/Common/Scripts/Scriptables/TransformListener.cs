using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Variables/Transform Listener")]
    public class TransformListener : VarListener
    {
        public TransformReference value;
        
        public TransformEvent OnValueChanged = new();
        public UnityEvent OnValueNull = new();

        public virtual Transform Value
        {
            get => value.Value;
            set
            {
                if (Auto) this.value.Value = value;
                Invoke(value);
            }
        }



        void OnEnable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged += Invoke;
            if (InvokeOnEnable) Invoke(value.Value);
        }

        void OnDisable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged -= Invoke;
            Invoke(value.Value);
        }

        public virtual void SetValue(TransformReference value)
        {
            Value = value.Value;
        }

        /// <summary> Used to use turn Objects to True or false </summary>
        public virtual void Invoke(Transform value)
        {
            OnValueChanged.Invoke(value);
            if (!value) OnValueNull.Invoke(); //Invoke Null
        }
    }

#if UNITY_EDITOR 
    //INSPECTOR
    [UnityEditor.CustomEditor(typeof(TransformListener)), UnityEditor.CanEditMultipleObjects]
    public class TransformListenerEditor : UnityEditor.Editor
    {
        private UnityEditor.SerializedProperty value, InvokeOnEnable, OnValueNull, Description, ShowDescription, OnValueChanged;
        protected GUIStyle style, styleDesc;
        void OnEnable()
        {
            value = serializedObject.FindProperty("value");
            InvokeOnEnable = serializedObject.FindProperty("InvokeOnEnable");
            OnValueChanged = serializedObject.FindProperty("OnValueChanged");
            OnValueNull = serializedObject.FindProperty("OnValueNull");
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

                        style.normal.textColor = UnityEditor.EditorStyles.boldLabel.normal.textColor;
                    }

                    Description.stringValue = UnityEditor.EditorGUILayout.TextArea(Description.stringValue, style);
                }
            }

            using (new GUILayout.HorizontalScope(UnityEditor.EditorStyles.helpBox))
            {
               EditorGUILayout.PropertyField(InvokeOnEnable, GUIContent.none, GUILayout.Width(18));
               EditorGUILayout.PropertyField(value);

                value.isExpanded =
                   GUILayout.Toggle(value.isExpanded,
                   new GUIContent((value.isExpanded ? "▲" : "▼"), "Show Events"), EditorStyles.miniButton, GUILayout.Width(25));
            }

            if (value.isExpanded)
            {
                EditorGUILayout.PropertyField(OnValueChanged);
                EditorGUILayout.PropertyField(OnValueNull);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
