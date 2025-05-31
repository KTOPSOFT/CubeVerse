using UnityEngine;
using MalbersAnimations.Scriptables;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Effect By Material")]

    public class EffectByMaterial : MonoBehaviour
    {
        public SurfaceID surface;
        public AudioClipReference sound;
        public GameObjectReference effect;

        //private GameObject runtimeEffect;

        //public void ApplyEffect()
        //{
        //    if (effect != null && effect.Value.IsPrefab())
        //    {
        //        runtimeEffect = Instantiate(effect.Value);
        //    }
        //}
    }
}