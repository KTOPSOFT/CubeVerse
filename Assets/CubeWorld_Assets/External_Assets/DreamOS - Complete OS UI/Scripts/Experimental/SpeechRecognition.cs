using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_WSA
using UnityEngine.Events;
using UnityEngine.Windows.Speech;
using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
#endif

namespace Michsky.DreamOS
{
	public class SpeechRecognition : MonoBehaviour
	{
#if UNITY_STANDALONE_WIN || UNITY_WSA
		// Recognition
		public string[] keywords = new string[] { "Hey Cori" };
		public List<CommandItem> commands = new List<CommandItem>();
		List<string> commandsHelper = new List<string>();
		public List<string> listeningMessages = new List<string>();
		public UnityEvent onKeywordCall;
		public UnityEvent onDismiss;

		// Resources
		[SerializeField] private PopupPanelManager coriPopup;
		public AudioSource feedbackSource;
		public TextMeshProUGUI listeningText;

		// Settings
		public bool enableKeywords = true;
		[SerializeField] private bool enableLogs = false;
		[Range(1, 30)] public float dismissAfter = 4;
		public AudioClip listeningEffect;
		public AudioClip dismissEffect;

		private DictationRecognizer dictationRecognizer;
		private KeywordRecognizer keywordRecognizer;
		private KeywordRecognizer commandRecognizer;

		// Debug
		[TextArea] public string hypotheses;
		[TextArea] public string recognitions;

		bool stopSTT = false;

		[System.Serializable]
		public class CommandItem
		{
			[TextArea] public string command;
			public UnityEvent onCalled;
		}

		void Awake()
		{
			try
			{
				PhraseRecognitionSystem.OnStatusChanged += SpeechSystemStatusFn;
				InitializeKeywords();
			}

			catch
            {
				Debug.LogWarning("<b>[Speech Recognition]</b> Cannot initialize STT. Make sure that there's" +
					"at least an available voice package installed on your Windows.");
				this.enabled = false;
				stopSTT = true;
            }
		}

		void OnDestroy()
		{
			if (stopSTT == true)
				return;

			if (keywordRecognizer != null && keywordRecognizer.IsRunning) { StopKeywordRecognizer(); }
			if (commandRecognizer != null && commandRecognizer.IsRunning) { StopCommandRecognizer(); }
			if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped) { PhraseRecognitionSystem.Shutdown(); }
		}

		void InitializeKeywords()
		{
			for (int i = 0; i < commands.Count; i++)
				commandsHelper.Add(commands[i].command);

			if (enableKeywords == true)
				StartKeywordRecognizer();
		}

		void SpeechSystemStatusFn(SpeechSystemStatus status)
		{
			if (enableLogs == true)
				Debug.Log("<b>[Speech Recognition]</b> Speech System Status: " + status);
		}

		public void StartDictation()
		{
			if (PhraseRecognitionSystem.Status != SpeechSystemStatus.Stopped)
				PhraseRecognitionSystem.Shutdown();

			dictationRecognizer = new DictationRecognizer();
			dictationRecognizer.DictationResult += (string text, ConfidenceLevel confidence) =>
			{
				recognitions += text + "\n";
			};

			dictationRecognizer.DictationHypothesis += ((string text) =>
			{
				hypotheses += text + "\n";
			});

			dictationRecognizer.DictationComplete += ((DictationCompletionCause completionCause) =>
			{
				if (enableLogs == true && completionCause != DictationCompletionCause.Complete)
					Debug.LogErrorFormat("<b>[Speech Recognition]</b> Dictation completed unsuccessfully: {0}.", completionCause);
			});

			dictationRecognizer.DictationError += ((string error, int hresult) =>
			{
				if (enableLogs == true)
					Debug.LogErrorFormat("<b>[Speech Recognition]</b> Dictation error: {0}; HResult = {1}.", error, hresult);
			});

			dictationRecognizer.Start();
			recognitions = "";
			hypotheses = "";
		}

