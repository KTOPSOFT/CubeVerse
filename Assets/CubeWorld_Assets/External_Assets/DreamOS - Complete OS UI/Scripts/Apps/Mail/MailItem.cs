using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Michsky.DreamOS
{
    [CreateAssetMenu(fileName = "New Item", menuName = "DreamOS/New Mail Item")]
    public class MailItem : ScriptableObject
    {
        // Settings
        public MailFolder mailFolder;
        public string subject = "Subject";
        public string from = "from@mail.com";
        public string fromName = "Butters Stotch";
        public string to = "to@mail.com";
        public string time = "12:00";
        public string date = "2022.01.01";
        public Sprite contactImage;
        public bool useCustomContent;
        [TextArea] public string mailContent = "";
        public GameObject customContentPrefab;

        // List
        public List<AttachmentItem> attachments = new List<AttachmentItem>();

        [System.Serializable]
        public class AttachmentItem
        {
            public string attachmentTitle = "Subject";
            public Attachment attachmentType;
            public AudioClip musicAttachment;
            public Sprite pictureAttachment;
            public VideoClip videoAttachment;
            [TextArea] public string noteAttachment;
        }

        public enum MailFolder { Inbox, Sent, Junk }
        public enum Attachment { Music, Note, Picture, Video }
    }
}