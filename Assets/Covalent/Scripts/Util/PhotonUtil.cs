using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper functions for Photon
/// </summary>
public static class PhotonUtil 
{
	public static Photon.Realtime.Player GetPlayerByActorNumber( int actor_number )
	{
		var players = PhotonNetwork.PlayerList;
		foreach( var plr in players )
			if( plr.ActorNumber == actor_number )
				return plr;

		return null;
	}
}
