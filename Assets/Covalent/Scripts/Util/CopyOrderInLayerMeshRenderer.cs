using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Allows MeshRenderer to copy its order in layer from a sprite.
/// Works on TextMeshPro.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class CopyOrderInLayerMeshRenderer: MonoBehaviour
{
	public SpriteRenderer copyFrom;
	public int offset = 0;

	MeshRenderer meshRenderer;

	private void Awake()
	{
		meshRenderer = GetComponent<MeshRenderer>();
	}


	private void LateUpdate()
	{
		meshRenderer.sortingOrder = copyFrom.sortingOrder + offset;
	}
}
