using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Convenience script for triggering our own AudioSource from an Animation.
/// Allows pitch variance.
/// </summary>
public class SelfSoundTrigger : MonoBehaviour
{
	public AudioSource audioSource;
    public float minPitch = 1.0f;
    public float maxPitch = 1.0f;



    Camera_Sound _cameraSound;


	private void Start()
	{
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();
	}

	public void PlaySound()
    {
        if( _cameraSound.CanPlaySoundAtPosition( transform.position ) )
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.Play();
        }
    }
}
