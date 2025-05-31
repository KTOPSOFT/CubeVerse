using UnityEngine;
using MalbersAnimations.Events;
using UnityEngine.Events;
using MalbersAnimations.VargrMultiplayer;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MalbersAnimations.Utilities
{
    [DefaultExecutionOrder(10000)]
    [AddComponentMenu("Malbers/Multiplayer/Aiming/NetAim")]
    public class NetAim : Aim
    {
        protected bool isController => m_aimSync != null ? m_aimSync.isController : false;
        protected bool isPlayerOwned => m_aimSync == null ? false : (m_aimSync is PlayerInstance playerInstance && playerInstance.isPlayerOwned);
        private IAimSyncer m_aimSync;
        //protected Vector3 m_networkAimDirection;
        //public Vector3Event RawDirectionUpdate = new Vector3Event();

        [SerializeField] protected TransformEvent m_PlayerOnAimRayTarget = new();
        [SerializeField] protected Vector3Event m_PlayerOnScreenCenter = new();
        [SerializeField] protected IntEvent m_PlayerOnAimSide = new();
        [SerializeField] protected BoolEvent m_PlayerOnAiming = new();
        [SerializeField] protected BoolEvent m_PlayerOnUsingTarget = new();
        [SerializeField] protected TransformEvent m_PlayerOnHit = new();

        [SerializeField] protected TransformEvent m_PlayerOnSetTarget = new();
        [SerializeField] protected UnityEvent m_PlayerOnClearTarget = new();

        protected override void Awake()
        {
            m_aimSync = GetComponent<IAimSyncer>();
            UseCamera = false;
            base.Awake();
            
            OnAimRayTarget.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnAimRayTarget.Invoke(data); });
            OnScreenCenter.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnScreenCenter.Invoke(data); });
            OnAimSide.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnAimSide.Invoke(data); });
            OnAiming.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnAiming.Invoke(data); });
            OnUsingTarget.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnUsingTarget.Invoke(data); });
            OnHit.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnHit.Invoke(data); });

            OnSetTarget.AddListener((data) => { if(isController && isPlayerOwned) m_PlayerOnSetTarget.Invoke(data); });
            OnClearTarget.AddListener(() => { if(isController && isPlayerOwned) m_PlayerOnClearTarget.Invoke(); });

            m_aimSync.AimDirection = AimOrigin.forward;
        }

        public override void EnterAim()
        {
            if(!isController) return;

            base.EnterAim();
        }

        public override void ExitAim()
        {
            if(!isController) return;

            base.ExitAim();
        }

        public override void AimLogic(bool useRaycasting)
        {
            if (!isController)
            {
                AimHit = DirectionFromNetwork(useRaycasting);
                RawPoint = AimHit.point;
            
                if (useRaycasting) //Invoke the OnHit Option
                {
                    if (AimHitTransform != AimHit.transform)
                    {
                        AimHitTransform = AimHit.transform;
                        OnHit.Invoke(AimHitTransform);
                        // if (debug) Debug.Log("AimHitTransform = " + AimHitTransform);
                    }
                }
            } else {
                base.AimLogic(useRaycasting);
                m_aimSync.AimDirection = RawAimDirection;
                //RawDirectionUpdate.Invoke(RawAimDirection);
            }
        }

        public RaycastHit DirectionFromNetwork(bool UseRaycasting)
        {
            RawAimDirection = m_aimSync.AimDirection;

            Ray ray = new Ray(AimOrigin.position, RawAimDirection);

            var hit = new RaycastHit()
            {
                distance = MaxDistance,
                point = ray.GetPoint(100)
            };

            return CalculateRayCasting(UseRaycasting, ray, ref hit);
        }
    }
    #region Inspector


#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(NetAim))]
    public class NetAimEditor : AimEditor
    {
        
        private SerializedProperty
            EditorTab1,
            NetOnAimRayTarget, NetOnScreenCenter, NetOnAimSide, NetOnAiming, NetOnUsingTarget, NetOnHit, NetOnSetTarget, NetOnClearTarget
        ;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            NetOnAimRayTarget = serializedObject.FindProperty("m_PlayerOnAimRayTarget");
            NetOnScreenCenter = serializedObject.FindProperty("m_PlayerOnScreenCenter");
            NetOnAimSide = serializedObject.FindProperty("m_PlayerOnAimSide");
            NetOnAiming = serializedObject.FindProperty("m_PlayerOnAiming");
            NetOnUsingTarget = serializedObject.FindProperty("m_PlayerOnUsingTarget");
            NetOnHit = serializedObject.FindProperty("m_PlayerOnHit");
            NetOnSetTarget = serializedObject.FindProperty("m_PlayerOnSetTarget");
            NetOnClearTarget = serializedObject.FindProperty("m_PlayerOnClearTarget");
            EditorTab1 = serializedObject.FindProperty("EditorTab1"); 

            int Selection = EditorTab1.intValue;

            if (Selection == 2) DrawPlayerEvents();

            serializedObject.ApplyModifiedProperties();            
        }

        protected virtual void DrawPlayerEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MalbersEditor.DrawHeader("Player Events");
                EditorGUILayout.PropertyField(NetOnAiming);
                EditorGUILayout.PropertyField(NetOnHit);
                EditorGUILayout.PropertyField(NetOnAimRayTarget);
                EditorGUILayout.PropertyField(NetOnUsingTarget);
                EditorGUILayout.PropertyField(NetOnClearTarget);
                EditorGUILayout.PropertyField(NetOnSetTarget);
                EditorGUILayout.PropertyField(NetOnScreenCenter);
                EditorGUILayout.PropertyField(NetOnAimSide);
            }
        }
    }
#endif
    #endregion
}