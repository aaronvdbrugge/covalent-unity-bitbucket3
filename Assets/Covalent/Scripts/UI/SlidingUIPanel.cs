using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Sliding UI panel which can slide in from outside the screen.
/// </summary>
public class SlidingUIPanel : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Time spent sliding in or out.")]
	public float slideTime;

	[Tooltip("Applied to both in and out animations.")]
	public EasingFunction.Ease slideEase;

	[Tooltip("Amount we'll change anchoredPosition by to put it offscreen.")]
	public Vector2 slideDist;


	[Header("Runtime")]
	[Tooltip("Set this to true during runtime to slide it out. You can also call the SlideIn and SlideAway functions. Note we'll disable ourselves when fully slid out, so you need to enable as well")]
	public bool doSlideIn = false;   

	public void SlideIn()
	{
		doSlideIn = true;
		gameObject.SetActive(true);
	}
	public void SlideAway() => doSlideIn = false;



	RectTransform _rectTransform;
	Vector2 _anchoredPositionOriginal;
	float _slideProgress = 0;   // 0 = fully away, 1 = fully in

	private void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
		_anchoredPositionOriginal = _rectTransform.anchoredPosition;

		_rectTransform.anchoredPosition = _anchoredPositionOriginal + slideDist;   // put it out of the screen by default
	}


	private void Update()
	{
		if( (doSlideIn && _slideProgress < 1) || !doSlideIn )
		{
			if( doSlideIn )
				_slideProgress = Mathf.Min(1, _slideProgress + Time.deltaTime / slideTime);
			else 
				_slideProgress = Mathf.Max(1, _slideProgress - Time.deltaTime / slideTime);

			float eased = EasingFunction.GetEasingFunction( slideEase )(0, 1, _slideProgress);
			_rectTransform.anchoredPosition = Vector2.Lerp( _anchoredPositionOriginal + slideDist, _anchoredPositionOriginal, eased);

			if( !doSlideIn && _slideProgress <= 0 )   // disable ourselves when we've slid out completely
				gameObject.SetActive(false);
		}
	}

}
