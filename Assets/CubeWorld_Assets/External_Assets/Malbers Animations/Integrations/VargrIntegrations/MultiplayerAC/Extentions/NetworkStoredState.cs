using MalbersAnimations.Events;
using System.Collections.Generic;
using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.MultiplayerExtention
{       
    public struct NetworkStoredState 
    {
            public StateID AnimalState;
            public Vector3 Move_Direction;
            public Vector3 MovementAxisRaw;
            public Vector3 MovementAxis;
            public Vector3 DeltaPos;
            public Vector3 DeltaRootMotion;
            public Vector3 HorizontalVelocity;
            public Transform transform;
            public Vector3 position;
            public Vector3 TargetSpeed;
            public Vector3 Gravity;
            public float GravityTime;
            public float HorizontalSpeed;

            public NetworkStoredState(MAnimal Old)
            {
                AnimalState = Old.ActiveStateID;
                Move_Direction = Old.Move_Direction;
                MovementAxisRaw = Old.MovementAxisRaw;
                MovementAxis = Old.MovementAxis;
                DeltaPos = Old.DeltaPos;
                DeltaRootMotion = Old.DeltaRootMotion;
                position = Old.t.position;
                TargetSpeed = Old.TargetSpeed;
                Gravity = Old.Gravity;
                GravityTime = Old.GravityTime;
                HorizontalSpeed = Old.HorizontalSpeed;
                HorizontalVelocity = Old.HorizontalVelocity;
                transform = Old.transform;
            }

            public void ApplyState(MAnimal New)
            {
                New.OverrideStartState = AnimalState;

                if (AnimalState == StateEnum.Jump || !New.HasState(AnimalState))
                {
                    //Change Jump for Fall or if the new animal does not have the given Old State
                    New.OverrideStartState = MTools.GetInstance<StateID>("Fall");
                }

                New.TeleportRot(transform);

                New.Move_Direction = Move_Direction;
                New.MovementAxisRaw = MovementAxisRaw;
                New.MovementAxis = MovementAxis;
                New.DeltaPos = DeltaPos;
                New.DeltaRootMotion = DeltaRootMotion;
                New.InertiaPositionSpeed = HorizontalVelocity * New.DeltaTime;
                New.t.position = position;

                New.SetMaxMovementSpeed();
                New.TargetSpeed = TargetSpeed;
                New.Gravity = Gravity;
                New.GravityTime = GravityTime;
                New.HorizontalSpeed = HorizontalSpeed;
                New.HorizontalVelocity = HorizontalVelocity;
            }
    }
}