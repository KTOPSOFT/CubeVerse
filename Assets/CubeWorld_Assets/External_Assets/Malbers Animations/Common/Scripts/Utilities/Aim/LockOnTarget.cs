using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/utilities/lock-on-target")]
    public class LockOnTarget : MonoBehaviour
    {
        [Tooltip("The Lock On Target will activate automatically if any target is stored on the list")]
        public BoolReference Auto = new(false);


        [Tooltip("The Lock On Target requires an Aim Component")]
        [RequiredField] public Aim aim;

        [Tooltip("Set of the focused 'potential' Targets")]
        [RequiredField] public RuntimeGameObjects Targets;

        [Tooltip("Time needed to change to the next or previous target")]
        public FloatReference NextTargetTime = new(0f);

        private float CurrentNextTime;

        private int CurrentTargetIndex = -1;
        public bool debug;


        [Header("Events")]
        public TransformEvent OnTargetChanged = new();
        public TransformEvent OnTargetAimAssist = new();
        public BoolEvent OnLockingTarget = new();

        /// <summary> </summary>
        public Transform LockedTarget
        {
            get => locketTarget;

            private set
            {
                locketTarget = value;
                aim.SetTarget(value);
                OnTargetChanged.Invoke(value);

                IsAimTarget = value != null ? value.FindComponent<AimTarget>() : null;

                OnTargetAimAssist.Invoke(IsAimTarget != null ? IsAimTarget.AimPoint : value);
            }
        }
        private Transform locketTarget;


        //  private Transform DefaultAimTarget;

        public bool LockingOn { get; private set; }

        public AimTarget IsAimTarget { get; private set; }
        public GameObject Owner => transform.root.gameObject;

        private void Awake()
        {
            Targets.Clear();

            if (aim != null) aim.FindComponent<Aim>();


            //DefaultAimTarget = null;
            //if (aim != null) DefaultAimTarget = aim.AimTarget;
        }

        private void OnEnable()
        {
            if (Targets != null)
            {
                Targets.OnItemAdded.AddListener(OnItemAdded);
                Targets.OnItemRemoved.AddListener(OnItemRemoved);
            }

            ResetLockOn();
        }




        private void OnDisable()
        {
            if (Targets != null)
            {
                Targets.OnItemAdded.RemoveListener(OnItemAdded);

                Targets.OnItemRemoved.RemoveListener(OnItemRemoved);
            }
            ResetLockOn();
        }

        private void OnItemAdded(GameObject arg0)
        {
            if (Auto.Value && !LockingOn)
            {
                LockTarget(true);
            }
        }

        public void LockTargetToggle()
        {
            LockingOn ^= true;
            LookingTarget();
        }



        public void LockTarget(bool value)
        {
            LockingOn = value;
            LookingTarget();
        }

        private void LookingTarget()
        {
            if (LockingOn)
            {
                if (Targets != null && Targets.Count > 0) //if we have a focused Item
                {
                    FindNearestTarget();
                    OnLockingTarget.Invoke(true);
                }
            }
            else
            {
                ResetLockOn();
            }
        }

        //Reset the values when the Lock Target is off
        private void ResetLockOn()
        {
            if (LockedTarget != null)
            {
                CurrentTargetIndex = -1;
                LockedTarget = null;
                OnLockingTarget.Invoke(LockingOn = false);
                Debugging($"Reset Locked Target: [Empty]");
            }
        }

        private void FindNearestTarget()
        {
            var closest = Targets.Item_GetClosest(gameObject);  //When Lock Target is On.. Get the nearest Target on the Set 

            if (closest)
            {
                LockedTarget = closest.transform;
                CurrentTargetIndex = Targets.items.IndexOf(closest);    //Save the Index so we can cycle to all the targets.
                Debugging($"Locked Target: {LockedTarget.name}");
            }
            else
            {
                ResetLockOn();
            }
        }


        public void Target_Scroll(Vector2 value)
        {
            if (value.y > 0 || value.x > 0)
                Target_Next();
            else if (value.y < 0 || value.x < 0)
                Target_Previous();
        }


        public void Target_Scroll(float value)
        {
            if (value > 0) Target_Next();
            else if (value < 0) Target_Previous();
        }

        public void Target_Next()
        {
            if (CurrentNextTime > 0 && !MTools.ElapsedTime(CurrentNextTime, NextTargetTime)) return;


            if (Targets != null && LockedTarget != null && CurrentTargetIndex != -1) //Check everything is working
            {
                CurrentTargetIndex++;
                CurrentTargetIndex %= Targets.Count; //Cycle to the first in case we are on the last item on the list
                LockedTarget = Targets.Item_Get(CurrentTargetIndex).transform;     //Store the Next Target

                //  Debug.Log(" Target_Next()");

                Debugging($"Locked Next Target: {LockedTarget.name}");


                CurrentNextTime = Time.time;
            }
        }

        public void Target_Previous()
        {
            if (CurrentNextTime > 0 && !MTools.ElapsedTime(CurrentNextTime, NextTargetTime)) return;

            if (Targets != null && LockedTarget != null && CurrentTargetIndex != -1) //Check everything is working
            {
                CurrentTargetIndex--;
                if (CurrentTargetIndex == -1) CurrentTargetIndex = Targets.Count - 1;
                LockedTarget = Targets.Item_Get(CurrentTargetIndex).transform;     //Store the Next Target

                //Debug.Log(" Target_Previous()");

                Debugging($"Locked Previous Target: {LockedTarget.name}");

                CurrentNextTime = Time.time;
            }
        }

        private void OnItemRemoved(GameObject _)
        {
            if (LockingOn) //If we are still on Lock Mode then find the next Target
                this.Delay_Action(() => FindNearestTarget()); //Find the nearest target the next frame
        }


        public void Debugging(string value)
        {
#if UNITY_EDITOR
            if (debug)
                Debug.Log($"<B>[{aim.name}]</B> → <color=white>{value}</color>", this);
#endif
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!aim) aim = this.FindComponent<Aim>();
        }


        private void Reset()
        {
            aim = this.FindComponent<Aim>();

            Targets = MTools.GetInstance<RuntimeGameObjects>("Lock on Targets");
            var lockedTarget = MTools.GetInstance<TransformVar>("Locked Target");

            var CamEvent = MTools.GetInstance<MEvent>("Set Camera LockOnTarget");


            UnityEditor.Events.UnityEventTools.AddPersistentListener(OnTargetChanged, CamEvent.Invoke);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(OnTargetChanged, lockedTarget.SetValue);
        }


        //[ContextMenu("Create Input")]
        //private void CreateInput()
        //{

        //}
#endif
    }
}