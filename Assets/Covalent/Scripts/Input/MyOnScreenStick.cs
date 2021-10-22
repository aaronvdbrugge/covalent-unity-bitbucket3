﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;



/// <summary>
/// Used to send out the joystick diffs
/// </summary>
[System.Serializable]
public class MyVector2Event: UnityEvent<Vector2>
{
}

/// <summary>
/// Unity's on screen stick isn't really that great for what we want.
/// This stick is more designed to stay in a fixed position, allow tapping anywhere inside
/// the stick area, and immediately snap to the position they put down their finger.
/// 
/// Also, don't bother going through Unity's dumb input system, just feed joystick values directly.
/// </summary>
public class MyOnScreenStick : MonoBehaviour
{
    [Tooltip("This will be sent out when the joystick even occurs, link it to whatever you want")]
    public MyVector2Event onJoystickValues;

    [Tooltip("A non-raycast-recieving visual dot that's just to show where the stick is positioned currently.")]
    public RectTransform joystickVisual;

    [Tooltip("Just a pointer to the UI camera we're displayed by... need it for calculations. I think you can leave it null for overlay")]
    public Camera uiCamera;



    bool dragging = false;

    MyTouch myTouch = null;   // Set in OnMyTouchDown, then we keep track of it



    void Update()
    {
        if( myTouch != null )
        {
            if( myTouch.touch.phase == UnityEngine.TouchPhase.Moved )
                DoDrag( myTouch );
            else if( myTouch.touch.phase == UnityEngine.TouchPhase.Ended || myTouch.touch.phase == UnityEngine.TouchPhase.Canceled )
            {
                TouchReleased(myTouch);
                myTouch = null;   // done with it
            }
        }
    }


    void OnMyTouchDown( MyTouch my_touch )
    {
        myTouch = my_touch;

        // this means it was within the rectangular bounds, but ensure it was actually within the smaller circular bounds.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, my_touch.touch.position, uiCamera, out var position);
        var delta = position - m_StartPos;   // Difference between touch position and the center of this stick

        if( delta.magnitude <= radius )
            dragging = true;   //we're inside the circle.

        // Move onto DoDrag logic immediately.
        DoDrag( my_touch );        
    }


   

    void DoDrag(MyTouch my_touch)
    {   
        if( !dragging )   //they touched down outside the circle.
            return;


        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, my_touch.touch.position, uiCamera, out var position);
        var delta = position - m_StartPos;   // Difference between touch position and the center of this stick

        // Move the visual dot around
        delta = Vector2.ClampMagnitude(delta, movementRange);
        if( joystickVisual )
            joystickVisual.anchoredPosition = m_StartPos + delta;


        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);

        onJoystickValues.Invoke( newPos );   //sends out a value between -1 and 1, with 0 being center.
    }

    void TouchReleased(MyTouch my_touch)
    {
        if( dragging & joystickVisual )   // put it back in the center
            joystickVisual.anchoredPosition = m_StartPos;
        dragging = false;
        onJoystickValues.Invoke( Vector2.zero );   //sends out a value between -1 and 1, with 0 being center.
    }

    private void Start()
    {
        rectTransform = (RectTransform)transform;

        Vector3[] local_corners = new Vector3[4];
        rectTransform.GetLocalCorners(local_corners);



        m_StartPos = (local_corners[2] + local_corners[0]) / 2;  //remember center
        radius = (local_corners[2].x - local_corners[0].x) / 2;  // assume it's a square with a circle sprite in it, so the width is its diameter.
    }

    public float movementRange
    {
        get => m_MovementRange;
        set => m_MovementRange = value;
    }

    [SerializeField]
    private float m_MovementRange = 50;
    private Vector2 m_StartPos;
    private float radius;   //calculate this from local corners


    private RectTransform rectTransform;
}
