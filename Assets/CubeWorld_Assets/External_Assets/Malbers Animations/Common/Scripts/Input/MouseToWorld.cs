using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Input/Mouse World Position")]

    public class MouseToWorld : MonoBehaviour
    {
        [Tooltip("Reference to the camera")]
        public TransformReference MainCamera = new();
        [Tooltip("Reference to the Mouse Point Transform")]
        public TransformReference MousePoint = new();
        [Tooltip("Reference to the Mouse Point Transform")]
        public LayerReference layer = new(-1);
        public QueryTriggerInteraction interaction = QueryTriggerInteraction.UseGlobal;
        public FloatReference MaxDistance = new(100f);

        [Tooltip("If the MousePoint Value is null set the value to this Transform")]
        public BoolReference SetOnNull = new(true);

        //[Space]
        //public bool Snap = true;
        //[Tooltip("Reference to the Mouse Point Transform")]
        //public LayerReference Snaplayer = new LayerReference(0);
        //public Tag[] tags;

        private Camera m_camera;

        private void Start()
        {
            if (MainCamera.Value == null)
            {
                m_camera = MTools.FindMainCamera();

                if (m_camera)
                {
                    MainCamera = m_camera.transform;
                }
                else
                {
                    Debug.LogWarning("There's no Main Camera on the Scene");
                    enabled = false;
                }
            }
            else
            {
                if (!MainCamera.Value.TryGetComponent(out m_camera))
                {
                    Debug.LogWarning("There's no Main Camera on the Scene");
                    enabled = false;
                }
            }

            if (MousePoint.Value == null) MousePoint.Value = transform;

        }


        private void Update()
        {
            if (SetOnNull.Value && MousePoint.Value == null) MousePoint.Value = transform; //If is null use itself 
            else if (MousePoint.Value != transform) return;

            //#if ENABLE_INPUT_SYSTEM
            //            var mousePosition = UnityEngine.InputSystem.Mouse.current.position;
            //#else
            //            var mousePosition = Input.mousePosition;
            //#endif

            var mousePosition = Input.mousePosition;

            Ray ray = m_camera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxDistance, layer, interaction))
            {
                if (MousePoint.Value == null)
                {
                    MousePoint.Value = transform; //ReCheck that the Mouse Point is Never Null
                }

                MousePoint.Value.position = hit.point; //Only Update the Point if the Mouse Point is This Transform


                MDebug.DrawWireSphere(hit.point, Quaternion.identity, Color.red, 0.02f);

            }
        }


        public Transform HitTransform { get; set; }
        public Vector3 TransformCenter { get; set; }

        private void Reset()
        {
            MousePoint.Value = transform;
        }
    }
}