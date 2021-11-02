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
/// I'll use a slightly modified version of this approach, so both players can connect to Photon as soon as possible:
/// 
/// "Primary" player is first in partyId, they lead the way. "Secondary" player is second in partyId.
/// 
/// Primary player:
/// 1.) Upon passing loading screen, immediately finds a random room to join, reserving two slots (one for secondary player)
/// 
/// Secondary player:
/// 1.) Upon passing loading screen, does FindFriends to see if their match is already in a room. If not, they'll just keep
///		checking it periodically
/// 2.) They'll continue to check FindFriends periodically until they're sure the primary player has joined a room, then, just join it.
/// </summary>
public class TeamRoomJoin : MonoBehaviourPunCallbacks
{
	[Header("Settings")]
	public byte maxPlayersPerRoom = 16;

	[Tooltip("If NativeEntryPoint.sandboxMode == true")]
	public byte maxPlayersPerRoomSandboxMode = 64;   

	/// <summary>
	/// Returns max players in the chosen NativeEntryPoint mode (sandbox or real matchmaking)
	/// </summary>
	public byte GetMaxPlayers() => NativeEntryPoint.sandboxMode ? maxPlayersPerRoomSandboxMode : maxPlayersPerRoom;

	[Tooltip("Check where our friend is every this amount of time, while we're in the private waiting room.")]
	public float checkFriendsInterval = 5.0f;



	[Header("Runtime")]
	public string myId;
	public string matchId;
	public bool amPrimaryPlayer;   // partyId is "primary:secondary"
	public bool isWaitingForFriend = false;      // will be set to true when StartJoin() is called on secondary player



	float _checkFriendsTimer = 0;  // counts up to checkFriendsInterval



	


	/// <summary>
	/// Call this to join a room initially.
	/// NOTE: you can also re-call this periodically, to see if your match actually got stuck in a different room.
	/// </summary>
	public void StartJoin()
	{
		if( !amPrimaryPlayer )
		{
			Debug.Log("Secondary player. Finding friends... (myId: " + myId + ", matchId: " + matchId + ")");
			isWaitingForFriend = true;
			PhotonNetwork.FindFriends( new string[1]{ matchId } );   // See if our match ID is in Photon world already. Await OnFriendListUpdate
		}
		else
		{
			// Primary player. We can join room immediately
			Debug.Log("Primary player. Joining room... (myId: " + myId + ", matchId: " + matchId + ")");
			string[] reserved_ids = { myId, matchId };
			PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, null, null, reserved_ids );   // Try to join a random room, reserving TWO slots
		}
	}


	/// <summary>
	/// Should come from Photon once we get a friends update.
	/// </summary>
	public override void OnFriendListUpdate(List<FriendInfo> friendList)
	{
		// Only secondary players get here.
		// Determine if we can join our friend's room, or if we have to join/create a new one.
		string found_room = null;
		foreach(var friend in friendList )
			if( friend.UserId == matchId && friend.IsOnline && friend.IsInRoom )
				found_room = friend.Room;

		if( !string.IsNullOrEmpty(found_room) )
		{
			isWaitingForFriend = false;  // don't need to keep re-checking FindFriends
			Debug.Log("Found friend's room: " + found_room);
			PhotonNetwork.JoinRoom(found_room);
		}
		else
			Debug.Log("OnFriendListUpdate didn't find our friend's room.");
		// else, we'll just keep checking in FixedUpdate
	}


	public override void OnJoinedRoom()
	{
		Debug.Log("Joined public room: " + PhotonNetwork.CurrentRoom.Name);
	}

	public RoomOptions GetRoomOptions()
	{
		RoomOptions roomOptions = new RoomOptions();

		roomOptions.IsOpen = true;
		roomOptions.BroadcastPropsChangeToAll = true;
		roomOptions.MaxPlayers = GetMaxPlayers();   // honors sandbox mode
		roomOptions.PublishUserId = true;   // broadcasts player Kippo IDs to everyone, should be accessible via Player.UserId
		roomOptions.IsVisible = !NativeEntryPoint.sandboxMode;  // disallow random matchmaking in sandbox mode

		return roomOptions;
	}


	public override void OnJoinRandomFailed (short returnCode, string message)
	{
		// Only primary players get here.
		Debug.Log("JoinRandomFailed( " + returnCode  + ", " + message + " ). Creating room");

		// Most likely reason is there are no rooms with two available slots.
		PhotonNetwork.CreateRoom( null, GetRoomOptions(), TypedLobby.Default, new string[]{myId, matchId}  );   // Create a new room with two reserved slots.
	}



	private void FixedUpdate()
	{
		if( isWaitingForFriend )
		{
			if( !PhotonNetwork.IsConnected )   // in this case, we might want to stop waiting so Dateland_Network can reconnect us
				isWaitingForFriend = false;
			else
			{
				// Secondary player. Need to check friends periodically
				_checkFriendsTimer += Time.fixedDeltaTime;
				if (_checkFriendsTimer >= checkFriendsInterval)
				{
					Debug.Log("Checking again for friend's server");
					_checkFriendsTimer = 0;
					PhotonNetwork.FindFriends( new string[1]{ matchId } );   // See if our match ID is in Photon world already. Await OnFriendListUpdate
				}
			}
		}
	}
}
