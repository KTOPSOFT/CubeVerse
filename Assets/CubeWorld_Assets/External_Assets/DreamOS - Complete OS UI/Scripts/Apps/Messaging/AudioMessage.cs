using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class AudioMessage : MonoBehaviour
    {
        [Header("Resources")]
        public Button playButton;
        public Button stopButton;
        public Slider durationSlider;
        public Image durationBackground;
        public Image durationForeground;

        [Header("Settings")]
        public bool rememberPosition = true;
        public List<Sprite> durationRandomizer = new List<Sprite>();

        // Hidden vars
        [HideInInspector] public AudioSource aSource;
        [HideInInspector] public AudioClip aClip;

        void Start()
        {
            this.enabled = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            durationSlider.value = 0;
            durationBackground.sprite = durationRandomizer[Random.Range(0, durationRandomizer.Count)];
            durationForeground.sprite = durationBackground.sprite;
        }


        void Update()
        {
            if (aSource.clip.name != aClip.name)
            {
                this.enabled = false;
                playButton.gameObject.SetActive(true);
                stopButton.gameObject.SetActive(false);
                durationSlider.value = 0;
                return;
            }

            durationSlider.value = aSource.time;

            if (durationSlider.value == aClip.length || durationSlider.value >= aClip.length)
                StopAudio();
        }

        public void PlayAudio()
        {
            this.enabled = true;
            playButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);
            durationSlider.maxValue = aClip.length;
            aSource.clip = aClip;
            aSource.Play();
        }

        public void StopAudio()
        {
            this.enabled = false;
            playButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);

            if (rememberPosition == true)
                aSource.Pause();
            else
            {
                durationSlider.value = 0;
                aSource.Stop();
            }
        }
    }
}