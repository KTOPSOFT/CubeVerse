using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Slider))]
    public class SliderManager : MonoBehaviour
    {
        [Header("Resources")]
        public Slider mainSlider;
        public TextMeshProUGUI valueText;

        [Header("Saving")]
        public bool enableSaving = false;
        public string sliderTag = "Tag Text";

        [Header("Settings")]
        public bool usePercent = false;
        public bool showValue = true;
        public bool useRoundValue = false;
        public float valueMultiplier = 1;

        [HideInInspector] public float saveValue;

        void Awake()
        {
            if (mainSlider == null) { mainSlider = gameObject.GetComponent<Slider>(); }
            if (enableSaving == true)
            {
                if (PlayerPrefs.HasKey(sliderTag + "Slider") == false) { saveValue = mainSlider.value; }
                else { saveValue = PlayerPrefs.GetFloat(sliderTag + "Slider"); }

                mainSlider.value = saveValue;
            }

            mainSlider.onValueChanged.AddListener(delegate
            {
                saveValue = mainSlider.value;
                UpdateUI();

                PlayerPrefs.SetFloat(sliderTag + "Slider", saveValue);
            });

            UpdateUI();
        }

        public void UpdateUI()
        {
            if (useRoundValue == true)
            {
                if (usePercent == true && valueText != null) { valueText.text = Mathf.Round(mainSlider.value * valueMultiplier).ToString() + "%"; }
                else if (usePercent == false && valueText != null) { valueText.text = Mathf.Round(mainSlider.value * valueMultiplier).ToString(); }
            }

            else
            {
                if (usePercent == true && valueText != null) { valueText.text = mainSlider.value.ToString("F1") + "%"; }
                else if (usePercent == false && valueText != null) { valueText.text = mainSlider.value.ToString("F1"); }
            }
        }
    }
}