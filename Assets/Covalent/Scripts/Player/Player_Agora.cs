using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles the interface between things the player does (like entering "public" spaces)
/// and Agora voice chat.
/// </summary>
public class Player_Agora : MonoBehaviour
{
    // Deprecated.
    // Player doesn't handle any voice chat now, it's all in Agora_Manager or StartAgoraWithPartyId.

  #if false
    public Agora_Manager agora;
    public uint agora_ID;


    bool _initialized = false;

	private void Awake()
	{
        agora = FindObjectOfType<Agora_Manager>();
	}


	// NOTE: I don't trust script execution order to get this done AFTER
	// Dateland_Network initializes partyId. Better to leave this to Update
	// or LateUpdate.
	/*
	private void Start()
	{
        agora.JoinChannel(PlayerPrefs.GetString("partyId"));
	}
    */

	private void LateUpdate()
	{
		if( !_initialized )    // partyId should hopefully be initialized by now
        {
            _initialized = true;
            agora.JoinChannel(PlayerPrefs.GetString("partyId"));
        }
	}



	public void LeaveChannel()
	{
        agora.LeaveChannel();
	}




    /// <summary>
    /// This message may be sent from Player_Collisions when they enter a public space.
    /// </summary>
    public void JoinPublicAgora()
    {
        agora.LeaveChannel();
        agora.JoinChannel("public_agora");
    }


    /// <summary>
    /// This message may be sent from Player_Collisions when they leave a public space.
    /// </summary>
    public void LeavePublicAgora()
    {
        agora.LeaveChannel();
        agora.JoinChannel(PlayerPrefs.GetString("partyId"));
    }
    #endif

}
