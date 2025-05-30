using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    public class GlobalTime : MonoBehaviour
    {
        // Time variables
        [Range(0.1f, 10000)] public float timeMultiplier = 10;
        [Range(1, 30)] public int currentDay = 1;
        [Range(1, 12)] public int currentMonth = 1;
        public int currentYear = 2019;
        [Range(0, 24)] public int currentHour;
        [Range(0, 60)] public int currentMinute;
        [Range(0, 60)] public float currentSecond;

        // Settings
        public bool saveAndGetValues = true;
        public bool useShortClockFormat = true;
        public bool enableTimedEvents = true;
        [SerializeField] private float checkForTimedEventEvery = 1;
        [SerializeField] private DefaultShortTime defaultShortTime;

        // Events
        public List<TimedEvent> timedEvents = new List<TimedEvent>();

        public float rotationDegreesPerDay = 360;
        public float hoursPerDay = 24;
        public float minutesPerHour = 60;
        public float secondsPerMinute = 60;
        public bool isAm;

        [System.Serializable]
        public class TimedEvent
        {
            public string eventTitle;
            public TimedEventType timedEventType;
            [Range(0, 24)] public int eventHour;
            [Range(0, 60)] public int eventMinute;
            [Range(1, 30)] public int eventDay = 1;
            [Range(1, 12)] public int eventMonth = 1;
            public int eventYear = 2019;
            public UnityEvent invokeEvents = new UnityEvent();
            [HideInInspector] public int itemID = 0;
        }

        public enum DefaultShortTime { AM, PM }
        public enum TimedEventType { Once, Daily, Weekly, Monthly, Yearly }

        void Awake()
        {
            if (saveAndGetValues == true && !PlayerPrefs.HasKey("CurrentYear")) { UpdateTimeData(); }
            InitializeTime();
        }

        void OnEnable()
        {
            if (enableTimedEvents == true) { StartCoroutine("ProcessTimedEvents"); }
        }

        void OnDisable()
        {
            if (enableTimedEvents == true) { StopCoroutine("ProcessTimedEvents"); }
        }

        void Update()
        {
            UpdateValues();
        }

        void UpdateValues()
        {
            currentSecond += Time.deltaTime * timeMultiplier;

            if (currentSecond >= 59)
            {
                currentSecond = 0;
                currentMinute += 1;
                PlayerPrefs.SetInt("CurrentMinute", currentMinute);

                if (currentMinute >= 60)
                {
                    currentHour += 1;
                    currentMinute = 0;
                    CheckForClockFormat();

                    if (useShortClockFormat == false && currentHour >= 24)
                    {
                        currentDay += 1;
                        currentHour = 0;
                        if (currentDay == 0) { currentDay = 1; }
                    }

                    else if (useShortClockFormat == true && isAm == false && currentHour >= 13)
                    {
                        currentDay += 1;
                        currentHour = 1;
                        PlayerPrefs.SetInt("CurrentDay", currentDay);
                    }

                    else if (useShortClockFormat == true && isAm == true && currentHour >= 13) { currentHour = 1; }

                    PlayerPrefs.SetInt("CurrentMinute", currentMinute);
                    PlayerPrefs.SetInt("CurrentHour", currentHour);

                    if (currentDay >= 30)
                    {
                        currentDay = 1;
                        currentMonth += 1;

                        if (currentMonth == 13)
                        {
                            currentMonth = 1;
                            currentYear += 1;
                            PlayerPrefs.SetInt("CurrentYear", currentYear);
                        }

                        PlayerPrefs.SetInt("CurrentDay", currentDay);
                        PlayerPrefs.SetInt("CurrentMonth", currentMonth);
                    }
                }
            }
        }

        private void InitializeTime()
        {
            if (saveAndGetValues == true)
            {
                if (!PlayerPrefs.HasKey("ShortClockFormat")) { PlayerPrefs.SetString("ShortClockFormat", useShortClockFormat.ToString().ToLower()); }
                else if (PlayerPrefs.GetString("ShortClockFormat") == "true") { useShortClockFormat = true; }
                else if (PlayerPrefs.GetString("ShortClockFormat") == "false") { useShortClockFormat = false; }

                currentMinute = PlayerPrefs.GetInt("CurrentMinute");
                currentHour = PlayerPrefs.GetInt("CurrentHour");
                currentDay = PlayerPrefs.GetInt("CurrentDay");
                currentMonth = PlayerPrefs.GetInt("CurrentMonth");
                currentYear = PlayerPrefs.GetInt("CurrentYear");

                if (!PlayerPrefs.HasKey("isAM"))
                {
                    if (defaultShortTime == DefaultShortTime.AM) { PlayerPrefs.SetInt("isAM", 1); isAm = true; }
                    else if (defaultShortTime == DefaultShortTime.PM) { PlayerPrefs.SetInt("isAM", 0); isAm = false; }
                }
                else if (PlayerPrefs.GetInt("isAM") == 1) { isAm = true; }
                else { isAm = false; }

                // Debug.Log("<b>[Time Information]</b> Current minute: " + currentMinute + " - Current hour: " + currentHour);
                // Debug.Log("<b>[Date Information]</b> Current day: " + currentDay + " - Current month: " + currentMonth + " - Current year: " + currentYear);
            }

            if (currentDay == 0) { currentDay = 1; }
        }

        public void CheckForTimedEvents()
        {
            for (int i = 0; i < timedEvents.Count; ++i)
            {
                if (timedEvents[i].eventHour == currentHour && timedEvents[i].eventMinute == currentMinute
                    && timedEvents[i].eventDay == currentDay && timedEvents[i].eventMonth == currentMonth
                    && timedEvents[i].eventYear == currentYear)
                {
                    timedEvents[i].invokeEvents.Invoke();
                    PlayerPrefs.SetString("TimedEvent" + timedEvents[i] + "Enabled", "true");
                    Debug.Log("<b>[Global Time]</b> Upcoming event: <b>" + timedEvents[i].eventTitle + "</b>");

                    if (timedEvents[i].timedEventType == TimedEventType.Once)
                    {
                        timedEvents.Remove(timedEvents[i]);
                        PlayerPrefs.DeleteKey("TimedEvent" + timedEvents[i] + "Enabled");
                    }

                    else if (timedEvents[i].timedEventType == TimedEventType.Daily)
                    {
                        timedEvents[i].eventDay = currentDay + 1;
                        if (timedEvents[i].eventDay >= 30) { timedEvents[i].eventDay = 0; }
                    }

                    else if (timedEvents[i].timedEventType == TimedEventType.Weekly)
                    {
                        timedEvents[i].eventDay += 7;

                        if (timedEvents[i].eventDay >= 30)
                        {
                            timedEvents[i].eventDay -= 30;
                            timedEvents[i].eventMonth += 1;
                            if (timedEvents[i].eventMonth >= 12) { timedEvents[i].eventYear += 1; }
                        }
                    }

                    else if (timedEvents[i].timedEventType == TimedEventType.Monthly)
                    {
                        timedEvents[i].eventMonth += 1;

                        if (timedEvents[i].eventMonth >= 12)
                        {
                            timedEvents[i].eventMonth = 0;
                            timedEvents[i].eventYear += 1;
                        }
                    }

                    else if (timedEvents[i].timedEventType == TimedEventType.Yearly) { timedEvents[i].eventYear += 1; }
                }
            }

            if (enableTimedEvents == true) { StartCoroutine("ProcessTimedEvents"); }
        }

        private void CheckForClockFormat()
        {
            if (useShortClockFormat == true)
            {
                if (currentHour >= 13 && isAm == false) { isAm = true; PlayerPrefs.SetInt("isAM", 1); }
                else if (currentHour >= 13 && isAm == true) { isAm = false; PlayerPrefs.SetInt("isAM", 0); }
            }

            else
            {
                if (currentHour >= 24 && isAm == false) { isAm = true; PlayerPrefs.SetInt("isAM", 0); }
                else if (currentHour >= 12 && isAm == true) { isAm = false; PlayerPrefs.SetInt("isAM", 1); }
            }
        }

        public void ShortClockFormat(bool value)
        {
            if (value == true) { useShortClockFormat = true; }
            else { useShortClockFormat = false; }

            PlayerPrefs.SetString("ShortClockFormat", useShortClockFormat.ToString().ToLower());

            UpdateClockFormat();
            UpdateTimeData();
            InitializeTime();
        }

        private void UpdateClockFormat()
        {
            if (useShortClockFormat == true && currentHour >= 12)
            {
                if (currentHour == 13) { currentHour = 1; }
                else if (currentHour == 14) { currentHour = 2; }
                else if (currentHour == 15) { currentHour = 3; }
                else if (currentHour == 16) { currentHour = 4; }
                else if (currentHour == 17) { currentHour = 5; }
                else if (currentHour == 18) { currentHour = 6; }
                else if (currentHour == 19) { currentHour = 7; }
                else if (currentHour == 20) { currentHour = 8; }
                else if (currentHour == 21) { currentHour = 9; }
                else if (currentHour == 22) { currentHour = 10; }
                else if (currentHour == 23) { currentHour = 11; }
                else if (currentHour == 0) { currentHour = 12; }
            }

            else if (useShortClockFormat == false && isAm == false)
            {
                if (currentHour == 1) { currentHour = 13; }
                else if (currentHour == 2) { currentHour = 14; }
                else if (currentHour == 3) { currentHour = 15; }
                else if (currentHour == 4) { currentHour = 16; }
                else if (currentHour == 5) { currentHour = 17; }
                else if (currentHour == 6) { currentHour = 18; }
                else if (currentHour == 7) { currentHour = 19; }
                else if (currentHour == 8) { currentHour = 20; }
                else if (currentHour == 9) { currentHour = 21; }
                else if (currentHour == 10) { currentHour = 22; }
                else if (currentHour == 11) { currentHour = 23; }
                else if (currentHour == 12) { currentHour = 0; }
            }
        }

        public void UpdateTimeData()
        {
            if (saveAndGetValues == false)
                return;

            PlayerPrefs.SetFloat("CurrentSecond", currentSecond);
            PlayerPrefs.SetInt("CurrentMinute", currentMinute);
            PlayerPrefs.SetInt("CurrentHour", currentHour);
            PlayerPrefs.SetInt("CurrentMonth", currentMonth);
            PlayerPrefs.SetInt("CurrentYear", currentYear);
        }

        public void DeleteSavedData()
        {
            PlayerPrefs.DeleteKey("CurrentSecond");
            PlayerPrefs.DeleteKey("CurrentMinute");
            PlayerPrefs.DeleteKey("CurrentHour");
            PlayerPrefs.DeleteKey("CurrentMonth");
            PlayerPrefs.DeleteKey("CurrentYear");
            PlayerPrefs.DeleteKey("isAM");
        }

        IEnumerator ProcessTimedEvents()
        {
            yield return new WaitForSeconds(checkForTimedEventEvery);
            CheckForTimedEvents();
        }

        public void AddTimedEvent()
        {
            TimedEvent titem = new TimedEvent();
            titem.eventTitle = "New Event";
            timedEvents.Add(titem);
        }
    }
}