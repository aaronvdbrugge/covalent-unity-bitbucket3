using System;
using UnityEngine;

namespace Covalent.HomeIsland
{
	public class Glow : MonoBehaviour
	{
		[SerializeField] private bool isActive;
		[SerializeField] private Material glowMaterial;
		[SerializeField] private Color baseColor;
		[SerializeField] private Vector2 intensityRange;
		[SerializeField] private Vector2 distanceMathRange;
		[SerializeField] private bool pulseActive;
		[SerializeField] private float pulseFrequency;
		[SerializeField] private float pulseIntensity;

		private bool withinRange;
		private readonly int outlineColor = Shader.PropertyToID("OutlineColor");
		private float pulse;
		private float intensityFromDistance;

		public bool IsActive => isActive;
		public bool WithinRange => withinRange;
		public bool PulseActive => pulseActive;
		
		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.tag.Equals("Player"))
			{
				withinRange = true;
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (other.gameObject.tag.Equals("Player"))
			{
				withinRange = false;
				intensityFromDistance = intensityRange.x;
			}
		}

		public void UpdatePulseValue()
		{
			pulse = MyMath.Pulse(Time.time, pulseFrequency) * pulseIntensity;
		}

		public void UpdateGlowDistanceIntensity()
		{
			float distance = Vector2.Distance(Player_Controller_Mobile.mine.transform.position, transform.position);
			// Set neutralDistance to the min/max values used to affect intensity
			float neutralDistance = distance;
			if (neutralDistance < distanceMathRange.x)
				neutralDistance = distanceMathRange.x;
			else if (neutralDistance > distanceMathRange.y)
				neutralDistance = distanceMathRange.y;
			intensityFromDistance = MyMath.ConvertRange(distanceMathRange.x, distanceMathRange.y, intensityRange.y,
				intensityRange.x, neutralDistance);
		}

		public void UpdateGlowIntensity()
		{
			float factor = Mathf.Pow(2, intensityFromDistance + pulse);
			Color newColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
			glowMaterial.SetColor(outlineColor, newColor);
		}
	}
}