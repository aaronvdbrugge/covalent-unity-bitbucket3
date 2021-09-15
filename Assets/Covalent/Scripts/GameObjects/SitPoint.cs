using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A spot where players can sit. Ideally, only one can sit here at a time.
/// Nest this under a chair or bench where the user would sit. You can have
/// multiple sit spots per bench, for example.
/// 
/// Works well with, but is not tightly coupled to, ProximityInteractable.
/// Takes an OnThisClicked message.
/// It would also work with SendClicksToCamera, if you wanted to be able to
/// do a leaping jump onto it from across the map :)
/// </summary>
public class SitPoint : MonoBehaviour
{
	// Originally, I used MonoBehaviourPun so this object can have a dedicated photonView ID,
	// which will be saved to player custom properties to remember where they're sitting.
	// However it's probably overkill to have a PhotonView for each bench, and I was getting problems
	// with duplicate IDs on scene load. Therefore, each SitPoint now has a unique ID which is just
	// based on its world XY.

	[Tooltip("You should uncheck this if the player is supposed to face left when they're sitting here.")]
	public bool faceRight = true;

    [Tooltip("Child which is the spot they'll return to when they stop sitting.")]
    public Transform returnTransform;

	
	// Returns the worldpos that we should hop down from the seat onto.
	public Vector3 returnPoint => returnTransform.position;


	public string uid {get; private set; } = null;   //unique ID. will be set on Awake; derived from world XY
	public static Dictionary<string, SitPoint> byUid = new Dictionary<string, SitPoint>();  // Allows retrieval of SitPoints via uid.
	public static SitPoint ByUidOrNull(string uid) { return !string.IsNullOrEmpty(uid) && byUid.ContainsKey(uid) ? byUid[uid] : null; }  //helper function 


	private void Awake()
	{
		// Assing unique ID based on position.
		Vector3 position = transform.position;
		uid = Mathf.Floor(position.x*100) + "," + Mathf.Floor(position.y*100); 
		byUid[ uid ] = this;
	}


	/// <summary>
	/// If a player sits here, they will set a custom property (via TryOccupy) so nobody else can sit here.
	/// If you check this to see if it's occupied, we'll search through all player custom properties to
	/// see if any of them claim to be sitting here.
	/// </summary>
	/// <returns>Actor Number of the player in the seat, or -1.</returns>
	public static int GetOccupiedBy(string sitpoint_uid)
	{
		int ret = -1;  //return value, actor number or -1

		Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
		foreach( var plr in players )
			if( plr.CustomProperties.ContainsKey("SittingOn") && (string)plr.CustomProperties["SittingOn"] == sitpoint_uid )
				ret = plr.ActorNumber;

		return ret;
	}



	private void Start()
	{
		// Bear in mind a SitPoint could already be occupied when we enter a server!
		SendMessage("SetInteractable", GetOccupiedBy(uid) == -1, SendMessageOptions.DontRequireReceiver);   // only interactable if nobody is sitting there.
	}

	/// <summary>
	/// Message sent from ProximityInteractable
	/// </summary>
	public void OnThisClicked()
	{
		if( Player_Controller_Mobile.mine.playerHop.hopProgress <= 0 )  // on ground...
			Player_Controller_Mobile.mine.playerHop.HopToSeat( this );
	}


	/// <summary>
	/// A player is trying to put their butt here. If it's already occupied return false,
	/// otherwise reserve the seat and return true.
	/// 
	/// Has to be static and use uid, because we may not have actually instantiated the SitPoint.
	/// </summary>
	public static bool TryOccupy( string sitpoint_uid, int actor_number )
	{
		if( GetOccupiedBy(sitpoint_uid) != -1 ) 
			return false;

		// Can reserve this seat. Find player with this actor number and set the custom property.
		var player = PhotonUtil.GetPlayerByActorNumber( actor_number );
		if( player != null )
		{
			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();   // Record this sitpoint's ID in the player's properties.
			hash["SittingOn"] = sitpoint_uid;
			player.SetCustomProperties( hash );

			if( byUid.ContainsKey( sitpoint_uid ) )   //ONLY do this if the object is actually instantiated
				byUid[ sitpoint_uid ].SendMessage("SetInteractable", false, SendMessageOptions.DontRequireReceiver);  // disable interactivity
			return true;
		}

		Debug.Log("ERROR: Actor number not found for TryOccupy.");
		return false;
	}


	/// <summary>
	/// Leaving the seat...
	/// </summary>
	public static void Vacate( string sitpoint_uid, int actor_number )
	{
		if( actor_number != GetOccupiedBy(sitpoint_uid) )
			Debug.LogError("ERROR: a different player vacated the seat, than we thought was sitting there!");

		var player = PhotonUtil.GetPlayerByActorNumber( actor_number );
		if( player != null )
		{
			ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();   // Record this sitpoint's ID in the player's properties.
			hash["SittingOn"] = null;
			player.SetCustomProperties( hash );

			if( byUid.ContainsKey( sitpoint_uid ) )   //ONLY do this if the object is actually instantiated
				byUid[ sitpoint_uid ].SendMessage("SetInteractable", true, SendMessageOptions.DontRequireReceiver);  // enable interactivity again.
			return;
		}

		Debug.LogError("ERROR: Actor number not found for Vacate.");
	}
}
