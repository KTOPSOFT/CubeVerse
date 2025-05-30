using UnityEngine;
using System.Collections;

namespace AdvancedColorPicker
{
    [AddComponentMenu("ColorPicker/PreviewRenderer")]
    [RequireComponent(typeof(MeshRenderer)), ExecuteInEditMode]
    public class PreviewRenderer : ColorComponent
    {
        private MeshRenderer _renderer;

        [SerializeField]
        private Material _material;
        private Material _materialInstance;

        public Material material
        {
            get
            {
                return _material;
            }
            set
            {
                _material = value;
            }
        }

        protected override void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();

            base.Awake();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_materialInstance != null)
                DestroyImmediate(_materialInstance);
        }

        protected override void DisplayNewColor()
        {
            if (!isActiveAndEnabled || _material == null)
                return;

            if (_materialInstance != null)
            {
#if UNITY_EDITOR
                Material previous = _materialInstance;
                UnityEditor.EditorApplication.delayCall += () => { DestroyImmediate(previous); }; // Delay destruction to prevent null references from the material inspector
#else
                DestroyImmediate(_materialInstance);
#endif
            }

            _materialInstance = new Material(_material);
            _materialInstance.name += " (Instance)";
            _materialInstance.hideFlags = HideFlags.DontSave;
            _materialInstance.color = Picker != null ? Picker.CurrentColor : Color.black;
            _renderer.sharedMaterial = _materialInstance;
        }
    }
}