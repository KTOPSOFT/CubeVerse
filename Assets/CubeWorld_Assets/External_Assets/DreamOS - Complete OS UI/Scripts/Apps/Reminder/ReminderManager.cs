using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class ReminderManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private GlobalTime globalTime;
        [SerializeField] private TMP_InputField eventTitleObject;
        [SerializeField] private TextMeshProUGUI eventHourObject;
        [SerializeField] private TextMeshProUGUI eventMinuteObject;
        [SerializeField] private HorizontalSelector hourSelector;
        [SerializeField] private HorizontalSelector minuteSelector;
        [SerializeField] private NotificationCreator reminderNotification;
        [SerializeField] private GameObject reminder1;
        [SerializeField] private GameObject reminder2;
        [SerializeField] private GameObject reminder3;
        [SerializeField] private GameObject reminder4;

        // Settings
        public string notificationTitle = "Heads up!";
        string saveKey;

        // Multi Instance Support
        [HideInInspector] public UserManager userManager;

        GlobalTime.TimedEventType reminderEventType;

        void Awake()
        {
            // if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            // if (userManager != null) { saveKey = userManager.machineID; }

            InitializeReminders();

            // If hour or minute selector is null, don't go further
            if (hourSelector == null || minuteSelector == null)
                return;

            // Add hour selector items
            for (int i = 0; i < 24; i++)
            {
                if (i < 10) { hourSelector.CreateNewItem("0" + i.ToString()); }
                else { hourSelector.CreateNewItem(i.ToString()); }
            }

            // Add minute selector items
            for (int i = 0; i < 60; i++)
            {
                if (i < 10) { minuteSelector.CreateNewItem("0" + i.ToString()); }
                else { minuteSelector.CreateNewItem(i.ToString()); }
            }
        }

        public void InitializeReminders()
        {
            // If reminder1 is assigned
            if (reminder1 != null)
            {
                // Get the component
                ReminderItem ritemObj = reminder1.GetComponent<ReminderItem>();

                // Check the data and set it deafults if it doesn't have any data
                if (!PlayerPrefs.HasKey(saveKey + "Reminder1" + "Title")) { ritemObj.SetDefaults(); }

                // Change the name and title
                reminder1.name = PlayerPrefs.GetString(saveKey + "Reminder1" + "Title");
                TextMeshProUGUI titleObj = reminder1.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleObj.text = reminder1.name;

                // Change reminder date
                TextMeshProUGUI dateObj = reminder1.transform.Find("Date").GetComponent<TextMeshProUGUI>();
                dateObj.text = PlayerPrefs.GetString(saveKey + "Reminder1" + "Hour")
                    + ":" + PlayerPrefs.GetString(saveKey + "Reminder1" + "Minute")
                    + "\n" + PlayerPrefs.GetString(saveKey + "Reminder1" + "Type");

                // Add reminder to global time as a timed event      
                GlobalTime.TimedEvent ritem = new GlobalTime.TimedEvent();
                ritem.eventTitle = reminder1.name;
                ritem.eventHour = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder1" + "Hour"));
                ritem.eventMinute = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder1" + "Minute"));
                ritem.eventDay = globalTime.currentDay;
                ritem.eventMonth = globalTime.currentMonth;
                ritem.eventYear = globalTime.currentYear;
                ritem.itemID = 1;

                // Fetch the event type data
                if (PlayerPrefs.GetString(saveKey + "Reminder1" + "Type") == "Once")
                    ritem.timedEventType = GlobalTime.TimedEventType.Once;
                else if (PlayerPrefs.GetString(saveKey + "Reminder1" + "Type") == "Daily")
                    ritem.timedEventType = GlobalTime.TimedEventType.Daily;
                else if (PlayerPrefs.GetString(saveKey + "Reminder1" + "Type") == "Weekly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Weekly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder1" + "Type") == "Monthly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Monthly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder1" + "Type") == "Yearly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Yearly;

                // Add events to timed event
                ritem.invokeEvents.AddListener(delegate
                {
                    reminderNotification.notificationTitle = notificationTitle;
                    reminderNotification.popupDescription = ritem.eventTitle;
                    reminderNotification.CreateOnlyPopup();

                    if (ritem.timedEventType == GlobalTime.TimedEventType.Once)
                        ritemObj.DisableReminder();
                });

                // Add the item to global time events and update the switch
                globalTime.timedEvents.Add(ritem);
                ritemObj.switchManager.UpdateUI();
            }

            // If reminder2 is assigned
            if (reminder2 != null)
            {
                // Get the component
                ReminderItem ritemObj = reminder2.GetComponent<ReminderItem>();

                // Check the data and set it deafults if it doesn't have any data
                if (!PlayerPrefs.HasKey(saveKey + "Reminder2" + "Title")) { ritemObj.SetDefaults(); }

                // Change the name and title
                reminder2.name = PlayerPrefs.GetString(saveKey + "Reminder2" + "Title");
                TextMeshProUGUI titleObj = reminder2.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleObj.text = reminder2.name;

                // Change reminder date
                TextMeshProUGUI dateObj = reminder2.transform.Find("Date").GetComponent<TextMeshProUGUI>();
                dateObj.text = PlayerPrefs.GetString(saveKey + "Reminder2" + "Hour")
                    + ":" + PlayerPrefs.GetString(saveKey + "Reminder2" + "Minute")
                    + "\n" + PlayerPrefs.GetString(saveKey + "Reminder2" + "Type");

                // Add reminder to global time as a timed event
                GlobalTime.TimedEvent ritem = new GlobalTime.TimedEvent();
                ritem.eventTitle = reminder2.name;
                ritem.eventHour = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder2" + "Hour"));
                ritem.eventMinute = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder2" + "Minute"));
                ritem.eventDay = globalTime.currentDay;
                ritem.eventMonth = globalTime.currentMonth;
                ritem.eventYear = globalTime.currentYear;
                ritem.itemID = 2;

                // Fetch the event type data
                if (PlayerPrefs.GetString(saveKey + "Reminder2" + "Type") == "Once")
                    ritem.timedEventType = GlobalTime.TimedEventType.Once;
                else if (PlayerPrefs.GetString(saveKey + "Reminder2" + "Type") == "Daily")
                    ritem.timedEventType = GlobalTime.TimedEventType.Daily;
                else if (PlayerPrefs.GetString(saveKey + "Reminder2" + "Type") == "Weekly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Weekly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder2" + "Type") == "Monthly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Monthly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder2" + "Type") == "Yearly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Yearly;

                // Add events to timed event
                ritem.invokeEvents.AddListener(delegate
                {
                    reminderNotification.notificationTitle = notificationTitle;
                    reminderNotification.popupDescription = ritem.eventTitle;
                    reminderNotification.CreateOnlyPopup();
                });

                // Add the item to global time events and update the switch
                globalTime.timedEvents.Add(ritem);
                ritemObj.switchManager.UpdateUI();
            }

            // If reminder3 is assigned
            if (reminder3 != null)
            {
                // Get the component
                ReminderItem ritemObj = reminder3.GetComponent<ReminderItem>();

                // Check the data and set it deafults if it doesn't have any data
                if (!PlayerPrefs.HasKey(saveKey + "Reminder3" + "Title")) { ritemObj.SetDefaults(); }

                // Change the name and title
                reminder3.name = PlayerPrefs.GetString(saveKey + "Reminder3" + "Title");
                TextMeshProUGUI titleObj = reminder3.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleObj.text = reminder3.name;

                // Change reminder date
                TextMeshProUGUI dateObj = reminder3.transform.Find("Date").GetComponent<TextMeshProUGUI>();
                dateObj.text = PlayerPrefs.GetString(saveKey + "Reminder3" + "Hour")
                    + ":" + PlayerPrefs.GetString(saveKey + "Reminder3" + "Minute")
                    + "\n" + PlayerPrefs.GetString(saveKey + "Reminder3" + "Type");

                // Add reminder to global time as a timed event
                GlobalTime.TimedEvent ritem = new GlobalTime.TimedEvent();
                ritem.eventTitle = reminder3.name;
                ritem.eventHour = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder3" + "Hour"));
                ritem.eventMinute = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder3" + "Minute"));
                ritem.eventDay = globalTime.currentDay;
                ritem.eventMonth = globalTime.currentMonth;
                ritem.eventYear = globalTime.currentYear;
                ritem.itemID = 3;

                // Fetch the event type data
                if (PlayerPrefs.GetString(saveKey + "Reminder3" + "Type") == "Once")
                    ritem.timedEventType = GlobalTime.TimedEventType.Once;
                else if (PlayerPrefs.GetString(saveKey + "Reminder3" + "Type") == "Daily")
                    ritem.timedEventType = GlobalTime.TimedEventType.Daily;
                else if (PlayerPrefs.GetString(saveKey + "Reminder3" + "Type") == "Weekly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Weekly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder3" + "Type") == "Monthly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Monthly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder3" + "Type") == "Yearly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Yearly;

                // Add events to timed event
                ritem.invokeEvents.AddListener(delegate
                {
                    reminderNotification.notificationTitle = notificationTitle;
                    reminderNotification.popupDescription = ritem.eventTitle;
                    reminderNotification.CreateOnlyPopup();
                });

                // Add the item to global time events and update the switch
                globalTime.timedEvents.Add(ritem);
                ritemObj.switchManager.UpdateUI();
            }

            // If reminder4 is assigned
            if (reminder4 != null)
            {
                // Get the component
                ReminderItem ritemObj = reminder4.GetComponent<ReminderItem>();

                // Check the data and set it deafults if it doesn't have any data
                if (!PlayerPrefs.HasKey(saveKey + "Reminder4" + "Title")) { ritemObj.SetDefaults(); }

                // Change the name and title
                reminder4.name = PlayerPrefs.GetString(saveKey + "Reminder4" + "Title");
                TextMeshProUGUI titleObj = reminder4.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleObj.text = reminder4.name;

                // Change reminder date
                TextMeshProUGUI dateObj = reminder4.transform.Find("Date").GetComponent<TextMeshProUGUI>();
                dateObj.text = PlayerPrefs.GetString(saveKey + "Reminder4" + "Hour")
                    + ":" + PlayerPrefs.GetString(saveKey + "Reminder4" + "Minute")
                    + "\n" + PlayerPrefs.GetString(saveKey + "Reminder4" + "Type");

                // Add reminder to global time as a timed event
                GlobalTime.TimedEvent ritem = new GlobalTime.TimedEvent();
                ritem.eventTitle = reminder4.name;
                ritem.eventHour = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder4" + "Hour"));
                ritem.eventMinute = int.Parse(PlayerPrefs.GetString(saveKey + "Reminder4" + "Minute"));
                ritem.eventDay = globalTime.currentDay;
                ritem.eventMonth = globalTime.currentMonth;
                ritem.eventYear = globalTime.currentYear;
                ritem.itemID = 4;

                // Fetch the event type data
                if (PlayerPrefs.GetString(saveKey + "Reminder4" + "Type") == "Once")
                    ritem.timedEventType = GlobalTime.TimedEventType.Once;
                else if (PlayerPrefs.GetString(saveKey + "Reminder4" + "Type") == "Daily")
                    ritem.timedEventType = GlobalTime.TimedEventType.Daily;
                else if (PlayerPrefs.GetString(saveKey + "Reminder4" + "Type") == "Weekly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Weekly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder4" + "Type") == "Monthly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Monthly;
                else if (PlayerPrefs.GetString(saveKey + "Reminder4" + "Type") == "Yearly")
                    ritem.timedEventType = GlobalTime.TimedEventType.Yearly;

                // Add events to timed event
                ritem.invokeEvents.AddListener(delegate
                {
                    reminderNotification.notificationTitle = notificationTitle;
                    reminderNotification.popupDescription = ritem.eventTitle;
                    reminderNotification.CreateOnlyPopup();
                });

                // Add the item to global time events and update the switch
                globalTime.timedEvents.Add(ritem);
                ritemObj.switchManager.UpdateUI();
            }
        }

        public void SetOnceType()
        {
            // Set reminder event type to once
            reminderEventType = GlobalTime.TimedEventType.Once;
        }

        public void SetDailyType()
        {
            // Set reminder event type to daily
            reminderEventType = GlobalTime.TimedEventType.Daily;
        }

        public void SetMonthlyType()
        {
            // Set reminder event type to monthly
            reminderEventType = GlobalTime.TimedEventType.Monthly;
        }

        public void SetWeeklyType()
        {
            // Set reminder event type to weekly
            reminderEventType = GlobalTime.TimedEventType.Weekly;
        }

        public void SetYearlyType()
        {
            // Set reminder event type to yearly
            reminderEventType = GlobalTime.TimedEventType.Yearly;
        }

        public void ChangeUpdateReminder(int index)
        {
            // Change the reminder data depending on the index and update the UI
            PlayerPrefs.SetInt(saveKey + "ReminderHelper", index);
            eventTitleObject.text = PlayerPrefs.GetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Title");
            eventHourObject.text = PlayerPrefs.GetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Hour");
            eventMinuteObject.text = PlayerPrefs.GetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Minute");
            hourSelector.index = int.Parse(eventHourObject.text);
            minuteSelector.index = int.Parse(eventMinuteObject.text);
            hourSelector.UpdateUI();
            minuteSelector.UpdateUI();
        }

        public void UpdateReminder()
        {
            // Add reminder to global time as a timed event
            GlobalTime.TimedEvent ritem = new GlobalTime.TimedEvent();
            ritem.eventTitle = eventTitleObject.text;
            ritem.eventHour = int.Parse(eventHourObject.text);
            ritem.eventMinute = int.Parse(eventMinuteObject.text);
            ritem.eventDay = globalTime.currentDay;
            ritem.eventMonth = globalTime.currentMonth;
            ritem.eventYear = globalTime.currentYear;

            // Save data depending on the event type
            if (reminderEventType == GlobalTime.TimedEventType.Once)
            {
                ritem.timedEventType = GlobalTime.TimedEventType.Once;
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Type", "Once");
            }

            else if (reminderEventType == GlobalTime.TimedEventType.Daily)
            {
                ritem.timedEventType = GlobalTime.TimedEventType.Daily;
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Type", "Daily");
            }

            else if (reminderEventType == GlobalTime.TimedEventType.Weekly)
            {
                ritem.timedEventType = GlobalTime.TimedEventType.Weekly;
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Type", "Weekly");
            }

            else if (reminderEventType == GlobalTime.TimedEventType.Monthly)
            {
                ritem.timedEventType = GlobalTime.TimedEventType.Monthly;
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Type", "Monthly");
            }

            else if (reminderEventType == GlobalTime.TimedEventType.Yearly)
            {
                ritem.timedEventType = GlobalTime.TimedEventType.Yearly;
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Type", "Yearly");
            }

            // If title is blank, then just save as My Reminder
            if (eventTitleObject.text == "" || eventTitleObject.text == null)
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Title", "My Reminder");
            else
                PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Title", eventTitleObject.text);

            // Save hour and minute data
            PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Hour", eventHourObject.text);
            PlayerPrefs.SetString(saveKey + "Reminder" + PlayerPrefs.GetInt(saveKey + "ReminderHelper").ToString() + "Minute", eventMinuteObject.text);

            globalTime.timedEvents.Add(ritem);

            // Add events to timed event
            ritem.invokeEvents.AddListener(delegate
            {
                reminderNotification.notificationTitle = notificationTitle;
                reminderNotification.popupDescription = ritem.eventTitle;
                reminderNotification.CreateOnlyPopup();
            });

            // Update the UI depending on the reminder index
            if (PlayerPrefs.GetInt(saveKey + "ReminderHelper") == 1) { reminder1.GetComponent<ReminderItem>().UpdateUI(); }
            else if (PlayerPrefs.GetInt(saveKey + "ReminderHelper") == 2) { reminder2.GetComponent<ReminderItem>().UpdateUI(); }
            else if (PlayerPrefs.GetInt(saveKey + "ReminderHelper") == 3) { reminder3.GetComponent<ReminderItem>().UpdateUI(); }
            else if (PlayerPrefs.GetInt(saveKey + "ReminderHelper") == 4) { reminder4.GetComponent<ReminderItem>().UpdateUI(); }
        }
    }
}