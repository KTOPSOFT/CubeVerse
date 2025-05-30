using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Video Player/Video Data Display")]
    public class VideoDataDisplay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Resources")]
        public VideoPlayerManager vManager;
        public UserManager userManager;

        [Header("Settings")]
        public bool alwaysUpdate = true;
        public ObjectType objectType;
        string saveKey = "DreamOS";

        TextMeshProUGUI textObj;
        Image coverImageObj;
        Slider sliderObj;
        Button btnObj;
        Animator animatorObj;
        AudioSource aSource;
        bool enableSliderUpdate = true;

        public enum ObjectType
        {
            Title,
            Description,
            Cover,
            CurrentTime,
            Duration,
            VideoSlider,
            PlayButton,
            PauseButton,
            SeekForward,
            SeekBackward,
            Loop,
            VolumeSlider
        }

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            InitalizeTags();
        }

        void OnEnable()
        {
            UpdateValues();
        }

        public void InitalizeTags()
        {
            // If video manager is not assigned, try to find it
            if (vManager == null)
            {
                try { vManager = (VideoPlayerManager)GameObject.FindObjectsOfType(typeof(VideoPlayerManager))[0]; }
                catch { Debug.Log("<b>[Video Data]</b> Manager is missing.", this); return; }
            }

            // Get and change value depending on the object type
            if (objectType == ObjectType.Title)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
            
            else if (objectType == ObjectType.Description)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();

            else if (objectType == ObjectType.Cover)
                coverImageObj = gameObject.GetComponent<Image>();
            
            else if (objectType == ObjectType.CurrentTime)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
            
            else if (objectType == ObjectType.Duration)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
           
            else if (objectType == ObjectType.VideoSlider)
                sliderObj = gameObject.GetComponent<Slider>();

            else if (objectType == ObjectType.PlayButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponentInParent<Animator>();
                btnObj.onClick.AddListener(vManager.Play);
            }

            else if (objectType == ObjectType.PauseButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponentInParent<Animator>();
                btnObj.onClick.AddListener(vManager.Pause);
            }

            else if (objectType == ObjectType.SeekForward)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(vManager.SeekForward);
            }

            else if (objectType == ObjectType.SeekBackward)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(vManager.SeekBackward);
            }

            else if (objectType == ObjectType.Loop)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(Loop);

                if (PlayerPrefs.GetString(saveKey + "Video Player Loop") == "On") { vManager.videoComponent.isLooping = true; }
                else { vManager.videoComponent.isLooping = false; }

                if (vManager.videoComponent.isLooping == true) { animatorObj.Play("Repeat On"); }
                else { animatorObj.Play("Repeat Off"); }
            }

            else if (objectType == ObjectType.VolumeSlider)
            {
                aSource = vManager.gameObject.GetComponent<AudioSource>();

                sliderObj = gameObject.GetComponent<Slider>();
                sliderObj.onValueChanged.AddListener(SetVolume);

                if (!PlayerPrefs.HasKey(saveKey + "Video Player Volume Default"))
                {
                    sliderObj.value = 1;
                    PlayerPrefs.SetString(saveKey + "Video Player Volume Default", "initalized");
                    PlayerPrefs.SetFloat(saveKey + "Video Player Volume", sliderObj.value);
                }

                sliderObj.value = PlayerPrefs.GetFloat(saveKey + "Video Player Volume");
                aSource.volume = sliderObj.value;
            }
        }

        void Update()
        {
            if (alwaysUpdate == false) { this.enabled = false; return; }
            UpdateValues();
        }

        public void UpdateValues()
        {
            if (vManager == null || vManager.enabled == false)
                return;

            // Get and change value depending on the object type
            if (objectType == ObjectType.Title)
                textObj.text = vManager.libraryAsset.videos[vManager.currentClipIndex].videoTitle;

            else if (objectType == ObjectType.Cover)
                coverImageObj.sprite = vManager.libraryAsset.videos[vManager.currentClipIndex].videoCover;

            else if (objectType == ObjectType.CurrentTime)
                textObj.text = string.Format("{0:00}:{1:00}", vManager.minutesPassed, vManager.secondsPassed);

            else if (objectType == ObjectType.Duration)
                textObj.text = string.Format("{0:00}:{1:00}", vManager.totalMinutes, vManager.totalSeconds);

            else if (objectType == ObjectType.VideoSlider)
            {
                if (vManager.videoComponent != null && enableSliderUpdate == true)
                {
                    sliderObj.maxValue = (float)vManager.videoComponent.length;
                    sliderObj.value = (float)vManager.videoComponent.time;
                }
                else if (vManager.videoComponent != null && enableSliderUpdate == false) { MoveSlider(); }
            }

            else if (objectType == ObjectType.PlayButton || objectType == ObjectType.PauseButton)
            {
                if (vManager.videoComponent.isPlaying == true) { animatorObj.Play("Pause In"); }
                else { animatorObj.Play("Play In"); }
            }
        }

        public void ResetSlider()
        {
            // Reset video duration slider and pause the video player
            vManager.videoComponent.time = 0;
            vManager.Pause();
        }

        public void MoveSlider()
        {
            // Change the duration if slider value is valid for the current video
            if (sliderObj.value > vManager.videoManager.Duration)
                return;

            vManager.videoComponent.time = sliderObj.value;
        }

        public void SetVolume(float volume)
        {
            // Get audio source if it's missing
            if (aSource == null) { aSource = vManager.gameObject.GetComponent<AudioSource>(); }

            // Set the volume depending on slider value and save the data
            aSource.volume = sliderObj.value;
            PlayerPrefs.SetFloat(saveKey + "Video Player Volume", sliderObj.value);
        }

        public void SeekAnim()
        {
            // Just play seek animation
            animatorObj.Play("Animate");
        }

        public void Loop()
        {
            // If loop is enabled, play the animation and save the data
            if (vManager.videoComponent.isLooping == true)
            {
                vManager.videoComponent.isLooping = false;
                animatorObj.Play("Repeat Off");
                PlayerPrefs.SetString(saveKey + "Video Player Loop", "Off");
            }

            else
            {
                vManager.videoComponent.isLooping = true;
                animatorObj.Play("Repeat On");
                PlayerPrefs.SetString(saveKey + "Video Player Loop", "On");
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (objectType == ObjectType.VideoSlider)
                enableSliderUpdate = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (objectType == ObjectType.VideoSlider)
                enableSliderUpdate = true;
        }
    }
}