﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player "hopping" in place, also handles player hopping on and
/// off of seats / benches.
/// </summary>
public class Player_Hop : MonoBehaviourPun
{
	[Header("References")]
	[Tooltip("Nested under the player object, we can move this up and down to make it look like they're hopping.")]	
	public Transform playerVisual;
	[Tooltip("We need to move this when we're hopping into a bench etc.")]
	public Transform playerParent;

	public IsoSpriteSorting isoSpriteSorting;
	public Player_Collisions playerCollisions;
	public Player_Movement playerMovement;

	[Header("Settings")]
	public float hopTime = 0.5f;
	public float hopHeight = 5.0f;
	[Tooltip("To make us appear on top of the seat, our sorting position will be smoothly scooted by this amount.")]
	public float seatSortingBias = 2.0f;   

	[Header("Interface")]
	[Tooltip("Sets to hopTime and counts down. You could set it to 0 if you want to cancel the hop. It's 0 when we're on thr ground")]
	public float hopProgress = 0;

	float _playerVisualYOriginal;
	float _isoSpriteSortingPositionOffsetYOriginal;  // we have to modify iso sprite sorting y offset to keep it on the ground.
	float _isoSpriteSortingPositionOffsetYOriginal2;  //there are two of them

	Vector3 _hopStart;   // World position at which we started the current hop (useful for hopping to a waypoint)
	Vector3 _hopEnd;   // World position at which we should end the current hop
	float _hopStartSortingBias = 0;    // We can also lerp isometric sorting bias along the hop...
	float _hopEndSortingBias = 0;


	bool _useStartEnd = false;    //Should we use start/end or just hop in place?

	bool _enableMovementWhenDone = false;    //Should we enable colliders and movement when we finish the hop? Useful for hopping off a bench

	private void Start()
	{
		_playerVisualYOriginal = playerVisual.transform.localPosition.y;
		_isoSpriteSortingPositionOffsetYOriginal = isoSpriteSorting.SorterPositionOffset.y;
		_isoSpriteSortingPositionOffsetYOriginal2 = isoSpriteSorting.SorterPositionOffset2.y;
	}


	/// <summary>
	/// Triggers a hop in place, which is cosmetic only. The actual player remains
	/// in the same place in the game world, and only the visuals move up and down.
	/// </summary>
	[ContextMenu("HopInPlace")]
	public void HopInPlace()
	{
		// NOTE: we should move this sound effect to HopInPlaceRPC(), but it would need to detect if the player is close enough to actually
		// play the sound. For now, we'll only play a sound if it's the controlled player hopping
		Camera.main.SendMessage("PlaySound", "hop");

		if( GetSittingOn() == null )
			this.photonView.RPC("HopInPlaceRPC", RpcTarget.All);
		else  // "Hop in place" while sitting on something! This counts as getting off the seat.
			HopToSeat( null );
	}

	[PunRPC]
	public void HopInPlaceRPC()
	{
		hopProgress = hopTime;  //starts the hop in Update
		_useStartEnd = false;     //no start/end point, just hop in place
	}



	/// <summary>
	/// Get the object we're sitting on, or null if N/A. This is retrieved from custom properties.
	/// The actual SitPoint can be retrieved by SitPoint.byUid(GetSittingOn()) ,
	/// however, be wary that we may not have actually instantiated the SitPoint yet!!
	/// </summary>
	public string GetSittingOn()
	{
		if( !photonView.Owner.CustomProperties.ContainsKey("SittingOn") )
			return null;
		return (string)photonView.Owner.CustomProperties["SittingOn"];
	}


	/// <summary>
	/// Will arc over to a SitPoint and plant our butt in it, turning off all colliders and
	/// relinquishing walking control until we tap the screen again, at which point we'll hop
	/// back down onto the SitPoint's returnPoint.
	/// 
	/// You can call this function with null to get off of a seat.
	/// </summary>
	public void HopToSeat( SitPoint seat )
	{
		if( seat == null || SitPoint.GetOccupiedBy( seat.uid ) == -1 )   //we're just getting off our seat, or nobody's sitting there
		{
			Camera.main.SendMessage("PlaySound", "hop");
			this.photonView.RPC("HopToSeatRPC", RpcTarget.All, new object[] { seat != null ? seat.uid : null });
		}
		//else   // can't sit there! already occupied
		//	Camera.main.SendMessage("PlaySound", "no_can_do");
		//This should never happen anyway, because we disable the ProximityInteractable as soon as someone is sitting there.
	}


