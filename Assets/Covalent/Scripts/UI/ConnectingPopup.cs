using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Manages behavior of the "Connecting" popup
/// </summary>
public class ConnectingPopup : MonoBehaviour
{
    public PopupManager popupManager;
    public Dateland_Network datelandNetwork;   // needed for debug purposes

    public float holdTime = 0.25f;   // we'll hold for a short bit before hiding "Connecting...", this should ensure it's still shown during the Photon stutter



    bool _didDisplayWelcome = false;

    void FixedUpdate()
    {
        // Make darn sure everything's ready
        if( (popupManager.curPopup ==  "connecting" || 
            popupManager.curPopup == "reconnecting" || 
            popupManager.curPopup == "disconnected" ||
            popupManager.curPopup == "waiting_for_partner" ||
            popupManager.curPopup == "partner_long_time"
            ) && Player_Controller_Mobile.mine != null && 
            (Player_Controller_Mobile.mine.playerPartner.GetPartner() != null || datelandNetwork.disablePartnerDisconnectForDebug) &&   // Note that having a partner is a prerequisite for closing these screens.
            PhotonNetwork.IsConnectedAndReady && 
            Dateland_Network.initialized)   
        {
            if( holdTime > 0 )   // hold for a bit
                holdTime -= Time.fixedDeltaTime;
            else   // OK! start the game
            {
                if( !_didDisplayWelcome )   // haven't displayed welcome yet
                {
                    _didDisplayWelcome = true;
                    popupManager.ShowPopup(  "welcome" );
                }
                else   // just close the popup
                    popupManager.ShowPopup("");
            }
        }
    }
}
