using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]

    [AddTypeMenu("Unity/Particle System")]

    public class ParticleReaction : Reaction
    {
        public override System.Type ReactionType => typeof(ParticleSystem);

        public Color color = Color.white;

        protected override bool _TryReact(Component component)
        {
            var p = component as ParticleSystem;

            var particle = p.main;
            particle.startColor = new ParticleSystem.MinMaxGradient(color);

            return true;
        }
    }
}
