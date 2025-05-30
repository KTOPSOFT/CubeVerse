using UnityEngine;

namespace Michsky.DreamOS
{
	public class AudioSyncer : MonoBehaviour
	{
		[Header("SETTINGS")]
		public float bias;
		public float timeStep;
		public float timeToBeat;
		public float restSmoothTime;

		private float previousAudioValue;
		private float audioValue;
		private float timer;

		protected bool isBeat;

		void Update()
		{
			OnUpdate();
		}

		public virtual void OnBeat()
		{
			timer = 0;
			isBeat = true;
		}

		public virtual void OnUpdate()
		{
			previousAudioValue = audioValue;
			audioValue = AudioSpectrum.SpectrumValue;

			if (previousAudioValue > bias && audioValue <= bias)
			{
				if (timer > timeStep)
					OnBeat();
			}

			if (previousAudioValue <= bias && audioValue > bias)
			{
				if (timer > timeStep)
					OnBeat();
			}

			timer += Time.deltaTime;
		}
	}
}