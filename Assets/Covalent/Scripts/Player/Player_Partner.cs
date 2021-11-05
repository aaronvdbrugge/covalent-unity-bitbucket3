using System.Collections;
using System.Collections.Generic;
using TMPro;
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

	[Tooltip("Text is a different color if it's us or our partner.")]
	public TMP_Text colorTextIfMineOrPartner;

	[Tooltip("Applies to colorTextIfMineOrPartner")]
	public Color partnerTextColor;


	bool _initalized = false;

	public void FixedUpdate()
	{
		if( !_initalized )
		{
			_initalized = true;


			// Do this here so we can be sure partnerId is set.
			if( playerControllerMobile.photonView.IsMine || playerControllerMobile.kippoUserId == Dateland_Network.partnerPlayer )
			{
				activateIfMineOrPartner.SetActive(true);
				colorTextIfMineOrPartner.color = partnerTextColor;
			}
		}
	}


	/// <summary>
	/// Gets our partner player, or null if they don't exist.
	/// </summary>
	public Player_Controller_Mobile GetPartner()
    {
        int partner_id = Dateland_Network.partnerPlayer;
        if( !Player_Controller_Mobile.playersByKippoId.ContainsKey( partner_id ) )
            return null;
        return Player_Controller_Mobile.playersByKippoId[partner_id];
    }
    
}
