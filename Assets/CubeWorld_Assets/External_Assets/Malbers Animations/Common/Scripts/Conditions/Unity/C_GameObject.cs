using MalbersAnimations.Scriptables;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Conditions
{
    public enum GOCondition { ActiveInHierarchy, ActiveSelf, Null, Equal, Prefab, Name, Layer, Tag, MalbersTag }

    [System.Serializable]
    [AddTypeMenu("Unity/GameObject")]
    public class C_GameObject : MCondition
    {
        //public override string DisplayName => "Unity/GameObject";

        public GameObjectReference Target;
        public GOCondition Condition;
        public GameObjectReference Value;
        public StringReference checkName;
        public LayerReference Layer;
        public Tag[] tags;

        public override bool _Evaluate()
        {
            if (Condition == GOCondition.Null) return Target.Value == null;

            if (Target.Value)
            {
                return Condition switch
                {
                    GOCondition.Name => Target.Value.name.Contains(checkName),
                    GOCondition.Prefab => Target.Value.IsPrefab(),
                    GOCondition.ActiveInHierarchy => Target.Value.activeInHierarchy,
                    GOCondition.ActiveSelf => Target.Value.activeSelf,
                    GOCondition.Equal => Target.Value == Value.Value,
                    GOCondition.Layer => MTools.Layer_in_LayerMask(Target.Value.layer, Layer.Value),
                    GOCondition.Tag => Target.Value.CompareTag(checkName),
                    GOCondition.MalbersTag => Target.Value.HasMalbersTag(tags),
                    _ => false,
                };
            }
            return false;
        }

        protected override void _SetTarget(Object target)
        {
            var Tar = Target.Value;
            VerifyTarget(target, ref Tar);
            Target.Value = Tar;
        }

        private void Reset() => Name = "New GameObject Condition";

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(C_GameObject))]
    public class C_GameObjectEditor : MConditionEditor
    {
        SerializedProperty checkName, Layer, tags;

        protected override void OnEnable()
        {
            base.OnEnable();
            checkName = so.FindProperty("checkName");
            Layer = so.FindProperty("Layer");
            tags = so.FindProperty("tags");
        }

        public override void CustomInspector()
        {
            var c = (GOCondition)Condition.intValue;


            if (c == GOCondition.Equal)
                EditorGUILayout.PropertyField(Value);

            else if (c == GOCondition.Name || c == GOCondition.Tag)

                EditorGUILayout.PropertyField(checkName, new GUIContent(c.ToString()));
            else if (c == GOCondition.Layer)
                EditorGUILayout.PropertyField(Layer);
            else if (c == GOCondition.MalbersTag)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(tags, true);
                EditorGUI.indentLevel--;
            }
        }
    }
#endif
}
