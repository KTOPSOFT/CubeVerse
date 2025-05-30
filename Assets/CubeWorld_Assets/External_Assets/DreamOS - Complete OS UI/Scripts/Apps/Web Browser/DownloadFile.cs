using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Button))]
    public class DownloadFile : MonoBehaviour
    {
        [Header("Resources")]
        public Slider downloadBar;
        public TextMeshProUGUI downloadStatus;

        [Header("Settings")]
        public float fileSize;
        public float downloadMultiplier;
        public string completeDescription = "Download completed";
        public bool enableNotification;

        // Hidden variables
        Button objButton;
        Animator sliderAnimator;
        NotificationCreator downloadNotifier;
        bool updateDownloadBar = false;

        [HideInInspector] public WebBrowserManager wbm;

        void Start()
        {
            if (wbm == null) { wbm = (WebBrowserManager)GameObject.FindObjectsOfType(typeof(WebBrowserManager))[0]; }
            // if (wbm == null) { Debug.Log("<b>[Download File]</b> Cannot find any available Web Browser Manager."); return; } }

            // If download isn't completed and is in progress
            if (PlayerPrefs.GetInt(wbm.saveKey + "Downloaded" + gameObject.name) == 0 || PlayerPrefs.GetInt(wbm.saveKey + "Downloaded" + gameObject.name) == 1)
            {
                // Check for notification settings
                if (enableNotification == true && downloadNotifier == null)
                    downloadNotifier = GameObject.Find("Download Notifier").gameObject.GetComponent<NotificationCreator>();

                // Get the necessary components and enable updating
                objButton = gameObject.GetComponent<Button>();
                sliderAnimator = downloadBar.gameObject.GetComponent<Animator>();
                downloadBar.value = 0;
                downloadBar.maxValue = fileSize;
                updateDownloadBar = true;
            }

            // If download is completed
            else
            {
                objButton = gameObject.GetComponent<Button>();
                objButton.interactable = true;
                Destroy(downloadStatus.gameObject);
                this.enabled = false;
            }
        }

        void Update()
        {
            // If updating is enabled and there's a connected network
            if (updateDownloadBar == true && PlayerPrefs.HasKey(wbm.userManager.machineID + "ConnectedNetworkTitle") == true)
            {
                // If download speed is different than the connected network, update it
                if (wbm != null && wbm.networkManager.dynamicNetwork == true && downloadMultiplier != wbm.networkManager.networkItems[wbm.networkManager.currentNetworkIndex].networkSpeed)
                    downloadMultiplier = wbm.networkManager.networkItems[wbm.networkManager.currentNetworkIndex].networkSpeed;
                else if (wbm != null && wbm.networkManager.dynamicNetwork == false && downloadMultiplier != wbm.networkManager.defaultSpeed)
                    downloadMultiplier = wbm.networkManager.defaultSpeed;

                // Increase the visuals depending on download size
                downloadBar.value += Time.deltaTime * downloadMultiplier;
                downloadStatus.text = downloadBar.value.ToString("F1") + " MB / " + downloadBar.maxValue.ToString("F1") + " MB";

                // When the download is completed
                if (downloadBar.value == fileSize)
                {
                    objButton.interactable = true; // Make it interactable
                    sliderAnimator.Play("Fade Out"); // Fade-out progress bar
                    StartCoroutine("EndProcess"); // Start EndProcess events
                    updateDownloadBar = false; // Disable uddating
                    PlayerPrefs.SetInt(wbm.userManager.machineID + "Downloaded" + gameObject.name, 2); // Change data

                    // Create a notification if it's enabled
                    if (enableNotification == true && downloadNotifier != null)
                    {
                        downloadNotifier.notificationTitle = gameObject.name;
                        downloadNotifier.popupDescription = completeDescription;
                        downloadNotifier.CreateOnlyPopup();
                    }
                }
            }
        }

        public void DeleteFile()
        {
            if (wbm == null) { wbm = (WebBrowserManager)GameObject.FindObjectsOfType(typeof(WebBrowserManager))[0]; }
            wbm.DeleteDownloadFile(gameObject.name);
        }

        IEnumerator EndProcess()
        {
            // Delete some visual objects on complete
            yield return new WaitForSeconds(1);
            Destroy(downloadBar.gameObject);
            Destroy(downloadStatus.gameObject);
            this.enabled = false;
        }
    }
}