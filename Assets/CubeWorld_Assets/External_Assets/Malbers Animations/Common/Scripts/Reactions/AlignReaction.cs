using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]

    [AddTypeMenu("Tools/Align Look At")]
    public class AlignReaction : Reaction
    {
        public override System.Type ReactionType => typeof(Component);

        [Tooltip("The target to Look At Align")]
        public TransformReference Target;
        public float AlignTime = 0.15f;
        public AnimationCurve AlignCurve = new(MTools.DefaultCurve);
        public float AlignOffset = 0f;



        protected override bool _TryReact(Component component)
        {
            if (component.TryGetComponent<MonoBehaviour>(out var Mono))
            {
                Mono.StartCoroutine(MTools.AlignLookAtTransform(component.transform, Target, AlignTime, AlignOffset, AlignCurve));
                return true;
            }

            return false;
        }
    }
}
