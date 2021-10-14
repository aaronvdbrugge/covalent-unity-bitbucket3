using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Convenience script for triggering sounds from animation
/// </summary>
public class CameraSoundTrigger : MonoBehaviour
{
    public void PlaySound(string name)
    {
        Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( name, transform.position );
    }
}
