#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]

    [AddTypeMenu("[Debug]")]

    public class DebugReaction : Reaction
    {
        public override System.Type ReactionType => typeof(Component);

        public string log = "debug";

        public bool pauseEditor = false;

#if UNITY_EDITOR
        public MessageType MessageType = MessageType.Info;
#endif

        protected override bool _TryReact(Component component)
        {
#if UNITY_EDITOR

            switch (MessageType)
            {
                case MessageType.None:
                    Debug.Log($"<color=white> [{component.name}]<b> [{log}] </b></color>", component);
                    break;
                case MessageType.Info:
                    Debug.Log($"<color=white>[{component.name}]<b> [{log}] </b></color>", component);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning($"<color=yellow>[{component.name}]<b> [{log}] </b></color>", component);
                    break;
                case MessageType.Error:
                    Debug.LogError($"<color=red>[{component.name}]<b> [{log}] </b></color>", component);

                    break;
                default:
                    break;
            }

            if (pauseEditor)
            {
                Debug.Log("Pause Editor [Debug Reaction]");
                Debug.Break();
            }

#else
              Debug.Log($"<color=white> [{component.name}]<b> [{log}] </b></color>",component);
#endif
            return true;
        }
    }
}
