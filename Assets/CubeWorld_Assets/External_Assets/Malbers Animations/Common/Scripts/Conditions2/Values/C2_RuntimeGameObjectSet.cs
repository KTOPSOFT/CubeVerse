using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("Values/Runtime GameObject Set")]
    public class C2_RuntimeGameObjectSet : ConditionCore
    {
        public enum RuntimeGameObjectCondition { Empty, Size, HasItem }

        [RequiredField]
        public RuntimeGameObjects Target;
        public RuntimeGameObjectCondition Condition = RuntimeGameObjectCondition.Empty;
        [Hide(nameof(Condition), false, true, (int)RuntimeGameObjectCondition.Size)]
        public int Size;
        [Hide(nameof(Condition), false, true, (int)RuntimeGameObjectCondition.HasItem)]
        public GameObjectReference item;
        /// <summary>Set target on the Conditions</summary>
        protected override void _SetTarget(Object target)
        {
            if (target == null && target is RuntimeGameObjects)
                Target = target as RuntimeGameObjects;
        }
        protected override bool _Evaluate()
        {
            return Condition switch
            {
                RuntimeGameObjectCondition.Empty => Target.IsEmpty,
                RuntimeGameObjectCondition.Size => Target.Count == Size,
                RuntimeGameObjectCondition.HasItem => Target.Has_Item(item.Value),
                _ => false,
            };
        }
    }
}
