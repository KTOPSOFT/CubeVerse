using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class ReminderItem : MonoBehaviour
    {
        [Header("Resources")]
        public SwitchManager switchManager;

        [Header("Settings")]
        [Range(1,4)] public int reminderID;
        public string defaultTitle = "Title";
        [Range(0, 24)] public int defaultHour = 8;
        [Range(0, 60)] public int defaultMinute = 30;
        public GlobalTime.TimedEventType defaultType;
        public bool isOn;

        void Start()
        {
            if (!PlayerPrefs.HasKey("Reminder" + reminderID.ToString() + "Title"))
                SetDefaults();
        }

        void OnEnable()
        {
            if (switchManager == null)
                return;

            // Check if reminder is disabled or not
            if (PlayerPrefs.GetString("Reminder" + reminderID.ToString() + "Enabled") == "true")
            {
                switchManager.isOn = true;
                switchManager.UpdateUI();
            }

            else
            {
                switchManager.isOn = false;
                switchManager.UpdateUI();
            }
        }

        public void EnableReminder()
        {
            // Enable reminder and change the data
            PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Enabled", "true");
            switchManager.isOn = true;
            switchManager.UpdateUI();
        }

        public void DisableReminder()
        {
            // Disable reminder and change the data
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Enabled");
            switchManager.isOn = false;
            switchManager.UpdateUI();
        }

        public void DestroyReminder()
        {
            // Destroy the reminder object
            Destroy(gameObject);
        }

        public void SetDefaults()
        {
            // Set everything to default
            PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Title", defaultTitle);
            PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Hour", defaultHour.ToString());
            PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Minute", defaultMinute.ToString());

            if (defaultType == GlobalTime.TimedEventType.Once)
                PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Type", "Once");
            else if (defaultType == GlobalTime.TimedEventType.Daily)
                PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Type", "Daily");
            else if (defaultType == GlobalTime.TimedEventType.Weekly)
                PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Type", "Weekly");
            else if (defaultType == GlobalTime.TimedEventType.Monthly)
                PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Type", "Monthly");
            else if (defaultType == GlobalTime.TimedEventType.Yearly)
                PlayerPrefs.SetString("Reminder" + reminderID.ToString() + "Type", "Yearly");

            if (isOn == true) { EnableReminder(); }
            else { DisableReminder(); }
        }

        public void UpdateUI()
        {
            // Update UI depending on the variables
            TextMeshProUGUI titleObj = gameObject.transform.Find("Title").GetComponent<TextMeshProUGUI>();
            titleObj.text = PlayerPrefs.GetString("Reminder" + reminderID.ToString() + "Title");

            TextMeshProUGUI dateObj = gameObject.transform.Find("Date").GetComponent<TextMeshProUGUI>();
            dateObj.text = PlayerPrefs.GetString("Reminder" + reminderID.ToString() + "Hour")
                + ":" + PlayerPrefs.GetString("Reminder" + reminderID.ToString() + "Minute")
                + "\n" + PlayerPrefs.GetString("Reminder" + reminderID.ToString() + "Type");

            EnableReminder();
        }

        public void DeleteReminderData()
        {
            // Delete reminder data
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Enabled");
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Title");
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Hour");
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Minute");
            PlayerPrefs.DeleteKey("Reminder" + reminderID.ToString() + "Type");
        }
    }
}