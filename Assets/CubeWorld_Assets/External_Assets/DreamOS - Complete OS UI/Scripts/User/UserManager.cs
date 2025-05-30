using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    public class UserManager : MonoBehaviour
    {
        // Resources
        public BootManager bootManager;
        public Animator setupScreen;
        public Animator lockScreen;
        public Animator desktopScreen;
        public TMP_InputField lockScreenPassword;
        public BlurManager lockScreenBlur;
        public ProfilePictureLibrary ppLibrary;
        public GameObject ppItem;
        public Transform ppParent;

        // Content
        [Range(1, 20)] public int minNameCharacter = 1;
        [Range(1, 20)] public int maxNameCharacter = 14;

        [Range(1, 20)] public int minPasswordCharacter = 4;
        [Range(1, 20)] public int maxPasswordCharacter = 16;

        // Events
        public UnityEvent onLogin;
        public UnityEvent onLock;
        public UnityEvent onWrongPassword;

        public string systemUsername = "Admin";
        public string systemLastname = "";
        public string systemPassword = "1234";
        public string systemSecurityQuestion = "Answer: DreamOS";
        public string systemSecurityAnswer = "DreamOS";

        // Settings
        public bool disableUserCreating = false;
        public bool saveProfilePicture = true;
        public bool deletePrefsAtStart = false;
        public int ppIndex;

        // Multi Instance Support
        public bool allowMultiInstance;
        public string machineID = "DreamOS";

        // User variables
        [HideInInspector] public string firstName;
        [HideInInspector] public string lastName;
        [HideInInspector] public string password;
        [HideInInspector] public string secQuestion;
        [HideInInspector] public string secAnswer;
        [HideInInspector] public Sprite profilePicture;

        [HideInInspector] public bool hasPassword;
        [HideInInspector] public bool nameOK;
        [HideInInspector] public bool lastNameOK;
        [HideInInspector] public bool passwordOK;
        [HideInInspector] public bool passwordRetypeOK;
        [HideInInspector] public int userCreated;

        [HideInInspector] public bool isLockScreenOpen = false;

        [HideInInspector] public List<GetUserInfo> guiList = new List<GetUserInfo>();

        void Awake()
        {
            // Delete given prefs if option is enabled
            if (deletePrefsAtStart == true) { PlayerPrefs.DeleteAll(); }

            // Check for multi instance support
            if (allowMultiInstance == false) { machineID = "DreamOS"; }

            InitializeUserManager();
            InitializeProfilePictures();
        }

        public void InitializeUserManager()
        {
            // Find Boot manager in the scene
            if (bootManager == null)
                bootManager = (BootManager)GameObject.FindObjectsOfType(typeof(BootManager))[0];

            if (disableUserCreating == false)
            {
                userCreated = PlayerPrefs.GetInt(machineID + "User" + "Created");
                firstName = PlayerPrefs.GetString(machineID + "User" + "FirstName");
                lastName = PlayerPrefs.GetString(machineID + "User" + "LastName");
                password = PlayerPrefs.GetString(machineID + "User" + "Password");
                secQuestion = PlayerPrefs.GetString(machineID + "User" + "SecQuestion");
                secAnswer = PlayerPrefs.GetString(machineID + "User" + "SecAnswer");

                if (!PlayerPrefs.HasKey(machineID + "User" + "ProfilePicture"))
                {
                    ppIndex = 0;
                    PlayerPrefs.SetInt(machineID + "User" + "ProfilePicture", ppIndex);
                }
                else { ppIndex = PlayerPrefs.GetInt(machineID + "User" + "ProfilePicture"); }

                profilePicture = ppLibrary.pictures[ppIndex].pictureSprite;

                // If password is null, change boolean
                if (password == "") { hasPassword = false; }
                else { hasPassword = true; }

                // If user is not created, show Setup screen
                if (userCreated == 0)
                {
                    bootManager.enabled = false;
                    bootManager.bootAnimator.gameObject.SetActive(false);
                    setupScreen.gameObject.SetActive(true);
                    setupScreen.Play("Panel In");
                }
                else { BootSystem(); }
            }

            else
            {
                // If password is null, change boolean
                if (systemPassword == "") { hasPassword = false; }
                else { hasPassword = true; }

                // Setting up the user details
                firstName = systemUsername;
                lastName = systemLastname;
                password = systemPassword;
                profilePicture = ppLibrary.pictures[ppIndex].pictureSprite;

                BootSystem();
            }
        }

        public void InitializeProfilePictures()
        {
            if (ppParent == null || ppItem == null)
                return;

            foreach (Transform child in ppParent) { Destroy(child.gameObject); }
            for (int i = 0; i < ppLibrary.pictures.Count; ++i)
            {
                GameObject go = Instantiate(ppItem, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(ppParent, false);
                go.name = ppLibrary.pictures[i].pictureID;

                Image prevImage = go.transform.Find("Image Mask/Image").GetComponent<Image>();
                prevImage.sprite = ppLibrary.pictures[i].pictureSprite;

                Button wpButton = go.GetComponent<Button>();
                wpButton.onClick.AddListener(delegate 
                { 
                    ChangeProfilePicture(go.transform.GetSiblingIndex()); 
                    UpdateUserInfoUI();

                    try { wpButton.gameObject.GetComponentInParent<ModalWindowManager>().CloseWindow(); }
                    catch { Debug.Log("Cannot close the window due to missing modal window manager."); }
                });
            }

            GetAllUserInfoComps();
            UpdateUserInfoUI();
        }

        public void ChangeFirstName(string textVar)
        {
            firstName = textVar;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "FirstName", firstName); }
        }

        public void ChangeFirstNameTMP(TMP_InputField tmpVar)
        {
            firstName = tmpVar.text;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "FirstName", firstName); }
        }

        public void ChangeLastName(string textVar)
        {
            lastName = textVar;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "LastName", lastName); }
        }

        public void ChangeLastNameTMP(TMP_InputField tmpVar)
        {
            lastName = tmpVar.text;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "LastName", lastName); }
        }

        public void ChangePassword(string textVar)
        {
            password = textVar;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "Password", password); }
        }

        public void ChangePasswordTMP(TMP_InputField tmpVar)
        {
            password = tmpVar.text;
            if (disableUserCreating == false) { PlayerPrefs.SetString(machineID + "User" + "Password", password); }
        }

        public void ChangeSecurityQuestion(string textVar) { PlayerPrefs.SetString(machineID + "User" + "SecQuestion", textVar); }
        public void ChangeSecurityQuestionTMP(TMP_InputField tmpVar) { PlayerPrefs.SetString(machineID + "User" + "SecQuestion", tmpVar.text); }
        public void ChangeSecurityAnswer(string textVar) { PlayerPrefs.SetString(machineID + "User" + "SecAnswer", textVar); }
        public void ChangeSecurityAnswerTMP(TMP_InputField tmpVar) { PlayerPrefs.SetString(machineID + "User" + "SecAnswer", tmpVar.text); }

        public void ChangeProfilePicture(int pictureIndex)
        {
            ppIndex = pictureIndex;
            profilePicture = ppLibrary.pictures[ppIndex].pictureSprite;
            if (saveProfilePicture == true) { PlayerPrefs.SetInt(machineID + "User" + "ProfilePicture", ppIndex); }
        }

        public void UpdateUserInfoUI()
        {
            for (int i = 0; i < guiList.Count; ++i)
                guiList[i].GetInformation();
        }

        public void GetAllUserInfoComps()
        {
            guiList.Clear();
            GetUserInfo[] list = FindObjectsOfType(typeof(GetUserInfo)) as GetUserInfo[];
            foreach (GetUserInfo obj in list) { guiList.Add(obj); }
        }

        public void CreateUser()
        {
            userCreated = 1;
            PlayerPrefs.SetInt(machineID + "User" + "Created", userCreated);
            password = PlayerPrefs.GetString(machineID + "User" + "Password");

            if (password == "") { hasPassword = false; }
            else { hasPassword = true; }
         
            UpdateUserInfoUI();
        }

        public void BootSystem()
        {
            bootManager.enabled = true;
            bootManager.bootAnimator.gameObject.SetActive(true);
            setupScreen.gameObject.SetActive(false);
            bootManager.bootAnimator.Play("Boot Start");
        }

        public void StartOS()
        {
            if (hasPassword == true)
            {
                lockScreenPassword.gameObject.SetActive(false);
                lockScreen.Play("Skip Login");
            }

            else
            {
                lockScreenPassword.gameObject.SetActive(true);
                lockScreen.Play("Lock Screen In");
            }
        }

        public void LockOS()
        {
            if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
            lockScreen.gameObject.SetActive(true);
            lockScreen.Play("Lock Screen In");
            desktopScreen.Play("Desktop Out");
            onLock.Invoke();
        }

        public void LockScreenOpenClose()
        {
            if (isLockScreenOpen == true)
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
                lockScreen.Play("Lock Screen Out");
            }
            else { lockScreen.Play("Lock Screen In"); }
        }

        public void LockScreenAnimate()
        {
            if (hasPassword == true)
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurInAnim(); }
                lockScreen.Play("Lock Screen Password In");
            }

            else
            {
                if (lockScreenBlur != null) { lockScreenBlur.BlurOutAnim(); }
                lockScreen.Play("Lock Screen Out");
                desktopScreen.Play("Desktop In");
                onLogin.Invoke();
            }
        }

        public void Login()
        {
            if (lockScreenPassword.text == password)
            {
                lockScreen.Play("Lock Screen Password Out");
                desktopScreen.Play("Desktop In");
                onLogin.Invoke();
                StartCoroutine("DisableLockScreenHelper");
            }
            else if (lockScreenPassword.text != password) { onWrongPassword.Invoke(); }
        }

        IEnumerator DisableLockScreenHelper()
        {
            yield return new WaitForSeconds(1f);
            lockScreen.gameObject.SetActive(false);
        }
    }
}