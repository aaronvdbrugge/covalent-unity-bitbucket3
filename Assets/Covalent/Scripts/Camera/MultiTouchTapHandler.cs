using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// This object should stay consistent and updated for the life of the touch, as long as MultiTouchTapHandler is alive to update it.
/// </summary>
public class MyTouch
{
    public Touch touch;   // struct
}



/// <summary>
/// Forwards taps to objects that care about them, and doesn't glitch out on multi touch, unlike Unity's lousy IPointerClickHandler.
/// 
/// Sends the OnMyTouchDown( Touch ) message to any colliders tapped.
/// </summary>
public class MultiTouchTapHandler : MonoBehaviour
{
    [Tooltip("Layers that want OnMyTouchDown messages")]
    public LayerMask layerMask;


    [Tooltip("Any canvas raycasters that can block raycasts to the game. We'll check them first")]
    public GraphicRaycaster[] graphicRaycasters;

    [Tooltip("Needed for graphic raycasters")]
    public EventSystem eventSystem;


    /// <summary>
    /// Keyed by FingerId. We'll have to keep this up to date ourselves
    /// </summary>
    Dictionary<int, MyTouch> myTouches = new Dictionary<int, MyTouch>();   


    /// <summary>
    /// We'll update this structure to try and behave like a real touch would...
    /// </summary>
    Touch fakeTouch = new Touch();

    


	private void Start()
	{
		fakeTouch.phase = TouchPhase.Ended;
	}

	void Update()
    {
        // EDITOR SIMULATION:
        // Update our fake touch
        if( (fakeTouch.phase == TouchPhase.Began || fakeTouch.phase == TouchPhase.Moved) && Input.GetMouseButton(0) )   // touch held
        {
            fakeTouch.position = Input.mousePosition;
            fakeTouch.phase = TouchPhase.Moved;
        }
        else if( (fakeTouch.phase == TouchPhase.Began || fakeTouch.phase == TouchPhase.Moved) && !Input.GetMouseButton(0) )   // touch released
            fakeTouch.phase = TouchPhase.Ended;



        // Update touches currently in our dictionary
        List<int> to_remove = new List<int>();
        foreach( var kvp in myTouches )
        {
            if( !Application.isEditor )
            {
                foreach( Touch touch in Input.touches )
                    if( touch.fingerId == kvp.Value.touch.fingerId )
                        kvp.Value.touch = touch;   // update class with new data we found
            }
            else    // editor simulation
                if( kvp.Value.touch.fingerId == fakeTouch.fingerId )
                    kvp.Value.touch = fakeTouch;   // update class with new simulated touch

            if( kvp.Value.touch.phase == TouchPhase.Ended || kvp.Value.touch.phase == TouchPhase.Canceled )
                to_remove.Add(kvp.Key);   // We don't need to update this anymore!
        }

        foreach( int key in to_remove )
            myTouches.Remove( key );    // NOTE: the MyTouch class will still keep trucking along in memory, until whatever component grabbed it in OnMyTouchDown is done with it.



        // Process new REAL touches
        if( !Application.isEditor )
        {
            foreach(Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                    ProcessNewTouch( touch );
            }
        }
        else    // Simulate new touches in editor!
        {
            if( Input.GetMouseButton(0) && fakeTouch.phase == TouchPhase.Ended )   // simulate touches in editor
            {
                if( Input.GetKey(KeyCode.LeftControl) )
                {
                    int _DEBUG = 0;
                }

                fakeTouch.phase = TouchPhase.Began;
                fakeTouch.position = Input.mousePosition;
                ProcessNewTouch( fakeTouch );
            }
        }

    }

    void ProcessNewTouch( Touch touch )
    {
        if( myTouches.ContainsKey( touch.fingerId ) )   // already has this finger id for some reason, just update the existing struct
            myTouches[touch.fingerId].touch = touch;
        else
            myTouches[touch.fingerId] = new MyTouch{touch = touch };   // create new MyTouch to hold the struct


        if( !AreGraphicRaycastersBlockingTouch( touch ) )
        {
			ContactFilter2D contact_filter = new ContactFilter2D();
			contact_filter.SetLayerMask( layerMask );   // only looking for player colliders
            RaycastHit2D[] results = new RaycastHit2D[8];

            /*
            int count = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touch.position), Vector2.zero, contact_filter, results);

            for( int i=0; i<count; i++)
                results[i].collider.SendMessage("OnMyTouchDown", myTouches[touch.fingerId], SendMessageOptions.DontRequireReceiver);  // send the MyTouch class, which will auto update!
            */

            var coll = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(touch.position), layerMask);
            if( coll != null )
                coll.SendMessage("OnMyTouchDown", myTouches[touch.fingerId], SendMessageOptions.DontRequireReceiver); 

        }
    }




    /// <summary>
    /// Returns true if the given screen position is blocked by UI graphics and can't click through to the game.
    /// Will actually send the proper message if it does hit something.
    /// </summary>
    bool AreGraphicRaycastersBlockingTouch(Touch touch)
    {
        foreach( var gr in graphicRaycasters )
        {
            var ped = new PointerEventData(eventSystem);
            ped.position = touch.position;
            List<RaycastResult> results = new List<RaycastResult>();
            gr.Raycast(ped, results);

            if( results.Count > 0 )  // Something is blocking the position!
            {
                foreach (var result in results )  // I'm not sure why it's a list... can't it only hit one thing?
                    result.gameObject.SendMessage("OnMyTouchDown", myTouches[touch.fingerId], SendMessageOptions.DontRequireReceiver);

                return true;
            }
        }

        return false;
    }
}
