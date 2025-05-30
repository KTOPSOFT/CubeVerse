using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
    public class SettingsManager : MonoBehaviour
    {
        // Resources
        public UIManager themeManager;
        [SerializeField] private RebootManager rebootManager;
        public UserManager userManager;
        [SerializeField] private SetupManager setupManager;
        [SerializeField] private Image lockscreenImage;
        [SerializeField] private Transform accentColorList;
        [SerializeField] private Transform accentReversedColorList;
        [SerializeField] private ItemDragContainer desktopDragger;

        // Settings
        public Sprite defaultWallpaper;
        public bool lockDesktopItems = true;
        string saveKey = "DreamOS";

        // Debug
        Toggle toggleHelper;

        void Awake()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }
            if (userManager != null) { saveKey = userManager.machineID; }

            // Set selected theme as custom and get the color data
            if (PlayerPrefs.GetString(saveKey + "UseCustomTheme") == "true")
            {
                ChangeAccentColor(PlayerPrefs.GetString(saveKey + "CustomTheme" + "AccentColor"));
                ChangeAccentReversedColor(PlayerPrefs.GetString(saveKey + "CustomTheme" + "AccentRevColor"));
                CheckForToggles();
            }

            if (!PlayerPrefs.HasKey(saveKey + "SnapDesktopItems"))
            {
                if (lockDesktopItems == false) { SnapDesktopItems(false); }
                else { SnapDesktopItems(true); }
            }

            else if (PlayerPrefs.GetString(saveKey + "SnapDesktopItems") == "false") { SnapDesktopItems(false); }
            else { SnapDesktopItems(true); }

            if (!PlayerPrefs.HasKey(saveKey + "SaveDesktopOrder")) { SaveDesktopOrder(false); }
            else if (PlayerPrefs.GetString(saveKey + "SaveDesktopOrder") == "false") { SaveDesktopOrder(false); }
            else { SaveDesktopOrder(true); }
        }

        public void SnapDesktopItems(bool value)
        {
            if (desktopDragger == null)
                return;

            if (value == true) { desktopDragger.SnappedDragMode(); PlayerPrefs.SetString(saveKey + "SnapDesktopItems", "true"); }
            else { desktopDragger.FreeDragMode(); PlayerPrefs.SetString(saveKey + "SnapDesktopItems", "false"); }
        }

        public void SaveDesktopOrder(bool value)
        {
            if (desktopDragger == null)
                return;

            if (value == true) 
            {
                for (int i = 0; i < desktopDragger.transform.childCount; ++i)
                {
                    ItemDragger tempDragger = desktopDragger.transform.GetChild(i).GetComponent<ItemDragger>();
                    tempDragger.rememberPosition = true;
                    tempDragger.UpdateObject();
                }

                PlayerPrefs.SetString(saveKey + "SaveDesktopOrder", "true"); 
            }

            else 
            {
                for (int i = 0; i < desktopDragger.transform.childCount; ++i)
                {
                    ItemDragger tempDragger = desktopDragger.transform.GetChild(i).GetComponent<ItemDragger>();
                    tempDragger.rememberPosition = false;
                    tempDragger.RemoveData();
                }

                PlayerPrefs.SetString(saveKey + "SaveDesktopOrder", "false");
            }
        }

        public void CheckForToggles()
        {
            // Invoke color toggle depending on the data
            foreach (Transform obj in accentColorList)
            {
                if (obj.name == PlayerPrefs.GetString(saveKey + "CustomTheme" + "AccentColor"))
                {
                    toggleHelper = obj.GetComponent<Toggle>();
                    toggleHelper.isOn = true;
                    toggleHelper.onValueChanged.Invoke(true);
                }
            }

            foreach (Transform obj in accentReversedColorList)
            {
                if (obj.name == PlayerPrefs.GetString(saveKey + "CustomTheme" + "AccentRevColor"))
                {
                    toggleHelper = obj.GetComponent<Toggle>();
                    toggleHelper.isOn = true;
                    toggleHelper.onValueChanged.Invoke(true);
                }
            }
        }

        public void SelectSystemTheme()
        {
            themeManager.selectedTheme = UIManager.SelectedTheme.Default;
            PlayerPrefs.SetString(saveKey + "UseCustomTheme", "false");
        }

        public void SelectCustomTheme()
        {
            themeManager.selectedTheme = UIManager.SelectedTheme.Custom;
            PlayerPrefs.SetString(saveKey + "UseCustomTheme", "true");
        }

        public void ChangeAccentColor(string colorCode)
        {
            // Change color depending on the color code
            Color colorHelper;
            ColorUtility.TryParseHtmlString("#" + colorCode, out colorHelper);
            themeManager.highlightedColorCustom = new Color(colorHelper.r, colorHelper.g, colorHelper.b, themeManager.highlightedColorCustom.a);
            PlayerPrefs.SetString(saveKey + "CustomTheme" + "AccentColor", colorCode);
        }

        public void ChangeAccentReversedColor(string colorCodeReversed)
        {
            // Change color depending on the color code
            Color colorHelper;
            ColorUtility.TryParseHtmlString("#" + colorCodeReversed, out colorHelper);
            themeManager.highlightedColorSecondaryCustom = new Color(colorHelper.r, colorHelper.g, colorHelper.b, themeManager.highlightedColorSecondaryCustom.a);
            PlayerPrefs.SetString(saveKey + "CustomTheme" + "AccentRevColor", colorCodeReversed);
        }

        public void WipeUserData()
        {
            // Delete user data
            DeleteUserData();
            rebootManager.WipeSystem();
            StartCoroutine("WaitForReboot");
        }

        public void WipeEverything()
        {
            // Delete EVERYTHING! Use with caution
            PlayerPrefs.DeleteAll();
        }

        IEnumerator WaitForReboot()
        {
            yield return new WaitForSeconds(rebootManager.waitTime);
            SelectSystemTheme();
            userManager.InitializeUserManager();
            userManager.desktopScreen.Play("Desktop Out");
            setupManager.PanelAnim(0);
            setupManager.currentBGAnimator = setupManager.steps[0].background.GetComponent<Animator>();
            setupManager.currentPanelAnimator = setupManager.steps[0].panel.GetComponent<Animator>();
            setupManager.currentBGAnimator.Play("Panel In");
            setupManager.currentPanelAnimator.Play("Panel In");
        }

        public void DeleteUserData()
        {
            // User data
            PlayerPrefs.DeleteKey(saveKey + "User" + "Created");
            PlayerPrefs.DeleteKey(saveKey + "User" + "FirstName");
            PlayerPrefs.DeleteKey(saveKey + "User" + "LastName");
            PlayerPrefs.DeleteKey(saveKey + "User" + "Password");
            PlayerPrefs.DeleteKey(saveKey + "User" + "SecQuestion");
            PlayerPrefs.DeleteKey(saveKey + "User" + "SecAnswer");
            PlayerPrefs.DeleteKey(saveKey + "User" + "ProfilePicture");
            PlayerPrefs.DeleteKey(saveKey + "User" + "UseCustomTheme");

            // Reminder data
            PlayerPrefs.DeleteKey(saveKey + "Reminder1Enabled");
            PlayerPrefs.DeleteKey(saveKey + "Reminder2Enabled");
            PlayerPrefs.DeleteKey(saveKey + "Reminder3Enabled");
            PlayerPrefs.DeleteKey(saveKey + "Reminder4Enabled");
            PlayerPrefs.DeleteKey(saveKey + "Reminder1Title");
            PlayerPrefs.DeleteKey(saveKey + "Reminder1Hour");
            PlayerPrefs.DeleteKey(saveKey + "Reminder1Minute");
            PlayerPrefs.DeleteKey(saveKey + "Reminder1Type");
            PlayerPrefs.DeleteKey(saveKey + "Reminder2Title");
            PlayerPrefs.DeleteKey(saveKey + "Reminder2Hour");
            PlayerPrefs.DeleteKey(saveKey + "Reminder2Minute");
            PlayerPrefs.DeleteKey(saveKey + "Reminder2Type");
            PlayerPrefs.DeleteKey(saveKey + "Reminder3Title");
            PlayerPrefs.DeleteKey(saveKey + "Reminder3Hour");
            PlayerPrefs.DeleteKey(saveKey + "Reminder3Minute");
            PlayerPrefs.DeleteKey(saveKey + "Reminder3Type");
            PlayerPrefs.DeleteKey(saveKey + "Reminder4Title");
            PlayerPrefs.DeleteKey(saveKey + "Reminder4Hour");
            PlayerPrefs.DeleteKey(saveKey + "Reminder4Minute");
            PlayerPrefs.DeleteKey(saveKey + "Reminder4Type");

            // Network data
            PlayerPrefs.DeleteKey(saveKey + "ConnectedNetworkTitle");
            PlayerPrefs.DeleteKey(saveKey + "CurrentNetworkIndex");
        }
    }
}