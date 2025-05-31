using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Controller
{
    /// <summary> This is used for all the components that use OnAnimator Move... it breaks the Timeline edition  </summary>
    [AddComponentMenu("Malbers/Timeline/Animator Move Timeline Fixer")]

    [ExecuteInEditMode]
    public class AnimatorMoveTimelineFixer : MonoBehaviour
    {
        public Animator anim;
        void Start()
        {
            if (Application.isPlaying)   
                Destroy(this);
              
            anim = GetComponent<Animator>();
        }

#if UNITY_EDITOR
        private void OnAnimatorMove() 
        {
            if (anim != null) 
                anim.ApplyBuiltinRootMotion(); 
        }

        private void Reset()   { anim = GetComponent<Animator>(); }
#endif
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AnimatorMoveTimelineFixer))]
    public class AnimatorMoveTimelineFixerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MalbersEditor.DrawDescription("This script fixes a bug with the Timeline when is playing in the Editor. " +
                "It happen with scripts that use OnAnimatorMove(), like AC ");
        }
    }
#endif
}
 