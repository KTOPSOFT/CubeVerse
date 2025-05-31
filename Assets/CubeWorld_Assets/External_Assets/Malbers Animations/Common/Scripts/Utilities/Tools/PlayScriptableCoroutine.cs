using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    public class PlayScriptableCoroutine : MonoBehaviour
    {
        public FloatReference time = new(0.5f);
        public AnimationCurve curve = new(new Keyframe[] { new(0, 0), new(1, 1) });


        public List<PresetItem> presets;

        private void Start()
        {
            PlayAll();
        }

        public virtual void PlayAll()
        {
            foreach (var p in presets)
            {
                p.Preset.Evaluate(this, p.Target, time, curve);
            }
        }
    }

    [System.Serializable]
    public struct PresetItem
    {
        public ScriptableCoroutine Preset;
        public Transform Target;
    }
}
