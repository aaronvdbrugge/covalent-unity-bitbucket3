using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When our Y coordinate surpasses that of the target transform,
/// we hide a child object (or other object)
/// </summary>
public class HideChildAboveTransform : MonoBehaviour
{
    public GameObject objectToHide;
    public Transform target;


    void LateUpdate()
    {
        if( transform.position.y > target.position.y )   // went above target. hide condition is met
            objectToHide.SetActive( false );
        else
            objectToHide.SetActive( true );
    }
}