	/// <summary>
	/// NOTE: null is allowed, this means get off the seat.
	/// </summary>
	[PunRPC]
	public void HopToSeatRPC( string seat_uid )
	{
		// Assuming our RPCs happen in consistent order for all clients, we should get reliable replication here
		// as to who gets the seat if they both tap at once.
		if( seat_uid != null )  // getting on a seat
		{
			SitPoint seat = SitPoint.ByUidOrNull(seat_uid);

			if( SitPoint.TryOccupy( seat_uid, photonView.Owner.ActorNumber ) )
			{
				if( photonView.IsMine )
					Debug.Log("Seated in seat: " + seat_uid );

				hopProgress = hopTime;  //starts the hop in Update
				_hopStart = transform.position;   //record start position, so we can arc from here to the seat.
				_hopEnd = _hopStart;
				if( seat != null )   // might not be instantiated!
					_hopEnd = seat.transform.position;    //hop onto the seat....

				_hopStartSortingBias = 0;   // ground bias
				_hopEndSortingBias = seatSortingBias;   // ...lerp it up to the seat bias

				_useStartEnd = true;   //uses previous two members for the hop
			}
		}
		else   // Vacating a seat
		{
			string sitting_on = GetSittingOn();
			SitPoint seat = SitPoint.ByUidOrNull( sitting_on );

			SitPoint.Vacate( sitting_on, photonView.Owner.ActorNumber );
			
			if( photonView.IsMine )
				Debug.Log("Vacating seat.");

			hopProgress = hopTime;
			_hopStart = transform.position;
			_hopEnd = _hopStart;
			if( seat != null )  // may not be instantiated yet!
				_hopEnd = seat.returnPoint;    // Seat tells us where to hop down.

			_hopStartSortingBias = seatSortingBias;   
			_hopEndSortingBias = 0;   // Lerp from seat bias to ground bias

			_useStartEnd = true;

			_enableMovementWhenDone = true;  //we'll wait till the end of the hop to re-enable movement.
		}
	}



	private void Update()
	{
		string sitting_on = GetSittingOn();
		if( !string.IsNullOrEmpty(sitting_on))   // may as well stick this in update. Remember that a player may already be sitting on something the instant they're instantiated.
		{
			playerCollisions.EnableColliders( false );   // Turn colliders off until we're done sitting.
			playerMovement.movementEnabled = false;    // Disable movement altogether.
			if( hopProgress <= 0 )  //not even hopping, so stay put on the bench...
			{
				SitPoint seat = SitPoint.ByUidOrNull( sitting_on );
				if( seat != null )  // it might not be instantiated
					playerParent.transform.position = seat.transform.position;
			}
		}


		if( hopProgress > 0 )
		{
			hopProgress = Mathf.Max( hopProgress - Time.deltaTime, 0 );
			float hop_norm = 1 - hopProgress / hopTime;   // goes from 0 to 1 as hop progresses


			// We want a parabola that goes from y=0 to 1 to 0, from x=0 to y=1.
			// Should be:    y = -(2x - 1)^2 +1
			//    https://www.desmos.com/calculator/qphzqwrput
			float hop_parabolic = -Mathf.Pow(2 * hop_norm - 1, 2) + 1;

			float to_add = hop_parabolic * hopHeight;   // de-normalize

			// We can now use this value to move the visual up and down.
			playerVisual.transform.localPosition = new Vector3(
				playerVisual.transform.localPosition.x,
				_playerVisualYOriginal + to_add,
				playerVisual.transform.localPosition.z
				);

			//Note that if this was the last hopProgress update, we should have set it perfectly back down at _playerVisualYOriginal.

			// Modify sprite sorting so it stays on the ground
			isoSpriteSorting.SorterPositionOffset.y = _isoSpriteSortingPositionOffsetYOriginal - to_add;   // reverse direction
			isoSpriteSorting.SorterPositionOffset2.y = _isoSpriteSortingPositionOffsetYOriginal2  - to_add;


			// We've scooted the player visual locally to make it look like we're hopping.
			// However, if we're doing _useStartEnd, we'll lerp the entire transform as well
			// to make us arc onto a chair or something.
			if( _useStartEnd )
			{
				playerParent.transform.position = Vector3.Lerp( _hopStart, _hopEnd, hop_norm );

				// Lerp bias, this makes sure we appear on top of the seat.
				isoSpriteSorting.SorterPositionOffset.y -= Mathf.Lerp( _hopStartSortingBias, _hopEndSortingBias, hop_norm );
				isoSpriteSorting.SorterPositionOffset2.y -= Mathf.Lerp( _hopStartSortingBias, _hopEndSortingBias, hop_norm );
			}


			if( hopProgress <= 0  )  // end of hop.
			{
				if( _enableMovementWhenDone )  // They were probably hopping off of a bench, re-enable colliders and player movement
				{
					_enableMovementWhenDone = false;
					playerCollisions.EnableColliders( true );
					playerMovement.movementEnabled = true;  
				}

				if( !string.IsNullOrEmpty(sitting_on) && photonView.IsMine )   // They hopped onto a seat, can play a sound effect. For now, only Mine can do it
					Camera.main.SendMessage("PlaySound", "sit_down");
			}
		}
	}



}
