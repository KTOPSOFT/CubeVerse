using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("Unity/GameObject")]
    public class C2_GameObject : ConditionCore
    {
        public enum GOCondition { ActiveInHierarchy, ActiveSelf, Null, Equal, isPrefab, isName, Layer, isStatic, isTag }

        [Hide(nameof(LocalTarget))]
        public GameObjectReference Target;
        public GOCondition Condition;
        [Hide(nameof(Condition), false, (int)GOCondition.Equal)]
        public GameObjectReference Value;
        [Hide(nameof(Condition), false, (int)GOCondition.isName, (int)GOCondition.isTag)]
        public StringReference m_Value;
        [Hide(nameof(Condition), false, (int)GOCondition.Layer)]
        public LayerReference Layer;

        protected override bool _Evaluate()
        {
            if (Condition == GOCondition.Null) return Target.Value == null;
            if (!Target.Value) return false;

            return Condition switch
            {
                GOCondition.isName => Target.Value.name.Contains(m_Value),
                GOCondition.isPrefab => Target.Value.IsPrefab(),
                GOCondition.ActiveInHierarchy => Target.Value.activeInHierarchy,
                GOCondition.ActiveSelf => Target.Value.activeSelf,
                GOCondition.Equal => Target.Value == Value.Value,
                GOCondition.Layer => MTools.Layer_in_LayerMask(Target.Value.layer, Layer.Value),
                GOCondition.isTag => Target.Value.CompareTag(m_Value),
                GOCondition.isStatic => Target.Value.isStatic,
                _ => false,
            };
        }

        protected override void _SetTarget(Object target) => Target.Value = MTools.VerifyComponent(target, Target.Value);
    }
}
