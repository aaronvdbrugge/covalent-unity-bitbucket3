using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses LateUpdate / transform.position
/// </summary>
public class SetPositionToOtherTransform : MonoBehaviour
{
	public Transform otherTransform;
	public Vector3 offset;

	private void LateUpdate()
	{
		transform.position = otherTransform.position + offset;
	}
}
