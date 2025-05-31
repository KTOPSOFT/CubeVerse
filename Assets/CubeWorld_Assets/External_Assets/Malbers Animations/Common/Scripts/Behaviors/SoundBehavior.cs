using UnityEngine;

namespace MalbersAnimations
{
    public class SoundBehavior : StateMachineBehaviour
    {
        [Tooltip("Game Object to Store the Audio Source Component. This allows Animation States to share the same AudioSource")]
        public string m_source = "Animator Sounds";

        public AudioClip[] sounds;

        [Tooltip("Play the sound when the Animation Starts")]
        public bool playOnEnter = true;

        [Hide(nameof(playOnEnter))]
        [Tooltip("PlayOnEnter After the transition is over")]
        public bool SkipTransition = false;
        [Tooltip("Loop forever the sound")]
        public bool Loop = false;
        [Tooltip("Stop playing if the Animation exits")]
        public bool stopOnExit;

        [Hide("playOnEnter", true)]
        [Range(0, 1)]
        public float PlayOnTime = 0.5f;
        [Space]
        [MinMaxRange(-3, 3)]
        public RangedFloat pitch = new(1, 1);
        [MinMaxRange(0, 1)]
        public RangedFloat volume = new(1, 1);

        private AudioSource _audio;
        private Transform audioTransform;

        public float MaxDistance = 10f;

        private bool played;

        private void CheckAudioSource(Animator animator)
        {
            if (audioTransform == null)
            {
                var goName = m_source;

                if (string.IsNullOrEmpty(goName)) goName = "Animator Sounds";

                audioTransform = animator.transform.FindGrandChild(goName);

                if (!audioTransform)
                {
                    var go = new GameObject() { name = goName };
                    audioTransform = go.transform;
                    audioTransform.parent = animator.transform;
                    audioTransform.ResetLocal();
                }

                _audio = audioTransform.GetComponent<AudioSource>();

                if (!_audio)
                {
                    _audio = audioTransform.gameObject.AddComponent<AudioSource>();
                    _audio.spatialBlend = 1; //Make it 3D
                    _audio.maxDistance = MaxDistance;
                }
                _audio.playOnAwake = false;
            }
        }



        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            CheckAudioSource(animator);
            played = false;

            if (playOnEnter && !SkipTransition)
                PlaySound();
        }



        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (played || animator.IsInTransition(layerIndex)) return; //Do not play while in transition

            if (playOnEnter && SkipTransition)
            {
                PlaySound();
            }
            else
            if (stateInfo.normalizedTime > PlayOnTime)
            {
                PlaySound();
            } 
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //dont stop the current animation if is this same animation
            if (stopOnExit && _audio && animator.GetCurrentAnimatorStateInfo(layerIndex).fullPathHash != stateInfo.fullPathHash)
            {
                //Debug.Log("STOP EXIT");
                _audio?.Stop();
                _audio.clip = null;
            }
        }

        public virtual void PlaySound()
        {
            if (_audio && _audio.enabled && sounds.Length > 0)
            {
                AudioClip clip = sounds[Random.Range(0, sounds.Length)];

                if (_audio.loop && clip == _audio.clip)
                {
                  // Debug.Log("LOOP");
                    played = true;
                    return; //meaning is looping 
                }

                if (_audio.isPlaying)
                { 
                    _audio.Stop();
                  //  Debug.Log($"STOP: Next Clip {(clip != null ? clip.name : "null")}");
                }

                _audio.clip = clip;

                if (clip != null)
                {
                    _audio.pitch = pitch.RandomValue;
                    _audio.volume = volume.RandomValue;
                    _audio.loop = Loop;
                    _audio.Play();
                    //Debug.Log($"Play {clip.name}");
                }
                played = true;
            }
        }
    }
}