using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>  Class used to show the Health in heart styles instead of a var </summary>
    [AddComponentMenu("Malbers/UI/Stat Unit UI")]
    public class StatUnitUI : MonoBehaviour
    {
        [Header("Data")]
        [Tooltip("Current Value of the Stat")]
        public FloatReference value = new(10);
        [Tooltip("Max Value of the Stat")]
        public IntReference maxValue = new(10);
        [Tooltip("Section the Max Value. E.g if Max Value is 100 and Divider is 10 it will show 10 Units ")]
        [Min(1)] public int Divider = 1;
        public StatUnit Unit;

        [Header("Animation")]
        [Min(0)] public float FillTime = 0.1f;
        public bool FillSequence = true;

        [Min(0)] public float EmptyTime = 0.1f;
        public bool EmptySequence = true;
        public float DefaultScale = 1f;

        private List<StatUnit> Units;

        private int MaxValue => maxValue / Divider;

        private float Value => value / Divider;


        private void Awake()
        {
            InitializeUnits();

            UpdateUnits();
        }


        private void InitializeUnits()
        {
            Units = new();

            for (int i = 0; i < MaxValue; i++)
            {
                var NewUnit = Instantiate(Unit, transform);

                NewUnit.name = NewUnit.name.Replace("(Clone)", $"({i + 1})");
                Units.Add(NewUnit);
                NewUnit.transform.localScale = Vector3.one * DefaultScale;
            }
        }

        private void UpdateUnits()
        {
            //Fill hearts 
            for (int i = 0; i < (int)Value; i++)
            {
                var unit = Units[i];
                unit.Full.fillAmount = 1;
            }

            //Unfill the missing hearts
            for (int i = (int)Value; i < MaxValue; i++)
            {
                var unit = Units[i];
                unit.Full.fillAmount = 0;
            }

            //Remove the values from the Active heart Half/Quarter
            var UnitID = Mathf.Clamp((int)Value, 0, MaxValue);

            if (Value - UnitID > 0)
            {
                Units[Mathf.Clamp(UnitID, 0, MaxValue - 1)].Full.fillAmount = Value - (int)Value;
            }

            LastUnitScaler((int)Value, UnitID);
        }

        private void Scaler(StatUnit unit, bool value)
        {
            //Activate the scaler 
            unit.SetScaler(value);
            unit.ResetScale();
        }

        public void SetValue(float newValue)
        {
            newValue = Mathf.Clamp(newValue, 0, maxValue); //Clamp to the Max

            if (newValue == Value) return; //Do nothing if the value are the same;

            //Reset the Scaler
            foreach (var item in Units) Scaler(item, false);

            bool Increasing = value < newValue; //Check if we are adding values or removing

            float OldValue = Value;
            value = newValue;
            var UnitID = Mathf.Clamp((int)(Value), 0, MaxValue - 1);
            lasUnit = Mathf.Clamp((int)(OldValue), 0, MaxValue - 1); ;

            float nextFillTime = 0;

            //Adding Hearts!!
            if (Increasing)
            {
                //Fill Full hearts 
                for (int i = (int)OldValue; i < (int)Value; i++)
                {
                    var unit = Units[i];

                    if (FillTime > 0)
                    {
                        if (FillSequence)
                        {
                            this.Delay_Action(nextFillTime, () => unit.SetFillValue(1, FillTime));
                            nextFillTime += FillTime;
                        }
                        else
                        {
                            unit.SetFillValue(1, FillTime);
                        }
                    }
                }
            }
            //Removing Hearts
            else
            {
                var RemoveFrom = Mathf.Clamp((int)OldValue, 0, MaxValue - 1);

                //Empty Full hearts 
                for (int i = RemoveFrom; i > (int)Value; i--)
                {
                    var unit = Units[i];
                    if (EmptySequence)
                    {
                        this.Delay_Action(nextFillTime, () => unit.SetFillValue(0, EmptyTime));
                        nextFillTime += EmptyTime;
                    }
                    else
                    {
                        unit.SetFillValue(0, EmptyTime);
                    }
                }
            }


            //Update partial hearts
            this.Delay_Action(nextFillTime,
                 () =>
                 {
                     var ClampUnit = Mathf.Clamp(UnitID, 0, MaxValue);

                     Units[ClampUnit].SetFillValue(Value - UnitID, EmptyTime);

                     if (lasUnit < ClampUnit)
                         Units[lasUnit].SetFillValue(1, 0); //Full Fill the last Slot (Sprint was not filling completely e.g.0.985

                     LastUnitScaler(UnitID, ClampUnit);
                 }
                 );
            // UpdateUnits();

        }

        private int lasUnit;

        private void LastUnitScaler(int UnitID, int ClampUnit)
        {
            if ((Value - UnitID) > 0)
            {
                Units[Mathf.Clamp(ClampUnit, 0, MaxValue - 1)].SetScaler(true);
            }
            else
            {
                Units[Mathf.Clamp(ClampUnit - 1, 0, MaxValue - 1)].SetScaler(true);
            }
        }

        public void SetMaxValue(int maxValue)
        {
            //Check if we are adding values or removing
            bool Increasing = MaxValue < maxValue;
            var OldValue = MaxValue;
            this.maxValue = maxValue;

            if (OldValue != MaxValue)
            {
                //Add more hearts
                if (Increasing)
                {
                    for (int i = OldValue; i < MaxValue; i++)
                    {
                        var NewUnit = Instantiate(Unit, transform);

                        NewUnit.name = NewUnit.name.Replace("(Clone)", $"({i + 1})");
                        Units.Add(NewUnit);
                        NewUnit.transform.localScale = Vector3.one * DefaultScale;
                    }
                }
                //Remove hearts
                else
                {
                    //Clamp to Zero;
                    if (MaxValue < 0) this.maxValue = 0;

                    for (int i = OldValue - 1; i >= MaxValue; i--)
                    {
                        var NewUnit = Units[i];
                        Units.Remove(NewUnit);
                        Destroy(NewUnit.gameObject);
                    }
                    SetValue(Value);
                }
            }
        }

        public void ResetToMax() => SetValue(MaxValue);



        private void OnEnable()
        {
            if (maxValue.UseConstant == false && maxValue.Variable != null)
                maxValue.Variable.OnValueChanged += SetMaxValue;

            if (value.UseConstant == false && value.Variable != null)
                value.Variable.OnValueChanged += SetValue;
        }

        private void OnDisable()
        {
            if (maxValue.UseConstant == false && maxValue.Variable != null)
                maxValue.Variable.OnValueChanged -= SetMaxValue;

            if (value.UseConstant == false && value.Variable != null)
                value.Variable.OnValueChanged -= SetValue;
        }
    }
}
