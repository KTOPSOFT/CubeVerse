using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Animal Controller/Mode Align")]
    public class MModeAlign : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;

        [ContextMenuItem("Attack Mode", "AddAttackMode")]
        public List<ModeID> modes = new();

        [Tooltip("Exclude these abilities when playing the mode")]
        public List<int> ExcludeAbilities = new();


        [Tooltip("If the Target animal is on any of these states then ignore alignment.")]
        public List<StateID> ignoreStates = new();

        [Tooltip("The animal will keep attacking the current target until the target enters in any of the ignore states")]
        public BoolReference KeepCurrentTarget = new(true);

        [Tooltip("Search only Tags")]
        public Tag[] Tags;
        public LayerReference Layer = new(0);
        [Tooltip("Radius used for the Search")]

        [Min(0)] public float SearchRadius = 2f;
        [Tooltip("Radius used push closer/farther the Target when playing the Mode")]
        [Min(0)] public float Distance = 0;
        //[Tooltip("Multiplier To apply to AITarget found. Set this to Zero to ignore IAI Targets")]
        //[Min(0)] public float TargetMultiplier = 1;
        [Tooltip("Time needed to complete the Position aligment")]
        [Min(0)] public float AlignTime = 0.3f;

        [Tooltip("Front Offset of the Animal to use as Pivot point for the orientation for the aligment")]
        [Min(0)] public float FrontOffet = 0.15f;

        [Tooltip("Ignore Moving the character if we are already too close to the target. Only apply look At rotation. Use this for Quadruped animals")]
        public bool IgnoreClose = true;
        [Tooltip("Override default Distance with th e AI Target Distance on a Target")]
        public bool UseAITargetDistance = true;

        [Tooltip("Align Curve used for the alignment")]
        public AnimationCurve AlignCurve = new(new Keyframe[] { new(0, 0), new(1, 1) });


        public Color debugColor = new(1, 0.5f, 0, 0.2f);

        [HideInInspector, SerializeField] private int EditorTab; //Editor Tabs

        public bool debug;

        private bool Aligning;

        void Awake()
        {
            if (animal == null)
                animal = this.FindComponent<MAnimal>();

            if (modes == null || modes.Count == 0)
            {
                Debug.LogWarning("Please Add Modes to the Mode Align. ", this);
                enabled = false;
            }

            ignoreStates ??= new();
        }

        void OnEnable()
        {
            animal.OnModeStart.AddListener(StartingMode);
            animal.PostStateMovement += PosStateMovement;

        }

        void OnDisable()
        {
            animal.OnModeStart.RemoveListener(StartingMode);

            animal.PostStateMovement -= PosStateMovement;
        }

        private Vector3 OverrideAdditivePos;

        private void PosStateMovement(MAnimal animal)
        {
            if (Aligning)
            {
                //Debug.Log($"Overriding ADPOS {OverrideAdditivePos}");
                animal.additivePosition = OverrideAdditivePos;
            }
        }

        void StartingMode(int ModeID, int ability)
        {
            if (!isActiveAndEnabled) return;

            //Debug.Log($"ability: {ability}");

            if (modes == null || modes.Count == 0 || modes.FirstOrDefault(x => x.ID == ModeID))
            {
                if (ExcludeAbilities.Contains(ability))
                {
                    // Debug.Log("DO NOT ALIGN");
                    return; //Skip the abilities included in this list
                }

                Align();
            }
        }

        public void Align()
        {
            //Search first Animals ... if did not find anyone then search for colliders
            if (!FindAnimals())
            {
                AlignCollider();
            }
        }

        //Store the closest Animal
        private MAnimal ClosestAnimal = null;

        private bool FindAnimals()
        {
            ClosestAnimal = null;
            float ClosestDistance = float.MaxValue;
            var Origin = transform.position;
            var radius = SearchRadius * animal.ScaleFactor;
            MDebug.DrawWireSphere(Origin, Color.red, radius, 1f);

            foreach (var a in MAnimal.Animals)
            {
                if (a == animal                                                 //We are the same animal
                    || a.Sleep                                                  //The Animal is sleep (Not working)
                    || !a.enabled                                               //The Animal Component is disabled    
                    || !MTools.Layer_in_LayerMask(a.gameObject.layer, Layer)    //Is not using the correct layer
                    || !a.gameObject.activeInHierarchy
                    || ignoreStates.Contains(a.ActiveStateID)       //Playing a skip State
                    )
                    continue; //Don't Find yourself or don't find death animals

                if (animal.transform.SameHierarchy(a.transform)) continue;      //Meaning that the animal is mounting the other animal.

                var TargetPos = a.Center;

                var dist = Vector3.Distance(Origin, TargetPos);

                Debug.DrawRay(Origin, TargetPos - Origin, Color.red, 2f);

                if (radius >= dist && ClosestDistance >= dist)
                {
                    ClosestDistance = dist;
                    ClosestAnimal = a;
                }
            }

            if (ClosestAnimal == animal) ClosestAnimal = null; //Clear the Closest animal is it the Animal owner

            //  Debug.Log($"closest {ClosestAnimal} ");

            if (ClosestAnimal)
            {
                if (!GetClosestAITarget(ClosestAnimal.transform))
                {
                    Debuging($"Alinging to [{ClosestAnimal.name}]", this);
                }
                //else
                //{
                //    StartAligning(ClosestAnimal.Center, null);
                //}
                return true;
            }
            return false;
        }

        private void AlignCollider()
        {
            var pos = animal.Center;

            Collider[] AllColliders = Physics.OverlapSphere(pos, SearchRadius * animal.ScaleFactor, Layer.Value, QueryTriggerInteraction.Ignore);

            Collider ClosestCollider = null;

            float ClosestDistance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if ((col.transform.SameHierarchy(animal.transform)  //Don't Find yourself
                    || Tags != null && Tags.Length > 0 && !col.gameObject.HasMalbersTagInParent(Tags) //Skip Tags
                    || !col.gameObject.activeInHierarchy            //Don't Find invisible colliders
                    || col.gameObject.isStatic                      //Don't Find Static colliders
                    || col.GetComponentInParent<MAnimal>()          //we already searched for Animals
                    || !col.enabled))       //Don't look  disabled colliders
                    continue;

                var DistCol = Vector3.Distance(transform.position, col.transform.position);

                if (ClosestDistance > DistCol)
                {
                    ClosestDistance = DistCol;
                    ClosestCollider = col;
                }
            }

            if (ClosestCollider)
            {
                if (!GetClosestAITarget(ClosestCollider.transform))
                    Debuging($"[{name}], Alinging to [{ClosestCollider.name}]", this);
            }
        }

        private bool GetClosestAITarget(Transform target)
        {
            var Center = ClosestAnimal != null && ClosestAnimal.transform == target ? ClosestAnimal.Center : target.position;
            var TargetTransform = ClosestAnimal != null && ClosestAnimal.transform == target ? ClosestAnimal.transform : target;

            var Origin = animal.Position;
            var radius = SearchRadius * animal.ScaleFactor;

            var Core = target.GetComponentInParent<IObjectCore>();
            if (Core != null) { target = Core.transform; } //Find the Core Transform

            var AllAITargets = target.FindInterfaces<IAITarget>(); //Find all

            IAITarget ClosestAI = null;
            var ClosestDistance = float.MaxValue;

            if (AllAITargets != null)
            {
                if (AllAITargets.Length == 1)
                {
                    ClosestAI = AllAITargets[0];
                    Center = ClosestAI.GetCenterPosition(-1);
                }
                else
                {
                    foreach (var a in AllAITargets)
                    {
                        if (!a.transform.gameObject.activeInHierarchy) continue; //Do not search Disabled AI Targets

                        var dist = Vector3.Distance(Origin, a.GetCenterPosition(-1));

                        if (radius >= dist && ClosestDistance >= dist)
                        {
                            ClosestDistance = dist;
                            Center = a.GetCenterPosition(-1);
                            ClosestAI = a;
                            TargetTransform = a.transform;
                        }
                    }
                }
            }

            Center = TargetTransform.InverseTransformPoint(Center); //Convert to Local Position

            var Distance = this.Distance; //Store the Distance

            //Change to the AI Stop Distance if is not Zero and we found an AI
            if (UseAITargetDistance && Distance != 0 && ClosestAI != null)
            {
                Distance = ClosestAI.StopDistance();
            }

            StartAligning(Center, Distance, TargetTransform);

            return ClosestAI != null;
        }

        private void StartAligning(Vector3 LocalTargetCenter, float currentStopDistance, Transform Target)
        {
            StopAllCoroutines();

            // if (!animal.FreeMovement) TargetCenter.y = animal.transform.position.y;


            var TargetCenter = Target.TransformPoint(LocalTargetCenter); //Convert to Global Position


            if (debug)
            {
                var t = 1f;
                MDebug.DrawLine(transform.position, TargetCenter, Color.white, t);
                MDebug.DrawWireSphere(TargetCenter, Quaternion.identity, 0.1f, Color.white, t);

                if (animal.FreeMovement)
                {
                    MDebug.DrawWireSphere(TargetCenter, Quaternion.identity, currentStopDistance, Color.white, t);
                }
                else
                {
                    MDebug.DrawCircle(TargetCenter, Quaternion.identity, currentStopDistance, Color.white, t);
                }
                MDebug.DrawRay(TargetCenter, Vector3.up, Color.white, t);
            }


            //Align Look At the Zone Using Distance
            if (currentStopDistance > 0)
            {
                currentStopDistance += FrontOffet * animal.ScaleFactor; //Align from the Position of the Aligner

                StartCoroutine(
                    LookAt(animal.transform, Target, TargetCenter, FrontOffet, AlignTime, animal.ScaleFactor, AlignCurve));

                MDebug.DrawLine(animal.Center, TargetCenter, Color.yellow, 2f);

                StartCoroutine(AlignTransformRadius(animal, Target, LocalTargetCenter, AlignTime, currentStopDistance, AlignCurve));
            }
            else
            {
                StartCoroutine(
                    LookAt(animal.transform, Target, TargetCenter, FrontOffet, AlignTime, animal.ScaleFactor, AlignCurve));
            }
        }

        public IEnumerator LookAt(Transform t1, Transform t2, Vector3 LocalTargetPos, float AlignOffset, float time, float scale, AnimationCurve AlignCurve)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;
            var target = t2.TransformPoint(LocalTargetPos); //Convert to Global Position
            Vector3 direction = (target - t1.position);

            direction = Vector3.ProjectOnPlane(direction, t1.up); //Clear Y values

            Quaternion FinalRot = Quaternion.LookRotation(direction);

            Vector3 Offset = t1.position + AlignOffset * scale * t1.forward; //Use Offset

            if (AlignOffset != 0)
            {
                //Calculate Real Direction at the End! 
                Quaternion TargetInverse_Rot = Quaternion.Inverse(t1.rotation);
                Quaternion TargetDelta = TargetInverse_Rot * FinalRot;

                var TargetPosition = t1.position + t1.DeltaPositionFromRotate(Offset, TargetDelta);

                direction = (target - TargetPosition);

                var debTime = 2f;

                if (debug)
                {
                    MDebug.Draw_Arrow(TargetPosition, direction, Color.yellow, debTime);
                    MDebug.DrawWireSphere(TargetPosition, 0.1f, Color.green, debTime);
                    MDebug.DrawWireSphere(target, 0.1f, Color.yellow, debTime);
                }
            }

            if (direction.CloseToZero())
            {
                Debug.LogWarning("Direction is Zero. Please set a correct rotation", t1);
                yield break;
            }
            else
            {
                Aligning = true;

                Quaternion Last_Platform_Rot = t1.rotation;

                while ((time > 0) && (elapsedTime <= time))
                {
                    yield return wait;

                    direction = (t2.position - t1.position);
                    direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values
                    FinalRot = Quaternion.LookRotation(direction);


                    //Evaluation of the Pos curve
                    float result = AlignCurve != null ? AlignCurve.Evaluate(elapsedTime / time) : elapsedTime / time;

                    t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                    if (AlignOffset != 0)
                    {
                        Quaternion Inverse_Rot = Quaternion.Inverse(Last_Platform_Rot);
                        Quaternion Delta = Inverse_Rot * t1.rotation;
                        t1.position += t1.DeltaPositionFromRotate(Offset, Delta);
                    }

                    elapsedTime += Time.fixedDeltaTime;
                    Last_Platform_Rot = t1.rotation;
                }


                Aligning = false;
            }
        }

        public IEnumerator AlignTransformRadius(MAnimal animal, Transform Target, Vector3 localTargetPos, float time, float radius, AnimationCurve curve = null)
        {
            if (radius > 0)
            {
                float elapsedTime = 0;

                var Wait = new WaitForFixedUpdate();

                Vector3 StartingPos = animal.Position;

                var WorldTargetPos = Target.TransformPoint(localTargetPos); //Convert to Global Position
                var Direction = (animal.Position - WorldTargetPos).normalized * radius;

                Ray TargetRay = new(WorldTargetPos, (animal.Position - WorldTargetPos).normalized);
                Vector3 TargetPos = TargetRay.GetPoint(radius);

                Debug.DrawRay(animal.Position, -Direction, Color.cyan, 1f);

                animal.TryDeltaRootMotion(); //Reset delta RootMotion

                MDebug.DrawWireSphere(TargetPos, Color.red, 0.05f, 3);

                var TargetDistance = Vector3.Distance(animal.Center, WorldTargetPos);

                Aligning = true;

                while ((time > 0) && (elapsedTime <= time))
                {
                    yield return Wait;

                    WorldTargetPos = Target.TransformPoint(localTargetPos); //Convert to Global Position
                    TargetDistance = Vector3.Distance(animal.Center, WorldTargetPos);

                    if (!IgnoreClose || TargetDistance >= radius)
                    {
                        //Evaluation of the Pos curve
                        float result = curve != null ? curve.Evaluate(elapsedTime / time) : elapsedTime / time;
                        Direction = (-animal.Position + WorldTargetPos).normalized * radius;

                        var TargetPoint = WorldTargetPos - Direction;

                        var nextPos = Vector3.Lerp(StartingPos, TargetPoint, result);
                        var deltaPos = nextPos - animal.Position;

                        OverrideAdditivePos = deltaPos; //Override the Delta Position

                        MDebug.DrawWireSphere(animal.Position, Color.yellow, 0.05f, 3);
                        MDebug.DrawWireSphere(TargetPos, Color.yellow, 0.05f, 3);

                        MDebug.DrawCircle(WorldTargetPos, Target.rotation, radius, Color.red);
                        MDebug.DrawWireSphere(TargetPoint, Color.white, 0.05f);
                    }
                    else //if (TargetDistance < radius)
                    {
                        OverrideAdditivePos = Vector3.zero;
                    }
                    elapsedTime += Time.fixedDeltaTime;

                    // animal.GlobalRootMotion.Value = TargetDistance >= radius; //Only apply RootMotion if we are not close to the target
                }

                WorldTargetPos = Target.TransformPoint(localTargetPos); //Convert to Global Position
                Direction = (-animal.Position + WorldTargetPos).normalized * radius;
                var FinalPos = WorldTargetPos - Direction;//Override the Delta Position

                animal.additivePosition = FinalPos - animal.Position;

                animal.InertiaPositionSpeed = Vector3.zero;

                //Make sure is not moving inside the target after the mode ends
                while (animal.IsPlayingMode && TargetDistance < radius)
                {
                    Aligning = true;
                    yield return Wait;
                    TargetDistance = Vector3.Distance(animal.Center, WorldTargetPos);
                    OverrideAdditivePos = Vector3.zero;
                }
                Aligning = false;
            }
            yield return null;
        }

        private void Debuging(string deb, Object ob)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<B>[{animal.name}]</B> {deb}", ob);
