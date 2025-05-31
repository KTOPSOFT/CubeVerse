using MalbersAnimations.Controller;
using UnityEngine;


namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    public abstract class MAnimalCondition : MCondition
    {
        [RequiredField]

        //  [Hide("LocalTarget")]
        public MAnimal Target;
        public virtual void SetTarget(MAnimal n) => Target = n;
        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target);
    }
}
