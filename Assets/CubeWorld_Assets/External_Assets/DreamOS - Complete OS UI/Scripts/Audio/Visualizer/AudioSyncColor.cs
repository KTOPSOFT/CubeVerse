using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.DreamOS
{
	[RequireComponent(typeof(Image))]
	public class AudioSyncColor : AudioSyncer
	{
		[Header("SETTINGS")]
		public Color[] beatColors;
		public Color restColor;

		private int randomIndx;
		private Image img;

		void Start()
		{
			img = gameObject.GetComponent<Image>();
		}

		private IEnumerator MoveToColor(Color target)
		{
			Color curr = img.color;
			Color initial = curr;
			float timer = 0;

			while (curr != target)
			{
				curr = Color.Lerp(initial, target, timer / timeToBeat);
				timer += Time.deltaTime;

				img.color = curr;

				yield return null;
			}

			isBeat = false;
		}

		private Color RandomColor()
		{
			if (beatColors == null || beatColors.Length == 0) return Color.white;
			randomIndx = Random.Range(0, beatColors.Length);
			return beatColors[randomIndx];
		}

		public override void OnUpdate()
		{
			base.OnUpdate();

			if (isBeat == true)
				return;

			img.color = Color.Lerp(img.color, restColor, restSmoothTime * Time.deltaTime);
		}

		public override void OnBeat()
		{
			base.OnBeat();
			Color c = RandomColor();
			StopCoroutine("MoveToColor");
			StartCoroutine("MoveToColor", c);
		}
	}
}