using MalbersAnimations.Reactions;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Reaction Task")]
    public class ReactionTask : MTask
    {
        public override string DisplayName => "General/Reaction";

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        [Tooltip("Use the Interval value to repeat the reaction")]
        public bool repeat = false;

        [SerializeReference, SubclassSelector]
        [Tooltip("Reaction when the AI Task begin")]
        public Reaction reaction;

        [SerializeReference, SubclassSelector]
        [Tooltip("Reaction when the AI State ends")]
        public Reaction reactionOnExit;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            brain.TasksVars[index].Components = new Component[1];

            //Store the Component for the reaction here so its easy to find later
            brain.TasksVars[index].Components[0] = reaction.VerifyComponent((affect == Affected.Self ? brain.Animal : brain.Target));

            React(brain, index, reaction);

            if (!repeat)
                brain.TaskDone(index);
        }

        private void React(MAnimalBrain brain, int index, Reaction reaction)
        {
            reaction?.React(brain.TasksVars[index].Components[0]); //Get the component again from the vars
        }


        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            //repeat the reaction using the Update Interaval (If Repeat is false) this will be skipped
            React(brain, index, reaction);
        }

        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            React(brain, index, reactionOnExit);
        }

        private void Reset()
        => Description = "Add a Reaction to the Target or the Animal";

    }
}
