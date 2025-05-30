using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.DreamOS
{
    public class RectTransformRotator : MonoBehaviour
    {
        [Header("Resources")]
        public RectTransform objectToRotate;

        [Header("Settings")]
        public float multiplier = 15;

        float currentPos;

        void Start()
        {
            if (objectToRotate == null)
                this.enabled = false;
        }

        void Update()
        {
            currentPos += multiplier * Time.deltaTime;
            objectToRotate.rotation = Quaternion.Euler(0, 0, currentPos);

            if (currentPos >= 360)
                currentPos = 0;
        }
    }
}