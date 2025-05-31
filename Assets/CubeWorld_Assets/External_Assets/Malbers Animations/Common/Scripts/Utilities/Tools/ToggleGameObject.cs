using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Toggle GameObject (Enable-Disable)")]

    public class ToggleGameObject : MonoBehaviour
    {
        public int index;
        public GameObject[] gameObjects;

        public void SetActive(int index)
        {
            this.index = index;

            if (index < 0 || index >= gameObjects.Length) //Check if the index is out of range
            {
                for (int i = 0; i < gameObjects.Length; i++)
                {
                    if (gameObjects[i] != null) gameObjects[i].SetActive(true); //Enable all the gameObjects if the index is out of range
                }

                return;
            }

            for (int i = 0; i < gameObjects.Length; i++)
            {
                if (gameObjects[i] != null) gameObjects[i].SetActive(i == index);
            }
        }

        public void SetActiveNext()
        {
            index++;
            if (index >= gameObjects.Length) index = 0;
            SetActive(index);
        }

        public void SetActivePrevious()
        {
            index--;
            if (index < 0) index = gameObjects.Length - 1;
            SetActive(index);
        }

    }
}