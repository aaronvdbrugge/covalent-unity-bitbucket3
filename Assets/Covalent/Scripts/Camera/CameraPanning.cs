using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This script handles panning the camera around.
/// Right now it tightly follows the player, but will pan
/// if they sit on a SitPoint that has a CameraPanOffset.
/// </summary>
public class CameraPanning : MonoBehaviour
{
	[Tooltip("Set during runtime from Player_Controller_Mobile.Start")]
	public Player_Controller_Mobile target;

	[Tooltip("For now, we'll use the \"quick and easy\" method of smoothing out the movement (just adds this amount of delta to positon every fixed update)")]
	public float panningResponsivenessRatio = 0.05f;

	[Tooltip("Camera keeps a constant Z position")]
	public float constZ = -10;


	[Tooltip("After getting off a Sitpoint, we'll continue to use smoothing for this amount of time, but gradually increase respionsiveness ratio.")]
	public float smoothCooldownTime = 1.0f;


	[Tooltip("Current zoom level.")]
	public float zoom = 1.0f;


	/// <summary>
	/// We'll check if any of these have wantsPan on.
	/// Note that they must add themselves to this list on Start.
	/// </summary>
	public List<CameraPanArea> cameraPanAreas;   


	float _smoothCooldown = 0.0f;    // from 1 to 0
	float _orthographicSizeOriginal;   // used for zooming


	private void Start()
	{
		_orthographicSizeOriginal = Camera.main.orthographicSize;
	}


	/// <summary>
	/// NOTE: my method of camera panning here might be inconsistent between framerates (see: Vector2 new_pos = pos + (pan_pos - pos) * panningResponsivenessRatio; )
	/// It would work in FixedUpdate, but that results in chunky camera behaviour.
	/// It looks fine for now, and will probably look fine regardless of frame rate (just different), so I'll just leave it for now
	/// </summary>
	private void LateUpdate()
	{
		if( target == null )
			return;

		bool do_pan = false;   // true if anything is asking us for a camera pan
		Vector2 target_pos = Vector2.zero;
		float target_zoom = 0;
		


		// See if any CameraPanAreas want pan...
		foreach( var campan in cameraPanAreas )
			if( campan.wantsPan )
			{
				do_pan = true;
				target_pos = campan.targetPosWorld;
				target_zoom = campan.targetZoom;
			}


		SitPoint sit = target.playerHop.GetSitPoint();
		if( sit != null && sit.useCameraOffset == true)
		{	
			// Has camera offset!
			// "Lazily" move over to where the sit point's pan offset is.
			do_pan = true;
			target_pos = sit.GetCameraOffsetWorld();
			target_zoom = sit.cameraZoom;

			_smoothCooldown = 1;   // in case we get off the sitpoint soon
		}



		if( do_pan )
		{
			// Has camera offset!
			// "Lazily" move over to where the sit point's pan offset is.
			Vector2 pos = transform.position;
			Vector2 new_pos = pos + (target_pos - pos) * panningResponsivenessRatio;
			transform.position = new Vector3(new_pos.x, new_pos.y, constZ );

			zoom += (target_zoom - zoom) * panningResponsivenessRatio;   // use same method for zoom

			_smoothCooldown = 1;   // in case we get off the sitpoint soon
		}
		else
		{
			if( _smoothCooldown <= 0 )
			{
				// Just follow the player spot-on
				transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10 );
			}
			else   // have to keep smoothing the movement, we just got out of a chair
			{
				float responsiveness = Mathf.Lerp(1.0f, panningResponsivenessRatio, _smoothCooldown); //gradually increase responsiveness so we get a clean break

				Vector3 pos = transform.position;
				Vector2 new_pos = pos + (target.transform.position - pos) * responsiveness;   
				transform.position = new Vector3( new_pos.x, new_pos.y, constZ );

				zoom += (1.0f - zoom) * responsiveness;   // use same method for zoom

				//Decrease smoothcooldown from 1 to 0
				_smoothCooldown = Mathf.Max(0.0f, _smoothCooldown - Time.fixedDeltaTime / smoothCooldownTime);
			}
		}


		// Set camera zoom as a ratio of original orthographic size
		Camera.main.orthographicSize = _orthographicSizeOriginal / zoom;
	}


}
