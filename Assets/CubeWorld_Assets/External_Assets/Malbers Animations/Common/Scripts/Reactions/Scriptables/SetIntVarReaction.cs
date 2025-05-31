using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable] //Needs to be Serializable!!!!
    [AddTypeMenu("Malbers/Scriptables/Set Int Var Listener")]
    public class SetIntVarReaction : Reaction
    {
        public override System.Type ReactionType => typeof(IntVarListener); //set the Type of component this Reaction Needs

        [Header("Set Int Var Listener")]
        [Tooltip("ID for the Var Listener. If is set to -1 it will get the first Bool Listener found")]
        public IntReference ID = new(-1);
        public IntReference newValue = new();


        protected override bool _TryReact(Component reactor)
        {
            var listeners = reactor.GetComponents<IntVarListener>().ToList();

            if (ID != -1)
            {
                listeners = listeners.FindAll(x => x.ID.Value == ID.Value);
            }

            if (listeners != null)
            {
                foreach (var item in listeners)
                {
                    item.SetValue(newValue.Value);
                }
                return true; //Reaction succesful!!
            }

            return false;
        }
    }
}