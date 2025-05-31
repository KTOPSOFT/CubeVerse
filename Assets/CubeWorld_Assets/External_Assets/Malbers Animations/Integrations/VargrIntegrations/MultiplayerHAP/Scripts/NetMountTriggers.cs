using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.HAP;

namespace MalbersAnimations.VargrMultiplayer
{
    /// <summary>This Enable the mounting System</summary> 
    [AddComponentMenu("Malbers/Riding/Net Mount Trigger")]
    public class NetMountTriggers : MountTriggers
    {
        protected override void GetAnimal(Collider other)
        {
            if (!Montura)
            {
                Debug.LogError("No Mount Script Found... please add one");
                return;
            }

            if (!Montura.Mounted && Montura.CanBeMounted)          //If there's no other Rider on the Animal or the the Animal isn't death
            {
                var newRider = other.FindComponent<MRider>();

                if (newRider != null)
                {
                    if (newRider.IsMountingDismounting) return;     //Means the Rider is already mounting/Dismounting the animal so skip
                    if (newRider.MainCollider != other) return;     //Check if we are entering the Trigger with the Riders Main Collider Only (Not Body Parts)

                    if (GetNearbyRider(newRider) == null || GetNearbyRider(newRider).MountTrigger != this)  //If we are checking the same Rider or a new rider
                    {
                        newRider.MountTriggerEnter(Montura, this);   //Set Everything Requiered on the Rider in order to Mount
                    
                        if (AutoMount.Value && !WasAutomounted)
                        {
                            newRider.MountAnimal();
                        }
                    }
                }
            }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!gameObject.activeInHierarchy || other.isTrigger) return;       // Do not allow triggers

            var newRider = other.FindComponent<MRider>();

            if (newRider != null && CheckNearbyRider(newRider))
            {
                if (GetNearbyRider(newRider).IsMountingDismounting) return;             //You Cannot Mount if you are already mounted

                //When exiting if we are exiting From the same Mount Trigger means that there's no mountrigger Nearby
                if (GetNearbyRider(newRider).MountTrigger == this
                    && !Montura.Mounted &&
                    newRider.MainCollider == other)      //Check if we are exiting the Trigger with the Main Collider Only (Not Body Parts)
                {
                    GetNearbyRider(newRider).MountTriggerExit();
                }
            }
        }

        protected bool CheckNearbyRider(MRider mRider){
            if(Montura is NetMount netMount){
                return netMount.CheckNearbyRider(mRider);
            }else{
                return false;
            }
        }

        protected MRider GetNearbyRider(MRider mRider){
            if(Montura is NetMount netMount){
                return netMount.GetNearbyRider(mRider);
            }else{
                return null;
            }
        }
    }
}