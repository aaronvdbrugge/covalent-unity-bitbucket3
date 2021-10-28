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
    public string connectingPopupName = "connecting";
    public string reconnectingPopupName = "reconnecting";
    public string disconnectedPopupName = "disconnected";
    public string welcomePopupName = "welcome";
    public float holdTime = 0.25f;   // we'll hold for a short bit before hiding "Connecting...", this should ensure it's still shown during the Photon stutter



    bool _didDisplayWelcome = false;

    void FixedUpdate()
    {
        // Make darn sure everything's ready
        if( (popupManager.curPopup == connectingPopupName || popupManager.curPopup == reconnectingPopupName || popupManager.curPopup == disconnectedPopupName ) && Player_Controller_Mobile.mine != null && PhotonNetwork.IsConnectedAndReady && Dateland_Network.initialized)
        {
            if( holdTime > 0 )   // hold for a bit
                holdTime -= Time.fixedDeltaTime;
            else   // OK! start the game
            {
                if( !_didDisplayWelcome )   // haven't displayed welcome yet
                {
                    _didDisplayWelcome = true;
                    popupManager.ShowPopup( welcomePopupName );
                }
                else   // just close the popup
                    popupManager.ShowPopup("");
            }
        }
    }
}
