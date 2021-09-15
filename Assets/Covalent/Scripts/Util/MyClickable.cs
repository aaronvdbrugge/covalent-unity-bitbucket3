using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Just a simple OnPointerDown handler
/// </summary>
public class MyClickable : MonoBehaviour, IPointerClickHandler
{
	public UnityEvent onClick;

	public void OnPointerClick(PointerEventData pointerEventData)
	{
        //Debug.Log("OnPointerDown: " + Time.time );

		onClick.Invoke();
	}
}
