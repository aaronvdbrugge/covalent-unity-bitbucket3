using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Uses OnEnable / OnDisable, forwards to other objects
/// </summary>
public class EnableOtherObjectsWithMe : MonoBehaviour
{
	public GameObject[] others;

	[Tooltip("Does the opposite of what we do.")]
	public GameObject[] disableWithMe;

	private void OnEnable()
	{
		foreach( GameObject go in others )
			go.SetActive(true);
		foreach( GameObject go in disableWithMe)
			go.SetActive(false);
	}

	private void OnDisable()
	{
		foreach( GameObject go in others )
			go.SetActive(false);
		foreach( GameObject go in disableWithMe)
			go.SetActive(true);
	}
}
