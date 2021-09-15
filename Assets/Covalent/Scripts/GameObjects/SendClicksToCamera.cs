using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// When this object gets OnPointerClick, it'll SendMessage "OnObjectClicked" to
/// Camera.main with our own gameObject as the parameter.
/// </summary>
public class SendClicksToCamera : MonoBehaviour, IPointerClickHandler
{
	[Tooltip("If left blank we'll fetch Camera.main")]
	public Camera camera;

	void Start()
	{
		if( camera == null )
			camera = Camera.main;
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		camera.SendMessage("OnObjectClicked", gameObject);
	} 
}
