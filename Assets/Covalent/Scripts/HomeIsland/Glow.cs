using UnityEngine;

namespace Covalent.HomeIsland
{
	public class Glow : MonoBehaviour
	{
		[SerializeField] private Material glowMaterial;
		[SerializeField] private Color baseColor;
		[SerializeField] private Vector2 intensityRange;

		private bool isActive;
		private readonly int outlineColor = Shader.PropertyToID("OutlineColor");

		public bool IsActive => isActive;

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.gameObject.tag.Equals("Player"))
			{
				isActive = true;
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (other.gameObject.tag.Equals("Player"))
			{
				isActive = false;
			}
		}

		public void UpdateGlowIntensity()
		{
			float distance = Vector2.Distance(Player_Controller_Mobile.mine.transform.position, transform.position);

			Debug.Log(distance); // 4.2 .... 1
			float intensity = intensityRange.y / distance;

			float factor = Mathf.Pow(2, intensity);
			Color newColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
			glowMaterial.SetColor(outlineColor, newColor);
		}
	}
}