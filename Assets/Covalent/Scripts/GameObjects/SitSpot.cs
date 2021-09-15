using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A spot where players can sit. Ideally, only one can sit here at a time.
/// Nest this under a chair or bench where the user would sit. You can have
/// multiple sit spots per bench, for example.
/// </summary>
public class SitSpot : MonoBehaviour
{
    [Tooltip("Give it a child collision bound which will turn visible when the player overlaps it (to show the sit point).")]
    public Collider2D hudPoint;

    [Tooltip("This is enabled or disabled depending on if the main player is overlapping it.")]
    public SpriteRenderer hudPointVisual;

    [Tooltip("Give it another child with a collision bound that the user can tap to sit there. NOTE: set this to forward clicks to SitSpot.TapPointClicked")]
    public Collider2D tapPoint;

    [Tooltip("Finally, give it one more child which is the spot they'll return to when they stop sitting.")]
    public Transform returnPoint;



    // The sit point can only be tapped if the hud is overlapping the player first.
    bool hudOverlappingPlayer = false;


	private void Update()
	{

        // Figure out if hudPoint overlaps player...
        bool hudOverlappingPlayer  = false;

        ContactFilter2D contact_filter = new ContactFilter2D();
        contact_filter.layerMask = LayerMask.NameToLayer("player_collider");   // we only care about players overlapping us...
        Collider2D[] results = new Collider2D[10];   //size of the array determines the maximum number of results that can be returned.

        hudPoint.OverlapCollider( contact_filter, results );

		foreach( Collider2D overlap in results )
            if( overlap != null )
            {
                Player_Controller_Mobile pcm = overlap.GetComponent<Player_Controller_Mobile>();
                if( pcm != null && pcm.photonView.IsMine )  // It's us...
                    hudOverlappingPlayer  = true;    //show the place where we can sit our butt
            }


        // OK, now we can decide if hudPoint should actually be visible.
        hudPointVisual.enabled = hudOverlappingPlayer;
	}



	public void TapPointClicked()
    {

        if( hudOverlappingPlayer )   //tapping the tap point only does something if the player is close.
        {
            Debug.Log("Sit down!");
        }
    }

    

}
