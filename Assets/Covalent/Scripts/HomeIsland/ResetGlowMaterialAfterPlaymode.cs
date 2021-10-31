using System.Collections.Generic;
using UnityEngine;

namespace Covalent.HomeIsland
{
	[ExecuteInEditMode]
	public class ResetGlowMaterialAfterPlaymode : MonoBehaviour
	{
		[SerializeField] private List<Material> materials;
		[SerializeField] private Color baseColor;
		[SerializeField] private int baseIntensity;
		
		private readonly int outlineColor = Shader.PropertyToID("HDRColor");
		
		private void Awake()
		{
			if (!Application.isPlaying)
			{
				float factor = Mathf.Pow(2, baseIntensity);
				Color newColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);
				for (int i = 0; i < materials.Count; i++)
				{
					materials[i].SetColor(outlineColor, newColor);
				}
			}
		}
	}
}