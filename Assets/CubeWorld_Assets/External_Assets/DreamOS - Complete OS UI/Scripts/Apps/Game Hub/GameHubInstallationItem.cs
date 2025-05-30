using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class GameHubInstallationItem : MonoBehaviour
    {
        [Header("Resources")]
        public Slider downloadBar;
        public Image gameImage;
        public TextMeshProUGUI gameText;
        public TextMeshProUGUI downloadStatus;

        [Header("ID")]
        public string gameTitle;
        public Sprite gameIcon;
        public float gameSize;
        [HideInInspector] public GameHubManager gameHubManager;
        Animator sliderAnimator;

        void OnEnable()
        {
            sliderAnimator = downloadBar.gameObject.GetComponent<Animator>();
            downloadBar.value = 0;
            downloadBar.maxValue = gameSize;
            gameText.text = gameTitle;
            gameImage.sprite = gameIcon;
        }

        public void StopDownload()
        {
            gameHubManager.StopDownload(gameTitle);
            Destroy(gameObject);
        }

        void Update()
        {
            if (gameHubManager.networkManager == null)
            {
                downloadBar.value += Time.deltaTime * gameHubManager.downloadSpeed;
                downloadStatus.text = downloadBar.value.ToString("F1") + " MB / " + downloadBar.maxValue.ToString("F1") + " MB";

                if (downloadBar.value == gameSize)
                    CompleteDownload();
            }

            else if (gameHubManager.networkManager.hasConnection == true)
            {
                downloadBar.value += Time.deltaTime * gameHubManager.networkManager.networkItems[gameHubManager.networkManager.currentNetworkIndex].networkSpeed;
                downloadStatus.text = downloadBar.value.ToString("F1") + " MB / " + downloadBar.maxValue.ToString("F1") + " MB";

                if (downloadBar.value == gameSize)
                    CompleteDownload();
            }
        }

        public void CompleteDownload()
        {
            this.enabled = false;
            sliderAnimator.Play("Fade Out"); // Fade-out progress bar
            gameHubManager.DownloadFinished(gameTitle);
            Destroy(gameObject);
        }
    }
}