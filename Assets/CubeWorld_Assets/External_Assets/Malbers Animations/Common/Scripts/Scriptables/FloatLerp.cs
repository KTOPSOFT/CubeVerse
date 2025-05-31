using UnityEngine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using static MalbersAnimations.Utilities.FloatLerp;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    /// <summary>Converts a value to string </summary>
    [AddComponentMenu("Malbers/UI/Float Lerp")]
    public class FloatLerp : MonoBehaviour
    {
        public enum LerpType { Lerp, MoveTowards, SmoothDamp, Curve }

        [Tooltip("Target value we want to achieve")]
        public FloatReference TargetValue = new();
        [Tooltip("Current float value")]
        public FloatReference CurrentValue = new();

        [Tooltip("Type of lerping to use")]
        public LerpType lerpType = LerpType.Lerp;

        [Tooltip("Lerp Value to use for the smoothness")]
        [Min(0.01f)] public float Lerp = (5);
        [Min(0.001f)] public float Threshhold = (0.001f);
        public AnimationCurve curve = new(MTools.DefaultCurve);
        [Min(0.01f)] public float curveTime = 0.5f;
        [Tooltip("Delay the Lerp for this amount of seconds")]
        public float delay = (0.2f);


        [Tooltip("Lerp only when the Target Value is lower than the current value")]
        public bool DecreasingOnly = false;

        IEnumerator ILerp;


        public FloatEvent OnValueLerped = new();

        private WaitForSeconds delayWait;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void SetValue_NoLerp(float value)
        {
            TargetValue = value;
            CurrentValue = value;
            OnValueLerped.Invoke(value);
        }

        public void SetValue(float value)
        {
            TargetValue = value;

            //Ignore when the value is Set to Decreasing only and the value is UP
            if (DecreasingOnly && TargetValue > CurrentValue)
            {
                CurrentValue = value;
                OnValueLerped.Invoke(value);
                return;
            }


            if (ILerp != null) StopCoroutine(ILerp);
            ILerp = null;

            delayWait ??= new WaitForSeconds(delay);


            switch (lerpType)
            {
                case LerpType.Lerp: ILerp = C_NormalLerp(); break;
                case LerpType.MoveTowards: ILerp = C_MoveTowards(); break;
                case LerpType.SmoothDamp: ILerp = C_SmoothDamp(); break;
                case LerpType.Curve: ILerp = C_Curve(); break;

                default:
                    break;
            }
            StartCoroutine(ILerp);
        }


        public IEnumerator C_NormalLerp()
        {
            yield return delayWait;

            while (Mathf.Abs(CurrentValue - TargetValue) > Threshhold)
            {
                CurrentValue = Mathf.Lerp(CurrentValue, TargetValue, Time.deltaTime * Lerp);

                OnValueLerped.Invoke(CurrentValue);
                yield return null;
            }
            CurrentValue = TargetValue;
            OnValueLerped.Invoke(CurrentValue);
        }

        public IEnumerator C_MoveTowards()
        {
            yield return delayWait;


            while (Mathf.Abs(CurrentValue - TargetValue) > Threshhold)
            {
                CurrentValue = Mathf.MoveTowards(CurrentValue, TargetValue, Time.deltaTime * Lerp);
                OnValueLerped.Invoke(CurrentValue);
                yield return null;
            }
            CurrentValue = TargetValue;
            OnValueLerped.Invoke(CurrentValue);
        }

        public IEnumerator C_SmoothDamp()
        {
            yield return delayWait;


            float r = 0;
            while (Mathf.Abs(CurrentValue - TargetValue) > Threshhold)
            {
                CurrentValue = Mathf.SmoothDamp(CurrentValue, TargetValue, ref r, Time.deltaTime * Lerp);
                OnValueLerped.Invoke(CurrentValue);
                yield return null;
            }
            CurrentValue = TargetValue;
            OnValueLerped.Invoke(CurrentValue);
        }

        public IEnumerator C_Curve()
        {
            yield return delayWait;

            float elapsedTime = 0;

            float StartValue = CurrentValue;

            while (elapsedTime < curveTime)
            {
                CurrentValue = Mathf.LerpUnclamped(StartValue, TargetValue, curve.Evaluate(elapsedTime / curveTime));
                elapsedTime += Time.deltaTime;
                OnValueLerped.Invoke(CurrentValue);
                yield return null;
            }

            CurrentValue = TargetValue;
            OnValueLerped.Invoke(CurrentValue);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(FloatLerp))]
    public class FloatLerpEditor : VarListenerEditor
    {
        SerializedProperty TargetValue, CurrentValue, lerpType, Lerp, Threshhold, curve, curveTime, delay, OnValueLerped, DecreasingOnly;


        private void OnEnable()
        {
            TargetValue = serializedObject.FindProperty("TargetValue");
            CurrentValue = serializedObject.FindProperty("CurrentValue");
            lerpType = serializedObject.FindProperty("lerpType");
            Lerp = serializedObject.FindProperty("Lerp");
            Threshhold = serializedObject.FindProperty("Threshhold");
            curve = serializedObject.FindProperty("curve");
            curveTime = serializedObject.FindProperty("curveTime");
            delay = serializedObject.FindProperty("delay");
            OnValueLerped = serializedObject.FindProperty("OnValueLerped");
            DecreasingOnly = serializedObject.FindProperty("DecreasingOnly");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(TargetValue);
            EditorGUILayout.PropertyField(CurrentValue);
            EditorGUILayout.PropertyField(lerpType);

            switch ((LerpType)lerpType.intValue)
            {
                case LerpType.Curve:
                    EditorGUILayout.PropertyField(curve);
                    EditorGUILayout.PropertyField(curveTime);
                    break;
                default:
                    EditorGUILayout.PropertyField(Lerp);
                    EditorGUILayout.PropertyField(Threshhold);
                    break;
            }

            EditorGUILayout.PropertyField(delay);
            EditorGUILayout.PropertyField(DecreasingOnly);

                
            EditorGUILayout.PropertyField(OnValueLerped);
            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}