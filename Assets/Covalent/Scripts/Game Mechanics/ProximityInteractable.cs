using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;




/// <summary>
/// When the controlled player comes close to this spot, a hudPoint will appear.
/// If they tap the hudPoint, we'll do Camera.main.SendMessage("OnObjectClicked", gameObject);
/// </summary>
public class ProximityInteractable : MonoBehaviour
{
    [Tooltip("Give it a child collision bound which will turn visible when the player overlaps it (to show the sit point).")]
    public Collider2D hudPoint;

    [Tooltip("This is enabled or disabled depending on if the main player is overlapping it.")]
    public SpriteRenderer hudPointVisual;

    [Tooltip("Give it another child with a collision bound that the user can tap to sit there. NOTE: set this to forward clicks to SitSpot.TapPointClicked")]
    public Collider2D tapPoint;

    [Header("Interface")]
    [Tooltip("If this is false we'll hide the visual and disable our colliders.")]
    public bool tapAllowed = true;



    // The tap point can only be tapped if the hud is overlapping the player first.
    bool _hudOverlappingPlayer = false;
    

	private void Update()
	{
        if( tapAllowed )
        {
            hudPoint.enabled = true;
            tapPoint.enabled = true;  //these may get disabled if tapAllowed is false

            // Figure out if hudPoint overlaps player...
            _hudOverlappingPlayer  = false;

            ContactFilter2D contact_filter = new ContactFilter2D();
            contact_filter.layerMask = LayerMask.NameToLayer("player_collider");   // we only care about players overlapping us...
            Collider2D[] results = new Collider2D[10];   //size of the array determines the maximum number of results that can be returned.

            hudPoint.OverlapCollider( contact_filter, results );

		    foreach( Collider2D overlap in results )
                if( overlap != null )
                {
                    Player_Controller_Mobile pcm = overlap.GetComponent<Player_Controller_Mobile>();
                    if( pcm != null && pcm.photonView.IsMine )  // It's us...
                        _hudOverlappingPlayer  = true;    //show the place where we can sit our butt
                }
        

            // OK, now we can decide if hudPoint should actually be visible.
            hudPointVisual.enabled = _hudOverlappingPlayer;
        }
        else
        {
            hudPoint.enabled = false;
            tapPoint.enabled = false;
            hudPointVisual.enabled = false;    //disable everything if tap not allowed.
        }
	}

	public void TapPointClicked()
    {
        if( _hudOverlappingPlayer )   //tapping the tap point only does something if the player is close.
        {
            Camera.main.SendMessage("OnObjectClicked", gameObject);

            // Also allow any other scripts on this object to respond
            SendMessage("OnThisClicked", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    /// <summary>
    /// Message, can be received from SitPoint
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        tapAllowed = interactable;
    }

}
