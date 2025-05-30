using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.DreamOS
{
    [AddComponentMenu("DreamOS/Events/Timed Event")]
    public class TimedEvent : MonoBehaviour
    {
        // Settings
        public float timer = 4;
        public bool enableAtStart;

        // Events
        public UnityEvent timerAction;

        void Start()
        {
            if(enableAtStart == true)
                StartCoroutine("ProcessTimedEvent");
        }

        IEnumerator ProcessTimedEvent()
        {
            yield return new WaitForSeconds(timer);
            timerAction.Invoke();
            StopCoroutine("ProcessTimedEvent");
        }

        public void StartIEnumerator()
        {
            StartCoroutine("ProcessTimedEvent");
        }

        public void StopIEnumerator()
        {
            StopCoroutine("ProcessTimedEvent");
        }
    }
}
