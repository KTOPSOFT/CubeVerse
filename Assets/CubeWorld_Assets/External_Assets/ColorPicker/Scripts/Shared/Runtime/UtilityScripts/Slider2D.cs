using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace AdvancedColorPicker
{
    /// <summary>
    /// A slider that can be moved in both the X and Y direction.
    /// </summary>
    /// <remarks>
    /// Based upon https://bitbucket.org/Unity-Technologies/ui/src/cc791b3335d2d46b70d932fc70e6ec6c4ea6d561/UnityEngine.UI/UI/Core/Slider.cs?at=5.2&fileviewer=file-view-default
    /// Version: 2015-09-08 : cc791b3
    /// It is advised that when the original slider changes, the same changes are done to this class. As we want this class to behave in the same way as a normal slider.
    /// 
    /// Unlike Slider, this component has no fillarea as it is kinda weird to fill a slider in 2 dimentions...
    /// </remarks>
    [RequireComponent(typeof(RectTransform))]
    public class Slider2D : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        [Serializable]
        public class Slider2DEvent : UnityEvent<float, float> { }

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

        [SerializeField]
        private Vector2 m_MinValue = Vector2.zero;
        public Vector2 minValue
        {
            get { return m_MinValue; }
            set
            {
                if (ColorPickerUtils.SetStruct(ref m_MinValue, value))
                {
                    SetX(m_ValueX);
                    SetY(m_ValueY);
                    UpdateVisuals();
                }
            }
        }

        [SerializeField]
        private Vector2 m_MaxValue = Vector2.one;
        public Vector2 maxValue
        {
            get { return m_MaxValue; }
            set
            {
                if (ColorPickerUtils.SetStruct(ref m_MaxValue, value))
                {
                    SetX(m_ValueX);
                    SetY(m_ValueY);
                    UpdateVisuals();
                }
            }
        }

        [SerializeField]
        private bool m_WholeNumbers = false;
        public bool wholeNumbers
        {
            get { return m_WholeNumbers; }
            set
            {
                if (ColorPickerUtils.SetStruct(ref m_WholeNumbers, value))
                {
                    SetX(m_ValueX);
                    SetY(m_ValueY);
                    UpdateVisuals();
                }
            }
        }

        [SerializeField]
        protected float m_ValueX;
        public virtual float valueX
        {
            get
            {
                if (wholeNumbers)
                    return Mathf.Round(m_ValueX);
                return m_ValueX;
            }
            set
            {
                SetX(value);
            }
        }

        public float normalizedValueX
        {
            get
            {
                if (Mathf.Approximately(minValue.x, maxValue.x))
                    return 0;
                return Mathf.InverseLerp(minValue.x, maxValue.x, valueX);
            }
            set
            {
                this.valueX = Mathf.Lerp(minValue.x, maxValue.x, value);
            }
        }

        [SerializeField]
        protected float m_ValueY;
        public float valueY
        {
            get
            {
                if (wholeNumbers)
                    return Mathf.Round(m_ValueY);
                return m_ValueY;
            }
            set
            {
                SetY(value);
            }
        }

        public float normalizedValueY
        {
            get
            {
                if (Mathf.Approximately(minValue.y, maxValue.y))
                    return 0;
                return Mathf.InverseLerp(minValue.y, maxValue.y, valueY);
            }
            set
            {
                this.valueY = Mathf.Lerp(minValue.y, maxValue.y, value);
            }
        }

        [SerializeField]
        protected bool inverseX;
        public bool InverseX
        {
            get
            {
                return inverseX;
            }
            set
            {
                if (ColorPickerUtils.SetStruct(ref inverseX, value))
                {
                    UpdateVisuals();
                }
            }
        }

        [SerializeField]
        protected bool inverseY;
        public bool InverseY
        {
            get
            {
                return inverseY;
            }
            set
            {
                if (ColorPickerUtils.SetStruct(ref inverseY, value))
                {
                    UpdateVisuals();
                }
            }
        }

        // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
        [SerializeField]
        private Slider2DEvent m_OnValueChanged = new Slider2DEvent();
        public Slider2DEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }


        // Private fields

        private Transform m_HandleTransform;
        protected RectTransform m_HandleContainerRect;

        // The offset from handle position to mouse down position
        private Vector2 m_Offset = Vector2.zero;

        private DrivenRectTransformTracker m_Tracker;

        protected Slider2D()
        { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (wholeNumbers)
            {
                m_MinValue = new Vector2(Mathf.Round(m_MinValue.x), Mathf.Round(m_MinValue.y));
                m_MaxValue = new Vector2(Mathf.Round(m_MaxValue.x), Mathf.Round(m_MaxValue.y));
            }

            //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
            if (IsActive())
            {
                UpdateCachedReferences();
                SetX(m_ValueX, false);
                SetY(m_ValueY, false);
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
                onValueChanged.Invoke(valueX, valueY);
#endif
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateCachedReferences();
            SetX(m_ValueX, false);
            SetY(m_ValueY, false);
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
            m_ValueX = ClampValue(m_ValueX, 0);
            m_ValueY = ClampValue(m_ValueY, 1);
            float oldNormalizedValueX = normalizedValueX;
            float oldNormalizedValueY = normalizedValueY;
            if (m_HandleContainerRect != null)
            {
                oldNormalizedValueX = (inverseX ? 1 - m_HandleRect.anchorMin[0] : m_HandleRect.anchorMin[0]);
                oldNormalizedValueY = (inverseY ? 1 - m_HandleRect.anchorMin[1] : m_HandleRect.anchorMin[1]);
            }

            UpdateVisuals();

            if (oldNormalizedValueX != normalizedValueX || oldNormalizedValueY != normalizedValueY)
                onValueChanged.Invoke(m_ValueX, m_ValueY);
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

        protected float ClampValue(float input, int index)
        {
            float newValue = Mathf.Clamp(input, minValue[index], maxValue[index]);
            if (wholeNumbers)
                newValue = Mathf.Round(newValue);
            return newValue;
        }

        // Set the valueUpdate the visible Image.
        private void SetX(float input)
        {
            SetX(input, true);
        }

        protected virtual void SetX(float input, bool sendCallback)
        {
            // Clamp the input
            float newValue = ClampValue(input, 0);

            // If the stepped value doesn't match the last one, it's time to update
            if (m_ValueX == newValue)
                return;

            m_ValueX = newValue;
            UpdateVisuals();
            if (sendCallback)
                m_OnValueChanged.Invoke(newValue, m_ValueY);
        }

        // Set the valueUpdate the visible Image.
        private void SetY(float input)
        {
            SetY(input, true);
        }

        protected virtual void SetY(float input, bool sendCallback)
        {
            // Clamp the input
            float newValue = ClampValue(input, 1);

            // If the stepped value doesn't match the last one, it's time to update
            if (m_ValueY == newValue)
                return;

            m_ValueY = newValue;
            UpdateVisuals();
            if (sendCallback)
                m_OnValueChanged.Invoke(m_ValueX, newValue);
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
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                anchorMin[0] = anchorMax[0] = inverseX ? (1 - normalizedValueX) : normalizedValueX;
                anchorMin[1] = anchorMax[1] = inverseY ? (1 - normalizedValueY) : normalizedValueY;
                m_HandleRect.anchorMin = anchorMin;
                m_HandleRect.anchorMax = anchorMax;
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
                localCursor -= clickRect.rect.position;

                float valX = Mathf.Clamp01((localCursor - m_Offset)[0] / rect.size[0]);
                normalizedValueX = (inverseX ? 1f - valX : valX);
                float valY = Mathf.Clamp01((localCursor - m_Offset)[1] / rect.size[1]);
                normalizedValueY = (inverseY ? 1f - valY : valY);
            }
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

            m_Offset = Vector2.zero;
            if (m_HandleContainerRect != null && RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
            {
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                    m_Offset = localMousePos;
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
                        SetX(inverseX ? valueX + GetStepSize(0) : valueX - GetStepSize(0));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Right:
                    if (FindSelectableOnRight() == null)
                        SetX(inverseX ? valueX - GetStepSize(0) : valueX + GetStepSize(0));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Up:
                    if (FindSelectableOnUp() == null)
                        SetY(inverseY ? valueY - GetStepSize(1) : valueY + GetStepSize(1));
                    else
                        base.OnMove(eventData);
                    break;
                case MoveDirection.Down:
                    if (FindSelectableOnDown() == null)
                        SetY(inverseY ? valueY + GetStepSize(1) : valueY - GetStepSize(1));
                    else
                        base.OnMove(eventData);
                    break;
            }
        }


        // Size of each step.
        private float GetStepSize(int index)
        {
            return wholeNumbers ? 1f : (maxValue[index] - minValue[index]) * 0.1f;
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
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
