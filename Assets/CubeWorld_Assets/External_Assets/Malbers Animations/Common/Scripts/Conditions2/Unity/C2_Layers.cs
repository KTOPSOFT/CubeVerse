using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("Unity/Layers")]
    public class C2_Layers : ConditionCore
    {
        [Tooltip("Target to check for the condition ")]
        [RequiredField, Hide(nameof(LocalTarget))] public Object Target;
        public LayerReference Layer;

        protected override bool _Evaluate()
        {
            if (Target != null)
            {
                if (Target is GameObject go)
                {
                    return MTools.Layer_in_LayerMask(go.layer, Layer.Value);
                }
                else if (Target is Component comp)
                {
                    return MTools.Layer_in_LayerMask(comp.gameObject.layer, Layer.Value);
                }
            }
            return false;
        }
        protected override void _SetTarget(Object target) => Target = MTools.VerifyComponent(target, Target);
    }
}
