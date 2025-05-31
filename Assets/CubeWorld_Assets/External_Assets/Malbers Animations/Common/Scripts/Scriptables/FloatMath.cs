using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary> Make basic math op </summary>
    public class FloatMath : MonoBehaviour
    {

        public FloatReference Value1 = new FloatReference();
        public MathOperation operation = MathOperation.Add;
        public FloatReference Value2 = new FloatReference();


        public FloatEvent Result = new FloatEvent();

        public virtual void SetValue2(FloatVar var) => SetValue2(var.Value);
        public virtual void SetValue1(FloatVar var) => SetValue1(var.Value);

        public virtual void SetValue2(IntVar var) => SetValue2(var.Value);
        public virtual void SetValue1(IntVar var) => SetValue1(var.Value);

        public virtual void SetValue2(int var) => SetValue2((float)var);
        public virtual void SetValue1(int var) => SetValue1((float)var);

        public virtual void SetValue1(float var)
        {
            Value1 = var;
            Calculate();
        }

        public virtual void SetValue2(float var)
        {
            Value2 = var;
            Calculate();
        }

        public virtual void Calculate()
        {
            switch (operation)
            {
                case MathOperation.Add:
                    Result.Invoke(Value1 + Value2);
                    break;
                case MathOperation.Substract:
                    Result.Invoke(Value1 - Value2);
                    break;
                case MathOperation.Multiply:
                    Result.Invoke(Value1 * Value2);
                    break;
                case MathOperation.Divide:
                    if (Value2 != 0) Result.Invoke(Value1 / Value2);
                    break;
                default:  break;
            }
        }
    }
}