using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{

    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Patrol")]
    public class PatrolTask : MTask
    {
        public override string DisplayName => "Movement/Patrol";

        [Tooltip("The Animal will Rotate/Look at the Target when he arrives to it")]
        public bool LookAtOnArrival = false;


        [Tooltip("Ignores the Wait time of all waypoints")]
        public bool IgnoreWaitTime = false;

        public PatrolType patrolType = PatrolType.LastWaypoint;

        [Tooltip("Use a Runtime GameObjects Set to find the Next waypoint")]
        public RuntimeGameObjects RuntimeSet;
        public RuntimeSetTypeGameObject rtype = RuntimeSetTypeGameObject.Random;
        public IntReference RTIndex = new();
        public StringReference RTName = new();


        public override void StartTask(MAnimalBrain brain, int index)
        {
            brain.AIControl.AutoNextTarget = true; //When Patrolling make sure AutoTarget is set to true... 

            switch (patrolType)
            {
                case PatrolType.LastWaypoint:
                    if (brain.LastWayPoint != null)                                         //If we had a last Waypoint then move to it
                    {
                        brain.TargetAnimal = null;                                          //Clean the Animal Target in case it was one
                        brain.AIControl.SetTarget(brain.LastWayPoint.WPTransform, true);    //Move to the last waypoint the animal  used
                    }
                    break;
                case PatrolType.UseRuntimeSet:
                    if (RuntimeSet != null)                                             //If we had a last Waypoint then move to it
                    {
                        brain.TargetAnimal = null;                                      //Clean the Animal Target in case it was one
                        GameObject go = RuntimeSet.GetItem(rtype, RTIndex, RTName, brain.Animal.gameObject);
                        if (go) brain.AIControl.SetTarget(go.transform, true);
                        break;
                    }
                    break;
                case PatrolType.LocalRuntimeSet:
                    var LocalRuntimeSet = brain.GetComponent<GetRuntimeGameObjects>();
                    if (LocalRuntimeSet != null && LocalRuntimeSet.Collection != null)
                    {
                        GameObject go = LocalRuntimeSet.Collection.GetItem(rtype, RTIndex, RTName, brain.Animal.gameObject);
                        if (go) brain.AIControl.SetTarget(go.transform, true);
                    }
                    break;
                default:
                    break;
            }

            brain.AIControl.LookAtTargetOnArrival = LookAtOnArrival;

            brain.TaskDone(index);
        }

        public override void ExitAIState(MAnimalBrain brain, int index)
        {
            brain.AIControl.StopWait(); //Remove in case it was waiting , when the State is interrupted.
        }

        public override void OnTargetArrived(MAnimalBrain brain, Transform Target, int index)
        {
            brain.AIControl.AutoNextTarget = true; //When Patrolling make sure AutoTarget is set to true... 

            switch (patrolType)
            {
                case PatrolType.LastWaypoint:
                    if (IgnoreWaitTime)
                    {
                        brain.AIControl.StopWait(); //Ingore wait time
                        brain.AIControl.SetTarget(brain.AIControl.NextTarget, true);
                    }
                    break;
                case PatrolType.UseRuntimeSet:

                    GameObject NextTarget = RuntimeSet.GetItem(rtype, RTIndex, RTName, brain.Animal.gameObject);

                    if (NextTarget && brain.AIControl.NextTarget == null)
                    {
                        if (IgnoreWaitTime)
                        {
                            brain.AIControl.StopWait(); //Ingore wait time
                            brain.AIControl.SetTarget(NextTarget.transform, true);
                        }
                        else
                        {
                            brain.AIControl.SetNextTarget(NextTarget);
                            brain.AIControl.MovetoNextTarget();
                        }
                    }
                    break;
                case PatrolType.LocalRuntimeSet:
                    var LocalRuntimeSet = brain.GetComponent<GetRuntimeGameObjects>();

                    if (LocalRuntimeSet != null && LocalRuntimeSet.Collection != null)
                    {
                        GameObject NextTarget2 = LocalRuntimeSet.Collection.GetItem(rtype, RTIndex, RTName, brain.Animal.gameObject);
                        if (NextTarget2) brain.AIControl.SetTarget(NextTarget2.transform, true);

                        if (NextTarget2 && brain.AIControl.NextTarget == null)
                        {
                            if (IgnoreWaitTime)
                            {
                                brain.AIControl.StopWait(); //Ingore wait time
                                brain.AIControl.SetTarget(NextTarget2.transform, true);
                            }
                            else
                            {
                                brain.AIControl.SetNextTarget(NextTarget2);
                                brain.AIControl.MovetoNextTarget();
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        void Reset() { Description = "Simple Patrol Logic using the Default AiAnimal Control Movement System"; }
    }

    public enum PatrolType
    {
        LastWaypoint, UseRuntimeSet,
        LocalRuntimeSet
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(PatrolTask))]
    public class PatrolTaskEditor : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Description, MessageID, patrolType, RuntimeSet, rtype, RTIndex, RTName, UpdateInterval,
            WaitForPreviousTask, LookAtOnArrival, IgnoreWaitTime;

        private void OnEnable()
        {
            WaitForPreviousTask = serializedObject.FindProperty("WaitForPreviousTask");
            Description = serializedObject.FindProperty("Description");
            MessageID = serializedObject.FindProperty("MessageID");
            patrolType = serializedObject.FindProperty("patrolType");
            rtype = serializedObject.FindProperty("rtype");
            RTIndex = serializedObject.FindProperty("RTIndex");
            RTName = serializedObject.FindProperty("RTName");
            UpdateInterval = serializedObject.FindProperty("UpdateInterval");
            RuntimeSet = serializedObject.FindProperty("RuntimeSet");
            LookAtOnArrival = serializedObject.FindProperty("LookAtOnArrival");
            IgnoreWaitTime = serializedObject.FindProperty("IgnoreWaitTime");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            UnityEditor.EditorGUILayout.PropertyField(Description);
            UnityEditor.EditorGUILayout.PropertyField(MessageID);
            UnityEditor.EditorGUILayout.PropertyField(UpdateInterval);
            UnityEditor.EditorGUILayout.PropertyField(WaitForPreviousTask);
            UnityEditor.EditorGUILayout.Space();

            UnityEditor.EditorGUILayout.PropertyField(patrolType);

            var tt = (PatrolType)patrolType.intValue;

            if (tt == PatrolType.LocalRuntimeSet)
            {
                UnityEditor.EditorGUILayout.HelpBox("Add a [GetRuntimeGameObjects] to the Brain GameObject to get the set from there", UnityEditor.MessageType.Info);
            }

            if (tt != PatrolType.LastWaypoint)
            {
                if (tt == PatrolType.UseRuntimeSet)
                    UnityEditor.EditorGUILayout.PropertyField(RuntimeSet);

                UnityEditor.EditorGUILayout.PropertyField(rtype, new GUIContent("Get"));
                var Sel = (RuntimeSetTypeGameObject)rtype.intValue;
                switch (Sel)
                {
                    case RuntimeSetTypeGameObject.Index:
                        UnityEditor.EditorGUILayout.PropertyField(RTIndex, new GUIContent("Element Index"));
                        break;
                    case RuntimeSetTypeGameObject.ByName:
                        UnityEditor.EditorGUILayout.PropertyField(RTName, new GUIContent("Element Name"));
                        break;
                    default:
                        break;
                }
            }
            UnityEditor.EditorGUILayout.PropertyField(LookAtOnArrival);
            UnityEditor.EditorGUILayout.PropertyField(IgnoreWaitTime);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}