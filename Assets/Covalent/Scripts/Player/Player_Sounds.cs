using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For any complicated audio in the player, like ice skating loop
/// </summary>
public class Player_Sounds : MonoBehaviour
{
	[Header("References")]
	[Tooltip("we refer to PlayerAnimations to see if we're skating")]
	public Player_Animations playerAnimations;
	
	[Tooltip("Modify sound by speed")]
	public Player_Movement playerMovement;

	[Tooltip("Can't play ice skating sound when they're in the air!")]
	public Player_Hop playerHop;

	public AudioSource skatingSound;

	
	[Header("Settings")]
	public float skatingMinPitch = 0.5f;
	public float skatingMaxPitch = 2.0f;
	public float skatingMinVolume = 0.0f;
	public float skatingMaxVolume = 0.25f;
	public float skatingMinSpeed = 0.5f;




	Camera_Sound _cameraSound;  //cached

	private void Start()
	{
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();
	}


	public void Update()
	{
		bool sound_enabled = _cameraSound.CanPlaySoundAtPosition( transform.position );

		if( !sound_enabled )
		{
			skatingSound.Stop();
		}
		else
		{
			if( playerAnimations.skating && playerHop.hopProgress <= 0 ) 
			{
				float speed = playerMovement.body.velocity.magnitude;
				bool skate_sound = speed > skatingMinSpeed;
				if( !skatingSound.isPlaying && skate_sound)
					skatingSound.Play();
				else if (!skate_sound)
					skatingSound.Stop();
				float lerp_amt = speed / playerMovement.maxSpeed;
				skatingSound.pitch = Mathf.Lerp(skatingMinPitch, skatingMaxPitch, lerp_amt);
				skatingSound.volume = Mathf.Lerp(skatingMinVolume, skatingMaxVolume, lerp_amt);
			}
			else
				skatingSound.Stop();
		}
	}


}
