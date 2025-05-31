using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Float Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple  </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Float Formula", order = 1000)]
    public class FloatFormula : FloatVar
    {
        public List<FloatOperation> values = new();

        /// <summary>Value of the Float Scriptable variable </summary>
        public override float Value
        {
            get
            {
                var result = value;

                foreach (var v in values)
                    result = v.GetResult(result);

                if (debug) Debug.Log($"<B>{name} -> [<color=red> {result} </color>] </B>", this);

                return result;
            }
        }

        public void SetFormula(string Name, MathOperation operation, float value) => values.Add(new FloatOperation(Name, operation, new FloatReference(value)));

        public void SetFormula(string Name, MathOperation operation, FloatReference value) => values.Add(new FloatOperation(Name, operation, value));

        public void SetFormula(FloatOperation formula) => values.Add(formula);


        public void FormulaAdd(FloatVar value) => values.Add(new FloatOperation(value.name, MathOperation.Add, new FloatReference(value)));

        public void FormulaSubstact(FloatVar value) => values.Add(new FloatOperation(value.name, MathOperation.Substract, new FloatReference(value)));

        public void FormulaMultiply(FloatVar value) => values.Add(new FloatOperation(value.name, MathOperation.Multiply, new FloatReference(value)));

        public void FormulaDivide(FloatVar value) => values.Add(new FloatOperation(value.name, MathOperation.Divide, new FloatReference(value)));

        public void RemoveFormula(string name)
        {
            values.RemoveAll(v => v.name == name);  
        }



        private void OnValidate()
        {
            var displayOld = $"{value}";
            foreach (var item in values)
            {
                item.display = displayOld + $" {item.GetOperation()} {item.value.Value}";

                displayOld = item.display;
            }
        }

        [System.Serializable]
        public class FloatOperation
        {
            [HideInInspector] public string display;
            public string name = "Formula Name";
            public MathOperation operation = MathOperation.Add;
            public FloatReference value = new();

            public float GetResult(float MainValue)
            {
                return operation switch
                {
                    MathOperation.Add => MainValue + value,
                    MathOperation.Substract => MainValue - value,
                    MathOperation.Multiply => MainValue * value,
                    MathOperation.Divide => MainValue / value,
                    _ => 0,
                };
            }

            public string GetOperation()
            {
                return operation switch
                {
                    MathOperation.Add => "+",
                    MathOperation.Substract => "-",
                    MathOperation.Multiply => "*",
                    MathOperation.Divide => "/",
                    _ => "",
                };
            }


            public FloatOperation(string name, MathOperation operation, FloatReference value)
            {
                this.name = name;
                this.operation = operation;
                this.value = value;
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects, UnityEditor.CustomEditor(typeof(FloatFormula))]
    public class FloatFormulaEditor : VariableEditor
    {
        public override void OnInspectorGUI() => PaintInspectorGUI("Advanced Value Formula(Add extra operations to the Float Var)");

        public override void ExtraValues()
        {
            UnityEditor. EditorGUI.indentLevel++;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("values"),true);
            UnityEditor.EditorGUI.indentLevel--;
        }
    }
#endif
}