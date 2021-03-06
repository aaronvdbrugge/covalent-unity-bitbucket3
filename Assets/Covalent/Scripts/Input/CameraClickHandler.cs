using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles messages from SendClicksToCamera and can do various things
/// depending on what in-game sprite got tclicked.
/// </summary>
public class CameraClickHandler : MonoBehaviour
{
    [Tooltip("We'll need the playerHop of the controlled player so we can make them hop when the background is clicked.")]
    public Player_Hop playerHop;

    [Tooltip("If the inventory panel is slid out, instead of hopping, we'll just close the panel")]
    public SlidingUIPanel inventoryPanel;


    public void OnObjectClicked(GameObject clicked)
    {
        // When the player clicks the background, they should hop.
        if( clicked.tag == "sky" )  
        {
            if( inventoryPanel.doSlideIn )   // actually, just close the inventory
                inventoryPanel.SlideAway();

            //Player hop:
            else if( playerHop != null && playerHop.hopProgress <= 0 ) // on ground
                playerHop.HopInPlace();
        }
    }
}
