using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Michsky.DreamOS
{
    [RequireComponent(typeof(Animator))]
    public class HorizontalSelector : MonoBehaviour
    {
        // Resources
        public Animator selectorAnimator;
        public TextMeshProUGUI label;
        public TextMeshProUGUI labelHelper;
        public Transform indicatorParent;
        public GameObject indicatorObject;
        string newItemTitle;

        // Saving
        public bool saveValue;
        public string selectorTag = "Tag Text";

        // Settings
        public bool enableIndicators = true;
        public bool invokeAtStart;
        public bool invertAnimation;
        public bool loopSelection;
        public int defaultIndex = 0;
        [HideInInspector] public int index = 0;

        // Items
        public List<Item> itemList = new List<Item>();
        [System.Serializable]
        public class SelectorEvent : UnityEvent<int> { }
        public SelectorEvent selectorEvent;

        [System.Serializable]
        public class Item
        {
            public string itemTitle = "Item Title";
            public UnityEvent onValueChanged = new UnityEvent();
        }

        void Awake()
        {
            if (selectorAnimator == null) { selectorAnimator = gameObject.GetComponent<Animator>(); }
            if (label == null || labelHelper == null)
            {
                Debug.LogError("<b>[Horizontal Selector]</b> Cannot initalize the object due to missing resources.", this);
                return;
            }

            SetupSelector();

            if (invokeAtStart == true)
            {
                itemList[index].onValueChanged.Invoke();
                selectorEvent.Invoke(index);
            }
        }

        public void SetupSelector()
        {
            if (itemList.Count != 0)
            {
                if (saveValue == true)
                {
                    if (PlayerPrefs.HasKey(selectorTag + "HorizontalSelector") == true) { defaultIndex = PlayerPrefs.GetInt(selectorTag + "HorizontalSelector"); }
                    else { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", defaultIndex); }
                }

                label.text = itemList[defaultIndex].itemTitle;
                labelHelper.text = label.text;
                index = defaultIndex;

                if (enableIndicators == true) { UpdateIndicators(); }
                else { Destroy(indicatorParent.gameObject); }
            }
        }

        public void UpdateIndicators()
        {
            if (enableIndicators == false)
                return;

            foreach (Transform child in indicatorParent) { Destroy(child.gameObject); }
            for (int i = 0; i < itemList.Count; ++i)
            {
                GameObject go = Instantiate(indicatorObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(indicatorParent, false);
                go.name = itemList[i].itemTitle;
                Transform onObj = go.transform.Find("On");
                Transform offObj = go.transform.Find("Off");

                if (i == index) { onObj.gameObject.SetActive(true); offObj.gameObject.SetActive(false); }
                else { onObj.gameObject.SetActive(false); offObj.gameObject.SetActive(true); }
            }
        }

        public void PreviousClick()
        {
            if (loopSelection == false)
            {
                if (index != 0)
                {
                    labelHelper.text = label.text;

                    if (index == 0) { index = itemList.Count - 1; }
                    else { index--; }

                    label.text = itemList[index].itemTitle;
                    itemList[index].onValueChanged.Invoke();
                    selectorEvent.Invoke(index);
                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation == true) { selectorAnimator.Play("Forward"); }
                    else { selectorAnimator.Play("Previous"); }

                    if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }
                }
            }

            else
            {
                labelHelper.text = label.text;

                if (index == 0) { index = itemList.Count - 1; }
                else { index--; }

                label.text = itemList[index].itemTitle;
                itemList[index].onValueChanged.Invoke();
                selectorEvent.Invoke(index);
                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation == true) { selectorAnimator.Play("Forward"); }
                else { selectorAnimator.Play("Previous"); }

                if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }
            }

            if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }

            if (enableIndicators == true)
            {
                for (int i = 0; i < itemList.Count; ++i)
                {
                    GameObject go = indicatorParent.GetChild(i).gameObject;
                    Transform onObj = go.transform.Find("On");
                    Transform offObj = go.transform.Find("Off");

                    if (i == index) { onObj.gameObject.SetActive(true); offObj.gameObject.SetActive(false); }
                    else { onObj.gameObject.SetActive(false); offObj.gameObject.SetActive(true); }
                }
            }
        }

        public void ForwardClick()
        {
            if (loopSelection == false)
            {
                if (index != itemList.Count - 1)
                {
                    labelHelper.text = label.text;

                    if ((index + 1) >= itemList.Count) { index = 0; }
                    else { index++; }

                    label.text = itemList[index].itemTitle;
                    itemList[index].onValueChanged.Invoke();
                    selectorEvent.Invoke(index);

                    selectorAnimator.Play(null);
                    selectorAnimator.StopPlayback();

                    if (invertAnimation == true) { selectorAnimator.Play("Previous"); }
                    else { selectorAnimator.Play("Forward"); }

                    if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }
                }
            }

            else
            {
                labelHelper.text = label.text;

                if ((index + 1) >= itemList.Count) { index = 0; }
                else { index++; }

                label.text = itemList[index].itemTitle;
                itemList[index].onValueChanged.Invoke();
                selectorEvent.Invoke(index);

                selectorAnimator.Play(null);
                selectorAnimator.StopPlayback();

                if (invertAnimation == true) { selectorAnimator.Play("Previous"); }
                else { selectorAnimator.Play("Forward"); }

                if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }
            }

            if (saveValue == true) { PlayerPrefs.SetInt(selectorTag + "HorizontalSelector", index); }

            if (enableIndicators == true)
            {
                for (int i = 0; i < itemList.Count; ++i)
                {
                    GameObject go = indicatorParent.GetChild(i).gameObject;

                    Transform onObj = go.transform.Find("On");
                    Transform offObj = go.transform.Find("Off");

                    if (i == index) { onObj.gameObject.SetActive(true); offObj.gameObject.SetActive(false); }
                    else { onObj.gameObject.SetActive(false); offObj.gameObject.SetActive(true); }
                }
            }
        }

        public void CreateNewItem(string title)
        {
            Item item = new Item();
            newItemTitle = title;
            item.itemTitle = newItemTitle;
            itemList.Add(item);
        }

        public void AddNewItem()
        {
            Item item = new Item();
            itemList.Add(item);
        }

        public void RemoveItem(string itemTitle)
        {
            var item = itemList.Find(x => x.itemTitle == itemTitle);
            itemList.Remove(item);
            SetupSelector();
        }

        public void UpdateUI()
        {
            label.text = itemList[index].itemTitle;
            UpdateIndicators();
        }
    }
}