using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{

    public enum EndType { None, Additive, Invert }

    /// <summary>  Based on 3DKit Controller from Unity  </summary>
    public abstract class MSimpleTransformer : MonoBehaviour
    {
        public enum UpdateCycle { Update, FixedUpdate, LateUpdate }

        [Tooltip("This is the object to move. Must be child of this gameobject")]
        [RequiredField] public Transform Object;

        public UpdateCycle update = UpdateCycle.FixedUpdate;

        [Tooltip("Once: The animation will be applied once and then the Component will be disabled\n" +
            "Ping Pong: The animation will be played on forward and backards forever\n" +
            "Repeat: The animation will be played on repeat forever.")]
        public LoopType loopType;

        [Hide(nameof(loopType), (int)LoopType.Once)]
        [Tooltip("Once the Animation end you can: \nAdditive: Keep Pusing Forward.\nInvert: If it gets to the End it will go on the oppositive Direction")]
        public EndType endType = EndType.None;

        [Tooltip("The Laps to play the animation when the Loop is set to PingPong or Repeat. less than 0 means infinite")]
        [Hide(nameof(loopType), true, (int)LoopType.Once)]
        public IntReference Laps = new(0);


        public bool UnScaleTime = false;
        public bool CannotBeInterrupted = true;

        public FloatReference StartDelay = new();
        public FloatReference EndDelay = new();
        public FloatReference duration = new(1);
        public AnimationCurve m_Curve = new(MTools.DefaultCurve);

        [Tooltip("Show/Hide the Events")]
        public bool events = false;

        [Hide(nameof(events))]
        public UnityEvent WaitStart = new();
        [Hide(nameof(events))]
        public UnityEvent OnStart = new();
        [Hide(nameof(events))]
        public UnityEvent OnEnd = new();
        [Hide(nameof(events))]
        public UnityEvent EndWait = new();



        [Range(0, 1)]
        public float preview;

        public int currentLap { get; protected set; }
        public float time { get; protected set; }
        public float value { get; protected set; }
        public float lastValue { get; protected set; }
        public bool Waiting { get; set; } = false;
        public bool Playing { get; set; } = false;
        public bool Inverted { get; protected set; }

        /// <summary>  Is the PingPong Animation going forward or backwards </summary>
        protected bool forward; //Used for the PingPong

        protected WaitForSeconds StartWaitSeconds;
        protected WaitForSeconds EndWaitSeconds;


        //private void Start()
        //{
        //    Restart();
        //}

        protected virtual void OnEnable()
        {
            Restart();

            SetStartWait(StartDelay);
            SetEndWait(EndDelay);
            DoWaitStart();
        }

        public virtual void Play() => Activate();


        public virtual void Stop()
        {
            Playing = false;
            Waiting = false;
            enabled = false;
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        private void SetStartWait(float delay) => StartWaitSeconds = new WaitForSeconds(delay);
        private void SetEndWait(float delay) => EndWaitSeconds = new WaitForSeconds(delay);

        protected virtual void Restart()
        {
            Waiting = false;
            //time = 0f;
            if (endType == EndType.None && loopType == LoopType.Once) value = 0f; //Reset the Once
            lastValue = 0f;
            currentLap = 0;
            forward = true;
            Evaluate(value);
            StopAllCoroutines();
        }


        private void Logic(float deltaTime)
        {
            if (Playing)
            {
                time += (deltaTime / duration) % 1;

                switch (loopType)
                {
                    case LoopType.Once:
                        LoopOnce();
                        break;
                    case LoopType.PingPong:
                        LoopPingPong();
                        break;
                    case LoopType.Repeat:
                        LoopRepeat();
                        break;
                }

                if (Playing) Evaluate(value); //Check if is still playing before evaluating
            }
        }


        private IEnumerator C_WaitStart()
        {


            Pre_Start();
            WaitStart.Invoke();

            if (StartDelay > 0)
            {
                Waiting = true;
                Playing = false;
                yield return StartWaitSeconds;
            }

            Pos_Start();
            OnStart.Invoke();

            Waiting = false;
            Playing = true;
        }

        private IEnumerator C_WaitEnd()
        {
            Pre_End();

            Playing = false;
            OnEnd.Invoke();

            if (EndDelay > 0)
            {
                Waiting = true;
                yield return EndWaitSeconds;
            }

            Pos_End();
            EndWait.Invoke();

            if (loopType == LoopType.PingPong)
            {
                OnStart.Invoke();
                Playing = true;
            }
            else
            {
                time = 0;

                if (loopType == LoopType.Once && endType != EndType.None)
                {
                    value = 0;
                }
            }

            Waiting = false;

            Evaluate(value);

            if (loopType == LoopType.Once)
            {
                enabled = false;
            }
        }


        private IEnumerator C_WaitRepeat()
        {
            Playing = false;

            Pre_End();

            OnEnd.Invoke();

            currentLap++;

            if (EndDelay > 0)
            {
                Waiting = true;

                yield return EndWaitSeconds;
            }
            Pos_End();

            //If the Laps are set to a number (STOP THE REPEAT) which is not unlimited
            if (Laps > 0 && currentLap >= Laps)
            {
                enabled = false;
            }
            else
            {
                value = 0;
                Evaluate(value);

                Pre_Start();

                if (StartDelay > 0)
                {
                    Waiting = true;
                    Playing = false;
                    yield return StartWaitSeconds;
                }
                OnStart.Invoke();
                Pos_Start();


                Waiting = false;
                Playing = true;
                yield return null;
            }
        }


        public void Activate()
        {
            if (Waiting) return;
            if (Playing && CannotBeInterrupted) return;

            Playing = true;
            enabled = true;
            OnEnable(); //Meaning it has not finished the last animation so start over
        }


        public void ActivateToggle()
        {
            enabled = !enabled;
        }


        private void LateUpdate()
        {
            if (update != UpdateCycle.LateUpdate) return;
            Logic(UnScaleTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void Update()
        {
            if (update != UpdateCycle.Update) return;
            Logic(UnScaleTime ? Time.unscaledDeltaTime : Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (update != UpdateCycle.FixedUpdate) return;
            Logic(UnScaleTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);
        }




        /// <summary> Main Method to Evaluate the Transformer  </summary>
        public abstract void Evaluate(float curveValue);

        protected virtual void Pre_Start() { }
        protected virtual void Pos_Start() { }
        protected virtual void Pre_End() { }
        protected virtual void Pos_End() { }

        void LoopPingPong()
        {
            lastValue = value;

            value = Mathf.PingPong(time, 1f);


            if (forward && lastValue > value)
            {
                OnEnd.Invoke();
                forward ^= true;
                DoWaitEnd();
            }
            else if (!forward && lastValue < value)
            {
                OnEnd.Invoke();
                forward ^= true;
                DoWaitStart();
                currentLap++;
            }

            if (Laps > 0 && currentLap >= Laps)
            {
                enabled = false;
                value = 0;
                time = 0;
                Evaluate(value);
            }
        }

        void LoopRepeat()
        {
            lastValue = value;
            value = Mathf.Repeat(time, 1f);

            if (lastValue > value)
            {
                value = 1;
                WaitRepeat();
            }
        }

        private void DoWaitEnd()
        {
            StartCoroutine(C_WaitEnd());
        }

        private void DoWaitStart() => StartCoroutine(C_WaitStart());
        private void WaitRepeat() => StartCoroutine(C_WaitRepeat());

        void LoopOnce()
        {
            value = Mathf.Clamp01(time);

            if (value >= 1)
            {
                value = 1;
                time = 1;
                DoWaitEnd();
            }
        }


        protected virtual void Reset()
        {
            if (transform.childCount > 0)
            { Object = transform.GetChild(0); }
            else
            {
                Object = transform;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MSimpleTransformer), true)]
    public class MSimpleTransformerEditor : Editor
    {
        MSimpleTransformer pt;

        private void OnEnable()
        {
            pt = target as MSimpleTransformer;
        }

        public override void OnInspectorGUI()
        {
            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();


                if (pt.Object == pt.transform)
                {
                    EditorGUILayout.HelpBox("The Object to modify cannot be the same as the owner of this script. It needs to be a child gameobject", MessageType.Error);
                }

                if (cc.changed && pt.Object != pt.transform)
                {
                    pt.Evaluate(pt.preview);
                }
            }

            if (Application.isPlaying)
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.Toggle("Playing", pt.Playing);
                    EditorGUILayout.Toggle("Waiting", pt.Waiting);
                    EditorGUILayout.Toggle("Inverted", pt.Inverted);
                    EditorGUILayout.FloatField("Time", pt.time);
                    EditorGUILayout.FloatField("Value", pt.value);
                    EditorGUILayout.IntField("Lap", pt.currentLap);
                    Repaint();
                }
        }
    }
#endif
}