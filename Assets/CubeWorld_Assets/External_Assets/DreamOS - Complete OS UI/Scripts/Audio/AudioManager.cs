using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace Michsky.DreamOS
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private SliderManager masterSlider;
        [SerializeField] private Image taskbarIndicator;
        [SerializeField] private Image mixerIndicator;

        [Header("Indicator States")]
        public Sprite volumeMuted;
        public Sprite volumeLow;
        public Sprite volumeHigh;

        private float muteValue = 0.001f;
        private float preMute;
        private bool muted;

        // Multi Instance Support
        [HideInInspector] public UserManager userManager;

        void Awake()
        {
            if (mixer == null && masterSlider == null)
                return;

            if (userManager == null) { mixer.SetFloat("Master", Mathf.Log10(PlayerPrefs.GetFloat(masterSlider.sliderTag + "Slider")) * 20); }
            else { mixer.SetFloat("Master", Mathf.Log10(PlayerPrefs.GetFloat(userManager.machineID + masterSlider.sliderTag + "Slider")) * 20); }

            masterSlider.mainSlider.onValueChanged.Invoke(masterSlider.mainSlider.value);
        }

        public void VolumeSetMaster(float volume)
        {
            if (mixer == null)
                return;

            mixer.SetFloat("Master", Mathf.Log10(volume) * 20);

            if (taskbarIndicator != null)
            {
                if (masterSlider.mainSlider.value <= muteValue) { taskbarIndicator.sprite = volumeMuted; }
                else if (masterSlider.mainSlider.value > 0.5f) { taskbarIndicator.sprite = volumeHigh; }
                else if (masterSlider.mainSlider.value < 0.5f) { taskbarIndicator.sprite = volumeLow; }
            }

            if (mixerIndicator != null)
            {
                if (masterSlider.mainSlider.value <= muteValue) { mixerIndicator.sprite = volumeMuted; }
                else if (masterSlider.mainSlider.value > 0.5f) { mixerIndicator.sprite = volumeHigh; }
                else if (masterSlider.mainSlider.value < 0.5f) { mixerIndicator.sprite = volumeLow; }
            }

            if (masterSlider.mainSlider.value > muteValue) { muted = false; }
            else { muted = true; }
        }

        public void Mute()
        {
            if (muted == true) { masterSlider.mainSlider.value = preMute; }
            else { preMute = masterSlider.mainSlider.value; masterSlider.mainSlider.value = muteValue; }

            VolumeSetMaster(masterSlider.mainSlider.value);
        }
    }
}