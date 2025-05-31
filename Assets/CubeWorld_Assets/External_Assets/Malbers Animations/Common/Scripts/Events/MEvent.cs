using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Events
{
    ///<summary>
    /// The list of listeners that this event will notify if it is Invoked. 
    /// Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple
    /// </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Event", fileName = "New Event Asset", order = 3000)]
    public class MEvent : ScriptableObject
    {
        /// <summary>The list of listeners that this event will notify if it is raised.</summary>
        internal readonly List<MEventItemListener> eventListeners = new();


#if UNITY_EDITOR
        [TextArea(3, 10)]
        public string Description;
#endif
        public bool debug;

        public virtual void Invoke()
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<B>{name}</B> - Invoke()", this);
#endif
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked();

        }

        public virtual void Invoke(float value)
        {
            DebugEvent($"{value:F2}", "float");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);

        }

        public virtual void InvokeToInt(float value)
        {
            DebugEvent(value, "float to int");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked((int)value);
        }

        public virtual void InvokeToFloat(int value)
        {
            DebugEvent(value, "int to float");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked((float)value);
        }

        public virtual void Invoke(FloatVar value)
        {
            DebugEvent(value, "Float Var");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);
        }
        public virtual void Invoke(bool value)
        {
            DebugEvent(value, "bool");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }
        public virtual void Invoke(BoolVar value)
        {
            DebugEvent(value.Value, "Bool Var");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);
        }

        public virtual void Invoke(string value)
        {
            DebugEvent($"'{value}'", "string");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(StringVar value)
        {
            DebugEvent(value.Value, "StringVar");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);
        }

        public virtual void Invoke(int value)
        {
            DebugEvent(value, "int");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(IntVar value)
        {
            DebugEvent(value.Value, "Int Var");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.Value);
        }

        public virtual void Invoke(IDs value)
        {
            DebugEvent($"({value.name} - {value.ID})", "Int[ID]");

            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value.ID);

        }

        public virtual void Invoke(GameObject value)
        {
            DebugEvent(value, "GameObject");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(GameObjectVar value) => Invoke(value.Value);

        public virtual void Invoke(Transform value)
        {
            DebugEvent(value != null ? value.name : null, "Transform");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(TransformVar value) => Invoke(value.Value);


        public virtual void Invoke(Vector3 value)
        {
            DebugEvent(value, "Vector3");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(Vector3Var value) => Invoke(value.Value);

        public virtual void Invoke(Vector2 value)
        {
            DebugEvent(value, "Vector2");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(Component value)
        {
            DebugEvent(value, "Component");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(Sprite value)
        {
            DebugEvent(value, "Sprite");
            for (int i = eventListeners.Count - 1; i >= 0; i--)
                eventListeners[i].OnEventInvoked(value);
        }

        public virtual void Invoke(SpriteVar value) => Invoke(value.Value);


        public virtual void RegisterListener(MEventItemListener listener)
        {
            if (!eventListeners.Contains(listener)) eventListeners.Add(listener);
        }

        public virtual void UnregisterListener(MEventItemListener listener)
        {
            if (eventListeners.Contains(listener)) eventListeners.Remove(listener);
        }

        public virtual void InvokeAsGameObject(Component value) => Invoke(value != null ? value.gameObject : null);
        public virtual void InvokeAsTransform(GameObject value) => Invoke(value != null ? value.transform : null);
        public virtual void InvokeAsTransform(Component value) => Invoke(value != null ? value.transform : null);
        public virtual void InvokeAsString(Object value) => Invoke(value != null ? value.name.Replace("(Clone)", "") : string.Empty);
        public virtual void InvokeAsBool(Object value) => Invoke(value != null);
        public virtual void InvokeAsBool(int value) => Invoke(value > 0);

        public virtual void InvokeAsFloat(bool value) => Invoke(value ? 1 : 0);
        public virtual void InvokeAsInt(bool value) => Invoke(value ? 1 : 0);
        public virtual void InvokeAsInt(Object value) => Invoke(value != null ? value.GetInstanceID() : -1);


        private void DebugEvent(object value, string type)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<color=cyan><B>{name}</B> - Invoke({value}) Type({type}) </color>", this);
#endif
        }


        public void LogDeb(int value, int value2) => LogDeb((object)value, value2);

        public void LogDeb(object value, object value2)
        {
#if UNITY_EDITOR
            if (debug) Debug.Log($"<B>{name}</B> - Invoke({value},{value2})", this);
#endif
        }


        ////This is for Debugin porpuses
        #region Debuging Methods
        public virtual void Pause()
        {
            Debug.Log("Pause Editor", this);
            Debug.Break();
        }

        public virtual void Pause(string value)
        {
            LogDeb(value);
            Debug.Break();
        }

        public virtual void LogDeb(string value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>", this);
        public virtual void LogDeb(bool value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(Vector3 value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(Vector2 value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(int value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(float value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(object value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");
        public virtual void LogDeb(Object value) => Debug.Log($"<color=white><B>{name} : [{value}] </B></color>");



        #endregion

#if UNITY_EDITOR
        [HideInInspector] public IntReference m_int;
        [HideInInspector] public FloatReference m_float;
        [HideInInspector] public StringReference m_string;
        [HideInInspector] public BoolReference m_bool;
        [HideInInspector] public Vector2Reference m_V2;
        [HideInInspector] public Vector3Reference m_V3;
        [HideInInspector] public GameObjectReference m_go;

        [HideInInspector] public TransformReference m_transform;
        [HideInInspector] public Sprite m_Sprite;
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MEvent))]
    public class MEventEditor : Editor
    {
        public static GUIStyle Style_ => MTools.StyleGreen;

        SerializedProperty
            Description, debug, m_int, m_float, m_string, m_bool, m_V2, m_V3, m_go, m_transform, m_Sprite;

        MEvent ev;
        private void OnEnable()
        {
            ev = (MEvent)target;
            Description = serializedObject.FindProperty("Description");
            debug = serializedObject.FindProperty("debug");
            m_int = serializedObject.FindProperty("m_int");
            m_float = serializedObject.FindProperty("m_float");
            m_string = serializedObject.FindProperty("m_string");
            m_bool = serializedObject.FindProperty("m_bool");
            m_V2 = serializedObject.FindProperty("m_V2");
            m_V3 = serializedObject.FindProperty("m_V3");
            m_go = serializedObject.FindProperty("m_go");
            m_transform = serializedObject.FindProperty("m_transform");
            m_Sprite = serializedObject.FindProperty("m_Sprite");
        }
        GUIStyle style;

        void Debuggin(string log, Object target)
        {
            Debug.Log($"<color=cyan>Event [{ev.name}]. <B>{log} Response</B> → Listener: <B>[{target.name}]</B> </color>", target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                MalbersEditor.DrawDebugIcon(debug);
                //debug.boolValue = GUILayout.Toggle(debug.boolValue, "Debug", EditorStyles.miniButton);

                if (GUILayout.Button("Find Invokers", GUILayout.Width(100)))
                {
                    FindAllInvokers();
                }

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Find Listeners", GUILayout.Width(100)))
                    {
                        FindListeners();
                    }
                }
            }

            if (style == null)
                style = new GUIStyle(Style_)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    stretchWidth = true
                };

            style.normal.textColor = EditorStyles.label.normal.textColor;

            Description.stringValue = UnityEditor.EditorGUILayout.TextArea(Description.stringValue, style);

            //EditorGUILayout.PropertyField(Description, GUIContent.none);


            if (debug.boolValue && Application.isPlaying)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var w = 60;

                    if (GUILayout.Button("Invoke Void")) { ev.Invoke(); }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_bool);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_bool); }
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_int);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_int); }
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_float);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_float); }
                    }




                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_string);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_string); }
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_V2);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_V2.Value); }
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_V3);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_V3.Value); }
                    }



                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_go);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_go.Value); }
                    }



                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_transform);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_transform.Value); }
                    }


                    using (new GUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(m_Sprite);
                        if (GUILayout.Button("Invoke", GUILayout.Width(w))) { ev.Invoke(ev.m_Sprite); }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }


        public class EventReferenceInfo
        {
            public MonoBehaviour Owner { get; set; }
            public UnityEventBase Event { get; set; }

            public string name { get; set; }
            // public List<string> MethodNames { get; set; } = new List<string>();
        }

        private void FindAllInvokers()
        {
            //find all objects in current opened scene
            // var allObjects = GameObject.FindObjectsOfType(typeof(MonoBehaviour));
            //find all objects in current opened scene
            var allObjects = GameObject.FindObjectsByType<MonoBehaviour>(sortMode: FindObjectsSortMode.None);
            //  Debug.Log("All MOno = " + allObjects.Length);

            //  Debug.Log("All MOno = " + allObjects.Length);

            var events = new List<EventReferenceInfo>();

            foreach (var b in allObjects)
            {
                var info = b.GetType().GetTypeInfo();

                //This only finds the Top Events
                var FoundUnityEvents = info.DeclaredFields.Where(f => f.FieldType.IsSubclassOf(typeof(UnityEventBase))).ToList();

                foreach (var e in FoundUnityEvents)
                {
                    events.Add(new EventReferenceInfo()
                    {
                        Owner = b as MonoBehaviour,
                        Event = (e.GetValue(b) as UnityEventBase),
                        name = e.Name
                    });
                }

                //Find all the List values with events inside
                var ListsFields = info.DeclaredFields.Where(x => typeof(IEnumerable).IsAssignableFrom(x.FieldType) && x.IsPublic).ToList();

                foreach (var item in ListsFields)
                {
                    if (item.FieldType.IsGenericType && item.GetValue(b) is IEnumerable)
                    {
                        //  Debug.Log($"item : {item.Name}, OWNER {b.name} {b.GetType().Name}");

                        foreach (var prop in item.GetValue(b) as IEnumerable)
                        {
                            // Debug.Log($"Internal : {prop.GetType().Name}");

                            if (prop == null) continue; //BUG


                            //Unity Event inside lists
                            var EventsInsideList =
                                prop.GetType().GetTypeInfo().DeclaredFields.Where(f => f.FieldType.IsSubclassOf(typeof(UnityEventBase))).ToList();

                            foreach (var e in EventsInsideList)
                            {
                                events.Add(new EventReferenceInfo()
                                {
                                    Owner = b as MonoBehaviour,
                                    Event = (e.GetValue(prop) as UnityEventBase),
                                    name = e.Name
                                });

                                //  Debug.Log($"EVENT NAME : {e.Name}");
                            }
                        }
                    }
                }
            }

            ///NEED TO FIND EVENTS IN SUBCLASSES



            //PRINT ALL EVENTS
            foreach (var e in events)
            {
                if (e.Event == null) continue;

                int count = e.Event.GetPersistentEventCount();

                for (int i = 0; i < count; i++)
                {
                    var obj = e.Event.GetPersistentTarget(i);

                    if (obj == target)
                    {
                        Debug.Log($"<color=cyan><B>[{target.name}]</B>. Invoker:" +
                            $" <B>[{e.Owner.name}]</B>→<B>[{e.Owner.GetType().Name}]→[{e.name}]</B></color>", e.Owner);
                    }
                }
            }


            // Debug.Log("All Events = " + events.Count);
        }

        private void FindListeners()
        {
            // Get all the listeners of the event
            // EventTrigger.Entry[] entries = eventSystem.GetEventTrigger(eventName).triggers.ToArray();

            // Loop through the listeners and log their game object names
            foreach (var eventItem in ev.eventListeners)
            {
                //Debug Void Responses
                for (int i = 0; i < eventItem.Response.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.Response.GetPersistentTarget(i);
                    var what = eventItem.Response.GetPersistentMethodName(i);
                    Debuggin($"Void -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseBool.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseBool.GetPersistentTarget(i);
                    var what = eventItem.ResponseBool.GetPersistentMethodName(i);
                    Debuggin($"Bool -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseBoolFalse.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseBoolFalse.GetPersistentTarget(i);
                    var what = eventItem.ResponseBoolFalse.GetPersistentMethodName(i);
                    Debuggin($"Bool False -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseBoolTrue.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseBoolTrue.GetPersistentTarget(i);
                    var what = eventItem.ResponseBoolTrue.GetPersistentMethodName(i);
                    Debuggin($"Bool True -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseInt.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseInt.GetPersistentTarget(i);
                    var what = eventItem.ResponseInt.GetPersistentMethodName(i);
                    Debuggin($"Int -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseFloat.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseFloat.GetPersistentTarget(i);
                    var what = eventItem.ResponseFloat.GetPersistentMethodName(i);
                    Debuggin($"Float -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseString.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseString.GetPersistentTarget(i);
                    var what = eventItem.ResponseString.GetPersistentMethodName(i);
                    Debuggin($"String -> [{what}]. Target [{item.name}]", eventItem.Owner); ;
                }


                for (int i = 0; i < eventItem.ResponseTransform.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseTransform.GetPersistentTarget(i);
                    var what = eventItem.ResponseTransform.GetPersistentMethodName(i);
                    Debuggin($"Transform -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseComponent.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseComponent.GetPersistentTarget(i);
                    var what = eventItem.ResponseComponent.GetPersistentMethodName(i);
                    Debuggin($"Component -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseGO.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseGO.GetPersistentTarget(i);
                    var what = eventItem.ResponseGO.GetPersistentMethodName(i);
                    Debuggin($"GameObject -> [{what}. Target [{item.name}]]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseVector2.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseVector2.GetPersistentTarget(i);
                    var what = eventItem.ResponseVector2.GetPersistentMethodName(i);
                    Debuggin($"Vector2 -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseVector3.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseVector3.GetPersistentTarget(i);
                    var what = eventItem.ResponseVector3.GetPersistentMethodName(i);
                    Debuggin($"Vector3 -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.ResponseSprite.GetPersistentEventCount(); i++)
                {
                    var item = eventItem.ResponseSprite.GetPersistentTarget(i);
                    var what = eventItem.ResponseSprite.GetPersistentMethodName(i);
                    Debuggin($"Sprite -> [{what}]. Target [{item.name}]", eventItem.Owner);
                }

                for (int i = 0; i < eventItem.IntEventList.Count; i++)
                {
                    var advanceInt = eventItem.IntEventList[i];

                    if (advanceInt.Response == null) continue;
                    for (int j = 0; j < advanceInt.Response.GetPersistentEventCount(); j++)
                    {
                        var item = advanceInt.Response.GetPersistentTarget(i);
                        var what = advanceInt.Response.GetPersistentMethodName(i);
                        Debuggin($"Advanced Int -> [{what}]. Target [{item.name}]", eventItem.Owner);
                    }
                }
            }
        }
    }
#endif
}
