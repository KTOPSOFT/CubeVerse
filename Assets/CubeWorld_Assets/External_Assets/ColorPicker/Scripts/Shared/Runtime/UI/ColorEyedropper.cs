using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections;

namespace AdvancedColorPicker
{
    public class ColorEyedropper : ColorComponent
    {
        [Serializable]
        public class ActiveChanged : UnityEvent { }

        public ActiveChanged OnActivated = new ActiveChanged();

        public ActiveChanged OnDeactivated = new ActiveChanged();

        [SerializeField]
        private bool changesColorInstantly = false;

        private bool activated = false;

        private Coroutine coroutine;

        public bool Activated
        {
            get
            {
                return activated;
            }
            set
            {
                if (value == activated)
                    return;

                activated = value;

                if (activated)
                    InputBlocker.CreateBlocker(SelectColor);

                if (activated)
                    OnActivated.Invoke();
                else
                    OnDeactivated.Invoke();
            }
        }

        public bool ChangesColorInstantly
        {
            get
            {
                return changesColorInstantly;
            }
            set
            {
                if (changesColorInstantly == value)
                    return;

                changesColorInstantly = value;
            }
        }

        private Color cachedColor;
        private Texture2D cachedImage;

        protected override void Awake()
        {
            base.Awake();
            cachedImage = new Texture2D(1, 1, TextureFormat.RGB24, false);
            cachedImage.hideFlags = HideFlags.HideAndDontSave;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            coroutine = StartCoroutine(ReadScreenColor());
        }

        protected override void OnDisable()
        {
            StopCoroutine(coroutine);
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyImmediate(cachedImage);
        }

        /// <summary>
        /// Activates the Eyedropper. A shortcut for setting Activated to true
        /// </summary>
        public void Activate()
        {
            Activated = true;
        }

        private void SelectColor()
        {
            Picker.CurrentColor = cachedColor;
            Activated = false;
        }

        private IEnumerator ReadScreenColor()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (activated)
                {
                    Vector2 mousePosition = Input.mousePosition;
                    if (mousePosition.x >= 0 && mousePosition.y >= 0 && mousePosition.x < Screen.width && mousePosition.y < Screen.height)
                    {
                        cachedImage.ReadPixels(new Rect(mousePosition, Vector2.one), 0, 0);
                        cachedImage.Apply();
                        cachedColor = cachedImage.GetPixel(0, 0);
                        if (changesColorInstantly)
                            Picker.CurrentColor = cachedColor;
                    }
                }
            }
        }

        protected override void DisplayNewColor()
        {
            // We don't display a color, we only edit it
        }
    }
}