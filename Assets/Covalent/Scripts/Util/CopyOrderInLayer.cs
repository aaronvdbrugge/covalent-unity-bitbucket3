using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Copies "Order in Layer" from another sprite, with an optional offset.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CopyOrderInLayer : MonoBehaviour
{
	public SpriteRenderer copyFrom;
	public int offset = 0;

    SpriteRenderer spriteRenderer;
	private void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}


	private void LateUpdate()
	{
		spriteRenderer.sortingOrder = copyFrom.sortingOrder + offset;
	}
}
