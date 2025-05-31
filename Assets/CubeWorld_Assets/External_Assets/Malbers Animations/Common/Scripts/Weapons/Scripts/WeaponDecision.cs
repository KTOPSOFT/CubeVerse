using MalbersAnimations.Weapons;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Arrived to Target", order = -100)]
    public class WeaponDecision : MAIDecision
    {
        public override string DisplayName => "Weapon/Check Weapon";

        public enum WeaponDecisionOptions { WeaponEquipped, WeaponIs, IsReloading, IsAiming, IsAttacking, AmmoInChamber, TotalAmmo, ChamberSize }

        public Affected CheckOn = Affected.Self;
        public WeaponDecisionOptions weapon = WeaponDecisionOptions.WeaponIs;

        [Hide(nameof(weapon), (int)WeaponDecisionOptions.WeaponIs)]
        public WeaponID weaponType;

        [Hide(nameof(weapon), (int)WeaponDecisionOptions.AmmoInChamber, (int)WeaponDecisionOptions.TotalAmmo, (int)WeaponDecisionOptions.ChamberSize)]
        public ComparerInt comparer = ComparerInt.Equal;
        [Hide(nameof(weapon), (int)WeaponDecisionOptions.AmmoInChamber, (int)WeaponDecisionOptions.TotalAmmo, (int)WeaponDecisionOptions.ChamberSize)]
        public int value;

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            switch (CheckOn)
            {
                case Affected.Self:
                    brain.DecisionsVars[Index].mono = brain.Animal.FindComponent<MWeaponManager>(); //Cache the Weapon Manager in the Animal
                    break;
                case Affected.Target:
                    brain.DecisionsVars[Index].mono = brain.Target.FindComponent<MWeaponManager>(); //Cache the Weapon Manager in the Target
                    break;
                default:
                    break;
            }
        }


        public override bool Decide(MAnimalBrain brain, int index)
        {
            var WM = brain.DecisionsVars[index].mono as MWeaponManager;

            if (WM == null) return false;

            switch (weapon)
            {
                case WeaponDecisionOptions.WeaponEquipped:
                    return WM.Weapon != null;

                case WeaponDecisionOptions.WeaponIs:
                    if (WM.Weapon == null) return false;

                    return WM.Weapon.WeaponID == weaponType;
                case WeaponDecisionOptions.IsReloading:
                    return WM.IsReloading;

                case WeaponDecisionOptions.AmmoInChamber:
                    if (WM.Weapon != null && WM.Weapon is MShootable s1)
                        return s1.AmmoInChamber.CompareInt(value, comparer);
                    return false;

                case WeaponDecisionOptions.TotalAmmo:
                    if (WM.Weapon != null && WM.Weapon is MShootable s2)
                        return s2.TotalAmmo.CompareInt(value, comparer);
                    return false;

                case WeaponDecisionOptions.ChamberSize:
                    if (WM.Weapon != null && WM.Weapon is MShootable s3)
                        return s3.ChamberSize.CompareInt(value, comparer);
                    return false;
                case WeaponDecisionOptions.IsAiming:
                    return WM.Aim;
                case WeaponDecisionOptions.IsAttacking:
                    return WM.IsAttacking;
                default:
                    break;
            }
            return false;
        }
    }
}