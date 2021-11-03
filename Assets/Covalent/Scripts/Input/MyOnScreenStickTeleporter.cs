using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Attach this to an invisible Raycast target which encompasses
/// the area the stick is allowed to teleport to, on tap.
/// </summary>
public class MyOnScreenStickTeleporter : MonoBehaviour
{
    public MyOnScreenStick myOnScreenStick;



    Vector2 _myOnScreenStickLocalPosOriginal;

    /// <summary>
    /// When OnMytouchDown is called, we have to keep track of the
    /// touch class. It'll be updated automatically, so keep
    /// checking its state.
    /// </summary>
    MyTouch _myTouch;


	private void Start()
	{
		_myOnScreenStickLocalPosOriginal = myOnScreenStick.transform.localPosition;
	}




	public void OnMyTouchDown( MyTouch my_touch )
    {
        _myTouch = my_touch;

        // Teleport the on screen stick to the touch point...
        myOnScreenStick.transform.position = (Vector2)myOnScreenStick.uiCamera.ScreenToWorldPoint( my_touch.touch.position );

        // Forward the touch to the stick, so it starts being used at this point
        myOnScreenStick.OnMyTouchDown( my_touch );
    }


	private void Update()
	{
		if( _myTouch != null )
            if( _myTouch.touch.phase == TouchPhase.Canceled || _myTouch.touch.phase == TouchPhase.Ended )
            {
                _myTouch = null;   // don't need it anymore
                myOnScreenStick.transform.localPosition = _myOnScreenStickLocalPosOriginal;   // Reset joystick back to center.
            }
	}

}
