using MalbersAnimations.Weapons;
using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine.Events;

namespace MalbersAnimations.VargrMultiplayer
{

    public class RangedUtility : WeaponUtility
    {    
        public IDs WeaponAmmoType;
        public int AmmoMultiplier = 1;
        public IntVar InChamberUI;
        public bool DumpChamber = false;
        [SerializeField] protected GameObjectEvent m_PlayerOnLoadProjectile;
        [SerializeField] protected GameObjectEvent m_PlayerOnFireProjectile;
        [SerializeField] protected UnityEvent m_PlayerOnReloadStart;
        [SerializeField] protected UnityEvent m_PlayerOnReloaded;
        private GameObject m_StoredProjectile = null;

        protected override void Awake()
        {
            base.Awake();

            if(Weapon is MShootable shootable){
                m_StoredProjectile = shootable.Projectile;
                shootable.OnFireProjectile.AddListener(SyncProjectile);
                shootable.OnReload.AddListener(() => SyncChamberUI(true));

                shootable.OnLoadProjectile.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnLoadProjectile.Invoke(data); } );
                shootable.OnFireProjectile.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnFireProjectile.Invoke(data); } );
                shootable.OnReloadStart.AddListener(() => { if(isController && isPlayerOwned) m_PlayerOnReloadStart.Invoke(); } );
                shootable.OnReload.AddListener(() => { if(isController && isPlayerOwned) m_PlayerOnReloaded.Invoke(); } );
            }
        }

        public override void RecivedEventSync(object data, int index)
        {
            if(isController) return;
            if(Weapon is MShootable shootable){
                switch(index)
                {
                    case 6: shootable.OnLoadProjectile.Invoke((GameObject)data); break;
                    case 7: shootable.OnFireProjectile.Invoke((GameObject)data); break;
                    case 8: shootable.OnReloadStart.Invoke(); break;
                    case 9: shootable.OnReload.Invoke(); break;
                    default: base.RecivedEventSync(data, index); break;
                }
            }else{
                base.RecivedEventSync(data, index);
            }
        }

        #region Invokes
        protected override void OnEquip(Transform data)
        {
            if(Weapon.Owner == null) return;

            base.OnEquip(data);

            if(m_ActiveOwner == null) return;

            if(Weapon is MShootable){
                m_ActiveOwner.OnAmmoUpdate += AmmoUpdate;
                m_ActiveOwner.ShooterReload(WeaponAmmoType, 0);
            }
        }
        protected override void OnUnequip(Transform data)
        {
            if(m_ActiveOwner == null) return;

            if(Weapon is MShootable shootable){
                m_ActiveOwner.OnAmmoUpdate -= AmmoUpdate;
                shootable.TotalAmmo = 0;
                shootable.Projectile = m_StoredProjectile;
            }

            base.OnUnequip(data);
        }

        #endregion

        public void Reload()
        {
            if(!(Weapon is MShootable) || WeaponAmmoType == null) return;
            
            m_ActiveOwner.ShooterReload(WeaponAmmoType, DumpChamber ? 
                (Weapon as MShootable).ChamberSize * (WeaponAmmoType is StatID ? AmmoMultiplier : 1)
            :
                ((Weapon as MShootable).ChamberSize - (Weapon as MShootable).AmmoInChamber) * (WeaponAmmoType is StatID ? AmmoMultiplier : 1)
            );
        }

        protected void SyncChamberUI(bool reload = true)
        {
            if(!isController || !isPlayerOwned) return;

            if(InChamberUI == null) return;

            InChamberUI.Value = (Weapon as MShootable).AmmoInChamber -(reload ? 0 : 1);
        }

        protected void AmmoUpdate(IDs ammoType, int count = -1)
        {
            if(!isController) count = -1;
            if(!(Weapon is MShootable)) return;
            MShootable shooter = Weapon as MShootable;
            if(ammoType is Ammo ammo){
                if(WeaponAmmoType != ammo.ammoType) return;
                if(ammo.prefab != null && shooter.Projectile != ammo.prefab) shooter.Projectile = ammo.prefab;
            }else{
                if(WeaponAmmoType != ammoType) return;
            }

            if(shooter.TotalAmmo != count) shooter.TotalAmmo = count;
        }
        
        public void SyncProjectile(GameObject projectile)
        {
            if(m_ActiveOwner == null) return;
            if(!m_ActiveOwner.isController) return;
            if(!(Weapon is MShootable)) return;

            SyncedProjectile syncedPro = projectile.GetComponent<SyncedProjectile>();

            m_ActiveOwner.SyncProjectile(projectile.GetComponent<IProjectile>().Velocity, projectile.transform.position, projectile.transform.forward, Weapon.ChargedNormalized, (syncedPro != null ? syncedPro.Seed : 0));
            SyncChamberUI(false);
        }

        public void FireProjectile (Vector3 velocity, Vector3 position, Vector3 forward, float charge, int sm = 0, int seed = 0)
        {
            if(!(Weapon is MShootable)) return;

            MShootable weapon = (MShootable) Weapon;
			GameObject ProjectileInstance = Instantiate(weapon.Projectile, position, Quaternion.LookRotation(forward));
			ProjectileInstance.transform.position = position;
			ProjectileInstance.transform.forward = forward;
			IProjectile I_Projectile = ProjectileInstance.GetComponent<IProjectile>();
			I_Projectile.Prepare(weapon.Owner, weapon.Gravity, velocity, weapon.Layer, weapon.TriggerInteraction, weapon.ProjectilePool);
			if (weapon.HitEffect != null) I_Projectile.HitEffect = weapon.HitEffect;
			var newDamage = new StatModifier(weapon.statModifier)
                { Value = Mathf.Lerp(weapon.MinDamage, weapon.MaxDamage, charge) };
            I_Projectile.PrepareDamage(newDamage, weapon.CriticalChance, weapon.CriticalMultiplier, weapon.element);
            if(I_Projectile is SyncedProjectile syncedProjectile) syncedProjectile.PrepareSync(sm, seed);
			I_Projectile.Fire();
            
            weapon.OnFireProjectile.Invoke(ProjectileInstance);
        }
    }
}