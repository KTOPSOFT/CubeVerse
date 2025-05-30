//This asset was uploaded by https://unityassetcollection.com

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS.Examples
{
    public class WebsiteScroller : MonoBehaviour
    {
        public Scrollbar scrollbar;
        [Range(0.01f, 0.1f)] public float scrollSmooth = 0.01f;
        [Range(0.005f, 0.1f)] public float scrollHelper = 0.01f;

        float scrollTo;
        float scrollToHelper;
        bool isHigher;

        void Start()
        {
            this.enabled = false;
        }

        void Update()
        {
            if (scrollbar.value != scrollTo)
                scrollbar.value = Mathf.Lerp(scrollbar.value, scrollTo, scrollSmooth);
            
            if (isHigher == true && scrollbar.value >= scrollToHelper
                || isHigher == false && scrollbar.value <= scrollToHelper)
                this.enabled = false;
        }

        public void ScrollTo(float value)
        {
            this.enabled = true;
            scrollTo = value;

            if (scrollbar.value >= scrollTo)
            {
                scrollToHelper = scrollTo + scrollHelper;
                isHigher = false;
            }

            else if (scrollbar.value <= scrollTo)
            {
                scrollToHelper = scrollTo - scrollHelper;
                isHigher = true;
            }
        }
    }
}