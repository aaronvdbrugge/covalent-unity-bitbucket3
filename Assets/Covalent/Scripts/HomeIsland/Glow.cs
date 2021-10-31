using System;
using UnityEngine;

namespace Covalent.HomeIsland
{
	public class Glow : MonoBehaviour
	{
		[SerializeField] private bool isActive;
		[SerializeField] private GlowTarget[] glowTargets;
		[SerializeField] private Vector2 distanceMathRange;
		[Header("Outline")] [SerializeField] private Color baseOutlineColor;
		[SerializeField] private Vector2 opacityRangeOutlineInner;
		[SerializeField] private Vector2 opacityRangeOutlineOuter;
		[Header("Glow")] [SerializeField] private Color baseGlowColor;
		[SerializeField] private Vector2 distanceMathRangeGlow;
		[SerializeField] private Vector2 opacityRangeOutlineGlow;
		[SerializeField] private Vector2 intensityRange;
		[Header("Pulse")] [SerializeField] private bool pulseActive;
		[SerializeField] private bool onlyPulseInRange;
		[SerializeField] private float pulseFrequency;
		[SerializeField] private float pulseIntensity;

		private bool withinRange;
		private readonly int hdrGlowColor = Shader.PropertyToID("HDRColor");
		private float pulse;
		private float intensityFromDistance;
		private float clampedDistance;

		public bool IsActive => isActive;

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

		private void OnEnable()
		{
			UpdateGlow(true);
		}

		public void UpdateGlow(bool forceUpdate = false)
		{
			GetClampedDistance();

			if (withinRange || forceUpdate)
			{
				UpdateOutlines();
				UpdateGlowIntensityFromDistance();
			}

			if (pulseActive && !(onlyPulseInRange && !withinRange))
			{
				UpdatePulseValue();
			}
			
			UpdateGlowMaterialColor();
		}

		private void GetClampedDistance()
		{
			float distance;
			if (Player_Controller_Mobile.mine == null)
			{
				distance = 999;
			}
			else
			{
				distance = Vector2.Distance(Player_Controller_Mobile.mine.transform.position, transform.position);
			}
			clampedDistance = Mathf.Clamp(distance, distanceMathRange.x, distanceMathRange.y);
		}

		private void UpdateOutlines()
		{
			Color innerColor = baseOutlineColor;
			innerColor.a = MyMath.ConvertRange(distanceMathRange.x, distanceMathRange.y, opacityRangeOutlineInner.y,
				opacityRangeOutlineInner.x, clampedDistance, true);
			
			Color outerColor = baseOutlineColor;
			outerColor.a = MyMath.ConvertRange(distanceMathRange.x, distanceMathRange.y, opacityRangeOutlineOuter.y,
				opacityRangeOutlineOuter.x, clampedDistance, true);
			
			Color glowColor = baseGlowColor;
			glowColor.a = MyMath.ConvertRange(distanceMathRangeGlow.x, distanceMathRangeGlow.y, opacityRangeOutlineGlow.y,
				opacityRangeOutlineGlow.x, clampedDistance, true);

			for (int i = 0; i < glowTargets.Length; i++)
			{
				glowTargets[i].OutlineInner.color = innerColor;
				glowTargets[i].OutlineOuter.color = outerColor;
				glowTargets[i].OutlineGlow.color = glowColor;
			}
		}

		private void UpdatePulseValue()
		{
			pulse = MyMath.Pulse(Time.time, pulseFrequency) * pulseIntensity;
		}

		private void UpdateGlowIntensityFromDistance()
		{
			intensityFromDistance = MyMath.ConvertRange(distanceMathRange.x, distanceMathRange.y, intensityRange.y,
				intensityRange.x, clampedDistance, true);
		}
		
		private void UpdateGlowMaterialColor()
		{
			float factor = Mathf.Pow(2, intensityFromDistance + pulse);
			Color newColor = new Color(baseGlowColor.r * factor, baseGlowColor.g * factor, baseGlowColor.b * factor, baseGlowColor.a);
			for (int i = 0; i < glowTargets.Length; i++)
			{
				glowTargets[i].GlowMaterial.SetColor(hdrGlowColor, newColor);
			}
		}
		
		/*


		// Update the intensity of the glow based on the distance from the source
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

		// Update the glow material values
		public void UpdateGlowIntensity()
		{
			float factor = Mathf.Pow(2, intensityFromDistance + pulse);
			Color newColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
			glowMaterial.SetColor(hdrGlowColor, newColor);
		}
		*/
	}
}