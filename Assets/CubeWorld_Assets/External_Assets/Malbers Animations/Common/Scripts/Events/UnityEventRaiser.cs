using MalbersAnimations.Scriptables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MalbersAnimations.Events
{
    /// <summary>Simple Event Raiser On Enable</summary>
    [AddComponentMenu("Malbers/Events/Unity Event Raiser [On Enable]")]
    public class UnityEventRaiser : UnityUtils
    {
        [Tooltip("Delayed time for invoking the Events, or the Repeated time  when Repeat is enable")]
        public FloatReference Delayed = new();
        public FloatReference RepeatTime = new();
        public bool Repeat;
        //[Tooltip("Disable this component after the event is raised")]
        //public bool Once;


        [FormerlySerializedAs("OnEnableEvent")]
        public UnityEngine.Events.UnityEvent onEnable = new();


        public string Description = "";
        [HideInInspector] public bool ShowDescription = false;
        [ContextMenu("Show Description")]
        internal void EditDescription() => ShowDescription ^= true;

        public void OnEnable()
        {
            if (Repeat && RepeatTime > 0f)
            {
                InvokeRepeating(nameof(StartEvent), Delayed, RepeatTime);
            }
            else if (Delayed > 0)
            {
                Invoke(nameof(StartEvent), Delayed);
            }
            else
            {
                onEnable.Invoke();
            }
        }

        public void StartEvent()
        {
            onEnable.Invoke();

            //  if (Once) enabled = false;
        }

        private void OnDisable()
        {
            CancelInvoke();
            StopAllCoroutines();
        }

        /// <summary> Disable all Invokes and Call again the OnEnable </summary>
        public virtual void Restart()
        {
            enabled = true;
            CancelInvoke();
            OnEnable();
        }

        //#if UNITY_EDITOR
        //        private void OnDrawGizmosSelected()
        //        {
        //            MalbersEditor.DrawEventConnection(transform, onEnable, true);
        //        }

        //        private void OnDrawGizmos()
        //        {
        //            MalbersEditor.DrawEventConnection(transform, onEnable, false);
        //        }
        //#endif
    }


#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(UnityEventRaiser)), UnityEditor.CanEditMultipleObjects]
    public class UnityEventRaiserInspector : UnityEditor.Editor
    {
        UnityEditor.SerializedProperty Delayed, Repeat, RepeatTime, OnEnableEvent, ShowDescription, Description;//,// Once;
        public static GUIStyle StyleBlue => Style(new Color(0, 0.5f, 1f, 0.3f));
        private GUIStyle style;
        private GUIContent _ReactIcon;


        private void OnEnable()
        {
            Delayed = serializedObject.FindProperty("Delayed");
            ShowDescription = serializedObject.FindProperty("ShowDescription");
            Description = serializedObject.FindProperty("Description");
            Repeat = serializedObject.FindProperty("Repeat");
            RepeatTime = serializedObject.FindProperty("RepeatTime");
            OnEnableEvent = serializedObject.FindProperty("onEnable");
            // Once = serializedObject.FindProperty("Once");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (ShowDescription.boolValue)
            {
                // if (style == null)
                style = new GUIStyle(MTools.StyleBlue)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    stretchWidth = true
                };

                style.normal.textColor = EditorStyles.boldLabel.normal.textColor;

                Description.stringValue = EditorGUILayout.TextArea(Description.stringValue, style);
            }


            using (new GUILayout.HorizontalScope(/*UnityEditor.EditorStyles.helpBox*/))
            {

                if (_ReactIcon == null)
                {
                    _ReactIcon = EditorGUIUtility.IconContent("d_PlayButton@2x");
                    _ReactIcon.tooltip = "Invoke at Runtime";
                }

                if (Application.isPlaying && GUILayout.Button(_ReactIcon, EditorStyles.miniButtonMid, GUILayout.Width(18), GUILayout.Height(20)))
                {
                    (target as UnityEventRaiser).onEnable.Invoke();
                }

                UnityEditor.EditorGUILayout.PropertyField(Delayed, GUILayout.MinWidth(100));
                if (Repeat.boolValue)
                {
                    EditorGUIUtility.labelWidth = 35;
                    EditorGUILayout.PropertyField(RepeatTime, new GUIContent(" RT", "Repeat Time"), GUILayout.MinWidth(40));
                    EditorGUIUtility.labelWidth = 0;
                }

                Repeat.boolValue = GUILayout.Toggle(Repeat.boolValue, new GUIContent("R", "Repeat"), UnityEditor.EditorStyles.miniButton, GUILayout.Width(25));
                //Once.boolValue = GUILayout.Toggle(Once.boolValue, new GUIContent("1", "Disable this component after the event is raised"), EditorStyles.miniButton, GUILayout.Width(25));
            }
            UnityEditor.EditorGUILayout.PropertyField(OnEnableEvent);
            serializedObject.ApplyModifiedProperties();
        }

        public static GUIStyle Style(Color color)
        {
            GUIStyle currentStyle = new(GUI.skin.box) { border = new RectOffset(-1, -1, -1, -1) };
            Color32[] pix = new Color32[1];
            pix[0] = color;
            Texture2D bg = new(1, 1);
            bg.SetPixels32(pix);
            bg.Apply();

            currentStyle.normal.background = bg;
            // MW 04-Jul-2020: Check if system supports newer graphics formats used by Unity GUI
            Texture2D bgActual = currentStyle.normal.scaledBackgrounds[0];

#if UNITY_2023_2_OR_NEWER
            if (SystemInfo.IsFormatSupported(bgActual.graphicsFormat, UnityEngine.Experimental.Rendering.GraphicsFormatUsage.Sample ) == false)
#else
            if (SystemInfo.IsFormatSupported(bgActual.graphicsFormat, UnityEngine.Experimental.Rendering.FormatUsage.Sample) == false)
#endif
            {
                currentStyle.normal.scaledBackgrounds = new Texture2D[] { }; // This can't be null
            }
            return currentStyle;
        }
    }
#endif
}