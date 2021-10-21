using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Sends a message to the UI that we are reading a sign in the game world.
/// Reacts to OnThisClicked message, which can be sent by ProximityInteractable or SendClickstoCamera.
/// </summary>
public class ReadableSign : MonoBehaviour
{
	[Tooltip("For now, we'll just input this in the inspector. Would it be fetched from the backend someday?")]
	[TextArea(4,20)]
	public string signText; 

	public void OnThisClicked()
	{
		Camera.main.SendMessage("ReadSignText", signText);
	}
}
