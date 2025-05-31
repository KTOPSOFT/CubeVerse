using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using System.Linq;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable] //Needs to be Serializable!!!!
    [AddTypeMenu("Malbers/Scriptables/Set Bool Var Listener")]
    public class SetBoolVarReaction : Reaction
    {
        public override System.Type ReactionType => typeof(BoolVarListener); //set the Type of component this Reaction Needs

        [Header("Set Bool Var Listener")]
        [Tooltip("ID for the Var Listener. If is set to -1 it will get the first Bool Listener found")]
        public IntReference ID = new(-1);
        public BoolReference newValue;
       

        protected override bool _TryReact(Component reactor)
        {
            var listeners = reactor.GetComponents<BoolVarListener>().ToList();

            if (ID != -1)
                listeners = listeners.FindAll(x => x.ID.Value == ID.Value);

            if (listeners != null)
            {
                foreach (var item in listeners)
                {
                    item.Value = (newValue.Value);
                }
                return true; //Reaction succesful!!
            }

            return false;
        }
    }
}