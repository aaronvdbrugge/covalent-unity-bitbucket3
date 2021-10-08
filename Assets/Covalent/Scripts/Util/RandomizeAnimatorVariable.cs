using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Constantly sets an Animator variable to a random value.
/// It may seem silly to set it every frame, but we don't presume to
/// know when it needs to be set; just set it every frame so we always
/// have randomness in the Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class RandomizeAnimatorVariable : MonoBehaviour
{
    public string variableName = "variation";
	[Tooltip("Exclusive. A value of 5 would randomize between 0-4")]
	public int variableMaxExclusive = 5;

	Animator animator;


	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.SetInteger( variableName, Random.Range(0, variableMaxExclusive) );
	}


	private void Update()
	{
		animator.SetInteger( variableName, Random.Range(0, variableMaxExclusive) );
	}
}
