using MalbersAnimations.Reactions;
using MalbersAnimations.Utilities;
using UnityEngine;

namespace MalbersAnimations
{
    [System.Serializable] //Needs to be Serializable!!!!
    [AddTypeMenu("Unity/Messages")]
    public class MessageReaction : Reaction
    {
        [Tooltip("Send Messages also to the Component Children")]
        public bool sendToChildren = true;
        [Tooltip("Use Component.SendMessage Instead of ")]
        public bool UseSendMessage = false;
        public MesssageItem[] messages;
        public bool debug = false;
        public override System.Type ReactionType => typeof(Component); //set the Type of component this Reaction Needs

 
        protected override bool _TryReact(Component reactor)
        {
            foreach (var item in messages)
            {
                Deliver(item, reactor);
            }

            return true;
        }


        private void Deliver(MesssageItem m, Component go)
        {
            if (UseSendMessage)
                m.DeliverMessage(go, sendToChildren, debug);
            else
            {
                IAnimatorListener[] listeners;

                if (sendToChildren)
                    listeners = go.GetComponentsInChildren<IAnimatorListener>();
                else
                    listeners = go.GetComponentsInParent<IAnimatorListener>();

                if (listeners != null && listeners.Length > 0)
                {
                    foreach (var animListeners in listeners)
                        m.DeliverAnimListener(animListeners, debug);
                }
            }
        }
    }
}