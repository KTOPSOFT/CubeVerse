using UnityEngine;
using TMPro;

namespace Michsky.DreamOS
{
    public class CalculatorManager : MonoBehaviour
    {
        // Resources
        [SerializeField] private TextMeshProUGUI displayText;
        [SerializeField] private TextMeshProUGUI displayOperator;
        [SerializeField] private TextMeshProUGUI displayResult;
        [SerializeField] private TextMeshProUGUI displayPreview;

        // Hidden helper variables
        float rememberValue;
        string previousAction;
        bool pressedOperators = false;
        bool enableDot = true;
        bool isResetted = true;

        void Awake()
        {
            try { displayText.text = "0"; displayResult.text = "0"; displayPreview.text = ""; }
            catch { Debug.LogError("Calculator - Display resources are not assigned.", this); }
        }

        public void ButtonNumber(int number)
        {
            // If the displayed number is not zero, add the number
            if (displayText.text != "0") { displayText.text = displayText.text + number.ToString(); }
            else { displayText.text = number.ToString(); }

            // If pressed operators, enable adding dot and replace the number
            if (pressedOperators == true)
            {
                displayText.text = number.ToString();
                pressedOperators = false;
                enableDot = true;
            }
        } 

        public void ButtonDot()
        {
            // If dot is enabled, then add it right next to display and disable for further
            if (enableDot == true)
            {
                displayText.text = displayText.text + ".";
                enableDot = false;
            }
        }

        public void ButtonDelete()
        {
            // Delete the numbers until there's at least one number in display
            if (displayText.text.Length >= 2) { displayText.text = displayText.text.Remove(displayText.text.Length - 1); }
            else
            {
                displayText.text = "0";
                enableDot = true;
            }
        }

        public void ButtonDivision()
        {
            if (isResetted == false)
            {
                if (rememberValue == 0)
                {
                    float result = float.Parse(displayResult.text) / float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "÷";
                    previousAction = "ButtonDivision";
                    pressedOperators = true;
                    displayPreview.text = "";
                }

                else
                {
                    float result = rememberValue / float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "÷";
                    previousAction = "ButtonDivision";
                    pressedOperators = true;
                    displayPreview.text = "";
                    rememberValue = 0;
                }
            }

            else
            {
                rememberValue = float.Parse(displayText.text);
                displayPreview.text = rememberValue.ToString() + " ÷";
                displayOperator.text = "÷";
                previousAction = "ButtonDivision";
                pressedOperators = true;
                isResetted = false;
            }
        }

        public void ButtonMultiply()
        {
            if (isResetted == false)
            {
                if (rememberValue == 0)
                {
                    float result = float.Parse(displayResult.text) * float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "×";
                    previousAction = "ButtonMultiply";
                    pressedOperators = true;
                    displayPreview.text = "";
                }

                else
                {
                    float result = rememberValue * float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "×";
                    previousAction = "ButtonMultiply";
                    pressedOperators = true;
                    displayPreview.text = "";
                    rememberValue = 0;
                }
            }

            else
            {
                rememberValue = float.Parse(displayText.text);
                displayPreview.text = rememberValue.ToString() + " ×";
                displayOperator.text = "×";
                previousAction = "ButtonMultiply";
                pressedOperators = true;
                isResetted = false;
            }
        }

        public void ButtonSubtraction()
        {
            if (isResetted == false)
            {
                if (rememberValue == 0)
                {
                    float result = float.Parse(displayResult.text) - float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "-";
                    previousAction = "ButtonSubtraction";
                    pressedOperators = true;
                    displayPreview.text = "";
                }

                else
                {
                    float result = rememberValue - float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "-";
                    previousAction = "ButtonSubtraction";
                    pressedOperators = true;
                    displayPreview.text = "";
                    rememberValue = 0;
                }
            }

            else
            {
                rememberValue = float.Parse(displayText.text);
                displayPreview.text = rememberValue.ToString() + " -";
                displayOperator.text = "-";
                previousAction = "ButtonSubtraction";
                pressedOperators = true;
                isResetted = false;
            }
        }
  
        public void ButtonAddition()
        {
            if (isResetted == false)
            {
                if (rememberValue == 0)
                {
                    float result = float.Parse(displayResult.text) + float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "+";
                    previousAction = "ButtonAddition";
                    pressedOperators = true;
                    displayPreview.text = "";
                }

                else
                {
                    float result = rememberValue + float.Parse(displayText.text);
                    displayResult.text = result.ToString();
                    displayOperator.text = "+";
                    previousAction = "ButtonAddition";
                    pressedOperators = true;
                    displayPreview.text = "";
                    rememberValue = 0;
                }
            }

            else
            {
                rememberValue = float.Parse(displayText.text);
                displayPreview.text = rememberValue.ToString() + " +";
                displayOperator.text = "+";
                previousAction = "ButtonAddition";
                pressedOperators = true;
                isResetted = false;
            }
        }

        public void ButtonEqual()
        {
            // Repeat the latest action
            Invoke(previousAction, 0f);
        }

        public void ButtonAC()
        {
            // Delete the numbers and enable dot
            displayText.text = "0";
            displayResult.text = "0";
            enableDot = true;
            isResetted = true;
        }
    }
}