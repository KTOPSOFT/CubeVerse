using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations
{
    public interface IZone
    {
        bool ActivateZone(MAnimal animal);

        /// <summary>  The Zone is a Mode Zone </summary>
        bool IsMode { get; }
        /// <summary>  The Zone is a State Zone </summary>
        bool IsState { get; }

        /// <summary>  The Zone is a Stance Zone </summary>
        bool IsStance { get; }

        void RemoveAnimal(MAnimal animal);

        Collider ZCollider { get; }

        /// <summary> ID Value of the zone</summary>
        int ZoneID { get; }

        Transform transform { get; }
    }
}