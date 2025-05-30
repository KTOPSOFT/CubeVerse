using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class MusicPlayerManager : MonoBehaviour
    {
        // Playlist
        public MusicPlayerPlaylist libraryPlaylist;
        public MusicPlayerPlaylist currentPlaylist;
        public MusicPlayerPlaylist modPlaylist;
        private MusicPlayerPlaylist playlistHelper;
        public List<MusicPlayerPlaylist> playlists = new List<MusicPlayerPlaylist>();

        // Resources
        public AudioSource source;
        [SerializeField] private Transform musicLibraryParent;
        public GameObject musicLibraryButton;
        public WindowPanelManager musicPanelManager;
        [SerializeField] private TextMeshProUGUI nowPlayingListTitle;

        // Playlist Resources
        [SerializeField] private Transform playlistParent;
        [SerializeField] private Transform playlistContentParent;
        public GameObject playlistButton;
        [SerializeField] private TextMeshProUGUI playlistTitle;
        [SerializeField] private TextMeshProUGUI playlistDescription;
        [SerializeField] private Image playlistCover;
        [SerializeField] private Image playlistCoverBanner;
        [SerializeField] private Button playlistPlayAllButton;
        [SerializeField] private LyricsManager lyricsManager;

        // Settings
        public bool repeat;
        public bool shuffle;
        public bool sortListByName = true;
        public bool enableLyrics = true;
        public string playlistSingularLabel = " Song";
        public string playlistPluralLabel = " Songs";

        // Notification
        public bool enablePopupNotification;
        [SerializeField] private NotificationCreator notificationCreator;

        // Hidden song data variables
        public int duration;
        public float playTime;
        public int seconds;
        public float secondsRaw;
        public int minutes;
        public int currentTrack;
        public int currentPlaylistIndex;
        public List<MusicDataDisplay> dataToBeUpdated = new List<MusicDataDisplay>();

        // Hidden helper variables
        [HideInInspector] public bool modSupport;
        private GameObject currentlyPlayingItem;
        private GameObject cpLibraryItem;
        private bool allowSwitchColor;
        private bool isReady = true;
        private int randomHelper;

        void Awake()
        {
            if (modSupport == true)
                return;

            InitializePlayer();
        }

        void OnDisable()
        {
            StopMusic();
        }

        public void InitializePlayer()
        {
            currentTrack = 0;
            currentPlaylist = libraryPlaylist;
            PrepareMusicPlayer();
            PlayMusic();
            StopMusic();
        }

        private static int SortByName(MusicPlayerPlaylist.MusicItem o1, MusicPlayerPlaylist.MusicItem o2)
        {
            // Compare the names and sort by A to Z
            return o1.musicTitle.CompareTo(o2.musicTitle);
        }

        void PrepareMusicPlayer()
        {
            // Destroy leftovers
            foreach (Transform child in musicLibraryParent) { Destroy(child.gameObject); }
            foreach (Transform child in playlistParent) { Destroy(child.gameObject); }
            foreach (Transform child in playlistContentParent) { Destroy(child.gameObject); }

            if (source == null) { source = GetComponent<AudioSource>(); }
            if (sortListByName == true) { libraryPlaylist.playlist.Sort(SortByName); }

            // Instantiate the entire playlist songs as buttons
            for (int i = 0; i < libraryPlaylist.playlist.Count; ++i)
            {
                // Checking for mods/nulls
                if (libraryPlaylist.playlist[i].musicClip == null || libraryPlaylist.playlist[i].isModContent == true
                    && libraryPlaylist.playlist[i].modHelper == true)
                {
                    libraryPlaylist.playlist.RemoveAt(i);
                    i--; continue;
                }

                if (libraryPlaylist.playlist[i].excludeFromLibrary == false)
                {
                    // Spawn music button
                    GameObject go = Instantiate(musicLibraryButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    go.transform.SetParent(musicLibraryParent, false);
                    go.gameObject.name = libraryPlaylist.playlist[i].musicTitle;

                    // Set button BG color
                    Button buttonGO = go.transform.GetComponent<Button>();
                    ColorBlock buttonColor;

                    // This is for visibility in library, qol
                    if (allowSwitchColor == false)
                    {
                        buttonColor = buttonGO.colors;
                        buttonColor.normalColor = new Color32(255, 255, 255, 10);
                        buttonGO.colors = buttonColor;
                        allowSwitchColor = true;
                    }

                    else
                    {
                        buttonColor = buttonGO.colors;
                        buttonColor.normalColor = new Color32(255, 255, 255, 0);
                        buttonGO.colors = buttonColor;
                        allowSwitchColor = false;
                    }

                    // Set cover image
                    Transform coverGO = go.transform.Find("Cover/Image").GetComponent<Transform>();
                    coverGO.GetComponent<Image>().sprite = libraryPlaylist.playlist[i].musicCover;

                    // Set ID tags
                    TextMeshProUGUI songName = go.transform.Find("Song Title").GetComponent<TextMeshProUGUI>();
                    songName.text = libraryPlaylist.playlist[i].musicTitle;
                    TextMeshProUGUI artistName = go.transform.Find("Artist Name").GetComponent<TextMeshProUGUI>();
                    artistName.text = libraryPlaylist.playlist[i].artistTitle;
                    TextMeshProUGUI durationText = go.transform.Find("Duration").GetComponent<TextMeshProUGUI>();
                    durationText.text = (((int)libraryPlaylist.playlist[i].musicClip.length / 60) % 60) + ":" + ((int)libraryPlaylist.playlist[i].musicClip.length % 60).ToString("D2");

                    // Add button events
                    Button itemButton = go.GetComponent<Button>();
                    itemButton.onClick.AddListener(delegate
                    {
                        currentPlaylist = playlists[0];
                        PlayCustomMusic(go.transform.GetSiblingIndex());
                        ChangeLibraryCPI(currentTrack);
                        nowPlayingListTitle.text = playlists[0].playlistName;
                    });
                }

                else if (libraryPlaylist.playlist[i].excludeFromLibrary == true)
                {
                    libraryPlaylist.playlist.RemoveAt(i);
                    if (sortListByName == true) { i--; }
                }
            }

            // Instantiate the entire playlists as buttons
            for (int i = 0; i < playlists.Count; ++i)
            {
                // Checking for nulls - mostly for mods
                for (int y = 0; y < playlists[i].playlist.Count; ++y)
                {
                    if (playlists[i].playlist[y].musicClip == null)
                    {
                        playlists[i].playlist.RemoveAt(y);
                        y--; continue;
                    }
                }

                if (playlists[i].playlist.Count != 0)
                {
                    // Spawn playlist button
                    GameObject go = Instantiate(playlistButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    go.transform.SetParent(playlistParent, false);
                    go.gameObject.name = playlists[i].ToString();

                    // Set cover image
                    Transform coverGO = go.transform.Find("Cover/Image").GetComponent<Transform>();
                    coverGO.GetComponent<Image>().sprite = playlists[i].coverImage;

                    // Set titles
                    TextMeshProUGUI playlistButtonTitle = go.transform.Find("Playlist Title").GetComponent<TextMeshProUGUI>();
                    playlistButtonTitle.text = playlists[i].playlistName;
                    TextMeshProUGUI countTitle = go.transform.Find("Music Count").GetComponent<TextMeshProUGUI>();

                    string countText = "";

                    if (playlists[i].playlist.Count == 1) { countText = playlists[i].playlist.Count.ToString() + playlistSingularLabel; }
                    else { countText = playlists[i].playlist.Count.ToString() + playlistPluralLabel; }

                    countTitle.text = countText;

                    // Add play button events
                    Button playButton = go.transform.Find("Buttons/Play").GetComponent<Button>();
                    playButton.onClick.AddListener(delegate
                    {
                        currentPlaylist = playlists[go.transform.GetSiblingIndex()];
                        currentPlaylistIndex = go.transform.GetSiblingIndex();
                        currentTrack = 0;
                        PreparePlaylist(go.transform.GetSiblingIndex());
                        PlayCustomMusic(0);
                        PlayMusic();
                        nowPlayingListTitle.text = playlistHelper.playlistName;
                    });

                    // Add show button events
                    Button showButton = go.transform.Find("Buttons/Show").GetComponent<Button>();
                    showButton.onClick.AddListener(delegate
                    {
                        PreparePlaylist(go.transform.GetSiblingIndex());
                        musicPanelManager.OpenPanel("Playlist Content");
                        playlistTitle.text = playlistHelper.playlistName;
                        playlistDescription.text = countText;
                        playlistCover.sprite = playlistHelper.coverImage;
                        playlistCoverBanner.sprite = playlistHelper.coverImage;
                    });

                    Button goButton = go.GetComponent<Button>();
                    goButton.onClick.AddListener(delegate { showButton.onClick.Invoke(); });
                }
            }

            // Set the first music and then pause it - for visual
            nowPlayingListTitle.text = playlists[0].playlistName;
        }

        public void PreparePlaylist(int playlistIndex)
        {
            // Set the playlist index to prepare for the next one
            playlistHelper = playlists[playlistIndex];

            // Destroy each object in playlist parent
            foreach (Transform child in playlistContentParent) { Destroy(child.gameObject); }

            // Sort playlist by A to Z
            if (sortListByName == true) { playlistHelper.playlist.Sort(SortByName); }

            // Instantiate the entire playlist songs as buttons
            for (int i = 0; i < playlistHelper.playlist.Count; ++i)
            {
                if (playlistHelper.playlist[i].excludeFromLibrary == false)
                {
                    // Spawn song button
                    GameObject go = Instantiate(musicLibraryButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    go.transform.SetParent(playlistContentParent, false);
                    go.gameObject.name = playlistHelper.playlist[i].musicTitle;

                    // Set button BG color
                    Button buttonGO;
                    buttonGO = go.transform.GetComponent<Button>();
                    ColorBlock buttonColor;

                    // This is for visibility in library, qol
                    if (allowSwitchColor == false)
                    {
                        buttonColor = buttonGO.colors;
                        buttonColor.normalColor = new Color32(255, 255, 255, 10);
                        buttonGO.colors = buttonColor;
                        allowSwitchColor = true;
                    }

                    else
                    {
                        buttonColor = buttonGO.colors;
                        buttonColor.normalColor = new Color32(255, 255, 255, 0);
                        buttonGO.colors = buttonColor;
                        allowSwitchColor = false;
                    }

                    // Set cover image
                    Transform coverGO;
                    coverGO = go.transform.Find("Cover/Image").GetComponent<Transform>();
                    coverGO.GetComponent<Image>().sprite = playlistHelper.playlist[i].musicCover;

                    // Set ID tags
                    TextMeshProUGUI songName;
                    songName = go.transform.Find("Song Title").GetComponent<TextMeshProUGUI>();
                    songName.text = playlistHelper.playlist[i].musicTitle;
                    TextMeshProUGUI artistName;
                    artistName = go.transform.Find("Artist Name").GetComponent<TextMeshProUGUI>();
                    artistName.text = playlistHelper.playlist[i].artistTitle;
                    TextMeshProUGUI durationText;
                    durationText = go.transform.Find("Duration").GetComponent<TextMeshProUGUI>();
                    durationText.text = (((int)playlistHelper.playlist[i].musicClip.length / 60) % 60) + ":" + ((int)playlistHelper.playlist[i].musicClip.length % 60).ToString("D2");

                    // Add button events
                    Button itemButton;
                    itemButton = go.GetComponent<Button>();
                    itemButton.onClick.AddListener(delegate
                    {
                        currentPlaylist = playlists[playlistIndex];
                        currentPlaylistIndex = playlistIndex;
                        PlayCustomMusic(go.transform.GetSiblingIndex());
                        ChangeLibraryCPI(currentTrack);
                        ChangePlaylistCPI(currentTrack);
                        nowPlayingListTitle.text = playlists[playlistIndex].playlistName;
                    });

                    // Reset Play All button and replace with the current playlist songs
                    playlistPlayAllButton.onClick.AddListener(delegate
                    {
                        playlistPlayAllButton.onClick.RemoveAllListeners();
                        currentPlaylist = playlists[playlistIndex];
                        currentPlaylistIndex = playlistIndex;
                        currentTrack = 0;
                        PlayCustomMusic(0);
                        PlayMusic();
                        ChangeLibraryCPI(currentTrack);
                        ChangePlaylistCPI(currentTrack);
                        nowPlayingListTitle.text = playlists[playlistIndex].playlistName;
                    });

                    if (playlistHelper == currentPlaylist && currentTrack == i)
                    {
                        currentlyPlayingItem = go.transform.Find("Now Playing").gameObject;
                        currentlyPlayingItem.SetActive(true);
                    }
                }

                else if (playlistHelper.playlist[i].excludeFromLibrary == true)
                {
                    playlistHelper.playlist.RemoveAt(i);
                    if (sortListByName == true) { i--; }
                }
            }
        }

        IEnumerator ProcessPlayback()
        {
            while (source.isPlaying)
            {
                // Update the playback time while source is playing
                playTime = source.time;
                ShowPlayTime();

                // Change playback depending on the variables scuh as shuffle
                if (playTime >= duration && shuffle == true && repeat == false) { isReady = true; }
                else if (playTime >= duration && repeat == true && shuffle == false) { source.Stop(); source.time = 0; source.Play(); }
                else if (playTime >= duration && shuffle == false && repeat == false) { isReady = true; }
                else if (playTime >= duration && shuffle == true && repeat == true) { isReady = true; }

                yield return null;
            }

            // Process NextTitle when the current song ends
            if (isReady == true)
            {
                NextTitle();
                isReady = false;
            }
        }

        public void PlayMusic()
        {
            this.enabled = true;
            if (source != null) { source.clip = currentPlaylist.playlist[currentTrack].musicClip; }

            // Play and change the data
            isReady = false;
            source.Play();
            ShowCurrentTitle();
            UpdateDataObjects();
            StartCoroutine("ProcessPlayback");

            if (cpLibraryItem != null && currentPlaylist == libraryPlaylist) { cpLibraryItem.SetActive(true); }
            else { ChangeLibraryCPI(currentTrack); }

            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(true); }
            else { ChangePlaylistCPI(currentTrack); }

            if (enableLyrics == true) 
            {
                if (lyricsManager.lyricFound == true) { lyricsManager.UpdateCurrentLyric(); }
                else { lyricsManager.ReadLyricData(currentPlaylist.playlist[currentTrack].musicTitle); }
            }
        }

        public void PlayCustomMusic(int musicIndex)
        {
            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(false); }
            if (cpLibraryItem != null) { cpLibraryItem.SetActive(false); }

            // Play a specific music depending on the index
            source.Stop();
            currentTrack = musicIndex;
            source.clip = currentPlaylist.playlist[musicIndex].musicClip;
            source.time = 0;
            source.Play();
            ShowCurrentTitle();
            UpdateDataObjects();
            ChangeLibraryCPI(musicIndex);
            ChangePlaylistCPI(musicIndex);
            StartCoroutine("ProcessPlayback");

            if (enableLyrics == true)
            {
                if (lyricsManager.lyricFound == true) { lyricsManager.Continue(); }
                else { lyricsManager.ReadLyricData(currentPlaylist.playlist[currentTrack].musicTitle); }
            }
        }

        public void PlayCustomClip(AudioClip clip, Sprite cover, string clipName, string clipAuthor)
        {
            // Adding a new clip to the playlist
            MusicPlayerPlaylist.MusicItem item = new MusicPlayerPlaylist.MusicItem();
            item.musicClip = clip;
            item.musicTitle = clipName;
            item.artistTitle = clipAuthor;
            item.musicCover = cover;
            item.excludeFromLibrary = true;
            currentPlaylist.playlist.Add(item);

            // Play the clip
            source.Stop();
            currentTrack = currentPlaylist.playlist.Count - 1;
            source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            source.time = 0;
            source.Play();
            ShowCurrentTitle();
            UpdateDataObjects();
            StartCoroutine("ProcessPlayback");
        }

        public void PauseMusic()
        {
            source.Pause();

            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(false); }
            if (cpLibraryItem != null) { cpLibraryItem.SetActive(false); }
            if (enableLyrics == true) { lyricsManager.Pause(); }

            UpdateDataObjects();
        }

        void ChangePlaylistCPI(int index)
        {
            if (playlistHelper == null || currentPlaylist != playlistHelper)
                return;

            GameObject cpiParent = playlistContentParent.transform.GetChild(index).gameObject;
            currentlyPlayingItem = cpiParent.transform.Find("Now Playing").gameObject;
            currentlyPlayingItem.SetActive(true);
        }

        void ChangeLibraryCPI(int index)
        {
            if (currentPlaylist != libraryPlaylist)
                return;

            GameObject cpiLibParent = musicLibraryParent.transform.GetChild(index).gameObject;
            cpLibraryItem = cpiLibParent.transform.Find("Now Playing").gameObject;
            cpLibraryItem.SetActive(true);
        }

        public void NextTitle()
        {
            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(false); }
            if (cpLibraryItem != null) { cpLibraryItem.SetActive(false); }

            // Stop!
            source.Stop();

            if (currentPlaylist.playlist.Count == 1) { source.Stop(); source.time = 0; source.Play(); }

            // If shuffle is true and repeat is false, select a random song from the current list
            else if (shuffle == true && repeat == false)
            {
                // Remember the current track and then pick a random one
                randomHelper = currentTrack;
                currentTrack = Random.Range(0, currentPlaylist.playlist.Count);

                // Change the song again - only if you get the same song with the previous one
                if (currentTrack == randomHelper || currentPlaylist.playlist[currentTrack].excludeFromLibrary == true)
                {
                    NextTitle();
                    return;
                }

                // Assign the current song to audio source
                source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            }

            // If not, then just skip to the next song
            else
            {
                currentTrack++;

                // Go back to the first song when reaching to the end of playlist
                if (currentTrack > currentPlaylist.playlist.Count - 1) { currentTrack = 0; }
                if (currentPlaylist.playlist[currentTrack].excludeFromLibrary == true) { NextTitle(); }

                // Assign the current song to audio source
                source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            }

            // Play and change the data
            source.time = 0;
            source.Play();
            ShowCurrentTitle();
            UpdateDataObjects();
            ChangeLibraryCPI(currentTrack);
            ChangePlaylistCPI(currentTrack);
            StartCoroutine("ProcessPlayback");

            // If notifications are on, then create one through the creator
            if (enablePopupNotification == true) { ShowNotification(); }

            // Check for lyrics
            if (enableLyrics == true) { lyricsManager.ReadLyricData(currentPlaylist.playlist[currentTrack].musicTitle); }
        }

        public void PrevTitle()
        {
            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(false); }
            if (cpLibraryItem != null) { cpLibraryItem.SetActive(false); }

            // Stop!
            source.Stop();

            // If shuffle is true and repeat is false, select a random song from the current list
            if (shuffle == true && repeat == false)
            {
                // Remember the current track and then pick a random one
                randomHelper = currentTrack;
                currentTrack = Random.Range(0, currentPlaylist.playlist.Count);

                // Change the song again - only if you get the same song with the previous one
                if (currentTrack == randomHelper) { currentTrack = Random.Range(0, currentPlaylist.playlist.Count); }

                // Assign the current song to audio source
                source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            }

            // If not, then just skip to the previous song
            else
            {
                currentTrack--;

                // Go back to the first song when it doesn't meet the requirements
                if (currentTrack < 0) { currentTrack = currentPlaylist.playlist.Count - 1; }

                // Assign the current song to audio source
                source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            }

            // Play and change the data
            source.clip = currentPlaylist.playlist[currentTrack].musicClip;
            source.time = 0;
            source.Play();
            ShowCurrentTitle();
            UpdateDataObjects();
            ChangeLibraryCPI(currentTrack);
            ChangePlaylistCPI(currentTrack);
            StartCoroutine("ProcessPlayback");

            // If notifications are on, then create one through the creator
            if (enablePopupNotification == true) { ShowNotification(); }

            // Check for lyrics
            if (enableLyrics == true) { lyricsManager.ReadLyricData(currentPlaylist.playlist[currentTrack].musicTitle); }
        }

        public void UpdateDataObjects()
        {
            for (int i = 0; i < dataToBeUpdated.Count; ++i)
            {
                if (dataToBeUpdated[i] == null) { dataToBeUpdated.RemoveAt(i); continue; }
                if (dataToBeUpdated[i].gameObject.activeInHierarchy == true) { dataToBeUpdated[i].UpdateValues(); }
            }
        }

        public void SetPopupNotification(bool value) 
        {
            if (value == true) { enablePopupNotification = true; }
            else { enablePopupNotification = false; }
        }

        void ShowNotification()
        {
            notificationCreator.notificationTitle = currentPlaylist.playlist[currentTrack].musicTitle;
            notificationCreator.popupDescription = currentPlaylist.playlist[currentTrack].artistTitle;
            notificationCreator.CreateOnlyPopup();
        }

        public void StopMusic()
        {
            StopCoroutine("ProcessPlayback");

            if (source != null) { source.Stop(); }
            if (currentlyPlayingItem != null) { currentlyPlayingItem.SetActive(false); }
            if (cpLibraryItem != null) { cpLibraryItem.SetActive(false); }
            if (enableLyrics == true) { lyricsManager.Pause(); }

            UpdateDataObjects();
            this.enabled = false;
        }

        public void MuteMusic() { source.mute = !source.mute; }
        public void ShowCurrentTitle() { duration = (int)source.clip.length; }
        public void ShowPlayTime() { seconds = (int)playTime % 60; secondsRaw = playTime % 60; minutes = ((int)playTime / 60) % 60; }
        public void AddPlaylist() { playlists.Add(null); }

        public void UpdateLyricUI() 
        {
            if (enableLyrics == true && lyricsManager.lyricFound == true) { lyricsManager.UpdateCurrentLyric(); }
        }
    }
}