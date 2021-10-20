using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Covalent.GameObjects
{
	public class StarsTwinkling : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer[] stars;
		[SerializeField] private float twinkleAnimationTime;
		[SerializeField] private Color clearColor;
		[SerializeField] private int maxTwinkleAmount;
		[SerializeField] private Vector2 waitRange;
		[SerializeField] private float rotationValue;

		private List<SpriteRenderer> starsActive = new List<SpriteRenderer>();
		private float[] twinkleTimers;
		private int previousStarId;
		private Tween twinklingTween;
		private Sequence twinkle;

		private void Awake()
		{
			twinkleTimers = new float[maxTwinkleAmount];
		}

		private void Update()
		{
			var t = Time.deltaTime;
			for (int i = 0; i < twinkleTimers.Length; i++)
			{
				twinkleTimers[i] -= t;
				if (twinkleTimers[i] <= 0)
				{
					TwinkleStar();
					twinkleTimers[i] = Random.Range(waitRange.x, waitRange.y);
				}
			}
		}

		private void OnEnable()
		{
			starsActive.AddRange(stars);
			for (int i = 0; i < twinkleTimers.Length; i++)
			{
				twinkleTimers[i] = Random.Range(waitRange.x, waitRange.y);
			}
		}

		private void OnDisable()
		{
			starsActive.Clear();
			DOTween.Kill(this, false);
		}

		private void TwinkleStar()
		{
			if (starsActive.Count == 0)
				return;
			int randomIndex = Random.Range(0, starsActive.Count);
			SpriteRenderer starTarget = starsActive[randomIndex];
			starsActive.Remove(starTarget);
			Vector3 rotation = new Vector3(0, 0, rotationValue);
			Vector3 originalScale = starTarget.transform.localScale;
			Tween twinkle = DOTween.Sequence()
				.Append(starTarget.transform.DOScale(originalScale, twinkleAnimationTime / 2)
					.SetEase(Ease.OutCubic))
				.Join(starTarget
					.DOColor(clearColor, twinkleAnimationTime / 2)
					.SetEase(Ease.Linear))
				.Join(starTarget.transform
					.DOLocalRotate(rotation, twinkleAnimationTime / 2, RotateMode.FastBeyond360)
					.SetRelative()
					.SetEase(Ease.Linear))
				.Append(starTarget.transform.DOScale(originalScale, twinkleAnimationTime / 2)
					.SetEase(Ease.InCubic))
				.Join(starTarget
					.DOColor(Color.white, twinkleAnimationTime / 2)
					.SetEase(Ease.Linear))
				.Join(starTarget.transform.DOLocalRotate(rotation, twinkleAnimationTime / 2)
					.SetRelative()
					.SetEase(Ease.Linear))
				.AppendCallback(() => { starsActive.Add(starTarget); })
				.SetId(this);
		}
	}
}