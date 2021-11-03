using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// Handles animations for the player.
/// </summary>
public class Player_Animations : MonoBehaviour
{
    [Header("References")]
    public Player_Collisions playerCollisions;
    public Player_Movement playerMovement;
    public Player_Hop playerHop;

    public SkeletonMecanim skeletonMecanim;

    public Animator anim;

    public SpriteRenderer splash_feet;

    [Tooltip("A transform that you don't want to be flipped horizontally (will counter-flip to undo the flip)")]
    public Transform dontFlip;



    [Tooltip("We'll hide this if hideVisuals is true.")]
    public MeshRenderer meshRenderer;

    [Tooltip("We'll hide this if hideVisuals is true.")]
    public GameObject shadowParent;


    [Tooltip("Consumed value for bug safety, needs to be set every FixedUpdate if you want to hide the visuals.")]
    public bool hideVisuals = false;


    [Header("Info")]
    [Tooltip("Will be set dynamically based on playerMovement")]
    public bool horizontalFlip = false;


    public bool skating{get; private set; }



    bool _hidShadow = false;
	private void Update()
	{
        if( !_hidShadow ) // NOTE: putting this in Start() is too soon apparently
            skeletonMecanim.skeleton.SetAttachment("shadow", null );   // We do not need the shadow from animation!! Position it procedurally instead

        // Decide whether we should splash
        if (playerCollisions.onBeach && playerMovement.IsWalking() )
            splash_feet.enabled = true;
        else if (playerCollisions.onBeach && !playerMovement.IsWalking() )
            splash_feet.enabled = false;
        else if (!playerCollisions.onBeach && splash_feet.enabled)
            splash_feet.enabled = false;


        
        // If we're sitting somewhere, get the SitPoint.
        // Special care is needed, because if it's another player, we may not have Instantiated the SitPoint.
        string seat_uid = playerHop.GetSittingOn();
        bool sitting = !string.IsNullOrEmpty(seat_uid);
        SitPoint seat = SitPoint.ByUidOrNull( seat_uid );

        // Horizontal flip.
        if( playerMovement.GetVelocity().x > 0.01f )
            horizontalFlip = false;
        else if( playerMovement.GetVelocity().x < -0.01f )
            horizontalFlip = true;

        // If we're sitting in a seat, defer to SitPoint for horizontal flip.
        if( seat != null && playerHop.hopProgress <= 0 )
            horizontalFlip = !seat.faceRight;


        float hflip_mult = horizontalFlip ? 1.0f : 0.0f;   //makes our math here a little easier...
        transform.localRotation = new Quaternion(0, hflip_mult*180, 0, 0);
        dontFlip.localRotation = new Quaternion( 0, hflip_mult*180, 0, 0);    // player names etc will have to be un-flipped

        // Relay proper info to Animator
        anim.SetBool("walking", playerMovement.IsWalking( skating ? 0.5f : 0.01f) );   // "waking" threshold is different for skating
        anim.SetBool("skating", skating );
        anim.SetBool("hopping", playerHop.hopProgress > 0 );
        anim.SetBool("sitting", sitting && playerHop.hopProgress <= 0 );   // only start sitting once we've finished out hop.
	}

	private void FixedUpdate()
	{
		if( hideVisuals )
        {
            hideVisuals = false;   // consume the value. it must be set every frame
            meshRenderer.enabled = false;
            shadowParent.SetActive(false);
        }
        else
        {
            meshRenderer.enabled = true;
            shadowParent.SetActive(true);
        }
	}


	/// <summary>
	/// Turn character ice skates on or off.
	/// </summary>
	public void SetIceSkates(bool enable)
    {
        skating = enable;
        skeletonMecanim.skeleton.SetAttachment("OverlayLeftFoot", enable ? "Shoes/colors/skate" : null );
        skeletonMecanim.skeleton.SetAttachment("OverlayRightFoot", enable ? "Shoes/colors/skate" : null );
    }


    /// <summary>
    /// Plays emote animation
    /// </summary>
    public void Emote(int slot)
    {
        string anim_suffix = Emoji_Manager.inst.emojiSettings.emojis[slot].playerAnim;
        if( string.IsNullOrEmpty(anim_suffix) )  // no animation for this one
            return;

        // Retrieve name of animation that this emoji slot is keyed to.
        anim.Play("emote_" + anim_suffix, -1, 0f);
    }
}
