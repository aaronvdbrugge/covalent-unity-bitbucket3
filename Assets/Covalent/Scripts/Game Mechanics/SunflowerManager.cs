using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The idea is to have a couple hundred Sunflower scripts that we're managing here.
/// It'd probably be crazy to have a PhotonView for each one, or even just an Animator.
/// So, we'll handle replication as one PhotonView here, and we'll animate procedurally
/// within the Sunflower script.
/// </summary>
[DefaultExecutionOrder(100)]   // NOTE: must execute after all Sunflowers to ensure we pick Interact() correctly.
public class SunflowerManager : MonoBehaviourPun
{
	[Tooltip("we'll sync all sunflowers every this amount of time, just in case something got desynced somehow. Otherwise, we'll just be depending on single-flower state updates, plus one sync at the start.")]
	public float synchronizeInterval = 30.0f;



	Sunflower[] _sunflowers;


	bool _gotState = false;   // if this photon view isn't mine, have we gotten its state?
	float _timeLastRequestedState = 0;   // limits spamming state requests

	float _synchronizeCooldown;   // handles periodic syncs on synchronizeInterval

	
	/// <summary>
	///  Have to queue up interactions and decide which flower was closest
	/// </summary>
	struct PlayerFlowerInteraction
	{
		public float distanceSq;   // precalculated distance between player and flower
		public Sunflower flower;
	}

	/// <summary>
	/// Can only have one interaction per player, so it makes sense to key by player... we'll clear this out in FixedUpdate
	/// </summary>
	Dictionary<Player_Controller_Mobile, PlayerFlowerInteraction> _interactions = new Dictionary<Player_Controller_Mobile, PlayerFlowerInteraction>();





	private void Start()
	{
		_synchronizeCooldown = synchronizeInterval;     // wait before doing full sync

		// Gather all child sunflowers, give them each an index.
		// The order of children should be the same on all clients.
		List<Sunflower> sunflowers = new List<Sunflower>();
		int i=0;
		foreach( Transform t in transform )
		{
			Sunflower sf = t.GetComponent<Sunflower>();
			if( sf )
			{ 
				sunflowers.Add(sf);
				sf.sunflowerManager = this;
				sf.index = i++;
			}
		}
		_sunflowers = sunflowers.ToArray();
	}


	/// <summary>
	/// Iterates through our child components and composes an array of their states.
	/// </summary>
	public Sunflower.State[] GetSunflowerStateArray()
	{
		Sunflower.State[] ret = new Sunflower.State[ _sunflowers.Length ];
		for( int i=0; i<_sunflowers.Length; i++)
			ret[i] = _sunflowers[i].state;
		return ret;
	}

	public int[] GetSunflowerStateIntArray()
	{
		int[] ret = new int[ _sunflowers.Length ];
		for( int i=0; i<_sunflowers.Length; i++)
			ret[i] = (int)_sunflowers[i].state;
		return ret;
	}


	/// <summary>
	/// Syncs all sunflowers (could be hundreds), should probably be used sparingly.
	/// </summary>
	[PunRPC]
	void SyncAllSunflowerStates( int[] states )
	{
		for( int i=0; i<states.Length && i < _sunflowers.Length; i++)
			_sunflowers[i].SetState( (Sunflower.State)states[i] );

		_gotState = true;
	}

	/// <summary>
	/// When a single sunflower's state changes (more efficient than sending the whole array)
	/// </summary>
	[PunRPC]
	void SingleSunflowerStateChanged( int index, Sunflower.State state )
	{
		_sunflowers[index].SetState( state );
	}

	/// <summary>
	/// Requests a SyncAllSunflowerStates RPC to a specific player.
	/// </summary>
	[PunRPC]
	void RequestAllSunflowerStates(int requesting_player_actor_num)
	{
        // Send response only to the requesting player.
        if( photonView.IsMine && Dateland_Network.initialized )  //just in case, though this should always be true
        {
            Photon.Realtime.Player player = PhotonUtil.GetPlayerByActorNumber( requesting_player_actor_num );               // Get the player we want to send it to...
            if( player != null )
                photonView.RPC("SyncAllSunflowerStates", player, new object[]{ GetSunflowerStateIntArray() });   
        }
	}



	/// <summary>
	/// Child sunflowers are to call this when their state changes, which needs to be net replicated.
	/// </summary>
	public void FlowerStateChanged(Sunflower sunflower)
	{
		if( photonView.IsMine && Dateland_Network.initialized )
            photonView.RPC("SingleSunflowerStateChanged", RpcTarget.Others, new object[]{ sunflower.index, sunflower.state });   
	}




	private void FixedUpdate()
	{
		if( Dateland_Network.initialized )
		{
			if( photonView.IsMine )
			{
				_gotState = true;   // we're the authority on state

				// Handle periodic synchronizations (all of the sunflowers, not just one at a time)
				_synchronizeCooldown -= Time.fixedDeltaTime;
				if( _synchronizeCooldown <= 0 )
				{
					_synchronizeCooldown = synchronizeInterval;
					photonView.RPC("SyncAllSunflowerStates", RpcTarget.Others, new object[]{ GetSunflowerStateIntArray() });   
				}
			}
			else   // not mine
			{  
				if( !_gotState && Time.time - _timeLastRequestedState >= 1.0f )  //We need the owner to tell us state. Can only request once per second
				{
					_timeLastRequestedState = Time.time;
					photonView.RPC("RequestAllSunflowerStates", RpcTarget.MasterClient, new object[]{ PhotonNetwork.LocalPlayer.ActorNumber } );
				}
			}
		}

		// Process any interactions between player and flower...
		foreach( var kvp in _interactions )
			kvp.Value.flower.Interact();
		_interactions.Clear();
	}


	/// <summary>
	/// Flowers have to call this function first, then we'll decide which flower "won" (was closest) if multiple
	/// were overlapping.
	/// </summary>
	public void PlayerInteractedWithFlower( Player_Controller_Mobile plr, Sunflower flower )
	{
		float dist_sq = (plr.transform.position - flower.transform.position).sqrMagnitude;
		if( _interactions.ContainsKey(plr) )   // we already have an interaction for this player. The new one must be closer to win the spot
		{
			if( _interactions[plr].distanceSq > dist_sq )   // new closest contender
				_interactions[plr] = new PlayerFlowerInteraction { distanceSq = dist_sq, flower = flower };
		}
		else   // no previous contended, can add key
			_interactions[plr] = new PlayerFlowerInteraction { distanceSq = dist_sq, flower = flower };
	}


}
