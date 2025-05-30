using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class NetworkManager : MonoBehaviour
    {
        // List
        public List<NetworkItem> networkItems = new List<NetworkItem>();

        // Resources
        [SerializeField] private Transform networkListParent;
        public GameObject networkItem;
        [SerializeField] private AudioSource feedbackSource;
        [SerializeField] private Image centerIndicator;

        // Settings
        public bool dynamicNetwork = true;
        [Range(0.1f, 100)] public float defaultSpeed = 20;
        public bool hasConnection;
        public int currentNetworkIndex = 0;
        [SerializeField] private Sprite signalDisconnected;
        [SerializeField] private Sprite signalWeak;
        [SerializeField] private Sprite signalNormal;
        [SerializeField] private Sprite signalStrong;
        [SerializeField] private Sprite signalBest;
        [SerializeField] private AudioClip wrongPassSound;
        string saveKey = "DreamOS";

        // Multi Instance Support
        public UserManager userManager;

        [System.Serializable]
        public class NetworkItem
        {
            public string networkTitle = "My Network";
            public string password = "password";
            public SignalPower signalPower;
            [Range(0.1f, 100)] public float networkSpeed = 20;
            public bool hasPassword;
        }

        public enum SignalPower { Weak, Normal, Strong, Best }

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            ListNetworks();
        }

        public void ListNetworks()
        {
            if (dynamicNetwork == false) { hasConnection = true; return; }
           
            if (PlayerPrefs.HasKey(saveKey + "CurrentNetworkIndex") == true) { currentNetworkIndex = PlayerPrefs.GetInt(saveKey + "CurrentNetworkIndex"); }
            else if (centerIndicator != null) { centerIndicator.sprite = signalDisconnected; }

            foreach (Transform child in networkListParent) { Destroy(child.gameObject); }
            for (int i = 0; i < networkItems.Count; i++)
            {
                int index = i;

                if (i == currentNetworkIndex && PlayerPrefs.HasKey(saveKey + "CurrentNetworkIndex") == true)
                {
                    GameObject networkObj = Instantiate(networkItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    networkObj.name = networkItems[index].networkTitle;
                    networkObj.transform.SetParent(networkListParent, false);

                    TextMeshProUGUI networkTxt = networkObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    networkTxt.text = networkItems[index].networkTitle;

                    TMP_InputField passField = networkObj.transform.Find("Connection/Password").GetComponent<TMP_InputField>();
                    passField.gameObject.SetActive(false);

                    Image signalImg = networkObj.transform.Find("Icon").GetComponent<Image>();
                    Image lockedImg = networkObj.transform.Find("Locked").GetComponent<Image>();
                    GameObject indicatorObj = networkObj.transform.Find("Indicator").gameObject;

                    Button connectBtn = networkObj.transform.Find("Connection/Connect").GetComponent<Button>();
                    Button disconnectBtn = networkObj.transform.Find("Connection/Disconnect").GetComponent<Button>();

                    if (networkItems[networkObj.transform.GetSiblingIndex()].hasPassword == true)
                    {
                        lockedImg.gameObject.SetActive(true);

                        connectBtn.onClick.AddListener(delegate
                        {
                            if (passField.text == networkItems[networkObj.transform.GetSiblingIndex()].password)
                            {
                                passField.gameObject.SetActive(false);
                                connectBtn.gameObject.SetActive(false);
                                disconnectBtn.gameObject.SetActive(true);
                                indicatorObj.SetActive(true);

                                if (PlayerPrefs.HasKey(saveKey + "ConnectedNetworkTitle") == true)
                                {
                                    GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
                                    Button dcBtn = cNetwork.transform.Find("Connection/Disconnect").GetComponent<Button>();
                                    dcBtn.onClick.Invoke();
                                }

                                currentNetworkIndex = networkObj.transform.GetSiblingIndex();
                                hasConnection = true;

                                PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkObj.name);
                                PlayerPrefs.SetInt(saveKey + "CurrentNetworkIndex", currentNetworkIndex);
      
                                SetIndicatorIcon(currentNetworkIndex);
                            }

                            else if (feedbackSource != null) { feedbackSource.PlayOneShot(wrongPassSound); }
                        });
                    }

                    else if (networkItems[networkObj.transform.GetSiblingIndex()].hasPassword == false)
                    {
                        passField.gameObject.SetActive(false);
                        lockedImg.gameObject.SetActive(false);

                        connectBtn.onClick.AddListener(delegate
                        {
                            connectBtn.gameObject.SetActive(false);
                            disconnectBtn.gameObject.SetActive(true);
                            indicatorObj.SetActive(true);

                            if (PlayerPrefs.HasKey(saveKey + "ConnectedNetworkTitle") == true)
                            {
                                GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
                                Button dcBtn = cNetwork.transform.Find("Connection/Disconnect").GetComponent<Button>();
                                dcBtn.onClick.Invoke();
                            }

                            currentNetworkIndex = networkObj.transform.GetSiblingIndex();
                            hasConnection = true;
                         
                            PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkObj.name);
                            PlayerPrefs.SetInt(saveKey + "CurrentNetworkIndex", currentNetworkIndex);
                          
                            SetIndicatorIcon(currentNetworkIndex);
                        });
                    }

                    disconnectBtn.onClick.AddListener(delegate
                    {
                        passField.gameObject.SetActive(false);
                        connectBtn.gameObject.SetActive(true);
                        disconnectBtn.gameObject.SetActive(false);
                        indicatorObj.SetActive(false);
                      
                        hasConnection = false;

                        PlayerPrefs.DeleteKey(saveKey + "CurrentNetworkIndex");
                        PlayerPrefs.DeleteKey(saveKey + "ConnectedNetworkTitle");

                        if (centerIndicator != null) { centerIndicator.sprite = signalDisconnected; }
                    });

                    if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Weak) { signalImg.sprite = signalWeak; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Normal) { signalImg.sprite = signalNormal; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Strong) { signalImg.sprite = signalStrong; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Best) { signalImg.sprite = signalBest; }

                    connectBtn.gameObject.SetActive(false);
                    disconnectBtn.gameObject.SetActive(true);
                    indicatorObj.SetActive(true);
                   
                    PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkObj.name);
                   
                    currentNetworkIndex = networkObj.transform.GetSiblingIndex();
                    hasConnection = true;

                    SetIndicatorIcon(currentNetworkIndex);
                }

                else
                {
                    GameObject networkObj = Instantiate(networkItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    networkObj.name = networkItems[index].networkTitle;
                    networkObj.transform.SetParent(networkListParent, false);

                    TextMeshProUGUI networkTxt = networkObj.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    networkTxt.text = networkItems[index].networkTitle;

                    TMP_InputField passField = networkObj.transform.Find("Connection/Password").GetComponent<TMP_InputField>();

                    Image signalImg = networkObj.transform.Find("Icon").GetComponent<Image>();
                    Image lockedImg = networkObj.transform.Find("Locked").GetComponent<Image>();
                    GameObject indicatorObj = networkObj.transform.Find("Indicator").gameObject;

                    Button connectBtn = networkObj.transform.Find("Connection/Connect").GetComponent<Button>();
                    Button disconnectBtn = networkObj.transform.Find("Connection/Disconnect").GetComponent<Button>();

                    if (networkItems[networkObj.transform.GetSiblingIndex()].hasPassword == true)
                    {
                        lockedImg.gameObject.SetActive(true);

                        connectBtn.onClick.AddListener(delegate
                        {
                            if (passField.text == networkItems[networkObj.transform.GetSiblingIndex()].password)
                            {
                                passField.gameObject.SetActive(false);
                                connectBtn.gameObject.SetActive(false);
                                disconnectBtn.gameObject.SetActive(true);
                                indicatorObj.SetActive(true);

                                if (PlayerPrefs.HasKey(saveKey + "ConnectedNetworkTitle") == true)
                                {
                                    GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
                                    Button dcBtn = cNetwork.transform.Find("Connection/Disconnect").GetComponent<Button>();
                                    dcBtn.onClick.Invoke();
                                }

                                currentNetworkIndex = networkObj.transform.GetSiblingIndex();
                                hasConnection = true;

                                PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkObj.name);
                                PlayerPrefs.SetInt(saveKey + "CurrentNetworkIndex", currentNetworkIndex);

                                SetIndicatorIcon(currentNetworkIndex);
                            }

                            else if (feedbackSource != null) { feedbackSource.PlayOneShot(wrongPassSound); }
                        });
                    }

                    else
                    {
                        passField.gameObject.SetActive(false);
                        lockedImg.gameObject.SetActive(false);

                        connectBtn.onClick.AddListener(delegate
                        {
                            connectBtn.gameObject.SetActive(false);
                            disconnectBtn.gameObject.SetActive(true);
                            indicatorObj.SetActive(true);

                            if (PlayerPrefs.HasKey(saveKey + "ConnectedNetworkTitle") == true)
                            {
                                GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
                                Button dcBtn = cNetwork.transform.Find("Connection/Disconnect").GetComponent<Button>();
                                dcBtn.onClick.Invoke();
                            }

                            currentNetworkIndex = networkObj.transform.GetSiblingIndex();
                            hasConnection = true;

                            PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkObj.name);
                            PlayerPrefs.SetInt(saveKey + "CurrentNetworkIndex", currentNetworkIndex);

                            SetIndicatorIcon(currentNetworkIndex);
                        });
                    }

                    disconnectBtn.onClick.AddListener(delegate
                    {
                        connectBtn.gameObject.SetActive(true);
                        disconnectBtn.gameObject.SetActive(false);
                        indicatorObj.SetActive(false);
                      
                        hasConnection = false;
                     
                        PlayerPrefs.DeleteKey(saveKey + "CurrentNetworkIndex");
                        PlayerPrefs.DeleteKey(saveKey + "ConnectedNetworkTitle");

                        if (centerIndicator != null) { centerIndicator.sprite = signalDisconnected; }
                    });

                    if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Weak) { signalImg.sprite = signalWeak; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Normal) { signalImg.sprite = signalNormal; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Strong) { signalImg.sprite = signalStrong; }
                    else if (networkItems[networkObj.transform.GetSiblingIndex()].signalPower == SignalPower.Best) { signalImg.sprite = signalBest; }

                    connectBtn.gameObject.SetActive(true);
                    disconnectBtn.gameObject.SetActive(false);
                }
            }
        }

        public void ClearNetworkList()
        {
            StartCoroutine("StartClearingList");
        }

        IEnumerator StartClearingList()
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < networkItems.Count; i++)
            {
                GameObject netObj = networkListParent.Find(networkItems[i].networkTitle).gameObject;
                Destroy(netObj);
            }

            StopCoroutine("StartClearingList");
        }

        public void CreateNetwork(string title, string password, SignalPower signalPower)
        {
            NetworkItem nitem = new NetworkItem();
            nitem.networkTitle = title;
            nitem.signalPower = signalPower;

            if (password == "" || password == null) { nitem.hasPassword = false; }
            else { nitem.hasPassword = true; nitem.password = password; }

            networkItems.Add(nitem);
        }

        public void ConntectToNetwork(string title, string password)
        {
            for (int i = 0; i < networkItems.Count; i++)
            {
                if (title == networkItems[i].networkTitle && password == networkItems[i].password)
                {
                    if (PlayerPrefs.HasKey(saveKey + "ConnectedNetworkTitle") == true)
                    {
                        GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
                        Button dcBtn = cNetwork.transform.Find("Disconnect").GetComponent<Button>();
                        dcBtn.onClick.Invoke();
                    }

                    currentNetworkIndex = i;
                    hasConnection = true;

                    PlayerPrefs.SetString(saveKey + "ConnectedNetworkTitle", networkItems[currentNetworkIndex].networkTitle);
                    PlayerPrefs.SetInt(saveKey + "CurrentNetworkIndex", currentNetworkIndex);

                    SetIndicatorIcon(currentNetworkIndex);
                    break;
                }
            }
        }

        public void DisconnectFromNetwork()
        {
            try
            {
                GameObject cNetwork = networkListParent.Find(PlayerPrefs.GetString(saveKey + "ConnectedNetworkTitle")).gameObject;
              
                Button dcBtn = cNetwork.transform.Find("Disconnect").GetComponent<Button>();
                dcBtn.onClick.Invoke();
               
                hasConnection = false;

                PlayerPrefs.DeleteKey(saveKey + "CurrentNetworkIndex");
                PlayerPrefs.DeleteKey(saveKey + "ConnectedNetworkTitle");

                if (centerIndicator != null) { centerIndicator.sprite = signalDisconnected; }
            }

            catch { }
        }

        void SetIndicatorIcon(int index)
        {
            if (centerIndicator != null && networkItems[index].signalPower == SignalPower.Weak) { centerIndicator.sprite = signalWeak; }
            else if (centerIndicator != null && networkItems[index].signalPower == SignalPower.Normal) { centerIndicator.sprite = signalStrong; }
            else if (centerIndicator != null && networkItems[index].signalPower == SignalPower.Strong) { centerIndicator.sprite = signalStrong; }
            else if (centerIndicator != null && networkItems[index].signalPower == SignalPower.Best) { centerIndicator.sprite = signalBest; }
        }

        public void AddNetwork()
        {
            NetworkItem nitem = new NetworkItem();
            nitem.networkTitle = "New Network";
            networkItems.Add(nitem);
        }
    }
}