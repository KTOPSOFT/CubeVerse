using System;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Prefab Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Game Object", order = 3000)]
    public class GameObjectVar : ScriptableVar
    {
        [SerializeField, HideInInspector]
        private GameObject value;

        /// <summary>Invoked when the value changes </summary>
        public Action<GameObject> OnValueChanged;

        /// <summary> Value of the Bool variable</summary>
        public virtual GameObject Value
        {
            get => value;
            set
            {
                this.value = value;
                OnValueChanged?.Invoke(value);         //If we are using OnChange event Invoked
#if UNITY_EDITOR
                if (debug) Debug.Log($"<B>{name} -> [<color=cyan> {value} </color>] </B>");
#endif
            }
        }

        public virtual void SetValue(GameObjectVar var) => Value = var.Value;
        public virtual void SetNull(GameObjectVar var) => Value = null;
        public virtual void SetValue(GameObject var) => Value = var;
        public virtual void SetValue(Component var) => Value = var.gameObject;

    }

    [System.Serializable]
    public class GameObjectReference : ReferenceVar
    {
        public GameObject ConstantValue;
        [RequiredField] public GameObjectVar Variable;

        public GameObjectReference() => UseConstant = true;
        public GameObjectReference(GameObject value) => Value = value;

        public GameObjectReference(GameObjectVar value)
        {
            Variable = value;
            UseConstant = false;
        }

        public GameObject Value
        {
            get => UseConstant ? ConstantValue : (Variable != null ? Variable.Value : null);
            set
            {
                if (UseConstant || Variable == null)
                {
                    ConstantValue = value;
                    UseConstant = true;
                }
                else
                {
                    Variable.Value = value;
                }
            }
        }

        public static implicit operator GameObject(GameObjectReference reference) => reference.Value;
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(GameObjectVar))]
    public class GameObjectVarEditor : VariableEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("GameObject/Prefab Variable");
            using (new GUILayout.VerticalScope(UnityEditor.EditorStyles.helpBox))
            {
                var go = value.objectReferenceValue as GameObject;

                using (new GUILayout.HorizontalScope())
                {
                    if (go == null || go.IsPrefab())
                    {
                        UnityEditor.EditorGUILayout.PropertyField(value, new GUIContent("Prefab", "The current value"));
                    }
                    else
                    {
                        if (Application.isPlaying)
                        {
                            UnityEditor.EditorGUILayout.ObjectField("Value ", go, typeof(GameObject), false);
                        }
                    }

                    MalbersEditor.DrawDebugIcon(debug);
                }

                UnityEditor.EditorGUILayout.PropertyField(Description, GUIContent.none);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
