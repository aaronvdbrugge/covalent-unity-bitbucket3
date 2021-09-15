﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles various things the player can collide with, and handles appropriate actions...
/// </summary>
public class Player_Collisions : MonoBehaviour
{
    [Tooltip("photonView.isMine is put here. Whether or not this player object is controlled by us.")]
    public bool isMine = false;  

    [Tooltip("These will be turned on/off when we sit in a seat, for example")]
    public Collider2D[] colliders;


    [Header("Colliding info")]
    public bool topHearts, botHearts, onBeach;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("topHeart") && topHearts == false)
            topHearts = true;
        else if (collision.gameObject.tag.Equals("botHeart") && botHearts == false)
            botHearts = true;
        if (collision.gameObject.tag.Equals("beach"))
            onBeach = true;
        if (collision.gameObject.tag.Equals("public_agora") && isMine)
            SendMessage("JoinPublicAgora");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("beach"))
            onBeach = false;

        if (collision.gameObject.tag.Equals("public_agora") && isMine)
            SendMessage("LeavePublicAgora");
    }

    /// <summary>
    /// Turns on/off all relevant colliders for this player.
    /// </summary>
    public void EnableColliders(bool enable)
    {
        foreach( Collider2D coll in colliders )
            coll.enabled = enable;
    }


}
