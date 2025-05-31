using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.Scriptables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Conditions
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/global-components/conditions")]
    [AddComponentMenu("Malbers/Interactions/Conditions2"), DisallowMultipleComponent]
    public class MConditions2 : MonoBehaviour
    {
        [Tooltip("Evaluate the conditions on Enable")]
        public bool EvaluateOnEnable = false;
        public bool EvaluateOnDisable = false;
        public bool Repeat = false;
        [Min(0.01f)] public float repeatTime = 0.1f;

        public GameObjectReference target;
        public Conditions2 conditions;

        public UnityEvent Then = new();
        public UnityEvent Else = new();

        public bool debug;

#pragma warning disable 414
        [HideInInspector, SerializeField] private int SelectedState = -1;
        [HideInInspector, SerializeField] private bool showResponse = true;
#pragma warning restore 414


        private void OnEnable()
        {
            if (EvaluateOnEnable) TryEvaluate(target.Value);

            if (Repeat) InvokeRepeating(nameof(Evaluate), 0, repeatTime);
        }

        private void OnDisable()
        {
            if (EvaluateOnDisable) TryEvaluate(target.Value);
            CancelInvoke();
        }


        public void Evaluate() => TryEvaluate(target.Value);

        public void Evaluate(UnityEngine.Object target)
        {
            TryEvaluate(target);
        }

        /// <summary> Evaluate all conditions when  </summary>
        /// <param name="value"></param>
        public void Evaluate_OnTrue(bool value)
        {
            if (value) TryEvaluate(target.Value);
        }
        public void Evaluate_OnFalse(bool value)
        {
            if (!value) TryEvaluate(target.Value);
        }

        public void Evaluate_OnInt(int value)
        {
            if (value > 0) TryEvaluate(target);
        }

        public bool TryEvaluate(UnityEngine.Object target)
        {
            var result = conditions.Evaluate(target);

            if (result) InvokeThen(); else InvokeElse();

            if (debug) Debug.Log($"[{name}] → Conditions Result → <B><color={(result ? "green" : "red")}>[{result}] </color></B>", this);


            return result;
        }
        public void InvokeThen() => Then.Invoke();
        public void InvokeElse() => Else.Invoke();


        public void Pause_Editor() => Debug.Break();
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MConditions2))]
    public class MConditions2Editor : Editor
    {
        SerializedObject so;
        MConditions2 M;
        SerializedProperty conditions, Target, Then, Else, SelectedState, showResponse, EvaluateOnEnable, EvaluateOnDisable, Repeat, repeatTime,
            debug
          ;

        private void OnEnable()
        {
            so = serializedObject;
            M = (MConditions2)target;
            conditions = so.FindProperty("conditions");
            Target = so.FindProperty("target");
            Then = so.FindProperty("Then");
            Then = so.FindProperty("Then");
            Else = so.FindProperty("Else");
            debug = so.FindProperty("debug");
            SelectedState = so.FindProperty("SelectedState");
            showResponse = so.FindProperty("showResponse");
            EvaluateOnEnable = so.FindProperty("EvaluateOnEnable");
            EvaluateOnDisable = so.FindProperty("EvaluateOnDisable");
            Repeat = so.FindProperty("Repeat");
            repeatTime = so.FindProperty("repeatTime");

            var allcomponents = M.GetComponents<Component>();
            foreach (var component in allcomponents)
            {
                // Debug.Log("component = " + component.GetType());
                component.hideFlags = HideFlags.None;
            }
        }
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription($"Global Conditions. Call MConditions.Evaluate() to invoke the response");

            serializedObject.Update();

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Evaluate All")) M.Evaluate();
            }
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(Target);
                    EditorGUIUtility.labelWidth = 0;
                    EvaluateOnEnable.boolValue = GUILayout.Toggle(EvaluateOnEnable.boolValue, _OnEnableG, EditorStyles.miniButton, GUILayout.Width(57));
                    EvaluateOnDisable.boolValue = GUILayout.Toggle(EvaluateOnDisable.boolValue, _OnDisableG, EditorStyles.miniButton, GUILayout.Width(57));
                    Repeat.boolValue = GUILayout.Toggle(Repeat.boolValue, _RepeatG, EditorStyles.miniButton, GUILayout.Width(57));
                    if (Repeat.boolValue)
                    {
                        EditorGUIUtility.labelWidth = 60;
                        EditorGUILayout.PropertyField(repeatTime, GUIContent.none, GUILayout.Width(40));
                    }

                    MalbersEditor.DrawDebugIcon(debug);
                }
                EditorGUILayout.PropertyField(conditions.FindPropertyRelative("conditions"));
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
            serializedObject.ApplyModifiedProperties();
        }

        private readonly GUIContent _OnEnableG = new("Enable", "Evaluate all conditions On Enable");
        private readonly GUIContent _OnDisableG = new("Disable", "Evaluate all conditions On Disable");
        private readonly GUIContent _RepeatG = new("Repeat", $"Evaluate all conditions every x seconds");
    }
#endif

}