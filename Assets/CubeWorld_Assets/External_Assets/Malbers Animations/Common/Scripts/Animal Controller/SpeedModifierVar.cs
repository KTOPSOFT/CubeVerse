using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Speeds/New Speed Modifier")]
    public class SpeedModifierVar : ScriptableObject
    {
        public string SpeedSet;
        public MSpeed NewSpeed;

        public virtual void ModifySpeed(MAnimal animal)
        {
            foreach (var states in animal.states)
            {
                var GetSpeedSet = states.SpeedSets.Find(x => x.name == SpeedSet);

                GetSpeedSet?.SwapSpeed(NewSpeed);
            }
        }

        public virtual void ModifySpeed(GameObject go)
        {
            var animal = go.FindComponent<MAnimal>();
            if (animal) ModifySpeed(animal);
        }

        public virtual void ModifySpeed(Component go)
        {
            var animal = go.FindComponent<MAnimal>();
            if (animal) ModifySpeed(animal);
        }


        public virtual void AddSpeed(MAnimal animal)
        {
            foreach (var states in animal.states_C)
            {
                var GetSpeedSet = states.state.SpeedSets.Find(x => x.name == SpeedSet);

                GetSpeedSet?.AddSpeed(NewSpeed);
            }
        }


        public virtual void AddSpeed(GameObject go)
        {
            var animal = go.FindComponent<MAnimal>();

            if (animal) AddSpeed(animal);
        }

        public virtual void AddSpeed(Component go)
        {
            var animal = go.FindComponent<MAnimal>();

            if (animal) AddSpeed(animal);
        }
    }
}
