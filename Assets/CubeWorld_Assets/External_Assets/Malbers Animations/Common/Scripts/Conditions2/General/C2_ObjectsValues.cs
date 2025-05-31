using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    [System.Serializable, AddTypeMenu("General/Check Extracted Value")]
    [ConditionDescription("Compare a Component Property")]
    public class C2_ObjectValues : ConditionCore
    {
        public enum ValueType { Int, Float, Bool }
        public ValueType valueType = ValueType.Int;

        [Hide(nameof(valueType), (int)ValueType.Int)]
        public ValueExtractor<Object, int> intProperty;
        [Hide(nameof(valueType), (int)ValueType.Float)]
        public ValueExtractor<Object, float> floatProperty;
        [Hide(nameof(valueType), (int)ValueType.Bool)]
        public ValueExtractor<Object, bool> boolProperty;

        [Hide(nameof(valueType), true, (int)ValueType.Bool)]
        public ComparerInt compare = ComparerInt.Equal;

        [Hide(nameof(valueType), (int)ValueType.Int)]
        public IntReference intValue = new(0);
        [Hide(nameof(valueType), (int)ValueType.Float)]
        public FloatReference floatValue = new(0);
        [Hide(nameof(valueType), (int)ValueType.Bool)]
        public BoolReference boolValue = new();


        protected override void _SetTarget(Object target)
        {
            //if (intProperty.Target != target)
            //    intProperty.SetTarget(target);
            //if (floatProperty.Target != target)
            //    floatProperty.SetTarget(target);
            //if (boolProperty.Target != target)
            //    boolProperty.SetTarget(target);
        }

        protected override bool _Evaluate()
        {
            Debug.Log($"intProperty.GetValue() {intProperty.GetValue()}. Property Name? {intProperty.Property}");

            return valueType switch
            {
                ValueType.Int => intProperty.GetValue().CompareInt(intValue, compare),
                ValueType.Float => floatProperty.GetValue().CompareFloat(floatValue, compare),
                ValueType.Bool => boolProperty.GetValue() == boolValue,
                _ => false,
            };
        }
    }
}
