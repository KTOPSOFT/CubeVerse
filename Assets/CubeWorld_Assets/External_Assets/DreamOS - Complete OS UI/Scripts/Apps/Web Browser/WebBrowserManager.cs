using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    public class WebBrowserManager : MonoBehaviour
    {
        // Resources
        public NetworkManager networkManager;
        public WebBrowserLibrary webLibrary;
        [SerializeField] private TextMeshProUGUI tabTitle;
        [SerializeField] private Image tabIcon;
        [SerializeField] private Transform pageViewer;
        [SerializeField] private TMP_InputField urlField;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Animator favAnimator;
        [SerializeField] private Transform favsParent;
        [SerializeField] private GameObject favListButton;
        [SerializeField] private GameObject downloadedFile;
        [SerializeField] private Transform downloadFolder;
        [SerializeField] private GameObject downloadEmpty;
        public WindowManager downloadsWindow;

        // Settings
        [SerializeField][Range(0, 30)] public float loadingTime = 1;
        [HideInInspector] public bool modSupport;
        [HideInInspector] public string saveKey = "DreamOS";

        // Multi Instance Support
        public UserManager userManager;

        // Debug
        GameObject currentTabPage;
        public List<string> previousSites = new List<string>();
        public List<FavoriteSite> favoriteSites = new List<FavoriteSite>();

        Animator pbAnimator;
        [HideInInspector] public int dirtIndex = 0;
        int urlIndex;
        int dlIndex;
        bool dirtHelper = false;
        bool updatePB = false;
        bool urlFieldActive = false;
        bool updateIndex = true;

        [System.Serializable]
        public class FavoriteSite
        {
            public Sprite icon;
            public string title = "Title";
            public string url = "Url";
        }

        void Awake()
        {
            // Check for multi instance support
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            InitializeWebBrowser();
        }

        public void InitializeWebBrowser()
        {
            if (pbAnimator == null) { pbAnimator = progressBar.gameObject.GetComponent<Animator>(); }
            OpenHome();
            
            previousButton.interactable = false;
            nextButton.interactable = false;
            progressBar.gameObject.SetActive(false);

            ListFavorites();
            RefreshFavStatus();
            ListDownloadedFiles();
        }

        void Update()
        {
            if (updatePB == true)
            {
                progressBar.value += Time.deltaTime / loadingTime;

                if (progressBar.value == 1)
                    pbAnimator.Play("Fade Out");
            }

#if ENABLE_LEGACY_INPUT_MANAGER
            if (urlFieldActive == true && Input.GetKeyDown(KeyCode.Return))
#elif ENABLE_INPUT_SYSTEM
            if (urlFieldActive == true && Keyboard.current.enterKey.wasPressedThisFrame)
#endif
            {
                OpenWebPage();
                urlField.interactable = false;
                urlField.interactable = true;
                urlFieldActive = false;
            }
        }

        public void UpdateUrlField()
        {
            urlFieldActive = true;
        }

        void OpenHome()
        {
            tabIcon.sprite = webLibrary.homePage.pageIcon;
            tabTitle.text = webLibrary.homePage.pageTitle;
            urlField.text = webLibrary.homePage.pageURL;

            foreach (Transform child in pageViewer)
                child.gameObject.SetActive(false);

            if (currentTabPage != null)
                currentTabPage.SetActive(false);

            GameObject tObject = Instantiate(webLibrary.homePage.pageContent, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            tObject.name = tObject.name.Replace("(Clone)", "").Trim();
            tObject.transform.SetParent(pageViewer, false);

            currentTabPage = tObject;
            currentTabPage.SetActive(true);
            previousSites.Add(urlField.text);
        }

        public void OpenWebPage()
        {
            StopCoroutine("StartLoadingPage");
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0;
            StartCoroutine("StartLoadingPage");
            updatePB = true;
        }

        IEnumerator StartLoadingPage()
        {
            // Start loading the page depending on loading time
            yield return new WaitForSeconds(loadingTime + 0.4f);
            InitializeWebPage();
            StopCoroutine("StartLoadingPage");
        }

        public void InitializeWebPage()
        {
            if (urlField.text == webLibrary.homePage.pageURL)
            {
                tabIcon.sprite = webLibrary.homePage.pageIcon;
                tabTitle.text = webLibrary.homePage.pageTitle;

                Destroy(currentTabPage);

                GameObject tObject = Instantiate(webLibrary.homePage.pageContent, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                tObject.name = tObject.name.Replace("(Clone)", "").Trim();
                tObject.transform.SetParent(pageViewer, false);

                currentTabPage = tObject;
                currentTabPage.SetActive(true);
            }

            else
            {
                for (int i = 0; i < webLibrary.webPages.Count; i++)
                {
                    if (urlField.text.ToLower() == webLibrary.webPages[i].pageURL
                        || urlField.text.ToLower() == "www." + webLibrary.webPages[i].pageURL)
                    {
                        urlIndex = i;
                        break;
                    }
                }

                if (networkManager.hasConnection == true && urlField.text.ToLower() == webLibrary.webPages[urlIndex].pageURL
                    || networkManager.hasConnection == true && urlField.text.ToLower() == "www." + webLibrary.webPages[urlIndex].pageURL)
                {
                    tabIcon.sprite = webLibrary.webPages[urlIndex].pageIcon;
                    tabTitle.text = webLibrary.webPages[urlIndex].pageTitle;
                    urlField.text = webLibrary.webPages[urlIndex].pageURL.Replace("www.", "").Trim();

                    Destroy(currentTabPage);

                    GameObject tObject = Instantiate(webLibrary.webPages[urlIndex].pageContent, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    tObject.name = tObject.name.Replace("(Clone)", "").Trim();
                    tObject.transform.SetParent(pageViewer, false);

                    currentTabPage = tObject;
                    currentTabPage.SetActive(true);
                }

                else if (networkManager.hasConnection == false)
                {
                    tabIcon.sprite = webLibrary.noConnectionPage.pageIcon;
                    tabTitle.text = webLibrary.noConnectionPage.pageTitle;
                    urlField.text = webLibrary.noConnectionPage.pageURL;

                    Destroy(currentTabPage);

                    GameObject tObject = Instantiate(webLibrary.noConnectionPage.pageContent, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    tObject.name = tObject.name.Replace("(Clone)", "").Trim();
                    tObject.transform.SetParent(pageViewer, false);

                    currentTabPage = tObject;
                    currentTabPage.SetActive(true);
                }

                else
                {
                    tabIcon.sprite = webLibrary.notFoundPage.pageIcon;
                    tabTitle.text = webLibrary.notFoundPage.pageTitle;
                    urlField.text = webLibrary.notFoundPage.pageURL;

                    Destroy(currentTabPage);

                    GameObject tObject = Instantiate(webLibrary.notFoundPage.pageContent, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    tObject.name = tObject.name.Replace("(Clone)", "").Trim();
                    tObject.transform.SetParent(pageViewer, false);

                    currentTabPage = tObject;
                    currentTabPage.SetActive(true);
                }
            }

            progressBar.value = 0;
            progressBar.gameObject.SetActive(false);
            updatePB = false;

            if (dirtHelper == false)
            {
                dirtIndex += 1;
                previousSites.Add(urlField.text);
            }

            if (updateIndex == true) { dirtIndex = previousSites.Count - 1; }
            else { updateIndex = true; }

            dirtHelper = false;
            CheckPreviousActions();
            RefreshFavStatus();
        }

        public void OpenPage(string url)
        {
            // Open a specific page
            urlField.text = url;
            OpenWebPage();
        }

        public void GoBack()
        {
            // Simple back method
            if (dirtIndex >= 1)
            {
                dirtIndex -= 1;
                urlField.text = previousSites[dirtIndex];
                dirtHelper = true;
                OpenWebPage();
            }

            updateIndex = false;
            CheckPreviousActions();
        }

        public void GoForward()
        {
            // Simple forward method
            if (dirtIndex <= previousSites.Count - 2)
            {
                dirtIndex += 1;
                urlField.text = previousSites[dirtIndex];
                dirtHelper = true;
                OpenWebPage();
            }

            updateIndex = false;
            CheckPreviousActions();
        }

        public void Refresh()
        {
            // Simple refresh method
            urlField.text = previousSites[dirtIndex];
            dirtHelper = true;
            OpenWebPage();
        }

        public void CheckPreviousActions()
        {
            // Check for previous button
            if (dirtIndex == 0) { previousButton.interactable = false; }
            else { previousButton.interactable = true; }

            // Check for next button
            if (dirtIndex == previousSites.Count - 1) { nextButton.interactable = false; }
            else { nextButton.interactable = true; }
        }

        public void SetFavorite()
        {
            // Add the current website to favorites - only if it is not already added
            if (PlayerPrefs.GetString(saveKey + urlField.text) == "")
            {
                FavoriteSite fav = new FavoriteSite();
                fav.icon = tabIcon.sprite;
                fav.title = tabTitle.text;
                fav.url = urlField.text;
                favoriteSites.Add(fav);

                GameObject favObj = Instantiate(favListButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                favObj.name = urlField.text;
                favObj.transform.SetParent(favsParent, false);

                Image favObjImg = favObj.transform.Find("Icon").GetComponent<Image>();
                favObjImg.sprite = tabIcon.sprite;

                TextMeshProUGUI favObjTxt = favObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                favObjTxt.text = tabTitle.text;

                PlayerPrefs.SetString(saveKey + fav.url, fav.url);

                Button favObjBtn = favObj.GetComponent<Button>();
                favObjBtn.onClick.AddListener(delegate
                {
                    OpenPage(PlayerPrefs.GetString(saveKey + fav.url));
                });

                favAnimator.Play("Enable");
            }

            // If not, delete it from favorites
            else
            {
                for (int i = 0; i < favoriteSites.Count; i++)
                {
                    if (urlField.text.ToLower() == favoriteSites[i].url)
                    {
                        GameObject favObj = favsParent.Find(favoriteSites[i].url).gameObject;
                        Destroy(favObj);
                        favoriteSites.RemoveAt(i);
                        PlayerPrefs.DeleteKey(saveKey + favObj.name);
                        favAnimator.Play("Disable");
                    }
                }
            }
        }

        public void RefreshFavStatus()
        {
            // Simple animating solution for refreshing favorite status
            if (favAnimator.gameObject.activeInHierarchy == true)
            {
                if (PlayerPrefs.GetString(saveKey + urlField.text) == "") { favAnimator.Play("Disable"); }
                else { favAnimator.Play("Enable"); }
            }
        }

        public void ListFavorites()
        {
            // First of all, clear the parent object
            foreach (Transform child in favsParent)
                Destroy(child.gameObject);

            // Start searching in web library list
            for (int i = 0; i < webLibrary.webPages.Count; i++)
            {
                int index = i;

                // Create the items which was already added to fav list
                if (webLibrary.webPages[i].pageURL == PlayerPrefs.GetString(saveKey + webLibrary.webPages[i].pageURL))
                {
                    FavoriteSite fav = new FavoriteSite();

                    GameObject favObj = Instantiate(favListButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    favObj.name = webLibrary.webPages[index].pageURL;
                    favObj.transform.SetParent(favsParent, false);

                    Image favObjImg = favObj.transform.Find("Icon").GetComponent<Image>();
                    favObjImg.sprite = webLibrary.webPages[index].pageIcon;

                    TextMeshProUGUI favObjTxt = favObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    favObjTxt.text = webLibrary.webPages[index].pageTitle;

                    Button favObjBtn = favObj.GetComponent<Button>();
                    favObjBtn.onClick.AddListener(delegate
                    {
                        OpenPage(PlayerPrefs.GetString(saveKey + webLibrary.webPages[index].pageURL));
                    });

                    fav.url = PlayerPrefs.GetString(saveKey + webLibrary.webPages[index].pageURL);
                    favoriteSites.Add(fav);
                }
            }
        }

        public void ClearFavoriteList()
        {
            // Search and clear favorite list
            for (int i = 0; i < favoriteSites.Count; i++)
            {
                GameObject favObj = favsParent.Find(favoriteSites[i].url).gameObject;
                Destroy(favObj);
                PlayerPrefs.DeleteKey(saveKey + favoriteSites[i].url);
            }
        }

        public void DownloadFile(string title)
        {
            // Open download window
            if (downloadsWindow != null)
                downloadsWindow.OpenWindow();

            // Checking if the file is already in download process
            Transform checker = downloadFolder.Find(title);

            // And if it is, don't go further
            if (checker != null)
                return;

            // If not, search for the file in web library
            for (int i = 0; i < webLibrary.dlFiles.Count; i++)
            {
                if (title == webLibrary.dlFiles[i].fileName)
                {
                    dlIndex = i;
                    PlayerPrefs.SetInt(saveKey + title + "EventHelper", i);
                    break;
                }
            }

            // Start downloading depending on the file state (0: not downloaded, 1: in progress, 2: downloaded)
            if (PlayerPrefs.GetInt(saveKey + "Downloaded" + title) == 0 || PlayerPrefs.GetInt(saveKey + "Downloaded" + title) == 1)
            {
                GameObject dfObj = Instantiate(downloadedFile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                dfObj.name = title;
                dfObj.transform.SetParent(downloadFolder, false);

                Image dfImg = dfObj.transform.Find("Icon").GetComponent<Image>();
                dfImg.sprite = webLibrary.dlFiles[dlIndex].fileIcon;

                TextMeshProUGUI dfTxt = dfObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                dfTxt.text = webLibrary.dlFiles[dlIndex].fileName;

                TextMeshProUGUI dfSize = dfObj.transform.Find("Size").GetComponent<TextMeshProUGUI>();
                dfSize.text = webLibrary.dlFiles[dlIndex].fileSize.ToString() + " MB";

                DownloadFile dfVar = dfObj.gameObject.GetComponent<DownloadFile>();
                dfVar.wbm = this;

                if (networkManager.dynamicNetwork == true) { dfVar.downloadMultiplier = networkManager.networkItems[networkManager.currentNetworkIndex].networkSpeed; }
                else { dfVar.downloadMultiplier = networkManager.defaultSpeed; }

                dfVar.fileSize = webLibrary.dlFiles[dlIndex].fileSize;
                dfVar.enableNotification = true;

                Button dfObjBtn = dfObj.GetComponent<Button>();
                dfObjBtn.onClick.AddListener(delegate
                {
                    webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].externalEvents.Invoke();

                    try
                    {
                        if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileType == WebBrowserLibrary.FileType.Music)
                        {
                            MusicPlayerManager mp = GameObject.Find("Managers/Music Player").GetComponent<MusicPlayerManager>();
                            WindowManager wm = mp.musicPanelManager.gameObject.GetComponent<WindowManager>();
                            wm.OpenWindow();
                            mp.PlayCustomClip(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].musicReference, // Music clip
                             webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileIcon, // Music cover
                             webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileName, // Music name
                             "Downloads"); // Artist name
                        }

                        else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileType == WebBrowserLibrary.FileType.Note)
                        {
                            NotepadManager nm = GameObject.Find("Managers/Notepad").GetComponent<NotepadManager>();
                            WindowManager wm = nm.notepadWindow.gameObject.GetComponent<WindowManager>();
                            wm.OpenWindow();
                            nm.OpenCustomNote(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileName, // Note title
                                webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].noteReference); // Note content
                        }

                        else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileType == WebBrowserLibrary.FileType.Photo)
                        {
                            PhotoGalleryManager pm = GameObject.Find("Managers/Photo Gallery").GetComponent<PhotoGalleryManager>();
                            WindowManager wm = pm.photoGalleryWindow.gameObject.GetComponent<WindowManager>();
                            wm.OpenWindow();
                            pm.OpenCustomSprite(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].photoReference, // Sprite
                                webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileName, // Photo title
                                "Downloads"); // Photo description
                        }

                        else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileType == WebBrowserLibrary.FileType.Video)
                        {
                            VideoPlayerManager vpm = GameObject.Find("Managers/Video Player").GetComponent<VideoPlayerManager>();
                            WindowManager wm = vpm.videoPlayerWindow.gameObject.GetComponent<WindowManager>();
                            wm.OpenWindow();
                            vpm.wManager.OpenPanel(vpm.videoPanelName);
                            vpm.PlayVideoClip(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].videoReference, // Video clip
                                webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + title + "EventHelper")].fileName); // Video name
                        }
                    }

                    catch { Debug.LogError("<b>[Web Browser]</b> Cannot process the events due to missing resources.", dfObjBtn.gameObject); }
                });

                PlayerPrefs.SetInt(saveKey + "Downloaded" + title, 1);
                dfObjBtn.interactable = false;

                if (downloadFolder != null && downloadEmpty != null)
                    downloadEmpty.SetActive(false);
            }
        }

        public void ListDownloadedFiles()
        {
            // First of all, clear the parent object
            foreach (Transform child in downloadFolder)
                Destroy(child.gameObject);

            // Start searching in web library list
            for (int i = 0; i < webLibrary.dlFiles.Count; i++)
            {
                int index = i;

                // Create the files which was already downloaded
                if (PlayerPrefs.GetInt(saveKey + "Downloaded" + webLibrary.dlFiles[i].fileName) == 2)
                {
                    GameObject dfObj = Instantiate(downloadedFile, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    dfObj.name = webLibrary.dlFiles[index].fileName;
                    dfObj.transform.SetParent(downloadFolder, false);

                    Image dfImg = dfObj.transform.Find("Icon").GetComponent<Image>();
                    dfImg.sprite = webLibrary.dlFiles[index].fileIcon;

                    TextMeshProUGUI dfTxt = dfObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    dfTxt.text = webLibrary.dlFiles[index].fileName;

                    TextMeshProUGUI dfSize = dfObj.transform.Find("Size").GetComponent<TextMeshProUGUI>();
                    dfSize.text = webLibrary.dlFiles[index].fileSize.ToString() + " MB";

                    DownloadFile dfVar = dfObj.gameObject.GetComponent<DownloadFile>();

                    if (networkManager.dynamicNetwork == true)
                        dfVar.downloadMultiplier = networkManager.networkItems[networkManager.currentNetworkIndex].networkSpeed;
                    else
                        dfVar.downloadMultiplier = networkManager.defaultSpeed;

                    dfVar.fileSize = webLibrary.dlFiles[index].fileSize;

                    Button dfObjBtn = dfObj.GetComponent<Button>();
                    dfObjBtn.onClick.AddListener(delegate
                    {
                        webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].externalEvents.Invoke();

                        try
                        {
                            if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileType == WebBrowserLibrary.FileType.Music)
                            {
                                MusicPlayerManager mp = GameObject.Find("Managers/Music Player").GetComponent<MusicPlayerManager>();
                                WindowManager wm = mp.musicPanelManager.gameObject.GetComponent<WindowManager>();
                                wm.OpenWindow();
                                mp.PlayCustomClip(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].musicReference, // Music clip
                                 webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileIcon, // Music cover
                                 webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileName, // Music name
                                 "Downloads"); // Artist name
                            }

                            else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileType == WebBrowserLibrary.FileType.Note)
                            {
                                NotepadManager nm = GameObject.Find("Managers/Notepad").GetComponent<NotepadManager>();
                                WindowManager wm = nm.notepadWindow.gameObject.GetComponent<WindowManager>();
                                wm.OpenWindow();
                                nm.OpenCustomNote(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileName, // Note name
                                    webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].noteReference); // Note content
                            }

                            else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileType == WebBrowserLibrary.FileType.Photo)
                            {
                                PhotoGalleryManager pm = GameObject.Find("Managers/Photo Gallery").GetComponent<PhotoGalleryManager>();
                                WindowManager wm = pm.photoGalleryWindow.gameObject.GetComponent<WindowManager>();
                                wm.OpenWindow();
                                pm.OpenCustomSprite(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].photoReference, // Sprite
                                    webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileName, // Photo name
                                    "Downloads"); // Description
                            }

                            else if (webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileType == WebBrowserLibrary.FileType.Video)
                            {
                                VideoPlayerManager vpm = GameObject.Find("Managers/Video Player").GetComponent<VideoPlayerManager>();
                                WindowManager wm = vpm.videoPlayerWindow.gameObject.GetComponent<WindowManager>();
                                wm.OpenWindow();
                                vpm.wManager.OpenPanel(vpm.videoPanelName);
                                vpm.PlayVideoClip(webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].videoReference, // Video clip
                                    webLibrary.dlFiles[PlayerPrefs.GetInt(saveKey + dfObj.name + "EventHelper")].fileName); // Video name
                            }
                        }

                        catch { Debug.LogError("<b>[Web Browser]</b> Cannot process the events due to missing resources.", dfObjBtn.gameObject); }
                    });

                    dfObjBtn.interactable = false;
                }
            }

            // If there isn't any downloaded file, then enable the label
            if (downloadFolder != null && downloadEmpty != null && downloadFolder.childCount == 0) { downloadEmpty.SetActive(true); }
            else if (downloadEmpty != null) { downloadEmpty.SetActive(false); }
        }

        public void DeleteDownloadFile(string title)
        {
            // Search for the file in the library
            for (int i = 0; i < webLibrary.dlFiles.Count; i++)
            {
                // When we got a match
                if (title == webLibrary.dlFiles[i].fileName)
                {
                    // Find the object in download folder, destroy it, and delete the data
                    GameObject dlObj = downloadFolder.Find(webLibrary.dlFiles[i].fileName).gameObject;
                    Destroy(dlObj);
                    PlayerPrefs.SetInt(saveKey + "Downloaded" + title, 0);
                }
            }

            // If there isn't any downloaded file, then enable the label
            if (downloadFolder != null && downloadEmpty != null && downloadFolder.childCount == 1)
                downloadEmpty.SetActive(true);
            else if (downloadEmpty != null)
                downloadEmpty.SetActive(false);
        }
    }
}