using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class GameHubManager : MonoBehaviour
    {
        // Main Resources
        public GameHubLibrary libraryAsset;
        public WindowManager gameHubWindow;
        public Transform gameParent;
        public Transform shortcutParent;
        public GameObject shortcutItem;
        WindowPanelManager wpm;

        // Store
        public GameObject storeItem;
        public Transform storeItemParent;

        // Slider
        public float sliderTimer = 4;
        [Range(1,10)] public float transitionSpeed = 4;
        [Range(0.05f, 1)] public float scaleSpeed = 0.1f;
        public Image sliderBanner;
        public Image sliderIcon;
        public TextMeshProUGUI sliderDescription;
        public Image transitionHelper;
        public Button sliderButton;
        public GameObject sliderIndicator;
        public Transform sliderIndicatorParent;
        public List<int> sliderItems = new List<int>();
        List<Animator> sliderIndicators = new List<Animator>();
        Animator currentIndicatorObject;
        Image currentIndicatorBar;
        int currentSliderIndex;
        float sliderTimerBar;
        bool isInTransition;

        // Library
        public GameObject libraryItem;
        public Transform libraryItemParent;
        public Image libraryTransitionHelper;
        public Sprite libraryEmptyBackground;
        [Range(1, 10)] public float libraryTransitionSpeed = 4;
        [Range(0.05f, 1)] public float backgroundScaleSpeed = 0.1f;
        [Range(1, 2)] public float maxBackgroundScale = 1.1f;
        [TextArea] public string libraryEmptyDescription;

        // Game Page
        public Animator gameWindowBar;
        public TextMeshProUGUI gameDescription;
        public Image gameIcon;
        public Image gameBanner;
        public Button downloadButton;
        public Button playButton;
        public Button uninstallButon;
        public Button createShortcutButton;
        public Button removeShortcutButton;

        // Downloads
        public PopupPanelManager downloadsPanel;
        public TextMeshProUGUI noDownloadsText;
        public Transform downloadItemParent;
        public GameObject downloadItem;
        [HideInInspector] public int downloadQueue;

        // External
        public float downloadSpeed;
        public NetworkManager networkManager;
        public NotificationCreator downloadNotification;

        // Settings
        public bool sortListByName;
        [Range(0, 30)] public float uninstallTime = 5;
        public string homePageTitle = "Store";
        public string libraryPageTitle = "Library";

        // Others
        [HideInInspector] public string currentGameTitle;
        [HideInInspector] public string currentGameDescription;
        [HideInInspector] public Sprite currentGameIcon;
        [HideInInspector] public Sprite currentGameSmallIcon;
        [HideInInspector] public Sprite currentGameBanner;
        [HideInInspector] public float currentGameSize;
        [HideInInspector] public string currentDownloadState;
        [HideInInspector] public GameObject currentGamePrefab;
        bool currentGameDownloading;

        private static int SortByName(GameHubLibrary.GameHubItem o1, GameHubLibrary.GameHubItem o2)
        {
            // Compare the names and sort by A to Z
            return o1.gameTitle.CompareTo(o2.gameTitle);
        }

        void Awake()
        {
            InitializeGames();
            CheckForDownloadQueue();

            transitionHelper.color = new Color(transitionHelper.color.r, transitionHelper.color.g, transitionHelper.color.b, 0);
            libraryTransitionHelper.color = new Color(libraryTransitionHelper.color.r, libraryTransitionHelper.color.g, libraryTransitionHelper.color.b, 0);

            this.enabled = false;
        }

        void Update()
        {
            if (sliderTimerBar <= sliderTimer && isInTransition == false)
            {
                sliderTimerBar += Time.deltaTime;
                currentIndicatorBar.fillAmount = sliderTimerBar / sliderTimer;
            }

            sliderBanner.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f) * scaleSpeed * Time.deltaTime;

            if (gameBanner.transform.localScale.x <= maxBackgroundScale)
                gameBanner.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f) * backgroundScaleSpeed * Time.deltaTime;
        }

        void OnEnable()
        {
            sliderTimerBar = 0;
            currentIndicatorBar = sliderIndicators[currentSliderIndex].transform.Find("Bar/Filled").GetComponent<Image>();
            currentIndicatorObject = sliderIndicators[currentSliderIndex];
            currentIndicatorObject.Play("In");

            UpdateSliderInfo();
            CheckForLibraryItems();
            CheckForAvailableGames();
            StartCoroutine(WaitForSliderTimer());
        }

        void InitializeGames()
        {
            if (libraryAsset == null && gameHubWindow == null)
            {
                Debug.LogError("<b>[Game Hub Manager]</b> Required variable(s) missing.", this);
                this.enabled = false;
                return;
            }

            // Get window manager from the main window
            wpm = gameHubWindow.gameObject.GetComponent<WindowPanelManager>();

            // Destroy each object in indicator parent
            foreach (Transform child in sliderIndicatorParent)
                Destroy(child.gameObject);

            // Destroy each object in library parent
            foreach (Transform child in libraryItemParent)
                Destroy(child.gameObject);

            // Destroy each object in store parent
            foreach (Transform child in storeItemParent)
                Destroy(child.gameObject);

            // Destroy each object in download parent
            foreach (Transform child in downloadItemParent)
                Destroy(child.gameObject);

            // Destroy each object in indicator parent
            foreach (Transform child in sliderIndicatorParent)
                Destroy(child.gameObject);

            // Sort items by A to Z if it's enabled
            if (sortListByName == true)
                libraryAsset.games.Sort(SortByName);

            // Instantiate available games to library
            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                if (libraryAsset.games[i].gameDownloaded == true && PlayerPrefs.HasKey(libraryAsset.games[i].gameTitle + "Installed") == false)
                    PlayerPrefs.SetString(libraryAsset.games[i].gameTitle + "Installed", "true");

                // Create library item
                GameObject libraryItemGO = Instantiate(libraryItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                libraryItemGO.transform.SetParent(libraryItemParent, false);
                libraryItemGO.gameObject.name = libraryAsset.games[i].gameTitle;

                // Set game title
                TextMeshProUGUI libraryItemTitle;
                libraryItemTitle = libraryItemGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                libraryItemTitle.text = libraryAsset.games[i].gameTitle;

                // Set game icon
                Image shortcutImage;
                shortcutImage = libraryItemGO.transform.Find("Icon").GetComponent<Image>();
                shortcutImage.sprite = libraryAsset.games[i].gameSmallIcon;

                // Add button events
                Button libraryItemButton;
                libraryItemButton = libraryItemGO.GetComponent<Button>();
                libraryItemButton.onClick.AddListener(delegate
                {
                    OpenGamePage(libraryItemGO.transform.name, true, false);
                });

                if (PlayerPrefs.GetString(libraryAsset.games[i].gameTitle + "Installed") == "true")
                    libraryItemGO.SetActive(true);
                else
                    Destroy(libraryItemGO);

                libraryAsset.games[i].isDownloading = false;
            }

            // Instantiate the entire game library to store
            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                // Spawn game button
                GameObject go = Instantiate(storeItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(storeItemParent, false);
                go.gameObject.name = libraryAsset.games[i].gameTitle;

                // Set image
                Transform artworkGO;
                artworkGO = go.transform.Find("Content/Artwork Parent/Artwork").GetComponent<Transform>();
                artworkGO.GetComponent<Image>().sprite = libraryAsset.games[i].gameArtwork;

                // Set title
                TextMeshProUGUI gameTitle;
                gameTitle = go.transform.Find("Content/Game Title").GetComponent<TextMeshProUGUI>();
                gameTitle.text = libraryAsset.games[i].gameTitle;

                // Add button events
                Button itemButton;
                itemButton = go.GetComponent<Button>();
                itemButton.onClick.AddListener(delegate
                {
                    OpenGamePage(go.transform.name, true, true);
                    wpm.OpenPanel(libraryPageTitle);
                });

                // Check if downloaded
                GameObject downloadedIndicator;
                downloadedIndicator = go.transform.Find("Content/Installed").gameObject;

                if (PlayerPrefs.GetString(libraryAsset.games[i].gameTitle + "Installed") == "true")
                    downloadedIndicator.SetActive(true);
                else
                    downloadedIndicator.SetActive(false);

                if (PlayerPrefs.GetString(libraryAsset.games[i].gameTitle + "Shortcut") == "true")
                {
                    GameObject shortcutGO = Instantiate(shortcutItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    shortcutGO.transform.SetParent(shortcutParent, false);
                    shortcutGO.gameObject.name = libraryAsset.games[i].gameTitle;

                    // Set shortcut title
                    TextMeshProUGUI shortcutTitle;
                    shortcutTitle = shortcutGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    shortcutTitle.text = libraryAsset.games[i].gameTitle;

                    // Set shortcut icon
                    Image shortcutImage;
                    shortcutImage = shortcutGO.transform.Find("Icon Background/Icon").GetComponent<Image>();
                    shortcutImage.sprite = libraryAsset.games[i].gameIcon;

                    // Add shortcut button events
                    Button shortcutButton;
                    shortcutButton = shortcutGO.GetComponent<Button>();
                    shortcutButton.onClick.AddListener(delegate
                    {
                        LaunchGame(go.transform.name);
                    });
                }
            }

            // Instantiate indicator objects to their parent
            for (int i = 0; i < sliderItems.Count; ++i)
            {
                // Spawn indicator object
                GameObject go = Instantiate(sliderIndicator, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(sliderIndicatorParent, false);
                go.gameObject.name = libraryAsset.games[i].gameTitle;

                // Add button events
                Button shortcutButton;
                shortcutButton = go.transform.Find("Dot").GetComponent<Button>();
                shortcutButton.onClick.AddListener(delegate
                {
                    StopCoroutine("WaitForSliderTimer");
                    StopCoroutine("SliderTransitionIn");
                    StopCoroutine("SliderTransitionOut");

                    currentIndicatorObject.Play("Out");
                    currentSliderIndex = go.transform.GetSiblingIndex();
                    currentIndicatorObject = sliderIndicators[currentSliderIndex];
                    currentIndicatorObject.Play("In");
                    sliderTimerBar = 0;
                    currentIndicatorBar.fillAmount = sliderTimerBar;
                    isInTransition = true;

                    StartCoroutine("SliderTransitionIn");
                });

                sliderIndicators.Add(go.GetComponent<Animator>());
            }
        }

        public void CheckForAvailableGames()
        {
            if (libraryItemParent.childCount == 0)
            {
                gameDescription.text = libraryEmptyDescription;
                gameBanner.sprite = libraryEmptyBackground;
                gameIcon.gameObject.SetActive(false);
            }

            else
            {
                for (int i = 0; i < libraryAsset.games.Count; ++i)
                {
                    if (PlayerPrefs.GetString(libraryAsset.games[i].gameTitle + "Installed") == "true")
                    {
                        OpenGamePage(libraryAsset.games[i].gameTitle, false, true);
                        break;
                    }
                }
            }

        }

        public void OpenDownloads()
        {
            downloadsPanel.OpenPanel();

            if (downloadQueue == 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(true);
            else if (downloadQueue != 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(false);
        }

        public void CloseDownloads()
        {
            downloadsPanel.ClosePanel();

            if (downloadQueue == 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(true);
            else if (downloadQueue != 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(false);
        }

        public void AnimateDownloads()
        {
            downloadsPanel.AnimatePanel();

            if (downloadQueue == 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(true);
            else if (downloadQueue != 0 && noDownloadsText != null)
                noDownloadsText.gameObject.SetActive(false);
        }

        public void CheckForGameInformation(string gameName)
        {
            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                if (libraryAsset.games[i].gameTitle == gameName)
                {
                    currentGameTitle = gameName;
                    currentGameDescription = libraryAsset.games[i].gameDescription;
                    currentGameIcon = libraryAsset.games[i].gameIcon;
                    currentGameSmallIcon = libraryAsset.games[i].gameSmallIcon;
                    currentGameBanner = libraryAsset.games[i].gameBanner;
                    currentGameSize = libraryAsset.games[i].gameSize;
                    currentGamePrefab = libraryAsset.games[i].gamePrefab;
                    currentGameDownloading = libraryAsset.games[i].isDownloading;

                    if (PlayerPrefs.GetString(gameName + "Installed") == "true")
                        currentDownloadState = "true";
                    else if (PlayerPrefs.GetString(gameName + "Installed") == "false")
                        currentDownloadState = "false";

                    break;
                }
            }
        }

        public void OpenGamePage(string gameName, bool triggerPanel, bool instantFading)
        {
            // Prepare info
            CheckForGameInformation(gameName);
            StopCoroutine("LibraryTransitionIn");
            StopCoroutine("LibraryTransitionOut");
            StartCoroutine("LibraryTransitionIn", instantFading);

            // Check for play, download and network buttons
            if (currentDownloadState == "true")
            {
                gameWindowBar.Play("Downloading to Downloaded");

                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(delegate
                {
                    LaunchGame(gameName);
                });

                uninstallButon.onClick.RemoveAllListeners();
                uninstallButon.onClick.AddListener(delegate
                {
                    UninstallGame(gameName);
                });

                createShortcutButton.onClick.RemoveAllListeners();
                createShortcutButton.onClick.AddListener(delegate
                {
                    CreateShortcut(gameName);
                });

                removeShortcutButton.onClick.RemoveAllListeners();
                removeShortcutButton.onClick.AddListener(delegate
                {
                    RemoveShortcut(gameName);
                });

                if (PlayerPrefs.GetString(gameName + "Shortcut") == "true")
                {
                    createShortcutButton.gameObject.SetActive(false);
                    removeShortcutButton.gameObject.SetActive(true);
                }

                else
                {
                    createShortcutButton.gameObject.SetActive(true);
                    removeShortcutButton.gameObject.SetActive(false);
                }
            }

            else
            {
                if (currentGameDownloading == true)
                {
                    for (int i = 0; i < libraryAsset.games.Count; ++i)
                    {
                        if (libraryAsset.games[i].gameTitle == gameName)
                        {
                            gameWindowBar.Play("Not Downloaded to Downloading");
                            break;
                        }
                    }
                }

                else
                {
                    gameWindowBar.Play("Uninstall to Not Downloaded");

                    downloadButton.onClick.RemoveAllListeners();
                    downloadButton.onClick.AddListener(delegate
                    {
                        DownloadGame(gameName);
                    });
                }
            }

            if (networkManager != null && networkManager.hasConnection == false && currentDownloadState == "false")
            {
                StartCoroutine("CheckForNetwork");
                gameWindowBar.Play("Not Downloaded to No Network");
            }

            // Open game panel
            gameIcon.gameObject.SetActive(true);
            CheckForLibraryItems();

            if (triggerPanel == true)
                wpm.OpenPanel(libraryPageTitle);

        }

        public void CheckForLibraryItems()
        {
            if (libraryItemParent.childCount == 0)
                libraryItemParent.parent.gameObject.SetActive(false);
            else
                libraryItemParent.parent.gameObject.SetActive(true);
        }

        public void LaunchGame(string gameName)
        {
            CheckForGameInformation(gameName);
            bool allowLaunch = true;

            foreach (Transform child in gameParent)
            {
                if (child.name == gameName)
                {
                    allowLaunch = false;
                    break;
                }
            }

            if (allowLaunch == false)
                return;

            GameObject gameGO = Instantiate(currentGamePrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            gameGO.transform.SetParent(gameParent, false);
            gameGO.gameObject.name = gameName;

            WindowManager gameWindow = gameGO.GetComponent<WindowManager>();
            gameWindow.SetupWindow();
            gameWindow.onQuitEvents.AddListener(delegate
            {
                StartCoroutine(DeleteGameInstance(gameGO));
            });

            gameGO.SetActive(true);
            gameWindow.OpenWindow();
        }

        IEnumerator DeleteGameInstance(GameObject gameObj)
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObj);
        }

        public void CreateShortcut(string gameName)
        {
            CheckForGameInformation(gameName);
            PlayerPrefs.SetString(gameName + "Shortcut", "true");

            GameObject shortcutGO = Instantiate(shortcutItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            shortcutGO.transform.SetParent(shortcutParent, false);
            shortcutGO.gameObject.name = currentGameTitle;

            // Set shortcut title
            TextMeshProUGUI shortcutTitle;
            shortcutTitle = shortcutGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            shortcutTitle.text = currentGameTitle;

            // Set shortcut icon
            Image shortcutImage;
            shortcutImage = shortcutGO.transform.Find("Icon Background/Icon").GetComponent<Image>();
            shortcutImage.sprite = currentGameSmallIcon;

            // Add shortcut button events
            DoubleClickEvent shortcutButton;
            shortcutButton = shortcutGO.GetComponent<DoubleClickEvent>();
            shortcutButton.doubleClickEvents.AddListener(delegate
            {
                LaunchGame(gameName);
            });

            removeShortcutButton.gameObject.SetActive(true);
            createShortcutButton.gameObject.SetActive(false);
        }

        public void RemoveShortcut(string gameName)
        {
            PlayerPrefs.SetString(gameName + "Shortcut", "false");
           
            removeShortcutButton.gameObject.SetActive(false);
            createShortcutButton.gameObject.SetActive(true);

            GameObject shortcutGO = shortcutParent.Find(gameName).gameObject;
            Destroy(shortcutGO);
        }

        public void DownloadGame(string gameName)
        {
            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                if (libraryAsset.games[i].gameTitle == gameName)
                {
                    libraryAsset.games[i].isDownloading = true;
                    break;
                }
            }

            CheckForGameInformation(gameName);

            GameObject downloadItemGO = Instantiate(downloadItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            downloadItemGO.transform.SetParent(downloadItemParent, false);
            downloadItemGO.gameObject.name = gameName;

            GameHubInstallationItem itemComp = downloadItemGO.GetComponent<GameHubInstallationItem>();
            itemComp.gameHubManager = this;
            itemComp.gameTitle = currentGameTitle;
            itemComp.gameIcon = currentGameSmallIcon;
            itemComp.gameSize = currentGameSize;
            itemComp.enabled = true;

            gameWindowBar.Play("Not Downloaded to Downloading");
            downloadQueue++;
            CheckForDownloadQueue();
        }

        public void DownloadFinished(string gameName)
        {
            PlayerPrefs.SetString(gameName + "Installed", "true");
            OpenGamePage(gameName, true, true);

            // For store item
            GameObject downloadIndicator;
            downloadIndicator = storeItemParent.Find(gameName).Find("Content/Installed").gameObject;
            downloadIndicator.SetActive(true);

            // For game page
            createShortcutButton.gameObject.SetActive(true);
            createShortcutButton.onClick.RemoveAllListeners();
            createShortcutButton.onClick.AddListener(delegate
            {
                CreateShortcut(gameName);
            });

            removeShortcutButton.gameObject.SetActive(false);
            removeShortcutButton.onClick.RemoveAllListeners();
            removeShortcutButton.onClick.AddListener(delegate
            {
                RemoveShortcut(gameName);
            });

            uninstallButon.gameObject.SetActive(true);
            uninstallButon.onClick.RemoveAllListeners();
            uninstallButon.onClick.AddListener(delegate
            {
                UninstallGame(gameName);
            });

            playButton.gameObject.SetActive(true);
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(delegate
            {
                LaunchGame(gameName);
            });

            if (downloadNotification != null)
            {
                downloadNotification.name = gameName;
                downloadNotification.notificationTitle = gameName;
                // downloadNotification.notificationIcon = currentGameSmallIcon;
                downloadNotification.notificationDescription = downloadNotification.notificationDescription + gameName;
                downloadNotification.CreateNotification();
            }

            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                if (libraryAsset.games[i].gameTitle == gameName)
                {
                    // Create library item
                    GameObject libraryItemGO = Instantiate(libraryItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    libraryItemGO.transform.SetParent(libraryItemParent, false);
                    libraryItemGO.gameObject.name = libraryAsset.games[i].gameTitle;

                    // Set game title
                    TextMeshProUGUI libraryItemTitle;
                    libraryItemTitle = libraryItemGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    libraryItemTitle.text = libraryAsset.games[i].gameTitle;

                    // Set game icon
                    Image shortcutImage;
                    shortcutImage = libraryItemGO.transform.Find("Icon").GetComponent<Image>();
                    shortcutImage.sprite = libraryAsset.games[i].gameSmallIcon;

                    // Add button events
                    Button libraryItemButton;
                    libraryItemButton = libraryItemGO.GetComponent<Button>();
                    libraryItemButton.onClick.AddListener(delegate
                    {
                        OpenGamePage(libraryItemGO.transform.name, true, false);
                    });

                    libraryAsset.games[i].isDownloading = false;
                    break;
                }
            }

            libraryItemParent.parent.gameObject.SetActive(true);
            gameWindowBar.Play("Downloading to Downloaded");
            downloadQueue--;
            CheckForDownloadQueue();
            CheckForLibraryItems();
        }

        public void StopDownload(string gameName)
        {
            downloadQueue--;
            CheckForDownloadQueue();

            for (int i = 0; i < libraryAsset.games.Count; ++i)
            {
                if (libraryAsset.games[i].gameTitle == gameName)
                {
                    libraryAsset.games[i].isDownloading = false;

                    if (currentGameTitle == libraryAsset.games[i].gameTitle)
                        gameWindowBar.Play("Downloading to Stop");

                    break;
                }
            }
        }

        public void CheckForDownloadQueue()
        {
            if (downloadQueue == 0)
            {
                noDownloadsText.gameObject.SetActive(true);
            }

            else
            {
                noDownloadsText.gameObject.SetActive(false);
            }
        }

        public void UninstallGame(string gameName)
        {
            gameWindowBar.Play("Downloaded to Uninstall");
            Destroy(libraryItemParent.Find(gameName).gameObject);

            // For store item
            PlayerPrefs.SetString(gameName + "Installed", "false");
            storeItemParent.Find(gameName).Find("Content/Installed").gameObject.SetActive(false);

            if (PlayerPrefs.GetString(gameName + "Shortcut") == "true")
            {
                PlayerPrefs.SetString(gameName + "Shortcut", "false");
                Destroy(shortcutParent.Find(gameName).gameObject);
            }

            CheckForLibraryItems();
            StartCoroutine("StartUninstallProcess", gameName);
        }

        public void UpdateSliderInfo()
        {
            sliderBanner.sprite = libraryAsset.games[currentSliderIndex].gameBanner;
            sliderIcon.sprite = libraryAsset.games[currentSliderIndex].gameIcon;
            sliderDescription.text = libraryAsset.games[currentSliderIndex].gameDescription;
            sliderButton.onClick.RemoveAllListeners();
            sliderButton.onClick.AddListener(delegate { OpenGamePage(libraryAsset.games[currentSliderIndex].gameTitle, true, true); });
        }

        IEnumerator StartUninstallProcess(string gameName)
        {
            yield return new WaitForSeconds(uninstallTime);

            if (networkManager != null && networkManager.hasConnection == false)
                gameWindowBar.Play("Uninstall to No Network");
            else
                gameWindowBar.Play("Uninstall to Not Downloaded");

            OpenGamePage(gameName, true, true);
        }

        IEnumerator WaitForSliderTimer()
        {
            yield return new WaitForSeconds(sliderTimer);

            if (this.enabled == true)
            {
                currentIndicatorObject.Play("Out");

                if (currentSliderIndex == sliderItems.Count - 1) { currentSliderIndex = 0; }
                else { currentSliderIndex++; }

                isInTransition = true;
                sliderTimerBar = 0;
                currentIndicatorBar.fillAmount = sliderTimerBar;
                currentIndicatorObject = sliderIndicators[currentSliderIndex];
                currentIndicatorObject.Play("In");
                StartCoroutine("SliderTransitionIn");
                StopCoroutine("WaitForSliderTimer");
            }

            else { StopCoroutine("WaitForSliderTimer"); }
        }

        public IEnumerator SliderTransitionIn()
        {
            while (transitionHelper.color.a <= 1)
            {
                float alphaValue = transitionHelper.color.a;
                alphaValue += Time.deltaTime * transitionSpeed;
                transitionHelper.color = new Color(transitionHelper.color.r, transitionHelper.color.g, transitionHelper.color.b, alphaValue);
                yield return null;
            }

            currentIndicatorBar = sliderIndicators[currentSliderIndex].transform.Find("Bar/Filled").GetComponent<Image>();
            sliderBanner.transform.localScale = new Vector3(1, 1, 1);
            isInTransition = false;
            UpdateSliderInfo();
            StartCoroutine("SliderTransitionOut");
            StopCoroutine("SliderTransitionIn");
        }

        IEnumerator SliderTransitionOut()
        {
            while (transitionHelper.color.a >= 0)
            {
                float alphaValue = transitionHelper.color.a;
                alphaValue -= Time.deltaTime * transitionSpeed;
                transitionHelper.color = new Color(transitionHelper.color.r, transitionHelper.color.g, transitionHelper.color.b, alphaValue);
                yield return null;
            }

            StartCoroutine("WaitForSliderTimer");
            StopCoroutine("SliderTransitionOut");
        }

        public IEnumerator LibraryTransitionIn(bool instantFadeIn)
        {
            if (instantFadeIn == true)
                libraryTransitionHelper.color = new Color(libraryTransitionHelper.color.r, libraryTransitionHelper.color.g, libraryTransitionHelper.color.b, 1);

            while (libraryTransitionHelper.color.a <= 1)
            {
                float alphaValue = libraryTransitionHelper.color.a;
                alphaValue += Time.deltaTime * libraryTransitionSpeed;
                libraryTransitionHelper.color = new Color(libraryTransitionHelper.color.r, libraryTransitionHelper.color.g, libraryTransitionHelper.color.b, alphaValue);
                yield return null;
            }

            // Set game info
            gameIcon.sprite = currentGameIcon;
            gameDescription.text = currentGameDescription;
            gameBanner.sprite = currentGameBanner;
            gameBanner.transform.localScale = new Vector3(1, 1, 1);

            StartCoroutine("LibraryTransitionOut");
            StopCoroutine("LibraryTransitionIn");
        }

        IEnumerator LibraryTransitionOut()
        {
            while (libraryTransitionHelper.color.a >= 0)
            {
                float alphaValue = libraryTransitionHelper.color.a;
                alphaValue -= Time.deltaTime * libraryTransitionSpeed;
                libraryTransitionHelper.color = new Color(libraryTransitionHelper.color.r, libraryTransitionHelper.color.g, libraryTransitionHelper.color.b, alphaValue);
                yield return null;
            }

            StopCoroutine("LibraryTransitionOut");
        }

        IEnumerator CheckForNetwork()
        {
            while (networkManager.hasConnection == false)
                yield return new WaitForSeconds(1f);

            if (networkManager.hasConnection == true && currentGameDownloading == false)
            {
                gameWindowBar.Play("No Network to Not Downloaded");
                StopCoroutine("CheckForNetwork");
            }

            else if (networkManager.hasConnection == true && currentGameDownloading == true)
            {
                gameWindowBar.Play("Not Downloaded to Downloading");
                StopCoroutine("CheckForNetwork");
            }
        }
    }
}