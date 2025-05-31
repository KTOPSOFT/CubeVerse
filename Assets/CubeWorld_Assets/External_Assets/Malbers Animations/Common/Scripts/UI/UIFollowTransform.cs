using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations.UI
{
    /// <summary>makes an UI Object to follow a World Object</summary>
    [AddComponentMenu("Malbers/UI/UI Follow Transform")]
    public class UIFollowTransform : MonoBehaviour
    {
        [Tooltip("Reference for the Main Camera on the Scene")]
        public Camera MainCamera;
        [Tooltip("Which Transform to Follow and Convert to Screen Position")]
        public TransformReference WorldTransform = new();

        [Tooltip("Use a child of the World Transform instead")]
        public StringReference UseChild = new();

        private Transform followT;

        [Tooltip("Use FixedUpdate cycle for rendering UI, false to use LateUpdate")]
        [SerializeField] private UpdateType cycle = UpdateType.LateUpdate;

        [Tooltip("If the Object is Off-Screen, disable it")]
        public Behaviour HideOffScreen;
        [Tooltip("Reset the World Transform to Null when this component is Disable")]
        public bool ResetOnDisable = false;

        [Tooltip("If the World transform is Null, hide the Behaviour [HideOffScreen]")]
        public bool HideOnNull = false;

        [Tooltip("Offset position for the tracked gameobject")]
        public Vector3Reference Offset = new(Vector3.zero);
        [Tooltip("Scale of the Instantiated prefab")]
        public Vector3Reference Scale = new(Vector3.one);

        public BoolEvent OnTarget = new();

        public Vector3 ScreenCenter { get; set; }
        public Vector3 DefaultScreenCenter { get; set; }
        public Transform FollowT
        {
            get => followT;
            set
            {
                //Debug.Log("value = " + value);
                followT = value;
            }
        }

        void Awake()
        {
            MainCamera = MTools.FindMainCamera();
            ScreenCenter = transform.position;
            DefaultScreenCenter = transform.position;

            WorldTransform ??= new();

            if (!WorldTransform.UseConstant && WorldTransform.Variable != null)
                WorldTransform.Variable.OnValueChanged += ListenTransform;

        }


        private void OnDestroy()
        {
            if (!WorldTransform.UseConstant && WorldTransform.Variable != null)
                WorldTransform.Variable.OnValueChanged -= ListenTransform;
        }

        private void OnEnable()
        {
            MainCamera = MTools.FindMainCamera();
            StopAllCoroutines();

            if (HideOffScreen)
                HideOffScreen.transform.localScale = Scale;


            if (WorldTransform.Value)
                Align();


            SetTransform(WorldTransform);


            // Choose between FixedUpdate or Late Update
            YieldInstruction UpdateTime = cycle == UpdateType.FixedUpdate ? new WaitForFixedUpdate() : new WaitForEndOfFrame();

            StartCoroutine(UpdateCycle(UpdateTime));

        }


        private IEnumerator UpdateCycle(YieldInstruction waitTime)
        {
            while (true)
            {
                Align();
                yield return waitTime;
            }
        }


        private void OnDisable()
        {
            if (ResetOnDisable) Clear();

            StopAllCoroutines();
        }

        public virtual void Clear()
        {
            WorldTransform.Value = null;
            transform.position = ScreenCenter; //Reset the Screen Center Position
            if (HideOffScreen) HideOffScreen.enabled = !HideOnNull;
        }

        public void ListenTransform(Transform newTarget)
        {
            enabled = newTarget != null;
            SetTransform(newTarget);
            Align();
        }

        public void SetTransform(Transform newTarget)
        {
            WorldTransform.Value = newTarget;

            FindFollow(newTarget);

            if (FollowT == null)
            {
                if (HideOffScreen) HideOffScreen.enabled = false;
            }
            else
            {
                Align();
                enabled = newTarget != null;
            }

            OnTarget.Invoke(FollowT != null);
        }

        private void FindFollow(Transform newTarget)
        {
            if (newTarget != null && !string.IsNullOrEmpty(UseChild.Value))
            {
                FollowT = newTarget.FindGrandChild(UseChild);

                if (FollowT == null) FollowT = newTarget;
            }
            else
            {
                FollowT = newTarget;
            }
        }

        public void SetScreenCenter(Vector3 newScreenCenter)
        {
            ScreenCenter = newScreenCenter;
            enabled = true;
        }
        public void Align()
        {
            //Debug.Log($"{name} :followT" + FollowT, this);

            if (MainCamera == null || FollowT == null) { /*enabled = false; */return; }

            var pos = MainCamera.WorldToScreenPoint(FollowT.position + Offset);
            transform.position = pos;
            if (HideOffScreen)
            {
                HideOffScreen.enabled = (DoHideOffScreen(pos));
            }
            else
            {
                if (pos.z < 0)
                {
                    pos.y = pos.y > Screen.height / 2 ? 0 : Screen.height;
                }

                transform.position = new Vector3(
                            Mathf.Clamp(pos.x, 0, Screen.width),
                            Mathf.Clamp(pos.y, 0, Screen.height),
                           0);
            }
        }



        private bool DoHideOffScreen(Vector3 position)
        {
            if (position.x < 0 || position.x > Screen.width) return false;
            if (position.y < 0 || position.y > Screen.height) return false;
            if (position.z < 0) return false;

            return true;
        }


        /*
       private void LateUpdate()
       {
           if (!useFixedUpdate)
           {
               Align();
           }
       }

       private void FixedUpdate()
       {
           if (useFixedUpdate)
           {
               Align();
           }
       }
       */


#if UNITY_EDITOR

        void Reset()
        {

            if (!TryGetComponent<MEventListener>(out var MeventL))
            {
                MeventL = gameObject.AddComponent<MEventListener>();
            }

            MeventL.Events = new(1) { new() };

            var listener = MeventL.Events[0];

            listener.useTransform = true;
            listener.useVector3 = true;
            listener.useVoid = false;

            listener.Event = MTools.GetInstance<MEvent>("Follow UI Transform");

            if (listener.Event != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(listener.ResponseTransform, SetTransform);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(listener.ResponseVector3, SetScreenCenter);
            }
        }
#endif
    }
}