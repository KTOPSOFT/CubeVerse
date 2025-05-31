using MalbersAnimations.Scriptables;
using UnityEngine;


namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Decision/Check Local Var", order = 201)]
    public class CheckLocalVarDecision : MAIDecision
    {
        public enum CheckLocalVarsOn { Self, Target, TransformHook, GameObjectVar }

        public override string DisplayName => "Variables/Check Local Variable";

        /// <summary>Range for Looking forward and Finding something</summary>
        [Space, Tooltip("Check the Decision on the Animal(Self) or the Target(Target), or on an object with a tag")]
        public CheckLocalVarsOn checkOn = CheckLocalVarsOn.Self;

        [Hide(nameof(checkOn), (int)CheckLocalVarsOn.TransformHook)]
        [RequiredField] public TransformVar TransformHook;
        [Hide(nameof(checkOn), (int)CheckLocalVarsOn.GameObjectVar)]
        [RequiredField] public GameObjectVar GameObjectVar;


        [Hide(nameof(isNumber))]
        public ComparerInt compare = ComparerInt.Equal;

        [Tooltip("Name and Type of variable to check on the gameObject")]
        public LocalVar value;

        public override void PrepareDecision(MAnimalBrain brain, int Index)
        {
            switch (checkOn)
            {
                case CheckLocalVarsOn.Self:
                    brain.ExtraLocalVars = brain.LocalVars;
                    break;
                case CheckLocalVarsOn.Target:
                    brain.ExtraLocalVars = brain.TargetVars;
                    break;
                case CheckLocalVarsOn.TransformHook:
                    if (TransformHook != null && TransformHook.Value != null)
                        brain.ExtraLocalVars = TransformHook.Value.FindComponent<MLocalVars>();
                    break;
                case CheckLocalVarsOn.GameObjectVar:
                    if (GameObjectVar != null && GameObjectVar.Value != null)
                        brain.ExtraLocalVars = GameObjectVar.Value.FindComponent<MLocalVars>();
                    break;
                default: break;
            }
        }


        public override bool Decide(MAnimalBrain brain, int Index)
        {
            if (brain.ExtraLocalVars != null)
            {
                return brain.ExtraLocalVars.Compare(value, compare);
            }
            return false;
        }


        [HideInInspector] public bool isNumber;
        private void OnValidate()
        {
            isNumber = value.type == LocalVar.VarType.Int || value.type == LocalVar.VarType.Float;
        }
    }
}
