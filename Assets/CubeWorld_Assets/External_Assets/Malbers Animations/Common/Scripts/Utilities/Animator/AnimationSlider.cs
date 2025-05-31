using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Transform/Animation Slider Preview")]
    public class AnimationSlider : MonoBehaviour
    {
        public Animator animator;
        public AnimationClip clip;
        public GameObject target;

        [Range(0f, 1f)]
        public float time;

        [MButton("RebindAnimator", false)]
        public bool rebind;
          
          
        private void Reset()
        {
            animator = GetComponent<Animator>();
            target = gameObject;
        }

        [ContextMenu("Rebind Animator")]
        public void RebindAnimator()
        {
            if (TryGetComponent<Animator>( out var anim))
            {
                anim.Rebind();
                MTools.SetDirty(this);
            }
        }

        void OnValidate()
        {
            if (target && clip)
            {
                var pos = target.transform.position;
                var rot = target.transform.rotation;

                var Cliptime = Mathf.Lerp(0, clip.length, this.time);

                clip.SampleAnimation(target, Cliptime); //Needs an Avatar
                target.transform.SetPositionAndRotation(pos, rot);
            }
        }
    }
}



