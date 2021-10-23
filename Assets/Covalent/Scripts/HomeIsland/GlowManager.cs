using System;
using System.Collections.Generic;
using UnityEngine;

namespace Covalent.HomeIsland
{
	public class GlowManager : MonoBehaviour
	{
		[SerializeField] private List<Glow> glows;

		private void Update()
		{
			for (int i = 0; i < glows.Count; i++)
			{
				Glow glow = glows[i];
				if (glow.IsActive)
				{
					glow.UpdateGlowIntensity();
				}
			}
		}
	}
}