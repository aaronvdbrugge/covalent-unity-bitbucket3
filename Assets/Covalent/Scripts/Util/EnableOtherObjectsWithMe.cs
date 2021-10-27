using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses OnEnable / OnDisable, forwards to other objects
/// </summary>
public class EnableOtherObjectsWithMe : MonoBehaviour
{
	public GameObject[] others;

	private void OnEnable()
	{
		foreach( GameObject go in others )
			go.SetActive(true);
	}

	private void OnDisable()
	{
		foreach( GameObject go in others )
			go.SetActive(false);
	}
}
