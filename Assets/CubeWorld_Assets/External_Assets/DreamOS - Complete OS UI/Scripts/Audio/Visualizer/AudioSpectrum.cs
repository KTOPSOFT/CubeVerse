using UnityEngine;

namespace Michsky.DreamOS
{
    public class AudioSpectrum : MonoBehaviour
    {
        public static float SpectrumValue { get; private set; }
        private float[] audioSpectrum;

        void Start()
        {
            audioSpectrum = new float[128];
        }

        void Update()
        {
            AudioListener.GetSpectrumData(audioSpectrum, 0, FFTWindow.Hamming);

            if (audioSpectrum != null && audioSpectrum.Length > 0)
                SpectrumValue = audioSpectrum[0] * 100;
        }
    }
}