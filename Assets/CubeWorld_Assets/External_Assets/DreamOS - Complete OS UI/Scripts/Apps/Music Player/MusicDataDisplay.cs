using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Apps/Music Player/Music Data Display")]
    public class MusicDataDisplay : MonoBehaviour
    {
        [Header("Resources")]
        public MusicPlayerManager mpManager;
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

        public enum ObjectType
        {
            Title,
            Artist,
            Album,
            Cover,
            CurrentTime,
            Duration,
            MusicSlider,
            PlayButton,
            PauseButton,
            NextButton,
            PrevButton,
            Repeat,
            Shuffle,
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
            // If music player manager is not assigned, then try to find it
            if (mpManager == null)
            {
                try { mpManager = (MusicPlayerManager)GameObject.FindObjectsOfType(typeof(MusicPlayerManager))[0]; }
                catch { Debug.Log("<b>[Music Data]</b> Manager is missing.", this); return; }
            }

            // Don't go further if music player manager is not assigned
            if (mpManager == null)
                return;

            // Get and change the value depending on the object type
            if (objectType == ObjectType.Title)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
            
            else if (objectType == ObjectType.Artist)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
           
            else if (objectType == ObjectType.Album)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
           
            else if (objectType == ObjectType.Cover)
                coverImageObj = gameObject.GetComponent<Image>();
           
            else if (objectType == ObjectType.CurrentTime)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();
           
            else if (objectType == ObjectType.Duration)
                textObj = gameObject.GetComponent<TextMeshProUGUI>();

            else if (objectType == ObjectType.MusicSlider)
            {
                sliderObj = gameObject.GetComponent<Slider>();
                sliderObj.onValueChanged.AddListener(delegate { MoveSlider(); });
            }

            else if (objectType == ObjectType.PlayButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponentInParent<Animator>();
                btnObj.onClick.AddListener(mpManager.PlayMusic);
            }

            else if (objectType == ObjectType.PauseButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponentInParent<Animator>();
                btnObj.onClick.AddListener(mpManager.PauseMusic);
            }

            else if (objectType == ObjectType.NextButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(mpManager.NextTitle);
                btnObj.onClick.AddListener(ResetSlider);
                btnObj.onClick.AddListener(Next);
            }

            else if (objectType == ObjectType.PrevButton)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(mpManager.PrevTitle);
                btnObj.onClick.AddListener(ResetSlider);
                btnObj.onClick.AddListener(Prev);
            }

            else if (objectType == ObjectType.Repeat)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(Repeat);

                if (!PlayerPrefs.HasKey(saveKey + "MusicPlayerRepeat")) { mpManager.repeat = false; animatorObj.Play("Repeat Off"); }
                else if (PlayerPrefs.GetString(saveKey + "MusicPlayerRepeat") == "On") { mpManager.repeat = true; animatorObj.Play("Repeat On"); }
                else { mpManager.repeat = false; animatorObj.Play("Repeat Off"); }
            }

            else if (objectType == ObjectType.Shuffle)
            {
                btnObj = gameObject.GetComponent<Button>();
                animatorObj = gameObject.GetComponent<Animator>();
                btnObj.onClick.AddListener(Shuffle);

                if (!PlayerPrefs.HasKey(saveKey + "MusicPlayerShuffle")) { mpManager.shuffle = false; animatorObj.Play("Shuffle Off"); }
                else if (PlayerPrefs.GetString(saveKey + "MusicPlayerShuffle") == "On") { mpManager.shuffle = true; animatorObj.Play("Shuffle On"); }
                else { mpManager.shuffle = false; animatorObj.Play("Shuffle Off"); }
            }

            else if (objectType == ObjectType.VolumeSlider)
            {
                sliderObj = gameObject.GetComponent<Slider>();
                sliderObj.onValueChanged.AddListener(SetVolume);

                if (!PlayerPrefs.HasKey(saveKey + "MusicPlayerVolumeFirst"))
                {
                    sliderObj.value = 1;
                    PlayerPrefs.SetString(saveKey + "MusicPlayerVolumeFirst", "initalized");
                    PlayerPrefs.SetFloat(saveKey + "MusicPlayerVolume", sliderObj.value);
                }

                sliderObj.value = PlayerPrefs.GetFloat(saveKey + "MusicPlayerVolume");
                if (mpManager.source != null) { mpManager.source.volume = sliderObj.value; }
            }

            mpManager.dataToBeUpdated.Add(this);
        }

        void Update()
        {
            if (alwaysUpdate == false) { this.enabled = false; return; }
            UpdateValues();
        }

        public void UpdateValues()
        {
            if (mpManager == null || mpManager.enabled == false || mpManager.source.clip == null)
                return;

            // Change the value depending on the object type
            if (objectType == ObjectType.Title)
                textObj.text = mpManager.currentPlaylist.playlist[mpManager.currentTrack].musicTitle;

            else if (objectType == ObjectType.Artist)
                textObj.text = mpManager.currentPlaylist.playlist[mpManager.currentTrack].artistTitle;

            else if (objectType == ObjectType.Album)
                textObj.text = mpManager.currentPlaylist.playlist[mpManager.currentTrack].albumTitle;

            else if (objectType == ObjectType.Cover)
                coverImageObj.sprite = mpManager.currentPlaylist.playlist[mpManager.currentTrack].musicCover;

            else if (objectType == ObjectType.CurrentTime)
                textObj.text = mpManager.minutes + ":" + mpManager.seconds.ToString("00");

            else if (objectType == ObjectType.Duration)
            {
                mpManager.ShowCurrentTitle();
                textObj.text = ((mpManager.duration / 60) % 60) + ":" + (mpManager.duration % 60).ToString("D2");
            }

            else if (objectType == ObjectType.MusicSlider)
            {
                sliderObj.maxValue = mpManager.source.clip.length;
                sliderObj.value = mpManager.source.time;
            }

            else if (objectType == ObjectType.PlayButton || objectType == ObjectType.PauseButton)
            {
                if (mpManager.source.isPlaying == true) { animatorObj.Play("Pause In"); }
                else { animatorObj.Play("Play In"); }
            }

            else if (objectType == ObjectType.Shuffle && mpManager.shuffle == true)
                animatorObj.Play("Shuffle On");
            
            else if (objectType == ObjectType.Shuffle && mpManager.shuffle == false)
                animatorObj.Play("Shuffle Off");

            else if (objectType == ObjectType.Repeat && mpManager.repeat == true)
                animatorObj.Play("Repeat On");
           
            else if (objectType == ObjectType.Repeat && mpManager.repeat == false)
                animatorObj.Play("Repeat Off");
        }

        public void UpdateManually()
        {
            if (mpManager == null)
                return;

            if (objectType == ObjectType.Shuffle && animatorObj.gameObject.activeInHierarchy == true)
            {
                if (mpManager.shuffle == true) { animatorObj.Play("Shuffle Off"); }
                else { animatorObj.Play("Shuffle On"); }
            }

            else if (objectType == ObjectType.Repeat && animatorObj.gameObject.activeInHierarchy == true)
            {
                if (mpManager.repeat == true) { animatorObj.Play("Repeat Off"); }
                else { animatorObj.Play("Repeat On"); }
            }

            else if (objectType == ObjectType.VolumeSlider && sliderObj != null)
                sliderObj.value = PlayerPrefs.GetFloat(saveKey + "MusicPlayerVolume");
        }

        public void ResetSlider() { mpManager.source.time = 0; }

        public void MoveSlider()
        {
            // Change the duration if slider value is valid for the current music
            if (sliderObj.value > mpManager.source.clip.length - 0.01f)
                return;

            mpManager.source.time = sliderObj.value;
        }

        public void SetVolume(float volume)
        {
            // Get audio source if it's missing
            if (mpManager.source == null) { mpManager.source = mpManager.gameObject.GetComponent<AudioSource>(); }

            // Set the volume depending on slider value and save the data
            mpManager.source.volume = sliderObj.value;
            PlayerPrefs.SetFloat(saveKey + "MusicPlayerVolume", sliderObj.value);
        }

        public void Prev() { animatorObj.Play("Animate"); }
        public void Next() { animatorObj.Play("Animate"); }

        public void Shuffle()
        {
            // If shuffle is enabled, play the animation and save the data
            if (mpManager.shuffle == true)
            {
                mpManager.shuffle = false;
                animatorObj.Play("Shuffle Off");
                PlayerPrefs.SetString(saveKey + "MusicPlayerShuffle", "Off");
            }

            else
            {
                mpManager.shuffle = true;
                animatorObj.Play("Shuffle On");
                PlayerPrefs.SetString(saveKey + "MusicPlayerShuffle", "On");
            }
        }

        public void Repeat()
        {
            // If repeat is enabled, play the animation and save the data
            if (mpManager.repeat == true)
            {
                mpManager.repeat = false;
                animatorObj.Play("Repeat Off");
                PlayerPrefs.SetString(saveKey + "MusicPlayerRepeat", "Off");
            }

            else
            {
                mpManager.repeat = true;
                animatorObj.Play("Repeat On");
                PlayerPrefs.SetString(saveKey + "MusicPlayerRepeat", "On");
            }
        }
    }
}