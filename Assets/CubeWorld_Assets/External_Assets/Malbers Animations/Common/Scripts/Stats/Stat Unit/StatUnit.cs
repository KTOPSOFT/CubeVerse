using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MalbersAnimations
{
    /// <summary>  Class used to show the Health in heart styles instead of a var </summary>
    [AddComponentMenu("Malbers/UI/Stat Unit")]
    public class StatUnit : MonoBehaviour
    {
        [Tooltip("Fill Image of the Stat")]
        public Image Full;
        [Tooltip("Background image of the Stat")]
        public Image Background;

        public MonoBehaviour Scaler;

        // public FloatEvent OnStatValueChanged = new();
        // public UnityEvent OnStatEnter = new();

        private Vector3 FullScale;
        private Vector3 BGScale;


        private void Awake()
        {
            FullScale = Full.transform.localScale;
            BGScale = Background.transform.localScale;
        }



        internal void ResetScale()
        {
            Full.transform.localScale = FullScale;
            Background.transform.localScale = BGScale;

        }

        public void SetScaler(bool va)
        {
            if (Scaler != null) { Scaler.enabled = va; }
        }

        public void SetFillValue(float value, float time)
        {
            if (value == 0)
            {
                ResetScale();
                if (Scaler) Scaler.enabled = false;
            }

            if (time == 0)
            {
                Full.fillAmount = value;

                // Debug.Log($"{name} Fill Amount: {value}");
                return;
            }

            StopAllCoroutines(); //Just in case for 
            StartCoroutine(FillValue(value, time));
        }

        IEnumerator FillValue(float newValue, float time)
        {
            float elapsedTime = 0;
            float startValue = Full.fillAmount;

            //if (startValue != newValue) //If the values are different
            {
                while ((time > 0) && (elapsedTime <= time))
                {
                    float result = elapsedTime / time;               //Evaluation of the Pos curve
                    Full.fillAmount = Mathf.Lerp(startValue, newValue, result);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            Full.fillAmount = newValue;

            yield return null;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }
    }
}
