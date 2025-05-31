using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MalbersAnimations.UI
{
    /// <summary> Script to Update in a Canvas the changes of the stats</summary>
    [DefaultExecutionOrder(501)]
    public class MFloatingNumbers : MonoBehaviour
    {
        public class MDamageableUI
        {
            public MDamageable damageable;
            public Transform followTransform;
            public UnityAction<float> OnValueChange = delegate { };
        }

        [Tooltip("Runtime Set that store all the DamageNumber you want to monitor")]
        [RequiredField] public RuntimeDamageableSet Set;

        [Tooltip("Damage Number Prefab to show the damage float value")]
        [RequiredField] public UIFollowTransform DamageNumber;

        [Tooltip("Damage Number Prefab to show the Critical damage float value")]
        [RequiredField] public UIFollowTransform CriticalNumber;


        [Tooltip("Reference for the Camera")]
        public TransformReference Camera;

        [Tooltip("Find a bone inside the Hierarchy of the Stat Manager")]
        public string FollowTransform = "Head";

        [Tooltip("if the damage was zero do not show the floating number")]
        public bool ignoreZero = true;

        private List<MDamageableUI> TrackedStats;

        [Tooltip("Change the Scale of the UI if the hit is critical")]
        public Vector3 CriticalScale = Vector3.one;

        private Camera MainCamera;

        private void Awake()
        {
            TrackedStats = new List<MDamageableUI>();

            Set.Clear();


            if (Camera.Value != null)
            {
                MainCamera = Camera.Value.GetComponent<Camera>();
            }
            else
            {
                MainCamera = MTools.FindMainCamera();
                Camera.Value = MainCamera.transform;
            }
        }


        private void OnEnable()
        {
            Set.OnItemAdded.AddListener(OnAddedMDamageable);
            Set.OnItemRemoved.AddListener(OnRemovedStat);
        }

        private void OnDisable()
        {
            Set.OnItemAdded.RemoveListener(OnAddedMDamageable);
            Set.OnItemRemoved.RemoveListener(OnRemovedStat);
        }

        private void OnAddedMDamageable(MDamageable dam)
        {
            var item = new MDamageableUI
            { damageable = dam };

            var child = dam.transform.FindGrandChild(FollowTransform);
            item.followTransform = child != null ? child : dam.transform;

            //Track when the Stat changes value
            item.OnValueChange = (floatValue) =>
            {
                if (ignoreZero && floatValue < 0.1f) return; //do nothing if the damage is close to zero

                UIFollowTransform FU = null;

                var WasCritical = item.damageable.LastDamage.WasCritical; //Store if the Damage was Critical

                var FloatingDamage = WasCritical ? CriticalNumber : DamageNumber;

                if (FloatingDamage != null)
                {
                    FU = Instantiate(FloatingDamage);
                    FU.SetTransform(item.followTransform);
                    FU.transform.SetParent(transform);

                    FU.name = FU.name.Replace("(Clone)", "");

                    FU.name += ": " + floatValue.ToString("F0");

                    // Debug.Log("floatValue = " + floatValue);

                    var text = FU.GetComponentInChildren<Text>();
                    if (text)
                    {
                        text.text = floatValue.ToString("F0");

                        //Draw the color of the Damage
                        if (item.damageable.LastDamage.Element.element != null)
                            text.color = item.damageable.LastDamage.Element.element.color;
                    }
                }
            };


            item.damageable.events.OnReceivingDamage.AddListener(item.OnValueChange);
            TrackedStats.Add(item);
        }

        //Remove stat from the Set
        private void OnRemovedStat(MDamageable stats)
        {
            var item = TrackedStats.Find(x => x.damageable == stats);

            if (item != null) RemoveFromGroup(item);
        }

        private void RemoveFromGroup(MDamageableUI item)
        {
            //Debug.Log($"Removed From Group {item.slider}", item.slider );

            item.damageable.events.OnReceivingDamage.RemoveListener(item.OnValueChange);
            item.OnValueChange = null;

            TrackedStats.Remove(item);
            Set.Item_Remove(item.damageable);
        }

        private void Reset()
        {
            Set = MTools.GetInstance<RuntimeDamageableSet>("Enemy Damageable");
        }
    }
}