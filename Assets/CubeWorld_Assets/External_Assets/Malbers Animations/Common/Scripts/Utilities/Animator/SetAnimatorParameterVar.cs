using System.Collections.Generic;
using UnityEngine;
  

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Extras/Set Animator Parameter", fileName = "New Animator Parameter Set")]

    public class SetAnimatorParameterVar : ScriptableObject
    {
        public List<MAnimatorParameter> parameters = new();

        public void Set(Animator anim)
        {
            foreach (var param in parameters)
                param.Set(anim);
        }

        public void Set(Component comp) => Set(comp.FindComponent<Animator>());

        public void Set(GameObject comp) => Set(comp.FindComponent<Animator>());

        private void Reset()
        {
            parameters.Add(new MAnimatorParameter() { param = "param", type = AnimatorControllerParameterType.Int });
        }

    }
}