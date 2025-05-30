using UnityEngine;
using UnityEngine.Events;

namespace Michsky.DreamOS.Examples
{
    [AddComponentMenu("DreamOS/Examples/Commander/Commander Feedback")]
    public class CommanderFeedback : MonoBehaviour
    {
        [Header("Resources")]
        public CommanderManager commanderManager;

        [Header("Settings")]
        [TextArea] public string feedbackText = "Sample text";
        public bool useTypewriterEffect;
        [Range(0.0001f, 0.25f)] public float typewriterDelay = 0.1f;

        [Header("Events")]
        public UnityEvent onApply;

        public void ApplyFeedback()
        {
            onApply.Invoke();
            commanderManager.AddToHistory(feedbackText, useTypewriterEffect, typewriterDelay);
        }
    }
}