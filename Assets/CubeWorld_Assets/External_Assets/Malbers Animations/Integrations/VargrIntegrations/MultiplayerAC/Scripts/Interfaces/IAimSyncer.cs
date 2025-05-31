using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    interface IAimSyncer
    {
        bool isController { get; } 
        Vector3 AimDirection {get; set;}
    }
}