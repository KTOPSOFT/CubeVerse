using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Change Speed")]
    public class ChangeSpeedTask : MTask
    {
        public override string DisplayName => "Animal/Set Speed";

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        [Tooltip("Name of the Speed Set you want to change. Leave it empty if you just want to change the Speed index of the current speed set ")]
        public string SpeedSet = "Ground";
        public IntReference SpeedIndex = new(3);
        public bool Sprint = false;
        public bool ResetSprintOnExit = false;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            switch (affect)
            {
                case Affected.Self: ChangeSpeed(brain.Animal); break;
                case Affected.Target: ChangeSpeed(brain.TargetAnimal); break;
            }
            brain.TaskDone(index); //Set Done to this task
        }


        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            if (ResetSprintOnExit)
            {
                if (affect == Affected.Self)
                    brain.Animal.SetSprint(false);
                else
                    brain.TargetAnimal?.SetSprint(false);
            }
        }


        public void ChangeSpeed(MAnimal animal)
        {
            if (animal)
            {
                animal.SpeedSet_Set_Active(SpeedSet, SpeedIndex);
                animal.SetSprint(Sprint);
            }
        }

        void Reset() => Description = "Change the Speed on the Animal";
    }
}
