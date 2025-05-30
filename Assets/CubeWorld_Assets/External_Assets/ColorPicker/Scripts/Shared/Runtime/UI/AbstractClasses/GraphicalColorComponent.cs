using UnityEngine;
using UnityEngine.UI;

namespace AdvancedColorPicker
{
    [ExecuteInEditMode]
    public abstract class GraphicalColorComponent : MaskableGraphic
    {
        [SerializeField]
        private ColorPicker picker;

        /// <summary>
        /// Gets or sets the ColorPicker which value(s) will be used by this component
        /// </summary>
        public ColorPicker Picker
        {
            get
            {
                return picker;
            }
            set
            {
                if (picker == value)
                    return;

#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "PickerChanged");
                if (picker != null)
                    UnityEditor.Undo.RecordObject(picker, "PickerChanged");
                if (value != null)
                    UnityEditor.Undo.RecordObject(value, "PickerChanged");
#endif

                if (picker != null)
                    picker.OnColorChanged.RemoveListener(OnColorChanged);

                picker = value;

                if (picker != null)
                {
                    picker.OnColorChanged.AddListener(OnColorChanged);
                }

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                if (picker != null)
                    UnityEditor.EditorUtility.SetDirty(picker);
                if (value != null)
                    UnityEditor.EditorUtility.SetDirty(value);
#endif

                DisplayNewColor();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            DisplayNewColor();
            if (picker != null)
                picker.OnColorChanged.AddListener(OnColorChanged);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (picker != null)
                picker.OnColorChanged.RemoveListener(OnColorChanged);
        }

#if UNITY_EDITOR

        protected override void Awake() // To handle Undo/Redo
        {
            base.Awake();
            UnityEditor.Undo.undoRedoPerformed += DisplayNewColor;
        }

        protected override void OnDestroy() // To handle Undo/Redo
        {
            UnityEditor.Undo.undoRedoPerformed -= DisplayNewColor;
            base.OnDestroy();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            DisplayNewColor();
        }

        protected override void Reset()
        {
            base.Reset();
            DisplayNewColor();
        }
#endif

        private void OnColorChanged(Color color)
        {
            DisplayNewColor();
        }

        protected abstract void DisplayNewColor();
    }
}
