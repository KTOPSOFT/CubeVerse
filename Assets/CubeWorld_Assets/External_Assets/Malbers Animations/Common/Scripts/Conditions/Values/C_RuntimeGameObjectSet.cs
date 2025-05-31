using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    [AddTypeMenu("Values/Runtime GameObject Set")]
    public class C_RuntimeGameObjectSet : MCondition
    {
        public enum RuntimeGameObjectCondition { Empty, Size, HasItem }

        // public override string DisplayName => "Values/Runtime GameObject Set";

        [RequiredField]
        public RuntimeGameObjects Target;
        public RuntimeGameObjectCondition Condition = RuntimeGameObjectCondition.Empty;
        [Hide("Condition", false, true, (int)RuntimeGameObjectCondition.Size)]
        public int Size;
        public GameObjectReference item;
        /// <summary>Set target on the Conditions</summary>
        protected override void _SetTarget(Object target)
        {
            if (target == null && target is RuntimeGameObjects)
                Target = target as RuntimeGameObjects;
        }

        public override bool _Evaluate()
        {
            switch (Condition)
            {
                case RuntimeGameObjectCondition.Empty:
                    return Target.IsEmpty;
                case RuntimeGameObjectCondition.Size:
                    return Target.Count == Size;
                case RuntimeGameObjectCondition.HasItem:
                    return Target.Has_Item(item.Value);
                default:
                    break;
            }
            return false;
        }
    }
}
