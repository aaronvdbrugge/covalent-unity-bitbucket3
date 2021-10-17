using DG.Tweening;
using UnityEngine;

namespace Covalent.GameObjects
{
	public class StarsTwinkling : MonoBehaviour
	{
		[SerializeField] private GameObject[] stars;
		[SerializeField] private float twinkleWaitTime;
		[SerializeField] private float twinkleAnimationTime;

		[Tooltip("To rotate back to the original rotation, stick to multiple of 180.")] [SerializeField]
		private float rotationValue;

		private int previousStarId;
		private Tween twinklingTween;
		private Sequence twinkle;

		private void OnEnable()
		{
			TwinkleAnimation();
		}

		private void OnDisable()
		{
			twinklingTween.Kill();
		}

		private GameObject starTarget;

		private void TwinkleAnimation()
		{
			twinklingTween = DOTween.Sequence()
				.AppendCallback(() =>
				{
					int i = previousStarId;
					while (i == previousStarId)
					{
						i = Random.Range(0, stars.Length);
					}

					starTarget = stars[i];
					previousStarId = i;
					Vector3 originalScale = starTarget.transform.localScale;
					Vector3 rotation = new Vector3(0, 0, rotationValue);
					twinkle = DOTween.Sequence()
						.Append(starTarget.transform.DOScale(originalScale * 2f, twinkleAnimationTime / 2)
							.SetEase(Ease.OutCubic))
						.Join(starTarget.transform
							.DOLocalRotate(rotation, twinkleAnimationTime / 2, RotateMode.FastBeyond360)
							.SetRelative()
							.SetEase(Ease.Linear))
						.Append(starTarget.transform.DOScale(originalScale, twinkleAnimationTime / 2)
							.SetEase(Ease.InCubic))
						.Join(starTarget.transform.DOLocalRotate(rotation, twinkleAnimationTime / 2)
							.SetRelative()
							.SetEase(Ease.Linear));
				})
				.Append(twinkle)
				.AppendInterval(twinkleWaitTime);

			twinklingTween.SetLoops(-1);
		}
	}
}