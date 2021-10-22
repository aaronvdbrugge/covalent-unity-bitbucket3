using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// When this object gets OnPointerClick, it'll SendMessage "OnObjectClicked" to
/// Camera.main with our own gameObject as the parameter.
/// </summary>
public class SendClicksToCamera : MonoBehaviour
{
	[Tooltip("If left blank we'll fetch Camera.main")]
	public Camera myCamera;


	void Start()
	{
		if( myCamera == null )
			myCamera = Camera.main;
	}

	public void OnMyTouchDown(MyTouch my_touch)
	{
		myCamera.SendMessage("OnObjectClicked", gameObject);

		// Also allow any other scripts on this object to respond
		SendMessage("OnThisClicked", SendMessageOptions.DontRequireReceiver);
	} 
}
