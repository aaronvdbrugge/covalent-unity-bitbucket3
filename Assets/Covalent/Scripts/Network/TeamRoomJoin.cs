using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This file handles the logistics of connecting BOTH players to the same room, which
/// should be a random available room, but we have to make sure they both go into the same one.
/// 
/// Photon's recommended practice seems to be here, under "Example Use Case: Teams Matchmaking."
/// https://doc.photonengine.com/zh-CN/realtime/current/lobby-and-matchmaking/userids-and-friends
/// 
/// However, this approach requires a "leader" to join the room before a follower, meaning one of our players
/// could be stuck in loading screen limbo if the other player is still loading.
/// 
/// I want to try the following approach, instead:
/// 
/// 1.) Upon passing loading screen, player does FindFriends to see if their match is already in a room.
/// 2.) If their match isn't already in a room, they try to JoinRandomRoom with reserved team IDs.
/// 3.) If that doesn't work, they create a room with reserved team IDs.
/// 4.) They will check FindFriends again periodically, just to cover against any special case where they may have both
///		created or joined a room at the same time.  "Secondary" player gets an added 10 second delay on this, to prevent gridlock.
/// </summary>
public class TeamRoomJoin : MonoBehaviourPunCallbacks
{
	[Header("Settings")]
	public byte maxPlayersPerRoom = 16;

	[Header("Runtime")]
	public string myId;
	public string matchId;
	


	/// <summary>
	/// Call this to join a room initially.
	/// NOTE: you can also re-call this periodically, to see if your match actually got stuck in a different room.
	/// </summary>
	public void StartJoin()
	{
		Debug.Log("Finding friends... (myId: " + myId + ", matchId: " + matchId + ")");
		PhotonNetwork.FindFriends( new string[1]{ matchId } );   // See if our match ID is in Photon world already. Await OnFriendListUpdate
	}


	/// <summary>
	/// Should come from Photon once we get a friends update.
	/// </summary>
	public override void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		// Determine if we can join our friend's room, or if we have to join/create a new one.
		string found_room = null;
		foreach(var friend in friendList )
			if( friend.UserId == matchId && friend.IsOnline && friend.IsInRoom )
				found_room = friend.Room;

		if( !string.IsNullOrEmpty(found_room) )
		{
			Debug.Log("Found friend's room: " + found_room);

			// NOTE: we may already be in a room, in the unlikely special case of both accidentally creating rooms.
			// Hopefully, this doesn't cause an issue.
			PhotonNetwork.JoinRoom(found_room);
		}
		else   // No friend room. Join or create new
		{
			if( PhotonNetwork.CurrentRoom == null )
			{
				Debug.Log("No friend's room. Joining random... ");
				string[] reserved_ids = new string[]{myId, matchId};
				PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, null, null, reserved_ids );   // Try to join a random room, reserving TWO slots
			}
			else
				Debug.Log("No friend's room. Continuing to wait in this room.");
		}
	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room succesfully!");
	}


	public override void OnJoinRandomFailed (short returnCode, string message)
	{
		Debug.Log("JoinRandomFailed( " + returnCode  + ", " + message + " ). Creating room");

		// Most likely reason is there are no rooms with two available slots.
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsOpen = true;
		roomOptions.IsVisible = true;
		roomOptions.BroadcastPropsChangeToAll = true;
		roomOptions.MaxPlayers = maxPlayersPerRoom;
		roomOptions.PublishUserId = true;   // broadcasts player Kippo IDs to everyone, should be accessible via Player.UserId
		PhotonNetwork.CreateRoom( null, roomOptions, TypedLobby.Default, new string[]{myId, matchId}  );   // Create a new room with two reserved slots.
	}
}
