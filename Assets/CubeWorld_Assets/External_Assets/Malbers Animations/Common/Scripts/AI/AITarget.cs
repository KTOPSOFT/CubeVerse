using MalbersAnimations.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/ai/ai-target")]
    /// <summary>
    /// This Script allows to set a gameObject as a Target for the Ai Logic. 
    /// So when an AI Animal sets a gameObject holding this script  as a target, 
    /// it can have a stopping distance and it can stop on a properly distance.
    /// </summary>
    [AddComponentMenu("Malbers/AI/AI Target")]
    [SelectionBase]
    public class AITarget : MonoBehaviour, IAITargeterTarget, IAITarget
    {
        public WayPointType pointType = WayPointType.Ground;

        public ICharacterAction character;

        [Tooltip("Distance for AI driven animals to stop when arriving to this gameobject. When is set as the AI Target.")]
        [Min(0)] public float stoppingDistance = 1f;

        [Tooltip("Distance for AI driven animals to start slowing its speed when arriving to this gameobject. If its set to zero or lesser than the Stopping distance, the Slowing Movement Logic will be ignored")]
        [Min(0)] public float slowingDistance = 0;

        [Tooltip("Offset to correct the Position of the Target")]
        [SerializeField] private Vector3 center;

        [Tooltip("Default Height for the Waypoints")]
        [SerializeField] private float m_height = 0.5f;

        [Tooltip(" When the AI  arrives to this target, The character will rotate in place to  looks at the center of the AI Target?")]
        [SerializeField] private bool m_arriveLookAt = true;

        [Min(0), Tooltip("How many AI character can target this gameObject at the same time. Zero means infinite targets")]
        public int m_TargetLimit = 0;

        [Tooltip("Distance for AI driven animals to stop when arriving to this gameobject. When is set as the AI Target.")]
        [Hide(nameof(m_TargetLimit), true)]
        [Min(0)] public float m_targeterStopDistance = 0.2f;

        [Hide(nameof(m_TargetLimit), true)]
        [Tooltip("Distance the AI has to wait if all the spots on this Target are ocupied")]
        [Min(0)] public float m_WaitTargeterDistance = 4f;
        public float TargeterStopDistance { get => m_targeterStopDistance; set => m_targeterStopDistance = value; }

        public int Targeters// { get; set; }
        {
            get => targeters;
            set
            {
                targeters = value;
                FullTargeters = value >= m_TargetLimit;
                //Debug.Log($"Targeters {value}");
            }
        }
        public int TargetsLimits
        {
            get => m_TargetLimit;
            set => m_TargetLimit = value;
        }

        /// <summary> The AI TARGET HAS ALL THE TARGETERs Occupied  </summary>
        public bool FullTargeters { get; set; }

        private int targeters;

        /// <summary>Current Queues </summary>
        public IAIControl[] TargetersObjects;
        public List<IAIControl> TargetersWaiting;

        public bool ArriveLookAt => m_arriveLookAt;
        public float Height => m_height * transform.localScale.y;

        //Event to listen when the Target Type of waypoint has changed
        public System.Action<WayPointType> TargetTypeChanged { get; set; }

        /// <summary>There's a new AI using this target so Refresh </summary>
        public UnityEvent TargetersRefresh { get; set; }


        public WayPointType TargetType
        {
            set
            {
                if (pointType != value)
                {
                    pointType = value;
                    TargetTypeChanged?.Invoke(value);
                }
            }
            get
            {
                return pointType;
            }
        }

        [Space]
        public GameObjectEvent OnTargetArrived = new();

        /// <summary>Center of the Animal to be used for AI and Targeting  </summary>
        public Vector3 Center
        {
            private set => center = value;
            get => transform.TransformPoint(center);
        }

        public float WaitTargeterDistance
        {
            private set => m_WaitTargeterDistance = value;
            get => m_WaitTargeterDistance + StopDistance();
        }

        private void OnEnable()
        {
            character = this.FindInterface<ICharacterAction>();

            if (TargetsLimits > 0)
            {
                TargetersObjects = new IAIControl[TargetsLimits]; //Set the Limit on the Targeter Objects
                TargetersWaiting = new(); //Set the Waiting Objects
            }

            if (character != null) character.OnState += (OnStateChanged);

            TargetersRefresh ??= new();
        }



        private void OnDisable()
        {
            if (character != null) character.OnState -= (OnStateChanged);

            ////Clear Every Targeter??
            //foreach (var ai in TargetersObjects)
            //{
            //    if (ai.IsAITarget.transform == transform) ai.ClearTarget();
            //    ai.Index = -1;
            //}
            //TargetersRefresh.Invoke();

            //TargetersObjects = new();
        }


        /// <summary> Listen if the character is swimming, flying, or underwater  </summary>
        private void OnStateChanged(int state)
        {
            if (state == StateEnum.UnderWater) TargetType = WayPointType.Underwater;
            else if (state == StateEnum.Fly) TargetType = WayPointType.Air;
            else if (state == StateEnum.Swim) TargetType = WayPointType.Water;
            else
            {
                TargetType = WayPointType.Ground;
            }
        }

        public void TargetArrived(GameObject target) => OnTargetArrived.Invoke(target);

        public void SetLocalCenter(Vector3 localCenter) => center = localCenter;

        /// <summary>Get the center of the AI Target</summary>
        public virtual Vector3 GetCenterPosition(int index) => TargeterPosition(index);

        public virtual Vector3 GetCenterPosition() => Center;

        /// <summary>Get the center of the AI Target plus the Height value</summary>
        public virtual Vector3 GetCenterY() => Center + (transform.up * Height);

        public float StopDistance() => stoppingDistance * transform.localScale.y; //IMPORTANT For Scaled objects like the ball
        public float SlowDistance() => slowingDistance * transform.localScale.y; //IMPORTANT For Scaled objects like the ball


        public virtual void AddTargeter(IAIControl targeter)
        {
            if (TargetsLimits == 0) return;

            bool FoundSpace = false;

            //Find the first space 
            for (int i = 0; i < TargetsLimits; i++)
            {
                if (TargetersObjects[i] == targeter) //We have found ourselves so skip everything Do not do anything if we are already here
                {
                    return;
                }

                if (TargetersObjects[i] == null)
                {
                    TargetersObjects[i] = targeter;
                    targeter.Index = i;
                    FoundSpace = true;
                    Targeters++;
                    break;
                }
            }

            //Meaning all spaces were ocupied
            if (!FoundSpace && !TargetersWaiting.Contains(targeter))
            {
                TargetersWaiting.Add(targeter); //Add the waiting Target to the wait list
                targeter.IsWaitingOnTarget = true;
                targeter.Index = Targeters + TargetersWaiting.Count - 1; //Set Index to the wating ones
            }

            TargetersRefresh.Invoke(); //BroadCast that the Ammount of targeters has changed
        }



        public virtual void RemoveTargeter(IAIControl targeter)
        {
            if (TargetsLimits == 0) return;

            int index = Array.IndexOf(TargetersObjects, targeter); //Find if the target was added

            //Debug.Log($"Remove Targeter {targeter.Transform}");

            if (index >= 0)
            {
                //SET again the first one 
                if (TargetersWaiting.Count > 0)
                {
                    TargetersObjects[index] = TargetersWaiting[0];
                    TargetersObjects[index].Index = index;

                    TargetersWaiting[0].IsWaitingOnTarget = false;
                    TargetersWaiting.RemoveAt(0);
                }
                else
                {
                    //Clear Remove
                    TargetersObjects[index].Index = -1;
                    TargetersObjects[index] = null;
                    Targeters--;
                }
            }
            else
            {
                //The Targeter was on the Wait list so remove it because it went to another Target
                if (TargetersWaiting.Contains(targeter))
                {
                    targeter.IsWaitingOnTarget = false;
                    TargetersWaiting.Remove(targeter);
                }
            }

            TargetersRefresh.Invoke(); //BroadCast that the Ammount of targeters has changed
        }

        /// <summary>  Calculates the position of the Targeter by its index </summary>
        /// <param name="index"></param>
        private Vector3 TargeterPosition(int index)
        {
            if (TargetsLimits == 0 || Targeters == 1) return Center;

            if (index > TargetsLimits - 1) return Center; //Skip for all the other targeters that are ouside the limit

            var total = Mathf.Min(Targeters, TargetsLimits);

            float arcDegree = 360.0f / total;
            var rot = arcDegree * index;

            if (float.IsNaN(rot) || float.IsInfinity(rot)) rot = 0; //Weird bug

            // Debug.Log($"rot: {rot}");

            Quaternion rotation = Quaternion.Euler(0, rot, 0);

            Vector3 currentDirection = Vector3.forward;
            currentDirection = rotation * currentDirection;

            Vector3 CurrentPoint = Center + (currentDirection * StopDistance());
            return CurrentPoint;
        }

        public bool TargeterIsWaiting(int index)
        {
            if (TargetsLimits == 0) { return false; }
            return (index > TargetsLimits - 1);
        }

        public float GetTargeterStoppingDistance(int index)
        {
            if (TargetsLimits == 0 || Targeters == 1) return StopDistance();
            //if (Targeters == 1 && index == 0) return StopDistance(); //Use the default if there's only one targeter

            //Sent the Wait Target Distance for the Outside waiting ones
            return index > TargetsLimits - 1 ? WaitTargeterDistance : TargeterStopDistance;
        }


        public void SetGrounded() => TargetType = WayPointType.Ground;
        public void SetAir() => TargetType = WayPointType.Air;
        public void SetWater() => TargetType = WayPointType.Water;


#if UNITY_EDITOR && MALBERS_DEBUG
        private void OnDrawGizmosSelected()
        {
            if (!UnityEditorInternal.InternalEditorUtility.GetIsInspectorExpanded(this)) return;

            StopDistanceGizmo(Color.red);

            if (stoppingDistance < slowingDistance)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(Center, transform.up, slowingDistance * transform.localScale.y);
            }

            // var limit = Application.isPlaying ? Targeters : m_TargetLimit;

            if (Application.isPlaying)
            {
                for (int i = 0; i < Targeters; i++)
                {
                    var CurrentPoint = TargeterPosition(i);
                    Gizmos.DrawWireSphere(CurrentPoint, m_targeterStopDistance);
                }
            }


            if (TargetsLimits > 0 && m_WaitTargeterDistance > 0)
            {
                UnityEditor.Handles.color = (Color.yellow + Color.white) / 2;
                UnityEditor.Handles.DrawWireDisc(Center, transform.up, (stoppingDistance + m_WaitTargeterDistance) * transform.localScale.y);
            }
        }



        private void StopDistanceGizmo(Color color)
        {
            //Draw Stopping Distance
            UnityEditor.Handles.color = Gizmos.color = color;
            UnityEditor.Handles.DrawWireDisc(Center, transform.up, stoppingDistance * transform.localScale.y);

            //Draw Height
            Gizmos.DrawRay(Center, transform.up * Height);
            UnityEditor.Handles.DrawWireDisc(Center, transform.up, stoppingDistance * transform.localScale.y * 0.1f);
            Gizmos.DrawWireSphere(Center + transform.up * Height, stoppingDistance * 0.1f);
        }
#endif
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(AITarget))]
    public class AITargetEditor : Editor
    {
        AITarget M;
        private void OnEnable()
        {
            M = target as AITarget;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Application.isPlaying && M.TargetersObjects != null)
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.IntField("Current Targeters", M.Targeters);

                    if (M.TargetersObjects != null && M.TargetersObjects.Length > 0)
                    {
                        for (int i = 0; i < M.TargetersObjects.Length; i++)
                        {
                            Transform value = M.TargetersObjects[i].IsUnityRefNull() ? null : M.TargetersObjects[i].Owner;

                            EditorGUILayout.ObjectField($"Targeter [{i}]", value, typeof(Transform), false);
                        }
                    }

                    if (M.TargetersWaiting != null && M.TargetersWaiting.Count > 0)
                    {
                        EditorGUILayout.IntField("Waiting Targeters", M.TargetersWaiting.Count);

                        for (int i = 0; i < M.TargetersWaiting.Count; i++)
                        {
                            Transform value = M.TargetersWaiting[i].IsUnityRefNull() ? null : M.TargetersWaiting[i].Owner;
                            EditorGUILayout.ObjectField($"Waiting Targeter [{i}]", value, typeof(Transform), false);
                        }
                    }
                }
            }
        }
    }
#endif
}