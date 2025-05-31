using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Conditions
{
    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/Boolean"), ConditionDescription("[Value1 == Value2]")]
    public class C2_Bool : ConditionCore
    {
        public BoolReference Value1 = new();
        public BoolReference Value2 = new();
        protected override bool _Evaluate() => Value1.Value == Value2.Value;
        protected override void _SetTarget(Object target) { } //null
    }
    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/Integer"), ConditionDescription("Compare Int [Value1 ? Value2]")]
    public class C2_Integer : ConditionCore
    {
        public IntReference Value1;
        public ComparerInt Condition;
        public IntReference Value2;
        protected override bool _Evaluate() => Value1.Value.CompareInt(Value2.Value, Condition);
        protected override void _SetTarget(Object target) { } //null
    }

    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/Float"), ConditionDescription("Compare Float [Value1 ? Value2]")]
    public class C2_Float : ConditionCore
    {
        public FloatReference Value1;
        public ComparerInt Condition;
        public FloatReference Value2;
        protected override bool _Evaluate() => Value1.Value.CompareFloat(Value2.Value, Condition);
        protected override void _SetTarget(Object target) { } //null
    }

    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/String"), ConditionDescription("Compare String [Value1 ? Value2]")]
    public class C2_String : ConditionCore
    {
        public enum StringCondition { Equal, Contains, ContainsLower }
        public StringReference Value1;
        public StringCondition Condition;
        public StringReference Value2;
        protected override bool _Evaluate()
        {
            return Condition switch
            {
                StringCondition.Equal => Value1.Value == Value2.Value,
                StringCondition.Contains => Value1.Value.Contains(Value2.Value),
                StringCondition.ContainsLower => Value1.Value.ToLower().Contains(Value2.Value.ToLower()),
                _ => false,
            };
        }
        protected override void _SetTarget(Object target) { } //null
    }

    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/Vector3"), ConditionDescription("Vector3 [Value1 == Value2]")]
    public class C2_Vector3 : ConditionCore
    {
        [Tooltip("if True, compare the position of this transform to a Vector3 value")]
        public bool useTransform;
        [Hide("useTransform")]
        public TransformReference Target;
        [Hide("useTransform", true)]
        public Vector3Reference Value1;
        public Vector3Reference Value2;
        protected override bool _Evaluate() => useTransform ? Target.Value.position == Value2.Value : Value1.Value == Value2.Value;
        protected override void _SetTarget(Object target) { Target.Value = MTools.VerifyComponent<Transform>(target, Target.Value); }
    }

    //-------------------------------------------------------------------------------------------------------
    [System.Serializable, AddTypeMenu("Values/Vector2"), ConditionDescription("Vector2 [Value1 == Value2]")]
    public class C2_Vector2 : ConditionCore
    {
        public Vector2Reference Value1;
        public Vector2Reference Value2;
        protected override bool _Evaluate() => Value1.Value == Value2.Value;
        protected override void _SetTarget(Object target) { } //null
    }
}
