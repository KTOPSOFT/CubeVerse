using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class WidgetManager : MonoBehaviour
    {
        // Resources
        public WidgetLibrary widgetLibrary;
        [SerializeField] private Transform widgetParent;
        [SerializeField] private Transform widgetLibraryParent;
        public GameObject widgetButton;

        // Settings
        public bool sortListByName = true;
        string saveKey = "DreamOS";

        // Multi Instance Support
        [HideInInspector] public UserManager userManager;

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            PrepareWidgets();
        }

        private static int SortByName(WidgetLibrary.WidgetItem o1, WidgetLibrary.WidgetItem o2)
        {
            return o1.widgetTitle.CompareTo(o2.widgetTitle);
        }

        public void PrepareWidgets()
        {
            if (sortListByName == true) { widgetLibrary.widgets.Sort(SortByName); }
            foreach (Transform child in widgetParent) { Destroy(child.gameObject); }
            foreach (Transform child in widgetLibraryParent) { Destroy(child.gameObject); }
            for (int i = 0; i < widgetLibrary.widgets.Count; ++i)
            {
                // Spawn widgets
                GameObject go = Instantiate(widgetLibrary.widgets[i].widgetPrefab, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(widgetParent, false);
                go.gameObject.name = widgetLibrary.widgets[i].widgetTitle;

                WidgetItem tempWidget = go.GetComponent<WidgetItem>();
                tempWidget.widgetManager = this;

                // Spawn widget button
                GameObject gob = Instantiate(widgetButton, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                gob.transform.SetParent(widgetLibraryParent, false);
                gob.gameObject.name = widgetLibrary.widgets[i].widgetTitle;

                // Set icon
                Transform imageObj;
                imageObj = gob.transform.Find("Icon").GetComponent<Transform>();
                imageObj.GetComponent<Image>().sprite = widgetLibrary.widgets[i].widgetIcon;

                // Set ID tags
                TextMeshProUGUI titleText;
                titleText = gob.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                titleText.text = widgetLibrary.widgets[i].widgetTitle;
                TextMeshProUGUI descriptionText;
                descriptionText = gob.transform.Find("Description").GetComponent<TextMeshProUGUI>();
                descriptionText.text = widgetLibrary.widgets[i].widgetDescription;

                // Add button events
                SwitchManager widgetSwitch;
                widgetSwitch = gob.transform.Find("Switch").GetComponent<SwitchManager>();

                widgetSwitch.OnEvents.AddListener(delegate
                {
                    widgetSwitch.isOn = true;
                    widgetSwitch.UpdateUI();
                    tempWidget.EnableWidget();

                });

                widgetSwitch.OffEvents.AddListener(delegate
                {
                    widgetSwitch.isOn = false;
                    widgetSwitch.UpdateUI();
                    tempWidget.DisableWidget();
                });

                if (!PlayerPrefs.HasKey(saveKey + "Widget" + tempWidget.gameObject.name) && tempWidget.isOn == true)
                {
                    widgetSwitch.isOn = true;
                    widgetSwitch.gameObject.SetActive(false);
                    widgetSwitch.gameObject.SetActive(true);
                }

                else if (!PlayerPrefs.HasKey(saveKey + "Widget" + tempWidget.gameObject.name) && tempWidget.isOn == false)
                {
                    widgetSwitch.isOn = false;
                    widgetSwitch.gameObject.SetActive(false);
                    widgetSwitch.gameObject.SetActive(true);
                }

                else if (PlayerPrefs.GetString(saveKey + "Widget" + tempWidget.gameObject.name) == "enabled")
                {
                    widgetSwitch.isOn = true;
                    widgetSwitch.gameObject.SetActive(false);
                    widgetSwitch.gameObject.SetActive(true);
                }

                else if (PlayerPrefs.GetString(saveKey + "Widget" + tempWidget.gameObject.name) == "disabled")
                {
                    widgetSwitch.isOn = false;
                    widgetSwitch.gameObject.SetActive(false);
                    widgetSwitch.gameObject.SetActive(true);
                }

                try
                {
                    WindowDragger tempDragger;
                    tempDragger = go.GetComponent<WindowDragger>();
                    tempDragger.dragArea = GameObject.Find("Widget Drag Area").GetComponent<RectTransform>();
                }

                catch { }
            }
        }
    }
}