using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Variables/Int Listener (Local Int)")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class IntVarListener : VarListener
    {
        public IntReference value;
        public IntEvent Raise = new();

        public virtual int Value
        {
            get => value;
            set
            {
                if (Auto) this.value.Value = value;
                Invoke(value);
            }
        }

        void OnEnable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged += Invoke;
            if (InvokeOnEnable) Invoke();
        }

        void OnDisable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged -= Invoke;
        }

        public virtual void Invoke() => Invoke(Value);
        public virtual void Invoke(int value) { if (Enable) Raise.Invoke(value); }
        public virtual void Invoke(float value) => Invoke((int)value);
        public virtual void Invoke(IDs value) => Invoke(value.ID);
        public virtual void Invoke(bool value) => Invoke(value ? 1 : 0);
        public virtual void SetValue(int value) => Value = value;
        public virtual void SetValue(float value) => Value = (int)value;
        public virtual void SetValue(IDs value) => Value = value.ID;
        public virtual void SetValue(bool value) => Value = value ? 1 : 0;


        #region Math Operations
        public virtual void _Add(IntVar var) => _Add(var.Value);
        public virtual void _Substract(IntVar var) => _Substract(var.Value);
        public virtual void _Multiply(IntVar var) => Value *= var;
        public virtual void _Divide(IntVar var) => Value /= var;
        public virtual void _Add(int var) => Value += var;
        public virtual void _Substract(int var) => Value -= var;
        public virtual void _Multiply(int var) => Value *= var;
        public virtual void _Divide(int var) => Value /= var;
        public virtual void _Add(float var) => _Add((int)var);
        public virtual void _Substract(float var) => _Substract((int)var);
        public virtual void _Multiply(float var) => _Multiply((int)var);
        public virtual void _Divide(float var) => _Divide((int)var);
        #endregion

    }



    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(IntVarListener)), UnityEditor.CanEditMultipleObjects]
    public class IntVarListenerEditor : VarListenerEditor
    {
        private UnityEditor.SerializedProperty Raise;

        void OnEnable()
        {
            base.SetEnable();
            Raise = serializedObject.FindProperty("Raise");
        }

        protected override void DrawElemets() => UnityEditor.EditorGUILayout.PropertyField(Raise);
    }
#endif
}