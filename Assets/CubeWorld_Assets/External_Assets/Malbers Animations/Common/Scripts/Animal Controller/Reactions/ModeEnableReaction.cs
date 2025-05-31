using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Mode Enable-Disable")]
    public class ModeEnableReaction : MReaction
    {
        [Tooltip("Enable or Disable the Mode temporally, this does not deactivate completely the Mode. Disables modes will remain disabled. and it wont be affected by this")]
        public bool TemporalEnable = false;
        public IDEnable<ModeID>[] modes;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            foreach (var id in modes)
            {
                var mode = animal.Mode_Get(id.ID);

                if (mode != null)
                {
                    if (TemporalEnable)
                        mode.Enable_Temporal(id.enable);
                    else
                        mode.Active = id.enable;

                }
            }
            return true;
        }
    }
}
