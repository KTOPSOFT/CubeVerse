using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Notifications/Notification Creator")]
    public class NotificationCreator : MonoBehaviour
    {
        // Resources
        public NotificationManager managerScript;
        public Sprite notificationIcon;

        // Content
        public string notificationTitle;
        [TextArea] public string notificationDescription;
        [TextArea] public string popupDescription;
        public List<ButtonItem> notificationButtons = new List<ButtonItem>();

        // Settings
        public bool enableButtonIcon;
        public bool enablePopupNotification;
        public bool showOnlyOnce;
        public bool enableNotificationSound = true;
        public AudioClip notificationSound;
        public Color notificationColor = new Color(255, 255, 255, 255);
        public NotificationType notificationType;

        // Placeholder
        string textHelper;
        TextMeshProUGUI setItemText;
        AudioSource aSource;

        [System.Serializable]
        public class ButtonItem
        {
            public string buttonText = "Button";
            public Sprite buttonIcon;
            public bool isCloseButton;
            public UnityEvent OnButtonClick;
        }

        public enum NotificationType { Standard }

        void Awake()
        {
            if (showOnlyOnce == true && PlayerPrefs.GetInt(gameObject.name + "Shown") == 1) { Destroy(gameObject); }
            if (enableNotificationSound == true) { aSource = managerScript.gameObject.GetComponent<AudioSource>(); }
        }

        public void CreateOnlyPopup()
        {
            // Spawn popup notification to the requested parent
            GameObject go = Instantiate(managerScript.popupNotification, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            go.transform.SetParent(managerScript.popupNotificationParent, false);

            // Set animation speed
            Animator goAnimator = go.gameObject.GetComponent<Animator>();
            goAnimator.SetFloat("Visible Time", managerScript.visibleTime);

            // Set popup resources with requested stuff
            managerScript.popupIcon = go.transform.Find("Content/Icon Parent/Icon").gameObject.GetComponent<Image>();
            managerScript.popupIcon.sprite = notificationIcon;
            managerScript.popupIcon.color = notificationColor;
           
            managerScript.popupTitle = go.transform.Find("Content/Texts/Title Text").GetComponent<TextMeshProUGUI>();
            managerScript.popupTitle.text = notificationTitle;
           
            managerScript.popupDescription = go.transform.Find("Content/Texts/Description Text").GetComponent<TextMeshProUGUI>();
            managerScript.popupDescription.text = popupDescription;

            if (enableNotificationSound == true && aSource != null)
            {
                aSource.clip = notificationSound;
                aSource.Play();
            }
        }

        public void CreateNotification()
        {
            // If popup notification is enabled
            if (enablePopupNotification == true)
            {
                // Spawn popup notification to the requested parent
                GameObject go = Instantiate(managerScript.popupNotification, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(managerScript.popupNotificationParent, false);

                // Set animation speed
                Animator goAnimator = go.gameObject.GetComponent<Animator>();
                goAnimator.SetFloat("Visible Time", managerScript.visibleTime);

                // Set popup resources with requested stuff
                managerScript.popupIcon = go.transform.Find("Content/Icon Parent/Icon").gameObject.GetComponent<Image>();
                managerScript.popupIcon.sprite = notificationIcon;
                managerScript.popupIcon.color = notificationColor;
                
                managerScript.popupTitle = go.transform.Find("Content/Texts/Title Text").GetComponent<TextMeshProUGUI>();
                managerScript.popupTitle.text = notificationTitle;
               
                managerScript.popupDescription = go.transform.Find("Content/Texts/Description Text").GetComponent<TextMeshProUGUI>();
                managerScript.popupDescription.text = popupDescription;
            }

            // If selected notification type is Standard
            if (notificationType == NotificationType.Standard)
            {
                // Spawn standard notification to the requested parent
                GameObject go = Instantiate(managerScript.standardNotification, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(managerScript.notificationParent);

                // Set standard resources with requested stuff
                managerScript.standardIcon = go.transform.Find("Content/Icon").GetComponent<Image>();
                managerScript.standardIcon.sprite = notificationIcon;

                managerScript.standardTitle = go.transform.Find("Content/Title Text").GetComponent<TextMeshProUGUI>();
                managerScript.standardTitle.text = notificationTitle;

                managerScript.standardDescription = go.transform.Find("Content/Description Text").GetComponent<TextMeshProUGUI>();
                managerScript.standardDescription.text = notificationDescription;

                // Count requested buttons
                for (int i = 0; i < notificationButtons.Count; ++i)
                {
                    // Spawn requested buttons to their parent
                    GameObject bgo = Instantiate(managerScript.notificationButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                    managerScript.standardButtonParent = go.transform.Find("Content/Button List").GetComponent<Transform>();
                    bgo.transform.SetParent(managerScript.standardButtonParent, false);

                    // Change text
                    setItemText = bgo.transform.Find("Normal/Text").GetComponent<TextMeshProUGUI>();
                    textHelper = notificationButtons[i].buttonText;
                    setItemText.text = textHelper;
                    setItemText = bgo.transform.Find("Highlighted/Text").GetComponent<TextMeshProUGUI>();
                    setItemText.text = textHelper;

                    // Change icon if it's enabled
                    if (enableButtonIcon == true)
                    {
                        Transform bgoImage;
                        bgoImage = bgo.transform.Find("Normal/Icon").GetComponent<Transform>();
                        bgoImage.GetComponent<Image>().sprite = notificationButtons[i].buttonIcon;
                        bgoImage.gameObject.SetActive(false);
                        bgoImage.gameObject.SetActive(true);
                        bgoImage = bgo.transform.Find("Highlighted/Icon").GetComponent<Transform>();
                        bgoImage.GetComponent<Image>().sprite = notificationButtons[i].buttonIcon;
                        bgoImage.gameObject.SetActive(false);
                        bgoImage.gameObject.SetActive(true);
                    }

                    else
                    {
                        Transform bgoImage;
                        bgoImage = bgo.transform.Find("Normal/Icon").GetComponent<Transform>();
                        bgoImage.gameObject.SetActive(false);
                        bgoImage = bgo.transform.Find("Highlighted/Icon").GetComponent<Transform>();
                        bgoImage.gameObject.SetActive(false);
                    }

                    // Add button events
                    Button itemButton = bgo.GetComponent<Button>();
                    itemButton.onClick.AddListener(notificationButtons[i].OnButtonClick.Invoke);

                    if (notificationButtons[i].isCloseButton == true)
                    {
                        Animator ntfAnimator = go.GetComponent<Animator>();
                        itemButton.onClick.AddListener(delegate { ntfAnimator.Play("Out"); });
                    }
                }
            }

            if (enableNotificationSound == true) { aSource.clip = notificationSound; aSource.Play(); }
            if (showOnlyOnce == true) { PlayerPrefs.SetInt(gameObject.name + "Shown", 1); }
        }

        public void CreateButton(string title, Sprite icon, UnityEvent clickEvent, bool isClose)
        {
            ButtonItem bitem = new ButtonItem();
            bitem.buttonText = title;
            bitem.buttonIcon = icon;
            bitem.OnButtonClick.AddListener(delegate { clickEvent = new UnityEvent(); });
            bitem.isCloseButton = isClose;
            notificationButtons.Add(bitem);
        }

        public void CreateNewButton()
        {
            ButtonItem bitem = new ButtonItem();
            bitem.buttonText = "New Button";
            notificationButtons.Add(bitem);
        }
    }
}