using MalbersAnimations.Scriptables;
using MalbersAnimations.Weapons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Malbers Animations/ID/Holster", fileName = "New Holster ID", order = -1000)]
    public class HolsterID : IDs { }


    [System.Serializable]
    public class Holster
    {
        public HolsterID ID;
        public int Index;

        [Tooltip("Slots Transforms used to store weapons")]
        public List<Transform> Slots;

        /// <summary> Transform Reference for the holster</summary>
        [Tooltip("Weapon GameObject asociated to the Holster")]
        public MWeapon Weapon;

        [Tooltip("Input to Equip the weapon in the holster")]
        public StringReference Input = new();

        [Tooltip("If the weapon is added to the holster then it will be equipped on the Hand automatically")]
        public BoolReference AutoEquip = new();

        /// <summary> Used to connect the Inputs to the Abilities instead of the General Mode </summary>
        public UnityAction<bool> InputListener;

        public WeaponEvent OnWeaponInHolster = new();

        public int GetID => ID != null ? ID.ID : 0;

        public Transform GetSlot(int index) => Slots[index];

        /// <summary>  Prepare the weapons. Instantiate/Place them in the holster if they are linked in the Weapon Manager </summary>
        public bool PrepareWeapon()
        {
            if (Weapon)
            {
                var slot = Slots[Weapon.HolsterSlot]; //Get the correct slot

                //if it is a prefab then instantiate it!!
                if (Weapon.gameObject.IsPrefab())
                {
                    if (slot.childCount > 0) //Check 
                    {
                        Object.Destroy(slot.GetChild(0).gameObject);
                    }

                    Weapon = GameObject.Instantiate(Weapon);
                    Weapon.name = Weapon.name.Replace("(Clone)", "");

                    Weapon.Debugging("[Instantiated]", Weapon);
                }

                //MAKE SURE THE WEAPON HAS THE SAME ID OF THE WEAPON 
                Weapon.Holster = ID;



                //Re-Parent a frame after
                Weapon.Delay_Action(() =>
                {
                    if (!Weapon.IsEquiped)
                    {
                        Weapon.transform.SetParent(slot);
                        Weapon.transform.SetLocalTransform(Weapon.HolsterOffset);
                    }
                }
                );

                OnWeaponInHolster.Invoke(Weapon);
                //Weapon.InHolster = true;

                // Debug.Log("Weapon = " + Weapon);
                Weapon.IsCollectable?.Pick(); //Pick the weapon in case is a collectible

                return true;
            }
            return false;
        }

        public static implicit operator int(Holster reference) => reference.GetID;
    }
}
