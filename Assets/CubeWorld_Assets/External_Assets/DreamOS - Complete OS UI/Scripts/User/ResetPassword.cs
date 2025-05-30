using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class ResetPassword : MonoBehaviour
    {
        [Header("Resources")]
        public UserManager userManager;
        public TextMeshProUGUI securityQuestion;
        public TMP_InputField securityAnswer;
        public TMP_InputField newPassword;
        public TMP_InputField newPasswordRetype;
        public Animator errorMessage;
        public ModalWindowManager modalManager;

        string tempSecAnswer;

        void OnEnable()
        {
            if (userManager == null) { userManager = (UserManager)GameObject.FindObjectsOfType(typeof(UserManager))[0]; }

            if (userManager.disableUserCreating == false && PlayerPrefs.GetString(userManager.machineID + "User" + "SecQuestion") != "")
            {
                securityQuestion.text = PlayerPrefs.GetString(userManager.machineID + "User" + "SecQuestion");
                tempSecAnswer = PlayerPrefs.GetString(userManager.machineID + "User" + "SecAnswer");
            }

            else
            {
                securityQuestion.text = userManager.systemSecurityQuestion;
                tempSecAnswer = userManager.systemSecurityAnswer;
            }
        }

        public void ChangePassword()
        {
            if (newPassword.text.Length >= userManager.minPasswordCharacter && newPassword.text.Length <= userManager.maxPasswordCharacter
                && newPassword.text == newPasswordRetype.text && securityAnswer.text == tempSecAnswer)
            {
                PlayerPrefs.SetString(userManager.machineID + "User" + "Password", newPassword.text);
                userManager.password = newPassword.text;
                userManager.hasPassword = true;
                modalManager.CloseWindow();
                newPassword.text = "";
                newPasswordRetype.text = "";
                securityAnswer.text = "";
            }

            else { errorMessage.Play("Auto In"); }
        }
    }
}