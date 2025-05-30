using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class MailManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private Transform mailViewer;
        [SerializeField] private Transform inboxParent;
        [SerializeField] private Transform sentParent;
        [SerializeField] private Transform junkParent;
        [SerializeField] private PopupPanelManager attachmentPanel;
        [SerializeField] private Transform attachmentParent;
        public GameObject itemTemplate;
        public GameObject mailTemplate;
        public GameObject attachmentItem;
        [SerializeField] private MusicPlayerManager musicManager;
        [SerializeField] private NotepadManager noteManager;
        [SerializeField] private PhotoGalleryManager pictureManager;
        [SerializeField] private VideoPlayerManager videoManager;

        // Settings
        public string fromPrefix = "<";
        public string fromSuffix = ">";

        // Content
        public List<MailAsset> mailList = new List<MailAsset>();

        GameObject currentViewerObject;
        MailItemHelper currentHelper;

        [System.Serializable]
        public class MailAsset
        {
            public string itemTitle = "Mail Title";
            public MailItem mailAsset;
        }

        void Awake()
        {
            InitializeMails();
        }

        public void InitializeMails()
        {
            foreach (Transform child in mailViewer) { Destroy(child.gameObject); }
            foreach (Transform child in inboxParent) { Destroy(child.gameObject); }
            foreach (Transform child in sentParent) { Destroy(child.gameObject); }
            foreach (Transform child in junkParent) { Destroy(child.gameObject); }
            foreach (Transform child in attachmentParent) { Destroy(child.gameObject); }
            for (int i = 0; i < mailList.Count; ++i)
            {
                // Create items
                GameObject itemObj = Instantiate(itemTemplate, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                itemObj.gameObject.name = mailList[i].itemTitle;

                if (mailList[i].mailAsset.mailFolder == MailItem.MailFolder.Inbox) { itemObj.transform.SetParent(inboxParent, false); }
                else if (mailList[i].mailAsset.mailFolder == MailItem.MailFolder.Sent) { itemObj.transform.SetParent(sentParent, false); }
                else if (mailList[i].mailAsset.mailFolder == MailItem.MailFolder.Junk) { itemObj.transform.SetParent(junkParent, false); }

                // Update UI
                if (mailList[i].mailAsset.contactImage != null)
                {
                    Image contactPic = itemObj.transform.Find("Picture/Image").GetComponent<Image>();
                    contactPic.sprite = mailList[i].mailAsset.contactImage;
                    contactPic.gameObject.SetActive(true);
                }

                else
                {
                    TextMeshProUGUI contactLetter = itemObj.transform.Find("Picture/Letter").GetComponent<TextMeshProUGUI>();
                    contactLetter.text = mailList[i].mailAsset.fromName.Substring(0, 1);
                }

                TextMeshProUGUI nameTitle = itemObj.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                nameTitle.text = mailList[i].mailAsset.fromName;

                TextMeshProUGUI subjectTitle = itemObj.transform.Find("Subject").GetComponent<TextMeshProUGUI>();
                subjectTitle.text = mailList[i].mailAsset.subject;

                TextMeshProUGUI timeTitle = itemObj.transform.Find("Time").GetComponent<TextMeshProUGUI>();
                timeTitle.text = mailList[i].mailAsset.time;

                TextMeshProUGUI dateTitle = itemObj.transform.Find("Date").GetComponent<TextMeshProUGUI>();
                dateTitle.text = mailList[i].mailAsset.date;

                // Assign mail item
                MailItemHelper mailHelper = itemObj.GetComponent<MailItemHelper>();
                mailHelper.mailItem = mailList[i].mailAsset;

                // Register item events
                Button itemBtn = itemObj.GetComponent<Button>();
                itemBtn.onClick.AddListener(delegate { ApplyToTemplate(mailHelper); });
            }

            if (attachmentPanel != null) { attachmentPanel.gameObject.SetActive(false); attachmentPanel.InstantMinimized(); }
        }

        void ApplyToTemplate(MailItemHelper mItem)
        {
            if (currentHelper == mItem) { return; }
            if (currentViewerObject != null) { Destroy(currentViewerObject); }
            if (attachmentPanel != null) { attachmentPanel.gameObject.SetActive(false); attachmentPanel.InstantMinimized(); }
            foreach (Transform child in attachmentParent) { Destroy(child.gameObject); }

            GameObject viewerObj = Instantiate(mailTemplate, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            viewerObj.gameObject.name = mItem.mailItem.name;
            viewerObj.transform.SetParent(mailViewer, false);

            currentHelper = mItem;
            currentViewerObject = viewerObj;

            if (mItem.mailItem.contactImage != null)
            {
                Image contactPic = viewerObj.transform.Find("Picture/Image").GetComponent<Image>();
                contactPic.sprite = mItem.mailItem.contactImage;
                contactPic.gameObject.SetActive(true);
            }

            else
            {
                TextMeshProUGUI contactLetter = viewerObj.transform.Find("Picture/Letter").GetComponent<TextMeshProUGUI>();
                contactLetter.text = mItem.mailItem.fromName.Substring(0, 1);
            }

            TextMeshProUGUI subjectTitle = viewerObj.transform.Find("Subject").GetComponent<TextMeshProUGUI>();
            subjectTitle.text = mItem.mailItem.subject;

            TextMeshProUGUI nameTitle = viewerObj.transform.Find("Title/Name").GetComponent<TextMeshProUGUI>();
            nameTitle.text = mItem.mailItem.fromName;

            TextMeshProUGUI fromTitle = viewerObj.transform.Find("Title/From").GetComponent<TextMeshProUGUI>();
            fromTitle.text = fromPrefix + mItem.mailItem.from + fromSuffix;

            LayoutRebuilder.ForceRebuildLayoutImmediate(fromTitle.transform.parent.GetComponent<RectTransform>());

            TextMeshProUGUI timeTitle = viewerObj.transform.Find("Date & Time/Time").GetComponent<TextMeshProUGUI>();
            timeTitle.text = mItem.mailItem.time;

            TextMeshProUGUI dateTitle = viewerObj.transform.Find("Date & Time/Date").GetComponent<TextMeshProUGUI>();
            dateTitle.text = mItem.mailItem.date;

            LayoutRebuilder.ForceRebuildLayoutImmediate(dateTitle.transform.parent.GetComponent<RectTransform>());

            GameObject contentList = viewerObj.transform.Find("Content/List").gameObject;

            if (mItem.mailItem.useCustomContent == false)
            {
                contentList.SetActive(true);

                TextMeshProUGUI contentText = viewerObj.transform.Find("Content/List/Text").GetComponent<TextMeshProUGUI>();
                contentText.text = mItem.mailItem.mailContent;
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentList.GetComponent<RectTransform>());
            }

            else
            {
                contentList.SetActive(false);

                Transform spawnParent = viewerObj.transform.Find("Content").transform;
                GameObject customContent = Instantiate(mItem.mailItem.customContentPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                customContent.gameObject.name = mItem.mailItem.name;
                customContent.transform.SetParent(spawnParent, false);
            }

            for (int i = 0; i < mItem.mailItem.attachments.Count; ++i)
            {
                GameObject atchObject = Instantiate(attachmentItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                atchObject.gameObject.name = mItem.mailItem.attachments[i].attachmentTitle;
                atchObject.transform.SetParent(attachmentParent, false);

                ButtonManager atchButtonMngr = atchObject.GetComponent<ButtonManager>();
                atchButtonMngr.buttonText = mItem.mailItem.attachments[i].attachmentTitle;
                atchButtonMngr.UpdateUI();

                Button atchButton = atchObject.GetComponent<Button>();

                if (mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentType == MailItem.Attachment.Music)
                {
                    GameObject atchIcon = atchObject.transform.Find("Attachment Type/Music").gameObject;
                    atchIcon.SetActive(true);

                    atchButton.onClick.AddListener(delegate 
                    {
                        WindowManager wm = musicManager.musicPanelManager.gameObject.GetComponent<WindowManager>();
                        wm.OpenWindow();
                        musicManager.PlayCustomClip(mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].musicAttachment, musicManager.libraryPlaylist.coverImage, mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentTitle, mItem.mailItem.fromName);
                    });
                }

                else if (mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentType == MailItem.Attachment.Note)
                {
                    GameObject atchIcon = atchObject.transform.Find("Attachment Type/Note").gameObject;
                    atchIcon.SetActive(true);

                    atchButton.onClick.AddListener(delegate
                    {
                        WindowManager wm = noteManager.notepadWindow.gameObject.GetComponent<WindowManager>();
                        wm.OpenWindow();
                        noteManager.OpenCustomNote(mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentTitle, mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].noteAttachment);
                    });
                }

                else if (mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentType == MailItem.Attachment.Picture)
                {
                    GameObject atchIcon = atchObject.transform.Find("Attachment Type/Picture").gameObject;
                    atchIcon.SetActive(true);

                    atchButton.onClick.AddListener(delegate
                    {
                        WindowManager wm = pictureManager.photoGalleryWindow.gameObject.GetComponent<WindowManager>();
                        wm.OpenWindow();
                        pictureManager.OpenCustomSprite(mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].pictureAttachment, mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentTitle, mItem.mailItem.fromName);
                        pictureManager.CheckForButtonStates();
                    });
                }

                else if (mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentType == MailItem.Attachment.Video)
                {
                    GameObject atchIcon = atchObject.transform.Find("Attachment Type/Video").gameObject;
                    atchIcon.SetActive(true);

                    atchButton.onClick.AddListener(delegate
                    {
                        WindowManager wm = videoManager.videoPlayerWindow.gameObject.GetComponent<WindowManager>();
                        wm.OpenWindow();
                        videoManager.PlayVideoClip(mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].videoAttachment, mItem.mailItem.attachments[atchObject.transform.GetSiblingIndex()].attachmentTitle);
                    });
                }
            }

            if (mItem.mailItem.attachments.Count != 0 && attachmentPanel != null) { attachmentPanel.OpenPanel(); }
        }
    }
}