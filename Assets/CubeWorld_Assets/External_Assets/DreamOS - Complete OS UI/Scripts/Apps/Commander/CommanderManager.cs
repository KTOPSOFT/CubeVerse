using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Michsky.DreamOS
{
    public class CommanderManager : MonoBehaviour
    {
        // Command List
        public List<CommandItem> commands = new List<CommandItem>();

        // Settings
        [TextArea] public string errorText = "is not recognized as a command.";
        [TextArea] public string onEnableText = "Welcome to Commander.";
        public Color textColor;
        public bool useTypewriterEffect;
        public bool antiFlicker = true;
        [Range(0.001f, 0.5f)] public float typewriterDelay = 0.03f;

        // Resources
        [SerializeField] private TMP_InputField commandInput;
        [SerializeField] private TextMeshProUGUI commandHistory;
        [SerializeField] private Scrollbar scrollbar;

        // External
        public bool getTimeData = true;
        [SerializeField] private GlobalTime timeManager;
        public Color timeColorCode = new Color(0, 255, 0);

        // Hidden
        private string currentCommand;
        private int commandIndex;
        private string typewriterHelper;
        private RectTransform historyRT;
        private RectTransform historyParentRT;

        [System.Serializable]
        public class CommandItem
        {
            public string commandName = "Command Name";
            public string command = "Actual Command";
            [TextArea] public string feedbackText;
            public float feedbackDelay = 0;
            public float onProcessDelay = 0;
            public UnityEvent onProcessEvent;
        }

        void OnEnable()
        {
            commandHistory.text = "";
            commandInput.text = "";
            UpdateColors();
            if (getTimeData == true && timeManager != null) { UpdateTime(); }
            commandHistory.text = commandHistory.text + onEnableText;
            commandInput.ActivateInputField();
            StartCoroutine("FixLayout");
        }

        void OnDisable()
        {
            StopCoroutine("FixLayout");
            StopCoroutine("ApplyTypewriter");
            StopCoroutine("WaitForFeedbackDelay");
            StopCoroutine("WaitForProcessDelay");
        }

        void Awake()
        {
            historyRT = commandHistory.GetComponent<RectTransform>();
            historyParentRT = commandHistory.transform.parent.GetComponent<RectTransform>();
            this.enabled = false;
        }

        void Update()
        {
            if (string.IsNullOrEmpty(commandInput.text) == true || EventSystem.current.currentSelectedGameObject != commandInput.gameObject) { return; }
            else if (commandInput.isFocused == false) { commandInput.ActivateInputField(); }

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Return)) { ProcessCommand(); }
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current.enterKey.wasPressedThisFrame) { ProcessCommand(); }
#endif
        }

        public void UpdateColors()
        {
            commandInput.textComponent.color = textColor;
            commandHistory.color = textColor;
        }

        public void ProcessCommand()
        {
            // Reset previously called command
            currentCommand = "";
            commandIndex = -1;
            currentCommand = commandInput.text;

            // Stop previous typewriter
            StopCoroutine("ApplyTypewriter");

            // Search within command list
            for (int i = 0; i < commands.Count; i++)
            {
                if (currentCommand == commands[i].command)
                {
                    currentCommand = commands[i].command;
                    commandIndex = i;
                    break;
                }
            }

            // Apply a new line
            commandHistory.text = commandHistory.text + "\n";

            // Update time data
            if (getTimeData == true && timeManager != null) { UpdateTime(); }

            // Apply called command
            commandHistory.text = commandHistory.text + commandInput.text;

            // Show error if there's an error
            if (commandIndex == -1)
            {
                // Apply a new line
                commandHistory.text = commandHistory.text + "\n";

                // Update time data
                if (getTimeData == true && timeManager != null) { UpdateTime(); }

                // Apply error text
                commandHistory.text = string.Format("{0}'{1}' {2}", commandHistory.text, commandInput.text, errorText);

                // Reset input
                commandInput.text = "";

                // Make sure that layout is displayed properly
                LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);
                LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
                StartCoroutine("FixLayout");

                // Set scrollbar value
                if (scrollbar != null) { scrollbar.value = 0; }

                // Don't process further
                return;
            }

            // Process feedback test
            if (commands[commandIndex].feedbackText != "") { StartCoroutine("WaitForFeedbackDelay", commands[commandIndex].feedbackDelay); }

            // Invoke events
            if (commands[commandIndex].onProcessDelay == 0) { commands[commandIndex].onProcessEvent.Invoke(); }
            else { StartCoroutine("WaitForProcessDelay", commands[commandIndex].onProcessDelay); }

            // Reset input
            commandInput.text = "";

            // Make sure that layout is displayed properly
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
            StartCoroutine("FixLayout");

            // Set scrollbar value
            if (scrollbar != null) { scrollbar.value = 0; }
        }

        IEnumerator ApplyTypewriter(float delay)
        {
            for (int i = 0; i < typewriterHelper.Length; i++)
            {
                commandHistory.text = commandHistory.text + typewriterHelper[i].ToString();
               
                // Prevent stuttering/flickering issues if there's any
                if (antiFlicker == true) { LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT); }

                // Set scrollbar value
                if (scrollbar != null) { scrollbar.value = 0; }

                yield return new WaitForSeconds(delay);
            }

            // Rebuild layout to prevent visual issues
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
        }

        public void UpdateTime()
        {
            if (getTimeData == true && timeManager != null)
            {
                string colorValue = ColorUtility.ToHtmlStringRGB(timeColorCode);
                commandHistory.text = string.Format("{0}<color=#{1}>[", commandHistory.text, colorValue);

                if (timeManager.currentHour.ToString().Length == 1) { commandHistory.text = string.Format("{0}0{1}:", commandHistory.text, timeManager.currentHour); }
                else { commandHistory.text = string.Format("{0}{1}:", commandHistory.text, timeManager.currentHour); }

                if (timeManager.currentMinute.ToString().Length == 1) { commandHistory.text = string.Format("{0}0{1}:", commandHistory.text, timeManager.currentMinute); }
                else { commandHistory.text = string.Format("{0}{1}:", commandHistory.text, timeManager.currentMinute); }

                if (timeManager.currentSecond.ToString("F0").Length == 1) { commandHistory.text = string.Format("{0}0{1}", commandHistory.text, timeManager.currentSecond.ToString("F0")); }
                else { commandHistory.text = string.Format("{0}{1}", commandHistory.text, timeManager.currentSecond.ToString("F0")); }

                commandHistory.text = string.Format("{0}]</color> ", commandHistory.text);
            }
        }

        public void AddToHistory(string text, bool useTypewriter, float typewriterDelay = 0.1f)
        {
            commandHistory.text = commandHistory.text + "\n";

            UpdateTime();
            StopCoroutine("ApplyTypewriter");

            if (useTypewriter == true) { typewriterHelper = text; StartCoroutine("ApplyTypewriter", typewriterDelay); }
            else { commandHistory.text = commandHistory.text + text; }

            // Make sure that layout is displayed properly
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
            StartCoroutine("FixLayout");
        }

        public void ClearHistory()
        {
            // Reset input
            commandHistory.text = onEnableText;

            // Make sure that layout is displayed properly
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
            StartCoroutine("FixLayout");
        }

        public void AddNewCommand() { commands.Add(null); }

        IEnumerator FixLayout()
        {
            yield return new WaitForSeconds(0.02f);

            // Rebuild layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);

            // Set scrollbar value
            if (scrollbar != null) { scrollbar.value = 0; }
        }

        IEnumerator WaitForFeedbackDelay(float forSec)
        {
            yield return new WaitForSeconds(forSec);
          
            // Add new line and apply time
            commandHistory.text = commandHistory.text + "\n";
            UpdateTime();

            // Check for typewriter
            if (useTypewriterEffect == true) { typewriterHelper = commands[commandIndex].feedbackText; StartCoroutine("ApplyTypewriter", typewriterDelay); }
            else { commandHistory.text = commandHistory.text + commands[commandIndex].feedbackText; }

            // Make sure that layout is displayed properly
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyRT);
            LayoutRebuilder.ForceRebuildLayoutImmediate(historyParentRT);
            StartCoroutine("FixLayout");
        }

        IEnumerator WaitForProcessDelay(float forSec)
        {
            yield return new WaitForSeconds(forSec);
            commands[commandIndex].onProcessEvent.Invoke();
            StopCoroutine("WaitForProcessDelay");
        }
    }
}