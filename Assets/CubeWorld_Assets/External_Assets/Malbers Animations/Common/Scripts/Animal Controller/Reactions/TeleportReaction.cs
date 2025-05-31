using MalbersAnimations.Scriptables;
using MalbersAnimations.Controller;
using UnityEngine;
 
namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Teleport")]
    public class TeleportReaction : MReaction
    {
        public TransformReference Destination;
        public BoolReference UseRotation;

        protected override bool _TryReact(Component component)
        {
            if (Destination.Value == null)
            {
                Debug.Log("Destination in Teleport Reaction is Null");
                return false;
            }
            var animal = component as MAnimal;

            if (UseRotation.Value)
                animal.TeleportRot(Destination.Value);
            else
                animal.Teleport(Destination.Value);
          
            return true;
        }
    }
}
