using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

namespace Michsky.DreamOS
{
    public class GetPhotoFromURL : MonoBehaviour
    {
        [Header("Resources")]
        public Button sourceButton;
        public PhotoGalleryManager photoManager;
        public Image mainImage;
        public TextMeshProUGUI titleObj;
        public TextMeshProUGUI descriptionObj;

        [Header("Settings")]
        public string photoTitle;
        public string photoDescription;
        public string imageUrl;

        void Start()
        {
            if (sourceButton == null) { sourceButton = gameObject.GetComponent<Button>(); }
            StreamFromURL();
        }

        public void UpdateImage(string newUrl) { StartCoroutine(GetTexture(newUrl)); }
        public void StreamFromURL() { StartCoroutine(GetTexture(imageUrl)); }

        IEnumerator GetTexture(string url)
        {
            Texture2D tex;
            tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
           
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, false);
            yield return www.SendWebRequest();

#if UNITY_2019
            if (www.isNetworkError || www.isHttpError)
                Debug.Log(www.error, this);
#else
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                Debug.Log(www.error, this);
#endif

            else
            {
                tex = DownloadHandlerTexture.GetContent(www);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                mainImage.sprite = sprite;
                titleObj.text = photoTitle;
                descriptionObj.text = photoDescription;

                sourceButton.onClick.RemoveAllListeners();
                sourceButton.onClick.AddListener(delegate
                {
                    photoManager.viewerTitle.text = photoTitle;
                    photoManager.viewerDescription.text = photoDescription;
                    photoManager.imageViewer.sprite = mainImage.sprite;
                    photoManager.wManager.OpenPanel(photoManager.viewerPanelName);
                });
            }
        }
    }
}