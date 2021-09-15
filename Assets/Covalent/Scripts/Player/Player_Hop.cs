using Photon.Pun;
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
	public IsoSpriteSorting isoSpriteSorting;

	[Header("Settings")]
	public float hopTime = 0.5f;
	public float hopHeight = 5.0f;


	[Header("Interface")]
	[Tooltip("Sets to hopTime and counts down. You could set it to 0 if you want to cancel the hop. It's 0 when we're on thr ground")]
	public float hopProgress = 0;


	float _playerVisualYOriginal;
	float _isoSpriteSortingPositionOffsetYOriginal;  // we have to modify iso sprite sorting y offset to keep it on the ground.
	float _isoSpriteSortingPositionOffsetYOriginal2;  //there are two of them


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


        this.photonView.RPC("HopInPlaceRPC", RpcTarget.All);
	}

	[PunRPC]
	public void HopInPlaceRPC()
	{
		hopProgress = hopTime;  //starts the hop in Update
	}



	private void Update()
	{
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
		}
	}
}
