using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Set Local Var")]
    public class SetLocalVarTask : MTask
    {
        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target), or on an object with a tag")]
        public Affected checkOn = Affected.Self;

        public List<LocalVar> variables;

        public override string DisplayName => "Variables/Set Local Var";

        public override void StartTask(MAnimalBrain brain, int index)
        {
            if (checkOn == Affected.Self && brain.LocalVars != null)
            {
                foreach (var item in variables)
                {
                    brain.LocalVars.SetVar(item);
                }

            }
            else if (checkOn == Affected.Target && brain.TargetVars != null)
            {
                foreach (var item in variables)
                {
                    brain.TargetVars.SetVar(item);
                }
            }
            brain.TaskDone(index);
        }
    }
}
