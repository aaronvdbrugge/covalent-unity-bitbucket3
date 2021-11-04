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
	public Color maxHitStreakColor;

	Color _originalTextColor;
	private void Start()
	{
		_originalTextColor = tmpText.color;
	}


	private void Update()
	{
		tmpText.text = volleyball.hitStreak.ToString();
		if( volleyball.hitStreak >= volleyball.maxSpeedupHits )   // It's at max speed! Color the text
			tmpText.color = maxHitStreakColor;
		else
			tmpText.color = _originalTextColor;
	}
}
