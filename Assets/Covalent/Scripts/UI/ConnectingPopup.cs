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
    public string welcomePopupName = "welcome";
    public float holdTime = 0.25f;   // we'll hold for a short bit before hiding "Connecting...", this should ensure it's still shown during the Photon stutter



    bool _didDisplayWelcome = false;

    void FixedUpdate()
    {
        if( popupManager.curPopup == connectingPopupName && Player_Controller_Mobile.mine != null && PhotonNetwork.IsConnectedAndReady )
        {
            if( holdTime > 0 )
                holdTime -= Time.fixedDeltaTime;
            else
            {
                if( !_didDisplayWelcome )   // haven't displayed welcome yet
                    popupManager.ShowPopup( welcomePopupName );
                else   // just close the popup
                    popupManager.ShowPopup("");
            }
        }
    }
}
