using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Variables/Float Comparer")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/variable-listeners-and-comparers")]
    public class FloatComparer : FloatVarListener
    {
        public List<AdvancedFloatEvent> compare = new();

        private AdvancedFloatEvent Pin;
        public FloatEvent OnValueChanged = new();


        public void Pin_Comparer(int index)
        {
            Pin = compare[index];
        }

        public void Pin_Comparer_SetValue(float value)
        {
            if (Pin != null) Pin.Value.Value = value;
        }

        public void Pin_Comparer_SetValue(FloatVar value)
        {
            if (Pin != null) Pin.Value.Value = value;
        }


        /// <summary>Set the first value on the comparer </summary>
        public float SetCompareFirstValue { get => compare[0].Value.Value; set => compare[0].Value.Value = value; }

        public override float Value
        {
            set
            {
                base.Value = value;
                if (Auto)
                {
                    OnValueChanged.Invoke(value);
                    Compare();
                }
            }
        }

        public float this[int index]
        {
            get => compare[index].Value.Value;
            set => compare[index].Value.Value = value;
        }

        void OnEnable()
        {
            if (value.Variable && Auto)
            {
                Debug.Log("auto = ", this);

                value.Variable.OnValueChanged += Compare;
                value.Variable.OnValueChanged += Invoke;
            }

            Raise.Invoke(Value);
        }

        void OnDisable()
        {
            if (value.Variable && Auto)
            {
                value.Variable.OnValueChanged -= Compare;
                value.Variable.OnValueChanged -= Invoke;
            }
        }

        /// <summary>Compares the Int parameter on this Component and if the condition is made then the event will be invoked</summary>
        public virtual void Compare()
        {
            if (isActiveAndEnabled)
                foreach (var item in compare)
                    item.ExecuteAdvanceFloatEvent(value);
        }


        /// <summary>Compares an given int Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(float value)
        {
            // Debug.Log("value = " + value, this);
            // Debug.Break();

            if (isActiveAndEnabled)
                foreach (var item in compare)
                    item.ExecuteAdvanceFloatEvent(value);
        }

        /// <summary>Compares an given intVar Value and if the condition is made then the event will be invoked</summary>
        public virtual void Compare(FloatVar value)
        {
            if (enabled)
                foreach (var item in compare)
                    item.ExecuteAdvanceFloatEvent(value.Value);
        }

        public void Index_Disable(int index) => compare[index].active = false;
        public void Index_Enable(int index) => compare[index].active = true;


        public void Index_Enable_Only(int index)
        {
            compare[index].active = true;

            for (int i = 0; i < compare.Count; i++)
            {
                if (i == index) continue;
                compare[i].active = false;
            }
        }

        public void Index_Disable_Only(int index)
        {
            compare[index].active = false;

            for (int i = 0; i < compare.Count; i++)
            {
                if (i == index) continue;
                compare[i].active = true;
            }
        }


        public void SetRandomValue01()
        {
            Value = Random.Range(0f, 1f);
        }
    }


    //INSPECTOR
#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FloatComparer))]
    public class FloatComparerListenerEditor : IntCompareEditor { }
#endif
}