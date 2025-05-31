using UnityEngine;
using UnityEngine.Events;

namespace MalbersAnimations
{
    /// <summary>Interface used for Moving the Animal using AI</summary>
    public interface IAIControl
    {
        /// <summary>Gets the Agent Transform Reference</summary>  
        Transform Transform { get; }

        /// <summary>Gets the Character Owner Transform Reference</summary>  
        Transform Owner { get; }

        /// <summary>Index used for Multiple AI targeting a unique target</summary>  
        int Index { get; set; }

        /// <summary>Target Assigned to the AI Control</summary>  
        Transform Target { get; set; }

        /// <summary>Target Assigned to the AI Control</summary>  
        Transform NextTarget { get; set; }

        /// <summary>Destination Position to use on Agent.SetDestination()</summary>
        Vector3 DestinationPosition { get; set; }

        /// <summary>Direction the Animal is Travelling</summary>
        Vector3 AIDirection { get; set; }

        /// <summary>Is the Target an AI Target</summary>
        IAITarget IsAITarget { get; set; }

        /// <summary>Returns the Current Target Position.</summary>
        Vector3 GetTargetPosition();

        /// <summary>Current Stopping Distance for the Current Destination</summary>
        float StoppingDistance { get; set; }

        /// <summary>Reset the Stopping Distance to its Default value</summary>
        void ResetStoppingDistance();

        /// <summary>Local Additive Stopping distance added to the current Stop Distance</summary>
        float AdditiveStopDistance { get; set; }

        /// <summary>Current Slowing Distance for the Destination</summary>
        float CurrentSlowingDistance { get; set; }
        float SlowingDistance { get; }

        /// <summary>Returns the Height of the AI Agent</summary>
        float Height { get; }

        /// <summary>Stores the Remainin distance to the Target's Position</summary>
        float RemainingDistance { get; set; }

        /// <summary>Set the next Target, and set if the Agent will move or not to that target</summary>  
        void SetTarget(Transform target, bool move);

        /// <summary>Set the next Target</summary>  
        void SetNextTarget(GameObject next);

        /// <summary>Remove the current Target and stop the Agent </summary>
        void ClearTarget();

        /// <summary>Calculate the Next Target from the Current Target; if the current target is a Waypoint</summary>
        void MovetoNextTarget();

        /// <summary>Set the next Destination Position without having a target, and set if the Agent will move or not to that destination</summary>   
        void SetDestination(Vector3 PositionTarget, bool move);

        /// <summary> Stop the Agent on the Animal... also remove the Transform target and the Target Position and Stops the Animal </summary>
        void Stop();

        /// <summary>If the Animal was waiting for a Next Target, it will stop the Wait Logic</summary>
        void StopWait();

        /// <summary>Calculates the Path and the Current Direction the Animal must go</summary>
        void Move();

        /// <summary>Enable/Disable the AI Source Control</summary>
        void SetActive(bool value);

        /// <summary>Has the Agent Arrived to the Target Position?</summary>
        bool HasArrived { get; set; }

        /// <summary>The Agent has a Target but is waiting because other AI are targeting the same target and the limits is full</summary>
        bool IsWaitingOnTarget { get; set; }

        /// <summary>The Animal is moving</summary>
        bool IsMoving { get; }

        /// <summary>Is the Agent in a OffMesh Link</summary>       
        bool InOffMeshLink { get; set; }

        /// <summary>Do the necesary logic to complete all the Off mesh link traversal</summary>
        void CompleteOffMeshLink();

        /// <summary>Is the target moving, changed position?</summary>
        bool TargetIsMoving { get; }

        /// <summary>The Character will assign and go to a new Target (Given by the current Target) when it arrives to the current target</summary>
        bool AutoNextTarget { get; set; }

        /// <summary>The Ai Control has not started yet. </summary>
        bool AIReady { get; }

        /// <summary>is the AI Enabled/Active?</summary>
        bool Active { get; }

        /// <summary>The Character will assign and go to a new Target (Given by the current Target) when it arrives to the current target</summary>
        bool LookAtTargetOnArrival { get; set; }

        /// <summary>Recalculate the Targets Destination, in case the target moved</summary>
        bool UpdateDestinationPosition { get; set; }

        /// <summary>Event to send when a new Target is assigned</summary>  
        Events.TransformEvent TargetSet { get; }

        /// <summary>Event to send when  the AI has arrived to the target</summary>  
        Events.TransformEvent OnArrived { get; }
    }

    /// <summary>Interface used to know if Target used on the AI Movement is and AI Target</summary>
    public interface IAITarget
    {
        /// <summary> Reference for the Target's Transform</summary>
        Transform transform { get; }

        /// <summary> Stopping Distance Radius used for the AI</summary>
        float StopDistance();

        /// <summary> Default Height for the ahi Target</summary>
        float Height { get; }

        /// <summary> When the AI animal arrives to the target, do we align it so it look ats at it?</summary>
        bool ArriveLookAt { get; }

        /// <summary>Distance for AI driven animals to start slowing its speed before arriving to a gameObject. If its set to zero or lesser than the Stopping distance, the Slowing Movement Logic will be ignored</summary>
        float SlowDistance();

        /// <summary>Returns the AI Destination on an AI Target</summary>
        Vector3 GetCenterPosition(int index);




        /// <summary>Returns the AI Destination on an AI Target</summary>
        Vector3 GetCenterPosition();

        /// <summary>Returns the AI Destination + the Height</summary>
        Vector3 GetCenterY();

        /// <summary>Where is the Target Located, Ground, Water, Air? </summary>
        WayPointType TargetType { get; }

        /// <summary>Call this method when someones arrives to the Waypoint</summary>
        void TargetArrived(GameObject target);
    }


    /// <summary> Allows the Target to have a better distribution when AI characters targeting this gameObject  </summary>
    public interface IAITargeterTarget : IAITarget
    {
        /// <summary> Stopping Distance when multiple targets are added to the AI Target</summary>
        float TargeterStopDistance { get; }

        /// <summary>Ammount of AI that have this object  as their target</summary>
        int Targeters { get; }

        /// <summary>Is the AI Target using Targeters (if the value is greater than 0)</summary>
        int TargetsLimits { get; }

        /// <summary>True if the AI Target has reached the max targeters it can have</summary>
        bool FullTargeters { get; }

        /// <summary> Stopping Distance when Limit of target has been reached and the AI has to wait in a safe distance</summary>
        float WaitTargeterDistance { get; }

        UnityEvent TargetersRefresh { get; set; }

        /// <summary>Get the Stopping Disatance from an AI Target using the Targeter Index</summary>
        float GetTargeterStoppingDistance(int index);

        /// <summary>Return if the Targeter is off the limits so it should be waiting</summary>
        bool TargeterIsWaiting(int index);

        void AddTargeter(IAIControl ai);
        void RemoveTargeter(IAIControl ai);
    }

}


