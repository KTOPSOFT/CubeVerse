using MalbersAnimations.Weapons;
using UnityEngine;
using MalbersAnimations.Events;
using UnityEngine.Events;

namespace MalbersAnimations.VargrMultiplayer
{
    public abstract class WeaponUtility : MonoBehaviour
    {    
        public MWeapon Weapon { get; protected set; }
        protected NetMWeaponManager m_ActiveOwner = null;
        protected bool isController => m_ActiveOwner != null ? m_ActiveOwner.isController : (Weapon.Owner != null && Weapon.Owner.GetComponent<NetMWeaponManager>().isController);
        protected bool isPlayerOwned => m_ActiveOwner != null ? m_ActiveOwner.isPlayerOwned : false;

        [SerializeField] protected TransformEvent m_PlayerOnHit;
        [SerializeField] protected Vector3Event m_PlayerOnHitPosition;
        [SerializeField] protected IntEvent m_PlayerOnHitInteractable;
        [SerializeField] protected IntEvent m_PlayerOnProfileChange;
        [SerializeField] protected TransformEvent m_PlayerOnEquip;
        [SerializeField] protected TransformEvent m_PlayerOnUnequip;
        [SerializeField] protected FloatEvent m_PlayerOnCharged;
        [SerializeField] protected UnityEvent m_PlayerOnChargedMax;
        [SerializeField] protected BoolEvent m_PlayerOnAim;
        
        protected virtual void Awake()
        {
            if(Weapon == null) 
                Weapon = GetComponent<MWeapon>();

            Weapon.OnHit.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnHit.Invoke(data); } );
            Weapon.OnHitPosition.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnHitPosition.Invoke(data); } );
            Weapon.OnHitInteractable.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnHitInteractable.Invoke(data); } );
            Weapon.OnEquiped.AddListener(OnEquip);
            Weapon.OnUnequiped.AddListener(OnUnequip);
            Weapon.OnProfileChanged.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnProfileChange.Invoke(data); } );
            Weapon.OnCharged.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnCharged.Invoke(data); } );
            Weapon.OnMaxCharged.AddListener(() => { if(isController && isPlayerOwned) m_PlayerOnChargedMax.Invoke(); } );
            Weapon.OnAiming.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnAim.Invoke(data); } );
        }

        #region Invokes
        protected virtual void OnEquip(Transform data)
        {
            if(Weapon.Owner == null) return;

            m_ActiveOwner = Weapon.Owner.GetComponent<NetMWeaponManager>();

            if(m_ActiveOwner == null) return;

            // Add listener for Charge Time
            if(!isController) m_ActiveOwner.WeaponChargeSync += (value) => Weapon.CurrentCharge = value;

            if(isController && isPlayerOwned) m_PlayerOnEquip.Invoke(data);
        }
        protected virtual void OnUnequip(Transform data)
        {
            if(m_ActiveOwner == null) return;

            // Remove listener for Charge Time
            if(!isController) m_ActiveOwner.WeaponChargeSync -= (value) => Weapon.CurrentCharge = value;

            if(isController && isPlayerOwned) m_PlayerOnUnequip.Invoke(data);

            m_ActiveOwner = null;
        }

        #endregion

        public virtual void RecivedEventSync(object data, int index)
        {
            if(isController) return;

            switch(index)
            {
                case 0: Weapon.OnHit.Invoke((Transform)data); break;
                case 1: Weapon.OnHitPosition.Invoke((Vector3)data); break;
                case 2: Weapon.OnHitInteractable.Invoke((int)data); break;
                case 3: Weapon.OnProfileChanged.Invoke((int)data); break;
                case 4: Weapon.OnMaxCharged.Invoke(); break;
                case 5: Weapon.OnAiming.Invoke((bool)data); break;
            }
        }
    }
}