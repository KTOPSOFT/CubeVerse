using MalbersAnimations.Weapons;
using UnityEngine;
using MalbersAnimations.Scriptables;
using MalbersAnimations.Events;
using UnityEngine.Events;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class SyncedProjectile : MProjectile
    {    
        private int m_CatchupSteps = 0;
        
        public int Seed
        {
            get;
            protected set;
        }

        public virtual void PrepareSync(int stepsMissed, int seed = 0)
        {
            Debug.Log("SYNC TIMER: "+stepsMissed);
            if(stepsMissed > 0) m_CatchupSteps = stepsMissed;
            Seed = seed;
        }

        protected override IEnumerator FlyingProjectile()
        {
            Vector3 start = transform.position;
            Prev_pos = start;
            float deltatime = Time.fixedDeltaTime;
            WaitForFixedUpdate waitForFixedUpdate = new();

            Direction = Velocity.normalized; //Start the

            int step = 1;

            Vector3 RotationAround = Vector3.zero;
            if (rotation == ProjectileRotation.Random)
                RotationAround = new Vector3(Random.value, Random.value, Random.value).normalized;
            else if (rotation == ProjectileRotation.Axis)
                RotationAround = torqueAxis.normalized;

            float TraveledDistance = 0;
            int NoGravityStep = 0;

            while (!HasImpacted && enabled)
            {                   
                var time = deltatime * (step + (m_CatchupSteps > 0 ? 1 : 0));
                var Gravitytime = deltatime * ((step + (m_CatchupSteps > 0 ? 1 : 0)) - NoGravityStep);

                Vector3 next_pos = (start + Velocity * time) + (Gravitytime * Gravitytime * Gravity / 2);

                if (!rb)
                {
                    transform.position = Prev_pos; //If there's no Rigid body move the Projectile!!

                    if (rotation == ProjectileRotation.Random || rotation == ProjectileRotation.Axis)
                    {
                        transform.Rotate(RotationAround, torque * deltatime, Space.World);
                    }
                }
                else
                {
                    // rb.velocity = Direction;
                    rb.MovePosition(Prev_pos);
                }

                Direction = (next_pos - Prev_pos);



                Debug.DrawLine(Prev_pos, next_pos, Color.yellow);
                if (Radius > 0)
                {
                    MDebug.DrawWireSphere(Prev_pos, Color.yellow, Radius);
                    MDebug.DrawWireSphere(next_pos, Color.yellow, Radius);
                }

                //RaycastHit hit;

                var Length = Vector3.Distance(next_pos, Prev_pos);
                //if ( Physics.Linecast(Prev_pos, next_pos,  out RaycastHit hit,  Layer, triggerInteraction))
                if (Physics.SphereCast(Prev_pos, Radius, Direction, out RaycastHit hit, Length, Layer, triggerInteraction))
                {
                    if (!IsInvalid(hit.collider))
                    {
                        yield return waitForFixedUpdate;
                        ProjectileImpact(hit.rigidbody, hit.collider, hit.point, hit.normal);
                        yield break;
                    }
                }

                if (FollowTrajectory) //The Projectile will rotate towards de Direction
                {
                    transform.rotation = Quaternion.LookRotation(Direction, transform.up);

                    //Rotate around an axis while following a trajectory
                    if (TrajectoryRoll != 0)
                        transform.Rotate(Direction, TrajectoryRoll * deltatime, Space.World);
                }


                //Check if the gravity can be applied after distance
                if (TraveledDistance < AfterDistance)
                {
                    TraveledDistance += Direction.magnitude;
                    NoGravityStep++;
                }



                Prev_pos = next_pos;
                step++;

                if(m_CatchupSteps > 0){
                    step++;
                    m_CatchupSteps--;
                }

                yield return waitForFixedUpdate;
            }

            Debug.Log("exit one");
            yield return null;
        }
    }
    #if UNITY_EDITOR
    [CustomEditor(typeof(SyncedProjectile))]
    public class SynceProjectileEditor : MProjectileEditor
    {
    }
    #endif
}