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


    [Tooltip("If this is used instead, we'll play a random one from this roster")]
    public AudioSource[] multiAudioSources;

    [Tooltip("Only people in our party (usually us and our partner) can hear this!")]
    public bool partyOnly = false;

    public float minPitch = 1.0f;
    public float maxPitch = 1.0f;

    public bool changeVolume = false;
    public float minVolume = 0.5f;
    public float maxVolume = 0.5f;

    [Tooltip("If this is true, we're probably just using this to randomize pitch")]
    public bool playOnAwake = false;



    [Header("Runtime")]
    [Tooltip("Another script will have to set this when triggered, if we're using partyOnly.")]
    public int triggeredByUid = -1;





    Camera_Sound _cameraSound;


	private void Start()
	{
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();
        if( playOnAwake )
            PlaySound();
	}


    /// <summary>
    /// Sets triggeredByUid. Can be used for SendMessage.
    /// </summary>
    public void SetTriggeredByUid(int triggered_by)
    {
        triggeredByUid = triggered_by;
    }

	public void PlaySound()
    {
        if( _cameraSound.CanPlaySoundAtPosition( transform.position, partyOnly, triggeredByUid ) )    // include "partyOnly" config in this test
        {
            AudioSource audio = audioSource;
            if( multiAudioSources.Length > 0 )   // pick a random sound from multiple
                audio = multiAudioSources[ Random.Range(0, multiAudioSources.Length) ];

            audio.pitch = Random.Range(minPitch, maxPitch);

            if( changeVolume )
                audio.volume = Random.Range(minVolume, maxVolume);

            audio.Play();
        }
    }
}
