using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdvancedColorPicker
{
    [RequireComponent(typeof(RectTransform))]
    public class SliderCircle2D : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [Serializable]
        public class SliderCircleEvent : UnityEvent<float, float> { }

        [SerializeField]
        private RectTransform m_HandleRect;
        public RectTransform handleRect
        {
            get { return m_HandleRect; }
            set
            {
                if (ColorPickerUtils.SetClass(ref m_HandleRect, value))
                {
                    UpdateCachedReferences();
                    UpdateVisuals();
                }
            }
        }

        [SerializeField, Range(0, 360)]
        protected float m_Angle;
        public virtual float angle
        {
            get
            {
                return m_Angle;
            }
            set
            {
                SetAngle(value);
            }
        }

        public float normalizedAngle
        {
            get
            {
                return Mathf.InverseLerp(0, 360, m_Angle);
            }
            set
            {
                angle = Mathf.Lerp(0, 360, value);
            }
        }

        [SerializeField, Range(0, 1)]
        protected float m_Distance;
        public virtual float distance
        {
            get
            {
                return m_Distance;
            }
            set
            {
                SetDistance(value);
            }
        }

        [SerializeField]
        private int m_Corners = 30;
        public int Corners
        {
            get
            {
                return m_Corners;
            }
            set
            {
                value = Math.Max(value, 6);

                if (value == m_Corners)
                    return;

                m_Corners = value;
                UpdateVisuals();
            }
        }

        [SerializeField]
        private bool m_InverseAngle;

        public bool InverseAngle
        {
            get
            {
                return m_InverseAngle;
            }
            set
            {
                if (m_InverseAngle == value)
                    return;

                m_InverseAngle = value;
                UpdateVisuals();
            }
        }

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private SliderCircleEvent m_OnValueChanged = new SliderCircleEvent();
        public SliderCircleEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }


        // Private fields

        private Transform m_HandleTransform;
        private RectTransform m_HandleContainerRect;

        private DrivenRectTransformTracker m_Tracker;

        protected SliderCircle2D()
        { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();


            //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
            if (IsActive())
            {
                UpdateCachedReferences();
                SetAngle(m_Angle, false);
                SetDistance(m_Distance, false);
                Corners = m_Corners;
                // Update rects since other things might affect them even if value didn't change.
                UpdateVisuals();
            }

			if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this) && !Application.isPlaying)
				CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		}
