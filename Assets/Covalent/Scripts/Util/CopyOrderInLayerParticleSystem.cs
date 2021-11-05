using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Allows ParticleSystem to copy its order in layer from a sprite.
/// </summary>
[RequireComponent(typeof(ParticleSystemRenderer))]
public class CopyOrderInLayerParticleSystem: MonoBehaviour
{
	public SpriteRenderer copyFrom;
	public int offset = 0;

	ParticleSystemRenderer particleSystemRenderer;

	private void Awake()
	{
		particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
	}


	private void LateUpdate()
	{
		particleSystemRenderer.sortingOrder = copyFrom.sortingOrder + offset;
	}
}
