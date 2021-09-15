using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles the interface between things the player does (like entering "public" spaces)
/// and Agora voice chat.
/// </summary>
public class Player_Agora : MonoBehaviour
{
    public Agora_Manager agora;
    public uint agora_ID;


	private void Awake()
	{
        agora = FindObjectOfType<Agora_Manager>();
	}

	private void Start()
	{
        agora.JoinChannel(PlayerPrefs.GetString("partyId"));
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

}
