using UnityEngine;
using MalbersAnimations.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Utilities
{
    /// <summary> Quick Class to Change the values of a Animator Parameter  </summary>
    [AddComponentMenu("Malbers/Utilities/Animator/Get Animator Parameter")]

    public class GetAnimatorParameter : MonoBehaviour
    {
        [RequiredField]
        public Animator animator;
        public string parameter = "Param Name";
        public AnimatorType type = AnimatorType.Float;


        public BoolEvent BoolParam = new();
        public IntEvent IntParam = new();
        public FloatEvent FloatParam = new();

        public int ParameterHash { get; private set; }

        public void Get()
        {
            if (ParameterHash == 0) ParameterHash = Animator.StringToHash(parameter);


            if (ParameterHash != 0 && animator)
                switch (type)
                {
                    case AnimatorType.Float:
                        FloatParam.Invoke(animator.GetFloat(ParameterHash));
                        break;
                    case AnimatorType.Int:
                        IntParam.Invoke(animator.GetInteger(ParameterHash));
                        break;
                    case AnimatorType.Bool:
                        BoolParam.Invoke(animator.GetBool(ParameterHash));
                        break;
                    default:
                        break;
                }
        }


        private void Reset()
        {
            animator = this.FindComponent<Animator>();
        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(GetAnimatorParameter))]
    public class GetAnimatorParameterEditor : Editor
    {
        SerializedProperty animator, parameter, type, BoolParam, FloatParam, IntParam;

        private void OnEnable()
        {
            animator = serializedObject.FindProperty("animator");
            parameter = serializedObject.FindProperty("parameter");
            type = serializedObject.FindProperty("type");
            BoolParam = serializedObject.FindProperty("BoolParam");
            FloatParam = serializedObject.FindProperty("FloatParam");
            IntParam = serializedObject.FindProperty("IntParam");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(animator);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(parameter);
                EditorGUILayout.PropertyField(type, GUIContent.none, GUILayout.Width(75));
            }

            var typeenum = (AnimatorType)type.intValue;

            switch (typeenum)
            {
                case AnimatorType.Float:
                    EditorGUILayout.PropertyField(FloatParam);
                    break;
                case AnimatorType.Int:
                    EditorGUILayout.PropertyField(IntParam);
                    break;
                case AnimatorType.Bool:
                    EditorGUILayout.PropertyField(BoolParam);
                    break;
                default:
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}
