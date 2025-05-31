using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/In Zone")]
    public class InZoneTask : MTask
    {
        public override string DisplayName => "Animal/In Zone";

        [Space, Tooltip("Apply the Task to the Animal(Self) or the Target(Target)")]
        public Affected affect = Affected.Self;

        public override void StartTask(MAnimalBrain brain, int index)
        {
            DoTask(brain, index);
        }

        public override void UpdateTask(MAnimalBrain brain, int index)
        {
            DoTask(brain, index);
        }

        private void DoTask(MAnimalBrain brain, int index)
        {
            bool Done = false;

            switch (affect)
            {
                case Affected.Self: Done = InZone(brain.Animal); break;
                case Affected.Target: Done = InZone(brain.TargetAnimal); break;
            }
            if (Done) brain.TaskDone(index); //Set Done to this task
        }

        public bool InZone(MAnimal animal)
        {
            if (animal && animal.InZone)
            {
                animal.Zone.ActivateZone(animal);
                return true;
            }
            return false;
        }


        void Reset() => Description = "Activate the Zone the animal is inside";
    }
}