using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class ShowPassword : MonoBehaviour
    {
        [Header("Resources")]
        public TMP_InputField inputField;
        public GameObject showObject;
        public GameObject hideObject;

        void OnEnable()
        {
            HidePassword();
        }

        public void TogglePassword()
        {
            if (inputField != null && inputField.contentType == TMP_InputField.ContentType.Standard)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
                inputField.ForceLabelUpdate();

                showObject.SetActive(true);
                hideObject.SetActive(false);
            }

            else if (inputField != null && inputField.contentType == TMP_InputField.ContentType.Password)
            {
                inputField.contentType = TMP_InputField.ContentType.Standard;
                inputField.ForceLabelUpdate();

                showObject.SetActive(false);
                hideObject.SetActive(true);
            }
        }

        public void HidePassword()
        {
            if (inputField != null && inputField.contentType == TMP_InputField.ContentType.Standard)
            {
                inputField.contentType = TMP_InputField.ContentType.Password;
                inputField.ForceLabelUpdate();

                showObject.SetActive(true);
                hideObject.SetActive(false);
            }
        }
    }
}