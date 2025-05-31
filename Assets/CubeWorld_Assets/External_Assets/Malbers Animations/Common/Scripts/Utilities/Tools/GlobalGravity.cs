using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>
    /// Stores the Value of a Gameobjcet down direction to a Scriptable Vector3 variable
    /// </summary>
    [AddComponentMenu("Malbers/Utilities/Tools/Global Gravity")]
    public class GlobalGravity : MonoBehaviour
    {
        [RequiredField] public Vector3Var Gravity;
        [Tooltip("Instead of using the Gravity Value, Use the ")]
        public bool UseUpVector = true;

        void Update()
        {
            if (Gravity != null) Gravity.Value = -transform.up;
        }
    }
}
