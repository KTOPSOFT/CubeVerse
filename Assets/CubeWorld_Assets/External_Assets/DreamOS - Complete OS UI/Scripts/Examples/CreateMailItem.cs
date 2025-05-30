//This asset was uploaded by https://unityassetcollection.com

using UnityEngine;

namespace Michsky.DreamOS.Examples
{
    public class CreateMailItem : MonoBehaviour
    {
        [Header("Resources")]
        public MailManager mailManager;
        public MailItem mailItem;

        [Header("Settings")]
        public bool createOnAwake;

        void Awake()
        {
            if (mailManager == null) { Debug.LogError("Mail Manager is missing.", this); return; }
            if (mailItem == null) { Debug.LogError("Mail Item is missing.", this); return; }
            if (createOnAwake == true) { CreateMail(); }
        }

        public void CreateMail()
        {
            MailManager.MailAsset item = new MailManager.MailAsset();
            item.mailAsset = mailItem;
            mailManager.mailList.Add(item);
            mailManager.InitializeMails();
        }
    }
}