using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Variables/Float Listener (Local Float)")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class FloatVarListener : VarListener, IAnimatorCurve
    {
        public FloatReference value;
        public FloatEvent Raise = new();

        public virtual float Value
        {
            get => value;
            set
            {
                this.value.Value = value;
                if (Auto) Invoke(value);
            }
        }

        void OnEnable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged += Invoke;
            if (InvokeOnEnable) Raise.Invoke(value);
        }

        void OnDisable()
        {
            if (value.Variable != null && Auto) value.Variable.OnValueChanged -= Invoke;
        }

        public virtual void Invoke(float value) { if (Enable) Raise.Invoke(value); }
        public virtual void Invoke(int value) => Invoke((float)value);
        public virtual void Invoke(IDs value) => Invoke((float)value.ID);
        public virtual void Invoke(IntVar value) => Invoke((float)value.Value);
        public virtual void Invoke(FloatVar value) => Invoke(value.Value);
        public virtual void Invoke(bool value) => Invoke((float)(value ? 1 : 0));
        public virtual void Invoke() => Invoke(Value);

        public virtual void SetValue(int value) => Value = value;
        public virtual void SetValue(float value) => Value = value;
        public virtual void SetValue(IDs value) => Value = value.ID;
        public virtual void SetValue(IntVar value) => Value = value.Value;
        public virtual void SetValue(FloatVar value) => Value = value.Value;
        public virtual void SetValue(bool value) => Value = value ? 1 : 0;

        public virtual void SetValueVectorX(Vector3 value) => Value = value.x;
        public virtual void SetValueVectorY(Vector3 value) => Value = value.y;
        public virtual void SetValueVectorZ(Vector3 value) => Value = value.z;


        #region Math Operations
        public virtual void _Add(IntVar var) => _Add(var.Value);
        public virtual void _Substract(IntVar var) => _Substract(var.Value);
        public virtual void _Multiply(IntVar var) => Value *= var;
        public virtual void _Divide(IntVar var) => Value /= var;

        public virtual void _Add(FloatVar var) => _Add(var.Value);
        public virtual void _Substract(FloatVar var) => _Substract(var.Value);
        public virtual void _Multiply(FloatVar var) => Value *= var;
        public virtual void _Divide(FloatVar var) => Value /= var;

        public virtual void _Add(float var) => Value += var;
        public virtual void _Substract(float var) => Value -= var;
        public virtual void _Multiply(float var) => Value *= var;
        public virtual void _Divide(float var) => Value /= var;
        public virtual void _Add(int var) => _Add(var);
        public virtual void _Substract(int var) => _Substract((float)var);
        public virtual void _Multiply(int var) => _Multiply((float)var);
        public virtual void _Divide(int var) => _Divide((float)var);
        #endregion



        /// <summary> Set the Value to Zero in x Seconds </summary>
        public virtual void Time_ValueToZero(float time)
        {
            if (Value == 0) return;

            StopAllCoroutines();

            StartCoroutine(I_FloatInTime(Value, 0, time));
        }


        /// <summary> Set the Value to Zero in x Seconds </summary>
        public virtual void Time_ZeroToValue(float time)
        {
            if (Value == 0) return;

            StopAllCoroutines();

            StartCoroutine(I_FloatInTime(0, Value, time));
        }


        /// <summary> Set the Value to Zero in x Seconds </summary>
        public virtual void Time_ValueToZero_FixedUpdate(float time)
        {
            if (Value == 0) return;

            StopAllCoroutines();

            StartCoroutine(I_FloatInTime_FixedUpdate(Value, 0, time));
        }


        /// <summary> Set the Value to Zero in x Seconds </summary>
        public virtual void Time_ZeroToValue_FixedUpdate(float time)
        {
            if (Value == 0) return;

            StopAllCoroutines();

            StartCoroutine(I_FloatInTime_FixedUpdate(0, Value, time));
        }


        IEnumerator I_FloatInTime(float start, float end, float time)
        {

            float currentTime = 0;

            while (currentTime <= time)
            {
                Value = Mathf.Lerp(start, end, currentTime / time);

                Debug.Log("Value = " + Value);


                currentTime += Time.deltaTime;

                yield return null;
            }

            Value = end;
            yield return null;
        }


        IEnumerator I_FloatInTime_FixedUpdate(float start, float end, float time)
        {
            var wait = new WaitForFixedUpdate();

            float currentTime = 0;

            while (currentTime <= time)
            {
                Value = Mathf.Lerp(start, end, currentTime / time);
                currentTime += Time.fixedDeltaTime;

                yield return wait;
            }

            Value = end;
            yield return null;
        }

        public void AnimatorCurve(int ID, float value)
        {
            if (this.ID == ID) SetValue(value);
        }
    }



    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FloatVarListener)), UnityEditor.CanEditMultipleObjects]
    public class FloatVarListenerEditor : VarListenerEditor
    {
        private UnityEditor.SerializedProperty Raise;

        private void OnEnable()
        {
            base.SetEnable();
            Raise = serializedObject.FindProperty("Raise");
        }

        protected override void DrawElemets()
        {
            UnityEditor.EditorGUILayout.PropertyField(Raise);
            base.DrawElemets();
        }
    }
#endif
}