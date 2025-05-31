using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Scriptables
{
    [AddComponentMenu("Malbers/Variables/Sprite Comparer")]
    public class SpriteComparer : VarListener
    {
        public SpriteReference value;

        public SpriteComparerUnit[] sprites;

        [System.Serializable]
        public class SpriteComparerUnit
        {
            public SpriteReference Target;
            public SpriteEvent Equal = new();
            public SpriteEvent NotEqual = new();
        }
         


        void OnEnable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged += Invoke;

            if (InvokeOnEnable) Invoke(value.Value);
        }

        void OnDisable()
        {
            if (value.Variable != null) value.Variable.OnValueChanged -= Invoke;
           
        }

        /// <summary> Used to use turn Objects to True or false </summary>
        public virtual void Invoke(Sprite value)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                var targ = sprites[i];

                if (value == targ.Target.Value)
                {
                    targ.Equal.Invoke(value);
                    Debbuging($"Sprite Target [{targ.Target.Value.name}][{i}] is equal to the current Value");
                }
                else
                {
                    targ.NotEqual.Invoke(value);
                    Debbuging($"Sprite Target [{targ.Target.Value.name}][{i}] is NOT equal to the Current Value");
                }
            }
        }


        public virtual void Invoke() => Invoke(value.Value);
 

        private void Debbuging(string log)
        {
            if (debug) Debug.Log($"{name}: <B>{log}</B>",this);
        } 
    }

#if UNITY_EDITOR 
    //INSPECTOR
    [UnityEditor.CustomEditor(typeof(SpriteComparer)), UnityEditor.CanEditMultipleObjects]
    public class SpriteComparerEditor : UnityEditor.Editor
    {
        private UnityEditor.SerializedProperty debug, value,  
            sprites, Description, ShowDescription, InvokeOnEnable;
        protected GUIStyle style, styleDesc;

        void OnEnable()
        {

            value = serializedObject.FindProperty("value");
            debug = serializedObject.FindProperty("debug");
          
            InvokeOnEnable = serializedObject.FindProperty("InvokeOnEnable");
          
            sprites = serializedObject.FindProperty("sprites");

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
                InvokeOnEnable.boolValue = GUILayout.Toggle(InvokeOnEnable.boolValue, new GUIContent("E", "Invoke on Enable"),
                    EditorStyles.miniButton, GUILayout.Width(20));
                MalbersEditor.DrawDebugIcon(debug);
            }
                EditorGUILayout.PropertyField(sprites);
 

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
