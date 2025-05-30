//This asset was uploaded by https://unityassetcollection.com

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using TMPro;

namespace Michsky.DreamOS
{
    public class FetchTextData : MonoBehaviour
    {
        [Header("Settings")]
        public bool checkOnEnable = true;
        public string compareDataWith = "2.0";
        public float resultDelay = 1.75f;
        public string fetchDataFrom = "https://www.michsky.com/dreamos/FetchDataExample.txt";

        [Header("Events")]
        public UnityEvent onFetchStart;
        public UnityEvent onCompareMatch;
        public UnityEvent onCompareMismatch;

        private string fetchedData;
        private bool isRunning;

        void OnEnable()
        {
            isRunning = false;

            if (checkOnEnable == true && gameObject.activeInHierarchy == true)
                FetchData();
        }

        public void FetchData()
        {
            if (isRunning == true)
                return;

            isRunning = true;
            onFetchStart.Invoke();
            StartCoroutine("FetchPlainData");
        }

        public void ApplyDataToTMP(TextMeshProUGUI textVar) { textVar.text = fetchedData; }

        IEnumerator FetchPlainData()
        {
            UnityWebRequest tempToken = UnityWebRequest.Get(fetchDataFrom);
            yield return tempToken.SendWebRequest();
            fetchedData = tempToken.downloadHandler.text;
            StartCoroutine("WaitForEventDelay");
        }

        IEnumerator WaitForEventDelay()
        {
            yield return new WaitForSeconds(resultDelay);

            if (fetchedData == compareDataWith) { onCompareMatch.Invoke(); }
            else if (fetchedData != compareDataWith) { onCompareMismatch.Invoke(); }

            isRunning = false;
        }
    }
}