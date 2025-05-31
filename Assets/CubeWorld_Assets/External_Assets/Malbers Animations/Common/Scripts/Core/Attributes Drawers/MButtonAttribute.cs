using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    public class MButtonAttribute : PropertyAttribute
    {
        public string MethodName { get; }

        public bool OnlyPlayMode;
        public string displayName;

        public MButtonAttribute(string methodName)
        {
            MethodName = methodName;
            OnlyPlayMode = false;
            displayName = methodName;
        }

        public MButtonAttribute(string methodName, bool inplaymode)
        {
            MethodName = methodName;
            OnlyPlayMode = inplaymode;
            displayName = methodName;
        }

        public MButtonAttribute(string methodName, string display, bool inplaymode)
        {
            MethodName = methodName;
            OnlyPlayMode = inplaymode;
            displayName = display;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (attribute as MButtonAttribute);

            if (att != null)
            {
                if (att.OnlyPlayMode && !Application.isPlaying) return;


                string methodName = att.MethodName;


                Object target = property.serializedObject.targetObject;
                System.Type type = target.GetType();
                System.Reflection.MethodInfo method = type.GetMethod(methodName);
                if (method == null)
                {
                    GUI.Label(position, "Method could not be found. Is it public?");
                    return;
                }
                if (method.GetParameters().Length > 0)
                {
                    GUI.Label(position, "Method cannot have parameters.");
                    return;
                }
                if (GUI.Button(position, att.displayName))
                {
                    method.Invoke(target, null);
                }
            }
        }
    }
#endif


}