using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/User/Get User Info")]
    public class GetUserInfo : MonoBehaviour
    {
        [Header("Resources")]
        public UserManager userManager;

        [Header("Settings")]
        public Reference getInformation;
        public bool updateOnEnable = true;

        TextMeshProUGUI textObject;
        Image imageObject;

        // Reference list
        public enum Reference
        {
            FullName,
            FirstName,
            LastName,
            Password,
            ProfilePicture
        }

        void OnEnable()
        {
            // Find User manager in the scene
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }

            // If it's true, then get the requested information at start
            if (updateOnEnable == true) { GetInformation(); }
        }

        public void GetInformation()
        {
            if (userManager == null)
                return;

            // If attached object is full name, then get FirstName and LastName value
            if (getInformation == Reference.FullName)
            {
                textObject = gameObject.GetComponent<TextMeshProUGUI>();
                textObject.text = userManager.firstName + " " + userManager.lastName;
            }

            // If attached object is first name, then get FirstName value
            else if (getInformation == Reference.FirstName)
            {
                textObject = gameObject.GetComponent<TextMeshProUGUI>();
                textObject.text = userManager.firstName;
            }

            // If attached object is last name, then get LastName value
            else if (getInformation == Reference.LastName)
            {
                textObject = gameObject.GetComponent<TextMeshProUGUI>();
                textObject.text = userManager.lastName;
            }

            // If attached object is password, then get Password value
            else if (getInformation == Reference.Password)
            {
                textObject = gameObject.GetComponent<TextMeshProUGUI>();
                textObject.text = userManager.password;
            }

            // If attached object is Picture, then load picture
            else if (getInformation == Reference.ProfilePicture)
            {
                imageObject = gameObject.GetComponent<Image>();
                imageObject.sprite = userManager.profilePicture;
            }
        }

        public void AddToGUIList() { if (userManager != null) { userManager.guiList.Add(this); } }
    }
}