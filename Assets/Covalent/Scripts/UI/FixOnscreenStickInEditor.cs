using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Unity's "constant physical size" canvas scaler is really handy for scaling the onscreen stick on mobile,
/// but it's glitchy as all getout in editor. This script will reparent the onscreen stick to the regular
/// canvas scaler in the editor, to undo that glitchiness.
/// </summary>
public class FixOnscreenStickInEditor : MonoBehaviour
{
	public RectTransform newParent;
	public float newScale = 10.0f;


	private void Start()
	{
		if( Application.isEditor )
		{
			GetComponent<RectTransform>().SetParent( newParent, false );
			transform.localScale = Vector3.one * newScale;
		}
	}


	private void LateUpdate()
	{
		// Something is setting joystick's scale constantly... I can't be bothered to find it...
		// I'll just brute force it here
		if (Application.isEditor)
			transform.localScale = Vector3.one * newScale;
	}
}
