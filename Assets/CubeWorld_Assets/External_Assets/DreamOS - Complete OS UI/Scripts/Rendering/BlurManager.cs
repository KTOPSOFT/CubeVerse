using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Michsky.DreamOS
{
    [ExecuteInEditMode]
    public class BlurManager : MonoBehaviour
    {
        [Header("Resources")]
        public Material blurMaterial;

        [Header("Settings")]
        [Range(0.0f, 10)] public float blurValue = 5.0f;
        [Range(0.1f, 50)] public float animationSpeed = 25;
        public string customProperty = "_Size";

        float currentBlurValue;

        void Awake()
        {
            if (GraphicsSettings.defaultRenderPipeline != null || blurMaterial == null)
                return;

            if (customProperty == null)
                customProperty = "_Size";

            blurMaterial.SetFloat(customProperty, 0);
        }

        public void BlurInAnim()
        {
            if (GraphicsSettings.defaultRenderPipeline != null || gameObject.activeInHierarchy == false)
                return;

            StopCoroutine("BlurOut");
            StartCoroutine("BlurIn");
        }

        public void BlurOutAnim()
        {
            if (GraphicsSettings.defaultRenderPipeline != null || gameObject.activeInHierarchy == false)
                return;

            StopCoroutine("BlurIn");
            StartCoroutine("BlurOut");
        }

        public void SetBlurValue(float cbv) { blurValue = cbv; }

        IEnumerator BlurIn()
        {
            currentBlurValue = blurMaterial.GetFloat(customProperty);

            while (currentBlurValue <= blurValue)
            {
                currentBlurValue += Time.deltaTime * animationSpeed;
                if (currentBlurValue >= blurValue) { currentBlurValue = blurValue; }
                blurMaterial.SetFloat(customProperty, currentBlurValue);
                yield return null;
            }

            StopCoroutine("BlurIn");
        }

        IEnumerator BlurOut()
        {
            currentBlurValue = blurMaterial.GetFloat(customProperty);

            while (currentBlurValue >= 0)
            {
                currentBlurValue -= Time.deltaTime * animationSpeed;
                if (currentBlurValue <= 0) { currentBlurValue = 0; }
                blurMaterial.SetFloat(customProperty, currentBlurValue);
                yield return null;
            }

            StopCoroutine("BlurOut");
        }
    }
}