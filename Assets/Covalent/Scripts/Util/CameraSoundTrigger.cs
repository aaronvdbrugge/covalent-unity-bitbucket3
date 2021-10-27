using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Convenience script for triggering sounds from animation
/// </summary>
public class CameraSoundTrigger : MonoBehaviour
{
    public bool ignorePosition = false;

    public void PlaySound(string name)
    {
        if( !ignorePosition )
            Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( name, transform.position );
        else
            Camera.main.GetComponent<Camera_Sound>().PlaySound( name );
    }
}
