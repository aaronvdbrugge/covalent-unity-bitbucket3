﻿using System.Collections;
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

    public Animator anim;
    public Animator hearts;

    public SpriteRenderer splash_feet;
    [Tooltip("A transform that you don't want to be flipped horizontally (will counter-flip to undo the flip)")]
    public Transform dontFlip;


    [Header("Info")]
    [Tooltip("Will be set dynamically based on playerMovement")]
    public bool horizontalFlip = false;



	private void Update()
	{
        // Decide whether we should splash
        if (playerCollisions.onBeach && playerMovement.IsWalking() )
            splash_feet.enabled = true;
        else if (playerCollisions.onBeach && !playerMovement.IsWalking() )
            splash_feet.enabled = false;
        else if (!playerCollisions.onBeach && splash_feet.enabled)
            splash_feet.enabled = false;

        // Note sure what this does, but I kept in in the refactor -Seb
        if (playerCollisions.topHearts == true && playerCollisions.botHearts == true)
        {
            //hearts.Play("animation", -1, 0);
                
            playerCollisions.topHearts = false;
            playerCollisions.botHearts = false;
        }

        
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
        anim.SetBool("walking", playerMovement.IsWalking() );
        anim.SetBool("hopping", playerHop.hopProgress > 0 );
        anim.SetBool("sitting", sitting && playerHop.hopProgress <= 0 );   // only start sitting once we've finished out hop.
	}



    /// <summary>
    /// Plays emote animation
    /// </summary>
    public void Emote(int slot)
    {
        anim.Play("emote_" + slot, -1, 0f);
    }
}