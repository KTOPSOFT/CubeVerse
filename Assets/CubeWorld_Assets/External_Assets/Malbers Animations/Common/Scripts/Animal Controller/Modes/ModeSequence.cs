using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{

    [AddComponentMenu("Malbers/Animal Controller/Mode Sequence")]
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/manimal-controller/mode-sequence")]
    public class ModeSequence : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;

        [Tooltip("While the sequence is playing the animal cannot be controlled")]
        public bool disableControl = true;


        [Tooltip("The Sequence will be Play on Start. To play Manually call the method 'PlaySequence()' ")]
        public bool PlayOnStart = false;

        [Tooltip("Play a mode using the mode list. Use Combine Index  (Mode_ID * 1000 + Ability_Index) (See Animal Modes) ")]
        public List<int> sequence = new();

        private int index = 0;
        private bool playing;

 
        public void PlaySequence()
        {
            var ModeID = sequence[index];
            var id = Mathf.Abs(ModeID / 1000);
            var ability = id == 0 ? -99 : ModeID % 100;

            if (animal.Mode_TryActivate(id, ability))
            {
                index++;
                playing = true;
                if (disableControl) animal.Lock(true);
            }
            else
            {
                Debug.LogWarning($"The current Mode sequence {ModeID} cannot be played.");
                EndSequence();
            }
        }



        private void OnModeEnd(int mode, int ability)
        {
            if (playing)
            {
                if (index < sequence.Count)
                {
                    PlaySequence();
                }
                else //Means the sequence has finished
                {
                    EndSequence();
                }
            }
        }

        private void EndSequence()
        {
            playing = false;
            index = 0;
            if (disableControl) animal.Lock(false);
        }

        private void OnEnable()
        {
            animal.OnModeEnd.AddListener(OnModeEnd);

            if (PlayOnStart) PlaySequence(); 
        }

        private void OnDisable() => animal.OnModeEnd.RemoveListener(OnModeEnd);

        private void Reset()
        {
            animal = this.FindComponent<MAnimal>();
        }

    }
}