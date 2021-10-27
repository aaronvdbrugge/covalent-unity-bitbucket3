using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Useful for animations, etc.
/// </summary>
public class ForwardToEvent : MonoBehaviour
{
	public UnityEvent[] events;

	public void DoForward()
	{
		events[0].Invoke();
	}

	public void DoForwardNum(int event_num)
	{
		events[event_num].Invoke();
	}
}
