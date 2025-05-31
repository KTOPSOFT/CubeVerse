using MalbersAnimations.Weapons;
using UnityEngine;
using MalbersAnimations.Events;

namespace MalbersAnimations.VargrMultiplayer
{
    public class MeleeUtility : WeaponUtility
    {    
        [SerializeField] protected BoolEvent m_PlayerOnAttackTrigger;

        protected override void Awake()
        {
            base.Awake();

            if(Weapon is MMelee){
                (Weapon as MMelee).OnCauseDamage.AddListener((data) => { if(isPlayerOwned) m_PlayerOnAttackTrigger.Invoke(data); } );
            }
        }
        
        public override void RecivedEventSync(object data, int index)
        {
            if(isController) return;

            if(index == 6 && Weapon is MMelee MeleeWeapon){
                MeleeWeapon.OnCauseDamage.Invoke((bool)data);
            }else{
                base.RecivedEventSync(data, index);
            }
        }
    }
}