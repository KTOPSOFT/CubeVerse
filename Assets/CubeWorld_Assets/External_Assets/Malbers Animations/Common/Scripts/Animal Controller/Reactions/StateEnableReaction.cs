using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/State Enable-Disable")]
    public class StateEnableReaction : MReaction
    { 
        public IDEnable<StateID>[] states;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            foreach (var id in states)
            {
                var st = animal.State_Get(id.ID);
                if (st != null)
                {
                    st.Enable(id.enable);
                }
            }
            return true;
        }
    }
}
