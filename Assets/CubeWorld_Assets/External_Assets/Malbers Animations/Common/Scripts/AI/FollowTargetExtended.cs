using MalbersAnimations.Controller;
using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// This is an updated version to the original script from Malbers. It allows you to:
/// * set a random offset to the target position
/// * change the random offset at a given time interval
/// </summary>
namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/AI/Follow Target Extended")]

    [System.Serializable]
    /// <summary> Simple follow target for the animal </summary>
    public class FollowTargetExtended : MonoBehaviour
    {
        public enum TargetType
        {
            [Tooltip("Use the specific target gameobject as target")]
            GameObject,

            [Tooltip("Use the first found gameobject as target which has the specified target tag. Usually the Player tag")]
            MalbersTag
        }

        [Header("Selection")]

        public TargetType type = TargetType.GameObject;

        [Tooltip("An optional target tag. If no target is explicitly set, then the target with this specified target name will be used")]
        [Hide("type", (int)TargetType.MalbersTag)]
        public Tag Tag;

        [Tooltip("The target gameobject. Either explicitly specified or defined at start depending on the target tag")]
        [Hide("type", (int)TargetType.GameObject)]
        public Transform target;

        [Tooltip("In case the target is instantiated at startup, setting this to true will try to find the target with the specified tag during the Update method")]
        public bool delayedTargetSearch = false;

        [Header("Speed")]

        public float stopDistance = 3;
        [Min(0)] public float SlowDistance = 6;
        [Tooltip("Limit for the Slowing Multiplier to be applied to the Speed Modifier")]
        [Range(0, 1)]
        [SerializeField] private float slowingLimit = 0.3f;
        private MAnimal animal;

        [Header("Random Position")]

        [Tooltip("Activate having random positions around the target")]
        public bool targetRandomness = false;

        [Tooltip("Number of seconds at which the target position changes. Use 0 for keeping the initial position")]
        public float updateInterval = 0f;

        [Tooltip("Minimum random distance from the target")]
        [Range(0f, 10f)]
        public float randomDistanceMin = 1f;

        [Tooltip("Maximum random distance from the target")]
        [Range(0f, 10f)]
        public float randomDistanceMax = 3f;

        private Vector3 targetDistanceOffset = Vector3.zero;

        [Header("Follow Mode")]
        /// <summary> Whether following is enabled or not  </summary>
        public bool followEnabled = true;

        /// <summary>  If a key is defined, then this key will be used for toggling the following. </summary>
        public string followToogleKey;

        [Tooltip("Toggle follow mode depending the target being within a given range")]
        public bool followRangeEnabled = true;

        [Tooltip("The range at which the target following is toggled")]
        [Min(0)]
        public float followRangeDistance = 10f;

        [Header("State Change")]

        [Tooltip("If the Target starts to fly then enable the Fly State on this Animal")]
        public bool flyEnabled = true;

        [Tooltip("Lands at the target if the target isn't flying and the distance is within slow distance")]
        public bool landEnabled = true;

        /// <summary>
        /// Whether the animal currently should follow the target.
        /// Depends on the following being enabled
        /// </summary>
        private bool shouldFollow = false;

        private ICharacterAction TargetCharacter;

        /// <summary>Used to Slow Down the Animal when its close the Destination</summary>
        public float SlowMultiplier
        {
            get
            {
                var result = 1f;
                if (SlowDistance > stopDistance && RemainingDistance < SlowDistance)
                    result = Mathf.Max(RemainingDistance / SlowDistance, slowingLimit);

                return result;
            }
        }

        private float RemainingDistance;


        // Use this for initialization
        void OnEnable()
        {
            animal = GetComponentInParent<MAnimal>();

            // randomize target position
            if (targetRandomness)
            {
                // initial target position
                CalculateTargetPositionOffset();

                // periodic target position changes
                if (updateInterval > 0)
                {

                    StartCoroutine(UpdateTargetPositionOffset());
                }
            }

            // find target in case it should be found by tag
            FindTarget();

            if (target != null)
            {
                TargetCharacter = target.GetComponentInParent<ICharacterAction>();

                if (TargetCharacter != null)
                    TargetCharacter.OnState += (OnTargetStateChanged);
            }
        }



        void OnDisable()
        {
            animal.Move(Vector3.zero);      //In case this script gets disabled stop the movement of the Animal

            if (TargetCharacter != null)
                TargetCharacter.OnState -= (OnTargetStateChanged);

            StopAllCoroutines();
        }


        /// <summary>  Enable Fly if the Target starts to fly </summary>
        /// <param name="state"></param>
        private void OnTargetStateChanged(int state)
        {
            if (flyEnabled && state == StateEnum.Fly)
            {
                animal.State_Activate(StateEnum.Fly);
            }
        }

        private void FindTarget()
        {
            if (type == TargetType.MalbersTag && Tag != null)
            {
                var result = Tags.GambeObjectbyTag(Tag).FirstOrDefault();

                if (result != null)
                    target = result.transform;
                else
                {
                    Debug.LogWarning($"There's no GameObject with the Tag {Tag.name} attached on it");
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!target)
            {
                // search until a target is found
                if (delayedTargetSearch)
                {
                    FindTarget();

                    Debug.Log("Delayed target update. Target = " + target);
                }

                // still no target found, abort
                if (!target)
                    return;
            }

            if (Input.anyKeyDown)
            {
                if (followToogleKey.Length > 0 && Input.inputString == followToogleKey)
                {
                    followEnabled = !followEnabled;
                    shouldFollow = followEnabled;
                }
            }


            if (followEnabled)
            {
                if (followRangeEnabled)
                {
                    float distanceThreshold = Vector3.Distance(transform.position, target.position);
                    shouldFollow = distanceThreshold <= followRangeDistance;
                }
            }

            if (!shouldFollow)
            {
                animal.Move(Vector3.zero);

                return;
            }

            Vector3 targetPosition = target.position;
            if (targetRandomness)
            {

                // calculate the new target position depending on the offset from the local position
                // targetPosition = target.transform.TransformPoint( target.localPosition + targetDistanceOffset);
                targetPosition = target.position + targetDistanceOffset;

                // Debug.DrawLine(transform.position, targetPosition, Color.yellow);

            }

            Vector3 Direction = targetPosition - transform.position;               //Calculate the direction from the animal to the target
            RemainingDistance = Vector3.Distance(transform.position, targetPosition); //Calculate the distance..
            animal.Move(RemainingDistance > stopDistance ? Direction * SlowMultiplier : Vector3.zero); //Move the Animal if we are not on the Stop Distance Radius

            // auto-fly logic:
            // fly when inside follow range distance
            // land when inside slow distance
            if (flyEnabled)
            {
                if (RemainingDistance >= SlowDistance && RemainingDistance <= followRangeDistance)
                {
                    if (animal.HasState(StateEnum.Fly))
                    {
                        animal.State_Activate(StateEnum.Fly);
                    }
                }
                else if (landEnabled && RemainingDistance <= SlowDistance)
                {
                    if (animal.activeState.ID == StateEnum.Fly)
                    {
                        animal.State_Allow_Exit(StateEnum.Fly);
                    }
                }
            }
        }


        private IEnumerator UpdateTargetPositionOffset()
        {


            var interval = updateInterval;
            var timing = new WaitForSeconds(interval);

            while (true)
            {
                //Update the timing (EDITOR)
                if (interval != updateInterval)
                    timing = new WaitForSeconds(interval = updateInterval);

                yield return timing;

                CalculateTargetPositionOffset();
            }
        }

        /// <summary>  Calculate a random local offset from the target </summary>
        private void CalculateTargetPositionOffset()
        {
            float distance = Random.Range(randomDistanceMin, randomDistanceMax);
            targetDistanceOffset = Random.insideUnitSphere.normalized * distance;
        }

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            var center = transform.position;

            if (Application.isPlaying && target)
            {

                center = target.position;
            }

            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(center, Vector3.up, stopDistance);

            if (SlowDistance > stopDistance)
            {
                UnityEditor.Handles.color = Color.cyan;
                UnityEditor.Handles.DrawWireDisc(center, Vector3.up, SlowDistance);
            }

            if (followRangeEnabled)
            {
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawWireDisc(center, Vector3.up, followRangeDistance);
            }

            if (followEnabled)
            {
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawLine(transform.position, target.position);
            }
        }
#endif
    }
}