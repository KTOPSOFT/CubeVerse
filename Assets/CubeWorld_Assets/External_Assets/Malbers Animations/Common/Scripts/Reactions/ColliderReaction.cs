using MalbersAnimations.Reactions;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable] //Needs to be Serializable!!!!
    [AddTypeMenu("Unity/Collider")]
    public class ColliderReaction : Reaction
    {

        public override System.Type ReactionType => typeof(Collider); //set the Type of component this Reaction Needs

        public enum ColliderOption { Enable = 1, IsTrigger = 2, Material = 4 }

        [Flag]
        public ColliderOption option = ColliderOption.Enable;

        [Hide("option", false, true, true, 1, 3, -1)]
        public bool enable;
        [Hide("option", false, true, true, 2, 6, -1)]
        public bool isTrigger;
        [Hide("option", false, true, true, 5, 6, -1)]
        public PhysicsMaterial material;

        protected override bool _TryReact(Component reactor)
        {
            Collider collider = reactor as Collider; //Cast the reactor as collider type.

            if (collider != null)
            {
                if ((option & ColliderOption.Enable) == ColliderOption.Enable) collider.enabled = enable;
                if ((option & ColliderOption.IsTrigger) == ColliderOption.IsTrigger) collider.isTrigger = isTrigger;
                if ((option & ColliderOption.Material) == ColliderOption.Material) collider.material = material;
                return true; //Reaction succesful!!
            }


            return false; //Reaction succesful!!
        }
    }
}