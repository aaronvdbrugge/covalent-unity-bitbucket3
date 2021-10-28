using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Contains logic relevant to the player's "partner," the other
/// player that they are supposed to have joined with and be voice chatting
/// with.
/// </summary>
public class Player_Partner : MonoBehaviour
{
	[Tooltip("So we can reference kippoUserId")]
    public Player_Controller_Mobile playerControllerMobile;   

	[Tooltip("An extra highlight we give to our player, and the partner player.")]
	public GameObject activateIfMineOrPartner;


	bool _initalized = false;

	public void FixedUpdate()
	{
		if( !_initalized )
		{
			_initalized = true;


			// Do this here so we can be sure partnerId is set.
			if( playerControllerMobile.photonView.IsMine || playerControllerMobile.kippoUserId == PlayerPrefs.GetInt("partnerId", -1) )
				activateIfMineOrPartner.SetActive(true);
		}
	}


	/// <summary>
	/// Gets our partner player, or null if they don't exist.
	/// </summary>
	public Player_Controller_Mobile GetPartner()
    {
        int partner_id = PlayerPrefs.GetInt("partnerId", -1);
        if( !Player_Controller_Mobile.playersByKippoId.ContainsKey( partner_id ) )
            return null;
        return Player_Controller_Mobile.playersByKippoId[partner_id];
    }
    
}
