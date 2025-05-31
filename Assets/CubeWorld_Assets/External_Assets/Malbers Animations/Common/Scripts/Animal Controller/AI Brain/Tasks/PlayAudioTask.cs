using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Controller.AI
{
    [CreateAssetMenu(menuName = "Malbers Animations/Pluggable AI/Tasks/Play Audio")]
    public class PlayAudioTask : MTask
    {
        public override string DisplayName => "General/Play Audio";

        [Space]
        public AudioClipReference Clips;
        public string AudioSource = "BrainAudio";
       

        public override void StartTask(MAnimalBrain brain, int index)
        {
            var findAudio = brain.transform.FindGrandChild(AudioSource);

            if (!findAudio)
            {
                findAudio = new GameObject(name: AudioSource).transform;
                findAudio.parent = brain.transform;
            }

            if (!findAudio.TryGetComponent<AudioSource>(out var sourc)) 
                sourc = findAudio.gameObject.AddComponent<AudioSource>();

            brain.TasksVars[index].AddComponent(sourc); //Save the audio source to the task variables

            Clips.Play(sourc);

            brain.TaskDone(index);
        }


        void Reset()
        { 
            Description = 
                "Plays an Audioclip in the Audio Source. If there's no Audio Source with the name assigned []. I will add a new Audio Source Component "; 
        }
    }
}
