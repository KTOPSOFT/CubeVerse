using UnityEngine;

namespace MalbersAnimations.Controller
{
    [CreateAssetMenu(menuName = "Malbers Animations/Modifier/Mode/Mode Index by State")]
    public class ModifierIndexByState : ModeModifier
    {
        [System.Serializable]
        public struct IndexByState
        {
            public StateID state;
            //  public StanceID stance;
            public int Index;
        }

        [Tooltip("Changes the Active State of the Mode by the current playing State ")]
        public IndexByState[] stateMultipliers;

        public override void OnModeEnter(Mode mode)
        {
            var currentState = mode.Animal.ActiveStateID;

            foreach (var stateM in stateMultipliers)
            {
                if (stateM.state == currentState)
                {
                    mode.AbilityIndex = stateM.Index;
                    break;
                }
            }
        }
    }
}