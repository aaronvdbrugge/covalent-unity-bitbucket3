using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Just a simple OnMyTouchDown handler
/// </summary>
public class MyClickable : MonoBehaviour
{
	public UnityEvent onClick;

	public void OnMyTouchDown(MyTouch touch)
	{
        //Debug.Log("OnPointerDown: " + Time.time );

		onClick.Invoke();
	}
}
