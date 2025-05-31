using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace MalbersAnimations.Utilities
{
    /// <summary>
    /// Utility Component to Notify if the collider was disabled to the Trigger Proxy component
    /// </summary>
    public class TriggerTarget : MonoBehaviour 
    {
        public Collider m_collider;

        public List<TriggerProxy> Proxies;
        public static List<TriggerTarget> set;
         
        private void Awake()
        {
            if (set == null) set = new();
            hideFlags = HideFlags.HideInInspector;
        }

        private void OnEnable()
        {
            set.Add(this);
        }

        private void OnDisable()
        {
            if (Proxies != null)
                foreach (var p in Proxies)
                {
                    if (p != null) p.RemoveTrigger(m_collider, false); //False because it will create an infinity loop
                }

            Proxies = new();     //Reset

            set.Remove(this);
        }

        public void AddProxy(TriggerProxy trigger,Collider col)
        {
            if (Proxies == null) Proxies = new();
            Proxies.Add(trigger);
            m_collider = col;
        }

        public void RemoveProxy(TriggerProxy trigger)
        {
           /* if (Proxies.Contains(trigger)) */
            Proxies.Remove(trigger);
        }
    }
}