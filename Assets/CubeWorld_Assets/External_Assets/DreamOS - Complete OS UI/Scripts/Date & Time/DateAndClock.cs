using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Date & Time/Date and Clock")]
    public class DateAndClock : MonoBehaviour
    {
        [Header("Resources")]
        public GlobalTime globalTimeScript;

        [Header("Settings")]
        public bool enableAmPmLabel;
        public bool addSeconds;
        public ObjectType objectType;
        [HideInInspector] public DateFormat dateFormat;

        [Header("Analog Clock")]
        [HideInInspector] public Transform clockHourHand;
        [HideInInspector] public Transform clockMinuteHand;
        [HideInInspector] public Transform clockSecondHand;

        [Header("Digital Clock")]
        [HideInInspector] public TextMeshProUGUI digitalClockText;

        [Header("Digital Date")]
        [HideInInspector] public TextMeshProUGUI digitalDateText;

        public enum ObjectType
        {
            AnalogClock,
            DigitalClock,
            DigitalDate,
        }

        public enum DateFormat
        {
            DD_MM_YYYY,
            MM_DD_YYYY,
            YYYY_MM_DD
        }

        void Start()
        {
            if (globalTimeScript == null)
                globalTimeScript = GameObject.Find("Date & Time").GetComponent<GlobalTime>();
           
            if (objectType == ObjectType.DigitalClock && digitalClockText == null)
                digitalClockText = gameObject.GetComponent<TextMeshProUGUI>();
            else if (objectType == ObjectType.DigitalDate && digitalDateText == null)
                digitalDateText = gameObject.GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if (objectType == ObjectType.AnalogClock) { AnalogClock(); }
            else if (objectType == ObjectType.DigitalClock) { DigitalClock(); }
            else if (objectType == ObjectType.DigitalDate) { DigitalDate(); }
        }

        public void AnalogClock()
        {
            clockHourHand.localRotation = Quaternion.Euler(0, 0, globalTimeScript.currentHour * -15 * 2);
            clockMinuteHand.localRotation = Quaternion.Euler(0, 0, globalTimeScript.currentMinute * -6);
            if (addSeconds == true) { clockSecondHand.localRotation = Quaternion.Euler(0, 0, globalTimeScript.currentSecond * -6); }
        }

        public void DigitalClock()
        {
            if (globalTimeScript.currentHour.ToString().Length != 1 && globalTimeScript.currentMinute.ToString().Length == 1)
                digitalClockText.text = string.Format("{0}:0{1}", globalTimeScript.currentHour, globalTimeScript.currentMinute);
            else if (globalTimeScript.currentHour.ToString().Length == 1 && globalTimeScript.currentMinute.ToString().Length == 1)
                digitalClockText.text = string.Format("0{0}:0{1}", globalTimeScript.currentHour, globalTimeScript.currentMinute);
            else if (globalTimeScript.currentHour.ToString().Length == 1 && globalTimeScript.currentMinute.ToString().Length != 1)
                digitalClockText.text = string.Format("0{0}:{1}", globalTimeScript.currentHour, globalTimeScript.currentMinute);
            else
                digitalClockText.text = string.Format("{0}:{1}", globalTimeScript.currentHour, globalTimeScript.currentMinute);

            if (addSeconds == true)
                digitalClockText.text = digitalClockText.text + ":" + globalTimeScript.currentSecond.ToString("00");

            if (globalTimeScript.useShortClockFormat == false)
                return;

            if (globalTimeScript.isAm == true && enableAmPmLabel == true)
                digitalClockText.text = digitalClockText.text + " AM";
            else if (globalTimeScript.isAm == false && enableAmPmLabel == true)
                digitalClockText.text = digitalClockText.text + " PM";
        }

        public void DigitalDate()
        {
            if (dateFormat == DateFormat.DD_MM_YYYY)
                digitalDateText.text = string.Format("{0}.{1}.{2}", globalTimeScript.currentDay, globalTimeScript.currentMonth, globalTimeScript.currentYear);
            else if (dateFormat == DateFormat.MM_DD_YYYY)
                digitalDateText.text = string.Format("{0}.{1}.{2}", globalTimeScript.currentMonth, globalTimeScript.currentDay, globalTimeScript.currentYear);
            else if (dateFormat == DateFormat.YYYY_MM_DD)
                digitalDateText.text = string.Format("{0}.{1}.{2}", globalTimeScript.currentYear, globalTimeScript.currentMonth, globalTimeScript.currentDay);
        }
    }
}