using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Animator))]
    public class WidgetItem : MonoBehaviour, IEndDragHandler
    {
        [Header("Resources")]
        public Animator widgetAnimator;
        [HideInInspector] public WidgetManager widgetManager;

        [Header("Settings")]
        public bool isOn;

        [Header("Events")]
        public UnityEvent enabledEvents;
        public UnityEvent disabledEvents;

        float widgetPosX;
        float widgetPosY;
        bool initialized = false;

        void Start()
        {
            PrepareWidget();
        }

        void OnEnable()
        {
            if (initialized == false)
                return;

            widgetAnimator.enabled = true;

            if (PlayerPrefs.GetString(widgetManager.userManager.machineID + "Widget" + gameObject.name) == "enabled") { widgetAnimator.Play("Widget In"); }
            else { widgetAnimator.Play("Widget Out"); }

            StartCoroutine("DisableAnimator");
        }

        public void PrepareWidget()
        {
            if (widgetAnimator == null) { widgetAnimator = gameObject.GetComponent<Animator>(); }
            widgetAnimator.enabled = true;

            if (!PlayerPrefs.HasKey(widgetManager.userManager.machineID + "Widget" + gameObject.name) && isOn == true)
            {
                AlignToCenter();

                widgetPosX = gameObject.transform.position.x;
                widgetPosY = gameObject.transform.position.y;

                PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX", widgetPosX);
                PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY", widgetPosY);
                PlayerPrefs.SetString(widgetManager.userManager.machineID + "Widget" + gameObject.name, "enabled");
              
                widgetAnimator.Play("Widget In");
                enabledEvents.Invoke();

                isOn = true;
                StartCoroutine("DisableAnimator");
            }

            else if (!PlayerPrefs.HasKey(widgetManager.userManager.machineID + "Widget" + gameObject.name) && isOn == false)
            {
                AlignToCenter();

                widgetPosX = gameObject.transform.position.x;
                widgetPosY = gameObject.transform.position.y;

                PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX", widgetPosX);
                PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY", widgetPosY);
                PlayerPrefs.SetString(widgetManager.userManager.machineID + "Widget" + gameObject.name, "disabled");

                widgetAnimator.Play("Widget Out");
                disabledEvents.Invoke();
             
                isOn = false;
                gameObject.SetActive(false);
            }

            else if (PlayerPrefs.GetString(widgetManager.userManager.machineID + "Widget" + gameObject.name) == "enabled")
            {
                widgetPosX = PlayerPrefs.GetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX");
                widgetPosY = PlayerPrefs.GetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY");
                gameObject.transform.position = new Vector3(widgetPosX, widgetPosY, 0);
              
                enabledEvents.Invoke();
                widgetAnimator.Play("Widget In");
                
                isOn = true;
                StartCoroutine("DisableAnimator");
            }

            else if (PlayerPrefs.GetString(widgetManager.userManager.machineID + "Widget" + gameObject.name) == "disabled")
            {
                disabledEvents.Invoke();
                       
                isOn = false;
                gameObject.SetActive(false);
            }

            initialized = true;
        }

        public void EnableWidget()
        {
            StopCoroutine("DisableAnimator");
            StopCoroutine("DisableObject");

            gameObject.SetActive(true);
            widgetAnimator.enabled = true;

            widgetPosX = PlayerPrefs.GetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX");
            widgetPosY = PlayerPrefs.GetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY");
            PlayerPrefs.SetString(widgetManager.userManager.machineID + "Widget" + gameObject.name, "enabled");

            gameObject.transform.position = new Vector3(widgetPosX, widgetPosY, 0);
            widgetAnimator.Play("Widget In");
            enabledEvents.Invoke();

            isOn = true;
            StartCoroutine("DisableAnimator");
        }

        public void DisableWidget()
        {
            StopCoroutine("DisableObject");
            StartCoroutine("DisableObject");

            isOn = false;
            widgetAnimator.enabled = true;

            PlayerPrefs.SetString(widgetManager.userManager.machineID + "Widget" + gameObject.name, "disabled");

            widgetAnimator.Play("Widget Out");
            disabledEvents.Invoke();
        }

        public void AlignToCenter()
        {
            gameObject.transform.localPosition = new Vector3(0, 0, 0);

            widgetPosX = gameObject.transform.position.x;
            widgetPosY = gameObject.transform.position.y;

            PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX", widgetPosX);
            PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY", widgetPosY);
        }

        public void OnEndDrag(PointerEventData data)
        {
            widgetPosX = gameObject.transform.position.x;
            widgetPosY = gameObject.transform.position.y;

            PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosX", widgetPosX);
            PlayerPrefs.SetFloat(widgetManager.userManager.machineID + "Widget" + gameObject.name + "PosY", widgetPosY);

            gameObject.transform.position = new Vector3(widgetPosX, widgetPosY, 0);
        }

        IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
        }

        IEnumerator DisableAnimator()
        {
            yield return new WaitForSeconds(0.5f);
            widgetAnimator.enabled = false;
        }
    }
}