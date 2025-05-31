using UnityEngine;

namespace MalbersAnimations.Controller
{
    [System.Serializable]
    public abstract class ModeModifier : ScriptableObject
    {
        public virtual void OnModeEnter(Mode mode) { }

        public virtual void OnModeMove(Mode mode) { }

        public virtual void OnModeExit(Mode mode) { }  
    }
}

