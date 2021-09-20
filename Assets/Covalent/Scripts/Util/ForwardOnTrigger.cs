using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Forwards a few OnTrigger events to another object (probably parent)
/// </summary>
public class ForwardOnTrigger : MonoBehaviour
{
	public GameObject forwardTo;

	[Tooltip("Only send what we need...")]
	public bool doOnTriggerEnter = false;
	public bool doOnTriggerExit = false;

	[Tooltip("It probably doesn't matter, but due to possible SendMessage performance, by default we avoid sending it every frame.")]
	public bool doOnTriggerStay = false;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if( doOnTriggerEnter )
			forwardTo.SendMessage("OnTriggerEnter2D", other);
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if( doOnTriggerExit )
			forwardTo.SendMessage("OnTriggerExit2D", other);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		if( doOnTriggerStay )
			forwardTo.SendMessage("OnTriggerStay2D", other);
	}
}
