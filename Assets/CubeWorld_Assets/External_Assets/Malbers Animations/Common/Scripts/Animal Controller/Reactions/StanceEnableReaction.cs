using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Stance Enable-Disable")]
    public class StanceEnableReaction : MReaction
    { 
        public IDEnable<StanceID>[] stances;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            foreach (var id in stances)
            {
                var st = animal.Stance_Get(id.ID);
                if (st != null)
                {
                    st.Enable(id.enable); 
                }
            }
            return true;
        }
    }
}