		public void StopDictation()
		{
			dictationRecognizer.Dispose();
			dictationRecognizer.Stop();
			dictationRecognizer = null;

			if (enableLogs == true)
				Debug.Log("<b>[Speech Recognition]</b> Dictation stopped.");
		}

		public void OpenCoriPopup()
		{
			coriPopup.OpenPanel();
			StopKeywordRecognizer();
			StartCommandRecognizer();
		}

		public void CloseCoriPopup()
		{
			coriPopup.ClosePanel();
			onDismiss.Invoke();
			StopCommandRecognizer();

			if (enableKeywords == true)
				StartKeywordRecognizer();
		}

		public void StartKeywordRecognizer()
		{
			keywordRecognizer = new KeywordRecognizer(keywords, ConfidenceLevel.Low);
			keywordRecognizer.OnPhraseRecognized += ((PhraseRecognizedEventArgs args) =>
			{
				if (coriPopup != null)
					coriPopup.OpenPanel();

				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
				recognitions += builder.ToString();

				if (enableLogs == true)
					Debug.Log("<b>[Speech Recognition]</b> Keyword recognized: <b>" + args.text + " (" + args.confidence + ")</b>");

				if (feedbackSource != null)
					feedbackSource.PlayOneShot(listeningEffect);

				onKeywordCall.Invoke();
				StopKeywordRecognizer();
				StartCommandRecognizer();
				StartCoroutine("WaitForCommand", dismissAfter);
			});

			recognitions = "";
			keywordRecognizer.Start();
			hypotheses = "Listening for: \n" + string.Join(", ", keywords);
		}

		public void StopKeywordRecognizer()
		{
			if (keywordRecognizer.IsRunning)
				keywordRecognizer.Stop();

			keywordRecognizer.Dispose();
			keywordRecognizer = null;

			if (enableLogs == true)
				Debug.Log("<b>[Speech Recognition]</b> Keyword recognizer stopped.");
		}

		public void StartCommandRecognizer()
		{
			if (feedbackSource != null)
				feedbackSource.PlayOneShot(listeningEffect);

			listeningText.text = listeningMessages[UnityEngine.Random.Range(0, listeningMessages.Count)];

			commandRecognizer = new KeywordRecognizer(commandsHelper.ToArray(), ConfidenceLevel.Low);
			commandRecognizer.OnPhraseRecognized += ((PhraseRecognizedEventArgs args) =>
			{
				StopCoroutine("WaitForCommand");

				StringBuilder builder = new StringBuilder();
				builder.AppendFormat("{0} ({1}){2}", args.text, args.confidence, Environment.NewLine);
				recognitions += builder.ToString();

				if (enableLogs == true)
					Debug.Log("<b>[Speech Recognition]</b> Command recognized: <b>" + args.text + " (" + args.confidence + ")</b>");

				for (int i = 0; i < commands.Count; i++)
				{
					if (args.text == commands[i].command)
						commands[i].onCalled.Invoke();
				}

				StopCommandRecognizer();

				if (enableKeywords == true)
					StartKeywordRecognizer();
			});

			recognitions = "";
			commandRecognizer.Start();
			hypotheses = "Listening for: \n" + string.Join(", ", commandsHelper);
		}

		public void StopCommandRecognizer()
		{
			if (coriPopup != null) { coriPopup.ClosePanel(); }
			if (feedbackSource != null) { feedbackSource.PlayOneShot(dismissEffect); }
			if (commandRecognizer != null && commandRecognizer.IsRunning) { commandRecognizer.Stop(); }

			commandRecognizer.Dispose();
			commandRecognizer = null;

			if (enableLogs == true)
				Debug.Log("<b>[Speech Recognition]</b> Command recognizer stopped.");
		}

		IEnumerator WaitForCommand(float waitFor)
		{
			yield return new WaitForSeconds(waitFor);

			onDismiss.Invoke();

			if (commandRecognizer.IsRunning) { StopCommandRecognizer(); }
			if (enableKeywords == true) { StartKeywordRecognizer(); }
		}
#endif
	}
}
