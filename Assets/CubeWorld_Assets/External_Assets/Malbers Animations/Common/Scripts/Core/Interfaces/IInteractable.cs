using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary>Used for identify Interactables</summary>
    public interface IInteractable
    {
        /// <summary>Reset the Interactable </summary>
        void Restart();

        /// <summary>Applies the Interaction Logic</summary>
        bool Interact(IInteractor interactor);

        /// <summary>Applies the Interaction Logic</summary>
        bool Interact(int InteracterID, GameObject interactor);

        /// <summary>Applies an Empty Interaction</summary>
        void Interact();

        /// <summary>Can be interated with only one time</summary>
        bool SingleInteraction { get; }

        /// <summary>If the Interactable is a single interaction check if it can Interact</summary>
        bool Active { get; set; }

        /// <summary>If the Interacter is ready to Interact it will interact automatically</summary>
        bool Auto { get; set; }

        /// <summary>Is the Interactable focused by an Interactor? </summary>
        bool Focused { get; set; }

        /// <summary>Who is the Interactor making the Focus</summary>
        IInteractor CurrentInteractor { get; set; }

        /// <summary> ID value to recognize the Interactable. its used on the Events</summary>
        int Index { get; }

        /// <summary>GameObject that has the Interactable component</summary>
        GameObject Owner { get; }
    }

    /// <summary>Used for identify who Interact with the Interactable</summary>
    public interface IInteractor
    {
        /// <summary> ID value to recognize who is doing the Interaction  </summary>
        int ID { get; }

        /// <summary>Can the Interacter Interact?</summary>
        bool Enabled { get; set; }

        /// <summary> GameObject of who is doing the Interaction  </summary>
        GameObject Owner { get; }

        /// <summary>Makes the Interacter Logic, It also calls the Interactable.Interact Logic</summary>
        bool Interact(IInteractable interactable);

        /// <summary>Unfocus an Interactable</summary>
        void UnFocus(IInteractable interactable);

        /// <summary>Focus an Interable</summary>
        void Focus(IInteractable interactable);
        void Restart();

        ///// <summary>Invoked when an Item has been focused</summary>
        //GameObjectEvent OnFocusing { get; }
        ///// <summary>Invoked when an Item has been unfocused</summary>
        //GameObjectEvent OnUnFocusing { get; }
    }

    [System.Serializable]
    public class InteractionEvents
    {
        public GameObjectEvent OnInteractWithGO = new();
        public IntEvent OnInteractWith = new();
    }

    /// <summary>Used for Play Animations on a Character, in case of the Animal Controller are the Modes</summary>
    public interface ICharacterAction
    {
        public GameObject gameObject { get; }

        /// <summary>Play an Animation Action on a Character and returns True if it can play it</summary>
        bool PlayAction(int Set, int Index);

        /// <summary>Force  an Animation Action on a Character and returns True if it can play it</summary>
        bool ForceAction(int Set, int Index);

        /// <summary>Is the Character playing an Action Animation (ICharacterAction Interface) </summary>
        bool IsPlayingAction { get; }

        /// <summary>Is the Character Moving </summary>
        bool MovementDetected { get; }

        /// <summary>Listen when the Character has changed the state. Returns the New State</summary>
        public System.Action<int> OnState { get; set; }

        /// <summary>Listen when the Character a mode Starts. Returns the  Mode Int Values. (Mode and Ability) </summary>
        public System.Action<int, int> ModeStart { get; set; }
        /// <summary>Listen when the Character a mode Ends. Returns the   Mode Int Values. (Mode and Ability)  </summary>
        public System.Action<int, int> ModeEnd { get; set; }

        /// <summary>Listen when the Character has changed the Stance. Returns the New Stance</summary>
        public System.Action<int> OnStance { get; set; }
    }
}