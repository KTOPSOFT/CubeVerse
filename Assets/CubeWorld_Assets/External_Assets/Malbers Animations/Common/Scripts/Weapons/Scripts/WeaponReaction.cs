using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Weapon Manager/Equip")]
    public class WeaponReaction : MReaction
    {
        public enum WeaponActions
        {
            Equip,
            Unequip,
            EquipFast,
            UnequipFast,
            HolsterClear,
            HolsterClearAll,
            NextHolster,
            PreviousHolster,
            ResetCombat,
            StoreWeapon,
            DrawWeapon
        }
        public WeaponActions Actions = WeaponActions.Equip;

        [Hide("Actions", 0, 2)]
        public GameObject Weapon;
        [Hide("Actions", 4)]
        public HolsterID Holster;

        protected override bool _TryReact(Component component)
        {
            var target = component as MWeaponManager;

            switch (Actions)
            {
                case WeaponActions.Equip:
                    if (target.UseHolsters)
                        target.Holster_SetWeapon(Weapon);
                    else
                        target.Equip_External(Weapon);
                    break;
                case WeaponActions.Unequip:         target.UnEquip(); break;
                case WeaponActions.EquipFast:       target.Equip_Fast(Weapon); break;
                case WeaponActions.UnequipFast:     target.UnEquip_Fast(); break;
                case WeaponActions.HolsterClear:    target.Holster_Clear(Holster); break;
                case WeaponActions.HolsterClearAll: target.HolsterClearAll(); break;
                case WeaponActions.NextHolster:     target.Holster_Next(); break;
                case WeaponActions.PreviousHolster: target.Holster_Previus(); break;
                case WeaponActions.ResetCombat:     target.ResetCombat(); break;
                case WeaponActions.StoreWeapon:     target.Store_Weapon(); break;
                case WeaponActions.DrawWeapon:     target.Draw_Weapon(); break;
                default: break;
            }
            return true;
        }
    }
}
