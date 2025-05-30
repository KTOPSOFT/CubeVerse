using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.PCW
{
    public class PathFinding : MonoBehaviour
    {
        #region public variables
        [Header("===================Settings===================")]
        [Range(1F, 5F)]
        [Tooltip("How much distance the character will try to keep from the target.You can set this to a higher value for characters using ranged weapons. For melee weapon, 1~2 meters will get nice result.")]
        public float stoppingDistance = 2F;
        [Tooltip("Set the moving speed (meters/second) of the character")]
        public float movingSpeed = 6F;
        public enum PathFindingState
        {
            Idle,
            Walking,
            Running,
            Jumping,
            Falling
        }
        [Header("===================States===================")]
        [Tooltip("Monitor this state to animate your character.")]
        public PathFindingState movingState; //You can use this state to animate the character.
                                             //Please note, the Jumping state only lasts until you actually jump, once your character jump off ground, the state may switch to Falling.
        [Tooltip("Move your character to this position by your script or mechanim.")]
        public Vector3 desiredPosition;//Move your character to this position by your script or mechanim
        [Tooltip("The next cube position the AI trying to move to.")]
        public Vector3 nextCubePosition;//The next cube the AI trying to move to
        [Tooltip("This will return the terrain speed multiplier where the character standing on.")]
        public float terrainMovingSpeed = 1F;
        [Tooltip("The distance between the character and its moving target.")]
        public float distanceFromTarget = 0F;

        [Header("Set the layermask of ground, walls and anything can block this character.")]
        public LayerMask GroundLayer;
        #endregion

        #region internal variables
        private bool canFindNextCube = true;
        private float stopPositionOffset = 0F;
        private float mDis = 0F;
        private Vector3 mPos;
        private Vector3 targetPos;
        private Vector3 targetDir;
        private Vector3 desiredDir;
        private Transform movingTarget;
        private Vector3 movingTargetPos;
        private Vector3[] Dirs = new Vector3[4]
       {
            new Vector3(1F,0F,0F),
            new Vector3(-1F,0F,0F),
            new Vector3(0F,0F,1F),
            new Vector3(0F,0F,-1F),
       };
        private List<Vector3> _testDir = new List<Vector3>();
        private float _dis;
        private float _lastHeight;
        private float _mHeight;
        private Vector3 _dir;
        private bool _moving = false;
        int frameOffset = 0;
        #endregion 

        #region MonoBehaviour
        void Update()
        {
            Movement();
        }

        #endregion

        #region public functions
        public void SetTargetPosition(Vector3 _position)
        {
            movingTargetPos = _position;
        }

        public void SetTargetTransform(Transform _trans)
        {
            movingTarget = _trans;
        }

        public void CleartTargetTransform()
        {
            movingTarget = null;
        }

        public void ClearTarget()
        {
            CleartTargetTransform();
            movingTargetPos = transform.position;
        }

        public bool hasTarget()
        {
            return (movingTarget!=null);
        }

        public bool isRunning()
        {
            return _moving;
        }

        public void Run()
        {
            _moving = true;
            Reset();
        }

        public void Run(Vector3 _position)
        {
            SetTargetPosition(_position);
            _moving = true;
            Reset();
        }

        public void Run(Transform _trans)
        {
            SetTargetTransform(_trans);
            _moving = true;
            Reset();
        }

        public void Stop(bool _clearTarget=false)
        {
            _moving = false;
            if (_clearTarget) ClearTarget();
            Reset();
        }

        public void Reset()
        {
            mPos = transform.position;
            desiredPosition = transform.position;
            nextCubePosition = transform.position;
            stopPositionOffset = Random.Range(-0.5F,0.5F);
            frameOffset = Random.Range(0,60);
            canFindNextCube = true;
        }
        #endregion

        #region internal functions
        private bool isBlocked(Vector3 _start, Vector3 _path, Vector3 _dir)
        {
            _dis = Mathf.Clamp( Vector3.Dot(_path, _dir),1,50);
            _lastHeight = desiredPosition.y;
            for (int i=0;i< _dis;i++) {
                if (!BlockGenerator.instance.GetSurfaceWalkableByPosition(new Vector3(_start.x + _dir.x, _start.y, _start.z + _dir.z)))
                {
                    return true;
                }
                _mHeight = BlockGenerator.instance.GetSurfaceHeightByPosition(new Vector3(_start.x + _dir.x, _start.y, _start.z + _dir.z));
                if(_mHeight> _lastHeight+1)return true;
                _lastHeight = _mHeight;
            }
            return false;
        }

        public Vector3 FindDir(Vector3 _start, Vector3 _target)
        {
            _dir = _target - _start;
            _dir.y = 0F;
            _testDir.Clear();
            _testDir.AddRange(Dirs);
            _testDir.Sort((p1, p2) => (Vector3.Angle(p1, _dir.normalized)+ Vector3.Angle(p1, desiredDir)*0.5F).CompareTo(Vector3.Angle(p2, _dir.normalized)+Vector3.Angle(p2, desiredDir) * 0.5F));
            for (int i=0;i< _testDir.Count;i++) {
                if (!isBlocked( _start, _dir, _testDir[i]))
                {
                    return _testDir[i];
                }
            }
            return Vector3.zero;
        }

        void Movement()
        {
            if (!_moving)
            {
                movingState = PathFindingState.Idle;
                return;
            }
            if (movingTarget != null) movingTargetPos= movingTarget.position;
            targetDir = movingTargetPos - mPos;
            targetDir.y = 0F;
            targetPos = movingTargetPos - targetDir.normalized* stoppingDistance + Vector3.Cross(-targetDir.normalized, Vector3.up) * stopPositionOffset;
            targetPos.y = mPos.y;
            mDis = Vector3.Distance(mPos,targetPos);
            distanceFromTarget = Vector3.Distance(desiredPosition, movingTargetPos);
            if (mDis > (distanceFromTarget < 1F ? 0.2F : Mathf.Max(1F, stoppingDistance * 0.4F)))
            {
                mPos.y = nextCubePosition.y;
                mPos = Vector3.MoveTowards(mPos, nextCubePosition, Time.deltaTime * Mathf.Max(3F, movingSpeed*1.2F));
                Vector3 _tPos = transform.position;
                _tPos.y = desiredPosition.y;
                if (Vector3.Distance(mPos, nextCubePosition) <= 0F && canFindNextCube)
                {
                    Vector3 _dir = FindDir(mPos, targetPos);
                    nextCubePosition = new Vector3(Mathf.RoundToInt(mPos.x), mPos.y, Mathf.RoundToInt(mPos.z)) + _dir;
                }
                else
                {
                    desiredDir = (nextCubePosition - mPos).normalized;
                }

                if (Physics.Linecast(transform.position + Vector3.up * 0.5F, desiredPosition + Vector3.up * 0.5F, GroundLayer))
                {
                    canFindNextCube = false;
                    mPos = transform.position;
                }
                if (Vector3.Distance(_tPos, desiredPosition) > 2F)
                {
                    canFindNextCube = false;
                }
                
                if (!canFindNextCube && Vector3.Distance(_tPos, desiredPosition) < 0.3F)
                {
                    canFindNextCube = true;
                }
                desiredPosition = new Vector3(mPos.x, BlockGenerator.instance.GetSurfaceHeightByPosition(mPos), mPos.z);
                float _desiredHeight = BlockGenerator.instance.GetSurfaceHeightByPosition(nextCubePosition);
                float _currentHeight= BlockGenerator.instance.GetSurfaceHeightByPosition(transform.position);
                if (transform.position.y < _currentHeight - 0.1F)
                {
                    transform.position = new Vector3(transform.position.x, _currentHeight+0.1F,transform.position.z);
                }

                if (_desiredHeight > transform.position.y + 0.5F) {
                    movingState = PathFindingState.Jumping;
                }
                else if (_currentHeight<transform.position.y-0.5F) {
                    movingState = PathFindingState.Falling;
                }
                else if (mDis < 3F)
                {
                    movingState = PathFindingState.Walking;
                }
                else
                {
                    movingState = PathFindingState.Running;
                }

                //We only call this every 60 frames to save performance, the frame offset is to avoid all characters calling this in the same frame.
                if ((movingState== PathFindingState.Running || movingState == PathFindingState.Walking) && (Time.frameCount+ frameOffset) %60==0) {
                    terrainMovingSpeed = BlockGenerator.instance.GetRunningSpeedByPosition(desiredPosition);
                }
            }
            else
            {
                movingState = PathFindingState.Idle;
            }

            
        }
        #endregion 

    }
}