#endif
        }


#if UNITY_EDITOR

        [ContextMenu("Add Attack Mode")]
        private void AddAttackMode()
        {
            Reset();
        }


        void Reset()
        {

            animal = gameObject.FindComponent<MAnimal>();

            modes = new List<ModeID>
            {
                MTools.GetResource<ModeID>("Attack1")
            };

            ignoreStates = new List<StateID>
            {
                MTools.GetResource<StateID>("Death")
            };


            MTools.SetDirty(this);
        }

#if MALBERS_DEBUG
        void OnDrawGizmosSelected()
        {
            if (animal && debug)
            {
                var scale = animal.ScaleFactor;
                var Pos = animal.Position + (FrontOffet * scale * animal.Forward);

                Handles.color = debugColor;
                var c = debugColor; c.a = 1;
                Handles.color = c;
                Handles.DrawWireDisc(Pos, Vector3.up, SearchRadius * scale);

                Handles.color = (c + Color.white) / 2;
                Handles.DrawWireDisc(Pos, Vector3.up, Distance * scale);


                Gizmos.color = debugColor;
                Gizmos.DrawSphere(Pos, 0.1f * scale);
                Gizmos.color = c;
                Gizmos.DrawWireSphere(Pos, 0.1f * scale);
            }
        }
#endif
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MModeAlign)), CanEditMultipleObjects]
    public class MModeAlignEditor : Editor
    {
        SerializedProperty animal,
            modes, ignoreStates, ExcludeAbilities, EditorTab,
            // AnimalsOnly,
            Layer, Tags, debug, LookRadius, DistanceRadius, UseAITargetDistance, AlignTime, FrontOffet, IgnoreClose, //TargetMultiplier, 
            AlignCurve,
            debugColor/*, pushTarget*/;
        private void OnEnable()
        {
            animal = serializedObject.FindProperty("animal");
            EditorTab = serializedObject.FindProperty("EditorTab");
            IgnoreClose = serializedObject.FindProperty("IgnoreClose");
            modes = serializedObject.FindProperty("modes");
            ExcludeAbilities = serializedObject.FindProperty("ExcludeAbilities");
            ignoreStates = serializedObject.FindProperty("ignoreStates");
            // AnimalsOnly = serializedObject.FindProperty("Animals");
            Layer = serializedObject.FindProperty("Layer");
            Tags = serializedObject.FindProperty("Tags");
            LookRadius = serializedObject.FindProperty("SearchRadius");
            DistanceRadius = serializedObject.FindProperty("Distance");
            UseAITargetDistance = serializedObject.FindProperty("UseAITargetDistance");

            AlignTime = serializedObject.FindProperty("AlignTime");
            debugColor = serializedObject.FindProperty("debugColor");
            //TargetMultiplier = serializedObject.FindProperty("TargetMultiplier");
            FrontOffet = serializedObject.FindProperty("FrontOffet");
            debug = serializedObject.FindProperty("debug");
            AlignCurve = serializedObject.FindProperty("AlignCurve");

        }

        private GUIContent _ReactIcon;

        private static readonly string[] Tabs = new string[] { "Alignment", "Filter by" };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription($"Execute a LookAt towards the closest Animal or GameObject  when is playing a Mode on the list");
            EditorTab.intValue = GUILayout.Toolbar(EditorTab.intValue, Tabs);

            if (EditorTab.intValue == 0)
                DrawAlignment();
            else
                DrawFilter();

            serializedObject.ApplyModifiedProperties();
        }



        private void DrawAlignment()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (Application.isPlaying)
                    {

                        if (_ReactIcon == null)
                        {
                            _ReactIcon = EditorGUIUtility.IconContent("d_PlayButton@2x");
                            _ReactIcon.tooltip = "Play at Runtime";
                        }

                        if (GUILayout.Button(_ReactIcon, EditorStyles.miniButton, GUILayout.Width(28f), GUILayout.Height(20)))
                        {
                            (target as MModeAlign).Align();
                        }
                    }


                    EditorGUILayout.PropertyField(animal);
                    if (debug.boolValue)
                        EditorGUILayout.PropertyField(debugColor, GUIContent.none, GUILayout.Width(36));

                    MalbersEditor.DrawDebugIcon(debug);
                }

            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(LookRadius);
                if (LookRadius.floatValue > 0)
                {
                    EditorGUILayout.PropertyField(DistanceRadius);
                    using (new GUILayout.HorizontalScope())
                    {

                        EditorGUILayout.PropertyField(AlignTime);
                        EditorGUILayout.PropertyField(AlignCurve, GUIContent.none, GUILayout.Width(50));
                    }
                    EditorGUILayout.PropertyField(FrontOffet);
                    EditorGUILayout.PropertyField(IgnoreClose);
                    EditorGUILayout.PropertyField(UseAITargetDistance);
                }
            }
        }


        private void DrawFilter()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                EditorGUILayout.PropertyField(Layer);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;

                var oldColor = GUI.contentColor;

                var ArrayColor = MTools.MBlue * 2;
                ArrayColor.a = 0.9f;


                if (Tags.arraySize > 0) GUI.contentColor = ArrayColor;
                EditorGUILayout.PropertyField(Tags);
                GUI.contentColor = oldColor;


                if (modes.arraySize > 0) GUI.contentColor = ArrayColor;
                EditorGUILayout.PropertyField(modes, true);
                GUI.contentColor = oldColor;

                if (ExcludeAbilities.arraySize > 0) GUI.contentColor = ArrayColor;
                EditorGUILayout.PropertyField(ExcludeAbilities, true);
                GUI.contentColor = oldColor;

                if (ignoreStates.arraySize > 0) GUI.contentColor = ArrayColor;
                EditorGUILayout.PropertyField(ignoreStates, true);
                GUI.contentColor = oldColor;


                EditorGUI.indentLevel--;
                //  EditorGUILayout.PropertyField(AnimalsOnly);

            }
        }
    }
#endif

}