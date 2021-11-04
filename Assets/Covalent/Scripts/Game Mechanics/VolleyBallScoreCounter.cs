using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Updates text to the volleyball's current hit streak.
/// Colors the text when the speed is maxed out.
/// </summary>
public class VolleyBallScoreCounter : MonoBehaviour
{
	public VolleyBall volleyball;
	public TMP_Text tmpText;
	[Tooltip("End of gradient is at max hit streak.")]
	public Gradient flashColor;

	public float flashTime = 0.25f;

	float _flashCooldown;   // set to 1.0, counts down to 0
	int _lastValue = int.MaxValue;   // last volleball.hitStreak
	Color _flashColor;   // color for current flash

	Color _originalTextColor;
	private void Start()
	{
		_originalTextColor = tmpText.color;
	}


	private void Update()
	{
		if( volleyball.hitStreak != _lastValue )   // Need to display new number.
		{
			if( volleyball.hitStreak > _lastValue )  // went up! play a flash animation
			{
				_flashCooldown = 1.0f;
				_flashColor = flashColor.Evaluate( Mathf.Clamp01(volleyball.hitStreak / (float)volleyball.maxSpeedupHits) );   // Pick an appropriate color based on how many times hit.
			}
			_lastValue = volleyball.hitStreak;
			
			tmpText.text = volleyball.hitStreak.ToString();
		}


		if( _flashCooldown > 0 )   // color the text
		{
			_flashCooldown -= Time.deltaTime / flashTime;
			tmpText.color = Color.Lerp(_originalTextColor, _flashColor, _flashCooldown);
		}
	}
}
