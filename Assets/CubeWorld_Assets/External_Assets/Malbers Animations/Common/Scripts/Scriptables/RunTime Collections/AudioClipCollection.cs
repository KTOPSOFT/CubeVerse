using MalbersAnimations.Events;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary> Store a list of Materials</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Collections/Audio Clip Collection", fileName = "New Audio Collection", order = 1000)]
    public class AudioClipCollection : RuntimeCollection<AudioClip>
    {
        public AudioEvent OnItemAdded = new();
        public AudioEvent OnItemRemoved = new();

        protected override void OnAddEvent(AudioClip newItem) => OnItemAdded.Invoke(newItem);
        protected override void OnRemoveEvent(AudioClip newItem) => OnItemRemoved.Invoke(newItem);

    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(AudioClipCollection))]
    public class AudioClipCollectionEd : RuntimeCollectionEditor<AudioClip> { }
#endif

}