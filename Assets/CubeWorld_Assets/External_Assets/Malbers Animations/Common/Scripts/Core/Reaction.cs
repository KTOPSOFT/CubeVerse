using System;
using UnityEditor;
using UnityEngine;


namespace MalbersAnimations.Reactions
{
    [Serializable]
    public abstract class Reaction//<T> where T : Component
    {
        public static MonoBehaviour Delay;

        /// <summary>Instant Reaction ... without considering Active or Delay parameters</summary>
        protected abstract bool _TryReact(Component reactor);

        /// <summary>Get the Type of the reaction</summary>
        public abstract Type ReactionType { get; }

        public void React(Component component) => TryReact(localComponent.useLocal ? localComponent.component : component);

        public void React(GameObject go) => TryReact(go.transform);

        [Tooltip("Enable or Disable the Reaction")]
        [HideInInspector] public bool Active = true;

        public LocalComponet localComponent;

        [HideInInspector]
        [Min(0)] public float delay = 0;

        /// <summary>  Checks and find the correct component to apply a reaction  </summary>  
        public Component VerifyComponent(Component component)
        {
            if (component == null) return null; //If the component is null return null (No Component to React_)

            Component TrueComponent;

            //Find if the component is the same 
            if (ReactionType.IsAssignableFrom(component.GetType()))
            {
                TrueComponent = component;
            }
            else
            {
                //Debug.Log($"Component {component.name} REACTION TYPE: {ReactionType.Name}");

                TrueComponent = component.GetComponent(ReactionType);

                if (TrueComponent == null)
                    TrueComponent = component.GetComponentInParent(ReactionType);
                if (TrueComponent == null)
                    TrueComponent = component.GetComponentInChildren(ReactionType);
            }

            return TrueComponent;
        }

        public bool TryReact(Component component)
        {
            if (Application.isPlaying) //Reactions cannot be called in Editor!!
            {
                if (Active)
                {
                    component = VerifyComponent(localComponent.useLocal ? localComponent.component : component);

                    if (component == null) //verification if the component is null
                    {
                        Debug.Log($"Component is null. Ignoring the Reaction. <b>[{ReactionType.Name}] </b>");
                        return false; //NO Component to React
                    }

                    if (delay > 0)
                    {
                        //Create the Delay Reactions for the first time
                        if (Delay == null)
                        {
                            var DelayGameObject = new GameObject("Reaction Delay");
                            Delay = DelayGameObject.AddComponent<UnityUtils>();

                            Delay.hideFlags = HideFlags.HideInInspector;
                            Debug.Log($"Creating Delay Reaction GameObject for Delay Reactions. Created by [{ReactionType.Name}]", component);
                        }


                        Delay.Delay_Action(delay, () => { if (Delay != null) _TryReact(component); });
                        return true;
                    }
                    else
                    {
                        return _TryReact(component);
                    }
                }
            }
            return false;
        }

        //React to multiple components
        public bool TryReact(params Component[] components)
        {
            if (Active && components != null && components.Length > 0)
            {
                foreach (var component in components)
                {
                    var comp = VerifyComponent(component);
                    _TryReact(comp);
                }
            }
            return true;
        }
    }


    [System.Serializable]
    public struct LocalComponet
    {
        public bool useLocal;
        [RequiredField] public Component component;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LocalComponet))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int indent = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            label = EditorGUI.BeginProperty(position, new GUIContent("Use Local", "If local is true, the component used for the reaction will not change when you send a Dynamic value"), property);
            position = EditorGUI.PrefixLabel(position, label);
            var component = property.FindPropertyRelative("component");
            var useLocal = property.FindPropertyRelative("useLocal");


            var useLocalRect = new Rect(position)
            {
                width = 20f,
                height = EditorGUIUtility.singleLineHeight,
                x = position.x,
            };

            EditorGUI.PropertyField(useLocalRect, useLocal, GUIContent.none, false);

            if (useLocal.boolValue)
            {
                position.x += 17;
                position.width -= 17;

                EditorGUI.PropertyField(position, component, GUIContent.none, false);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}