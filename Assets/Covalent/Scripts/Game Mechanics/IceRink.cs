using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes player movement and visuals when they enter the rink trigger.
/// </summary>
public class IceRink : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		Player_Controller_Mobile plr = collision.gameObject.GetComponent<Player_Controller_Mobile>();
		if( plr )
		{
			plr.playerAlternateMovements.currentMovement = 1;   // ice rink movement
			plr.playerAnimations.SetIceSkates(true);   // put on visual ice skates attachment
		}
	}


	private void OnTriggerExit2D(Collider2D collision)
	{
		Player_Controller_Mobile plr = collision.gameObject.GetComponent<Player_Controller_Mobile>();
		if( plr )
		{
			plr.playerAlternateMovements.currentMovement = -1;   // back to default movement
			plr.playerAnimations.SetIceSkates(false);   // remove visual ice skates attachment
		}
	}
}
