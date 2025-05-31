using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable]
    public abstract class MWeaponConditions : MCondition
    {
        [RequiredField] public MWeapon Target;
        public virtual void SetTarget(MWeapon n) => Target = n;
        protected override void _SetTarget(Object target) => VerifyTarget(target, ref Target);
    }
}