#endif // if UNITY_EDITOR

        public virtual void Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged.Invoke(angle, m_Distance);
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            SetAngle(m_Angle, false);
            SetDistance(m_Distance, false);
            // Update rects since they need to be initialized correctly.
            UpdateVisuals();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
            // We also need to ensure the value stays within min/max.
            m_Angle = Mathf.Clamp(m_Angle, 0, 360);
            m_Distance = Mathf.Clamp01(m_Distance);

            Vector2 old = CalculateHandlePositionReversed();

            UpdateVisuals();

            if (old[0] != m_Angle || old[1] != m_Distance)
                onValueChanged.Invoke(m_Angle, m_Distance);
        }

        void UpdateCachedReferences()
        {
            if (m_HandleRect)
            {
                m_HandleTransform = m_HandleRect.transform;
                if (m_HandleTransform.parent != null)
                    m_HandleContainerRect = m_HandleTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                m_HandleContainerRect = null;
            }
        }

        // Set the valueUpdate the visible Image.
        void SetAngle(float input)
        {
            SetAngle(input, true);
        }

        protected virtual void SetAngle(float input, bool sendCallback)
        {
            while (input < 0)
                input += 360;
            while (input > 360)
                input -= 360;

            // If the stepped value doesn't match the last one, it's time to update
            if (m_Angle == input)
                return;

            m_Angle = input;
            UpdateVisuals();
            if (sendCallback)
                m_OnValueChanged.Invoke(input, m_Distance);
        }

        // Set the valueUpdate the visible Image.
        void SetDistance(float input)
        {
            SetDistance(input, true);
        }

        protected virtual void SetDistance(float input, bool sendCallback)
        {
            // Clamp the input
            float newValue = Mathf.Clamp01(input);

            // If the stepped value doesn't match the last one, it's time to update
            if (m_Distance == newValue)
                return;
            m_Distance = newValue;
            UpdateVisuals();
            if (sendCallback)
                m_OnValueChanged.Invoke(angle, newValue);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
            if (!IsActive())
                return;

            UpdateVisuals();
        }

        // Force-update the slider. Useful if you've changed the properties and want it to update visually.
        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            m_Tracker.Clear();

            if (m_HandleContainerRect != null)
            {
                m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
                Vector2 handlePos = CalculateHandlePosition();
                m_HandleRect.anchorMin = handlePos;
                m_HandleRect.anchorMax = handlePos;
            }
        }

        // Update the slider's position based on the mouse.
        protected virtual void UpdateDrag(PointerEventData eventData, Camera cam)
        {
            RectTransform clickRect = m_HandleContainerRect;
            Rect rect = clickRect.rect;
            if (clickRect != null && rect.size[0] > 0 && rect.size[1] > 0)
            {
                Vector2 localCursor;
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out localCursor))
                    return;

                // Normalize cursor position to be independant of aspect ratio
                localCursor.x /= rect.width * 0.5f;
                localCursor.y /= rect.height * 0.5f;

                float angle = Vector2.Angle(new Vector2(0, 1), localCursor);
                angle = localCursor.x < 0 ? angle : 360f - angle;
                SetAngle(m_InverseAngle ? -angle : angle);

                SetDistance(Vector2.Distance(Vector2.zero, localCursor) / GetDistanceDevider(this.angle));
            }
        }

        private float GetDistanceDevider(float angle)
        {
            float t = 360f / m_Corners;
            float t2 = angle / t;
            float tmin = (Mathf.Floor(t2) * t) * Mathf.Deg2Rad;
            float tmax = (Mathf.Ceil(t2) * t) * Mathf.Deg2Rad;
            Vector2 p1 = Quaternion.Euler(0, 0, 90) * new Vector3(Mathf.Cos(tmin), Mathf.Sin(tmin));
            Vector2 p2 = Quaternion.Euler(0, 0, 90) * new Vector3(Mathf.Cos(tmax), Mathf.Sin(tmax));

            return Vector2.Distance(Vector2.zero, Vector2.Lerp(p1, p2, t2 - Mathf.Floor(t2)));

        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;

            base.OnPointerDown(eventData);

            if (m_HandleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
            {
                // Previously, we set an offset here that we dont use??? yay :)
            }
            else
            {
                // Outside the slider handle - jump to this point instead
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
                return;
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                base.OnMove(eventData);
                return;
            }
            switch (eventData.moveDir)
            {
                case MoveDirection.Left:
                    if (FindSelectableOnLeft() == null)
                        SetAngle(InverseAngle ? angle + 36 : angle - 36);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (FindSelectableOnRight() == null)
                        SetAngle(InverseAngle ? angle - 36 : angle + 36);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (FindSelectableOnUp() == null)
                        SetDistance(distance + 0.1f);
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (FindSelectableOnDown() == null)
                        SetDistance(distance - 0.1f);
                    else
                        base.OnMove(eventData);
                    break;
            }
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        private Vector2 CalculateHandlePosition()
        {
            float t = 360f / m_Corners;
            float t2 = this.angle / t;
            float tmin = (Mathf.Floor(t2) * t) * Mathf.Deg2Rad;
            float tmax = (Mathf.Ceil(t2) * t) * Mathf.Deg2Rad;
            if (m_InverseAngle)
            {
                tmin = -tmin;
                tmax = -tmax;
            }
            Vector2 p1 = (Quaternion.Euler(0, 0, 90) * (new Vector3(Mathf.Cos(tmin), Mathf.Sin(tmin)) * distance * 0.5f));
            Vector2 p2 = (Quaternion.Euler(0, 0, 90) * (new Vector3(Mathf.Cos(tmax), Mathf.Sin(tmax)) * distance * 0.5f));

            return Vector2.Lerp(p1, p2, t2 - Mathf.Floor(t2)) + new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Calculates the angle and distance based on the handle position
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Currently the distance not not 100% accurate at every angle, which might result in wrong onValueChanged calls?!?
        /// </remarks>
        private Vector2 CalculateHandlePositionReversed()
        {
            if (m_HandleContainerRect != null)
            {
                Vector2 localPos = m_HandleRect.anchorMin - new Vector2(0.5f, 0.5f);

                float angle = Vector2.Angle(new Vector2(0, 1), localPos);
                angle = localPos.x < 0 ? angle : 360f - angle;
                angle = m_InverseAngle ? -angle : angle;

                float distance = (Vector2.Distance(Vector2.zero, localPos) * 2f) / GetDistanceDevider(angle);
                return new Vector2(angle, distance);
            }
            return new Vector2(m_Angle, m_Distance);
        }

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2_0 || UNITY_5_2_1
        // Nothing to add here
#else // Newest version of UNITY

        public virtual void GraphicUpdateComplete()
        {

        }

        public virtual void LayoutComplete()
        {

        }
#endif
    }
}
