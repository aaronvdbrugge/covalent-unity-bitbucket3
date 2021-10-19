using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Handles the game where you try to match two cards.
/// </summary>

public class MatchGame : MonoBehaviourPun
{
	[Header("References")]
	[Tooltip("We'll create these on Start")]
	public GameObject cardPrefab;

	[Header("Settings")]
	public int cardsWide = 6;
	public int cardsHigh = 6;

	[Tooltip("Isometric offset for a card that adds 1 to x index")]
	public Vector2 cardOffsetX;
	[Tooltip("Isometric offset for a card that adds 1 to y index")]
	public Vector2 cardOffsetY;

	[Tooltip("Corresponds to sprite names to be used with the AnimFrameSwapper component.")]
	public string[] cardNames;

	[Tooltip("Our card centers are not perfect... need to add -0.75 at time of this writing")]
	public Vector2 cardCenterOffset = new Vector3(0, -0.75f);

	[Tooltip("Cards can only be flipped once per certain amount of time")]
	public float flipCooldownTime = 0.5f;


	[Tooltip("Wait this amount of time before triggering mismatched cards to flip back over")]
	public float mismatchCooldownTime = 1.0f;


	[Tooltip("When game ends, wait this long for animations and sounds to finish before allowing any more flips.")]
	public float gameEndFlipCooldownTime = 5.0f;

	[Tooltip("The instant the game ends, with this long before we reset all the cards, triggering a flashing animation.")]
	public float gameResetCooldownTime = 1.0f;




	GameObject[] _cards;   // instantiated objects
	int[] _cardValues;    // what type of card is it (balloon, seahorse, etc)... NET REPLICATED
	int[] _cardStates;    // 0 = hidden,  1 = temp flipped, 2 = permanent flipped (matched until end of game)
	int[] _oldCardStates;    // used to detect when things change

	Collider2D[] _cardColliders;   // so we don't have to GetComponent() every frame

	float _flipCooldown;   // set to cardCooldownTime when flipped
	float _mismatchCooldown;   // set to mismatchCooldownTime on mismatch, then sends another RPC flipping them back to 0.
	float _gameResetCooldown;   // set to gameResetCooldownTime on game end, then sends another RPC resetting everything (triggering flashing animation before everything flips back over)


	public int numCardTypes => cardsWide * cardsHigh / 2;
	public int numCards => cardsWide * cardsHigh;


	float _lastRequestedState;   // Time.time when we last requested the game's state from its owner (limit to once a second or so)



	// Use this in editor... make the cards ahead of time.
	// Seems to make the sprite sorter work better, for some reason
	[ContextMenu("Make Cards")]
	public void MakeCards()
	{
		for( int x=0; x<cardsWide; x++)
			for( int y=0; y<cardsHigh; y++)
			{
				GameObject go =  PrefabUtility.InstantiatePrefab(cardPrefab) as GameObject;  //Instantiate( cardPrefab, transform );   // we pre-instantiate the cards now
				go.transform.SetParent(transform, false);
				go.transform.localPosition = cardOffsetX * x + cardOffsetY * y;
			}
	}


	/// <summary>
	/// The sprite sorting script needs "movable" to be on for a frame to work properly
	/// </summary>
	public void Start()
	{
		_cards = new GameObject[cardsWide * cardsHigh];
		_cardValues = new int[cardsWide * cardsHigh];
		_cardStates = new int[cardsWide * cardsHigh];
		_oldCardStates = new int[cardsWide * cardsHigh];
		_cardColliders = new Collider2D[cardsWide * cardsHigh];
		

		for( int x=0; x<cardsWide; x++)
			for( int y=0; y<cardsHigh; y++)
			{
				//GameObject go = Instantiate( cardPrefab, transform );   // we pre-instantiate the cards now
				GameObject go = transform.GetChild(x + y*cardsWide).gameObject;

				//go.transform.localPosition = cardOffsetX * x + cardOffsetY * y;   // already done
				
				_cards[x + y*cardsWide] = go;  //save it for later
				_cardColliders[x + y*cardsWide] = go.GetComponent<Collider2D>();

				// temp: flip all
				go.GetComponent<Animator>().SetBool("flipped", true );

			}
	}

	private void Update()
	{
		if( Application.isEditor && Input.GetKeyDown(KeyCode.C) )   // Editor cheat code: move to card that matches currently flipped one
		{
			int cur_flipped = -1;
			for(int card_i=0; card_i<numCards; card_i++)
				if( _cardStates[card_i] == 1 )
					cur_flipped = card_i;

			if( cur_flipped >= 0 )
			{
				int card_val = _cardValues[cur_flipped];
				// Find the other one...
				for(int card_i=0; card_i<numCards; card_i++)
					if( card_i != cur_flipped && _cardValues[card_i] == card_val )
					{
						foreach( var player_controller in FindObjectsOfType<Player_Controller_Mobile>() )
							if( player_controller.photonView.IsMine )
								player_controller.transform.position = _cards[card_i].transform.position;
					}
			}

		}
	}


	private void FixedUpdate()
	{
		if( PhotonNetwork.InRoom )   // must wait until we're in a room to initialize cards.
		{
			// NOTE: we can safeguard against any possible disconnect failures, by manually checking the
			// list of card values and making sure it's valid. If not, we definitely need to (re)initialize
			int[] card_count = new int[numCardTypes];   // should initialize all with 0
			for(int i=0; i<numCards; i++)
				if( i < _cardValues.Length && _cardValues[i] < card_count.Length)
					card_count[ _cardValues[i] ]++;

			bool cards_ok = true;
			for(int i=0; i<card_count.Length; i++)
				if( card_count[i] != 2 )   // something's wrong with our cards...
				{
					cards_ok = false;
					break;
				}

			if( !cards_ok )
				InitializeCards();


			// NOTE: once the game is won, we detect that everything is set to state 2, 
			// and then set it all back to 0 so 






			if( photonView.IsMine )
			{
				if( _flipCooldown <= 0 && _mismatchCooldown <= 0 && _gameResetCooldown <= 0)
				{
					// Check for any players jumping within card boundaries.
					// This is how we flip cards.
					int triggered_card = -1;
					float triggered_card_dist_sq = float.MaxValue;   // we'll only use the card for which the player was closest to card coordinate (plus cardCenterOffset).


					for( int card_i=0; card_i<numCards; card_i++)
					{
						if( _cardStates[card_i] > 0 )   // the card is already flipped!
							continue;

						ContactFilter2D contact_filter = new ContactFilter2D();
						contact_filter.SetLayerMask( LayerMask.GetMask("player_collider") );   // only consider overlaps with player colliders!
						Collider2D[] cols = new Collider2D[8];   // can consider up to 8 collisions
        
						int num_cols = _cardColliders[card_i].OverlapCollider(contact_filter, cols );

						for( int col_i=0; col_i<num_cols; col_i++ )
						{
							Player_Controller_Mobile plr = cols[col_i].GetComponent<Player_Controller_Mobile>();
							if( plr && plr.playerHop.hoppedInPlace )   // they hopped in place! this event is in the running for a card flip.
							{
								// Get the distance so we make sure to only use the closest event...
								float dist_sq = ((Vector2)plr.transform.position - ((Vector2)_cardColliders[card_i].transform.position + cardCenterOffset)).sqrMagnitude;
								if( dist_sq < triggered_card_dist_sq )   // new best choice
								{
									triggered_card_dist_sq = dist_sq;
									triggered_card = card_i;
								}
							}
						}
					}

					if( triggered_card >= 0 )   // a card was triggered!
					{
						_flipCooldown = flipCooldownTime;
						
						// Decide what happens.
						// Note that, if two non-matching cards are changed to state 1, we'll be sending another RPC soon that
						// switches them back to 0.
						_cardStates[triggered_card] = 1;

						// Search to see if we have another flipped card matching this triggered card's value
						int match = -1;
						for( int card_i=0; card_i<numCards; card_i++)
							if( card_i != triggered_card && _cardValues[card_i] == _cardValues[triggered_card] && _cardStates[card_i] > 0 )
							{
								match = card_i;
								break;
							}

						if( match >= 0 )   // found a matching card! Set them both to state 2 (permanently matched until round end)
						{
							_cardStates[match] = 2;
							_cardStates[triggered_card] = 2;

							// Did they win the entire game? If so set cooldowns...
							bool won_game = true;
							for( int card_i=0; card_i<numCards; card_i++)
								if( _cardStates[card_i] != 2 )
									won_game = false;

							if( won_game )
							{
								_gameResetCooldown = gameResetCooldownTime;   // wait for the flip animation to finish, then reset everything to 0, triggering a flashy animation indicating success, and then a reset
								_flipCooldown = gameEndFlipCooldownTime;       // wait significantly longer before allowing any more flips
							}


						}
						else
						{
							// See if there is another MISMATCHED card on the field.
							// This would mean we'll need to wait and then send another RPC when our mismatched cooldown is done.
							for( int card_i=0; card_i<numCards; card_i++)
								if( card_i != triggered_card && _cardStates[card_i] == 1 )   // we already know there isn't a match, so this is definitely a mismatch
									_mismatchCooldown = mismatchCooldownTime;

							// We'll need to detect this again in the RPC so we can play a sound or something.
						}

						// Tell everyone what happened. This will also trigger animations
						photonView.RPC("CardStateRPC", RpcTarget.All, new object[] { _cardValues, _cardStates });   
					}
				}
				else  // waiting for cooldowns
				{
					// Flip cooldown... simple
					_flipCooldown = Mathf.Max(0, _flipCooldown - Time.fixedDeltaTime);


					// Mismatch cooldown... must flip them back when over
					if( _mismatchCooldown > 0 )
					{
						_mismatchCooldown = Mathf.Max(0, _mismatchCooldown - Time.fixedDeltaTime);
						if( _mismatchCooldown == 0 )   // done showing the mismatch! flip the mismatched cards back over.
						{
							for( int card_i=0; card_i<numCards; card_i++)
								if( _cardStates[card_i] == 1 )  // flipped, non permanently... put it back
									_cardStates[card_i] = 0;

							// Tell everyone what happened. This will also trigger animations
							photonView.RPC("CardStateRPC", RpcTarget.All, new object[] { _cardValues, _cardStates });   
						}
					}


					// Game reset cooldown... must reset board when over
					if( _gameResetCooldown > 0 )
					{
						_gameResetCooldown = Mathf.Max(0, _gameResetCooldown - Time.fixedDeltaTime);
						if( _gameResetCooldown == 0 )   // done showing game end! reset the entire game.   KNOWN ISSUE: cards will probably change before they flip back over.
							InitializeCards();
					}
				}
			}
		}
	}




	/// <summary>
	/// Will choose a value for each card, then send out an RPC making it consistent for
	/// each client.
	/// </summary>
	void InitializeCards()
	{
		if( photonView.IsMine )
		{
			// Randomize card values.
			// We'll assume an even number of cards, and only one pair of each.
			// Easiest way is to just create a List containing all the card values, then randomly pick
			// them one by one.
			_cardValues = new int[cardsWide * cardsHigh];
			_cardStates = new int[cardsWide * cardsHigh];   // ensure card states are reset.


			List<int> cards_left = new List<int>();
			for( int i=0; i<numCardTypes; i++ )
			{
				cards_left.Add(i);
				cards_left.Add(i);  // two cards of each
			}




			for( int x=0; x<cardsWide; x++)
				for( int y=0; y<cardsHigh; y++)
				{
					int rand_pick = Random.Range(0, cards_left.Count);
					int card_val = cards_left[rand_pick];
					cards_left.RemoveAt(rand_pick);  // take out pick out of the list

					_cardValues[x + y * cardsWide] = card_val;   // insert this value into the grid of cards.
				}


			photonView.RPC("CardStateRPC", RpcTarget.All, new object[] { _cardValues, _cardStates });   
		}
		else
		{
			// If the game isn't ours, but we still need initialization, we need to request that the game's owner send out another RPC.
			if( _lastRequestedState - Time.time >= 1.0f )
			{
				_lastRequestedState = Time.time;
				photonView.RPC("RequestState", RpcTarget.MasterClient, new object[] { PhotonNetwork.LocalPlayer.ActorNumber });   // asks the owner to send us back state in a call to CardStateRPC.
			}
		}
	}



	bool _firstStateUpdate = true;  // false if we called CardStateRPC at least once

	[PunRPC]
	void CardStateRPC(int[] new_card_values, int[] new_card_states)
	{
		_cardValues = new_card_values;
		_cardStates = new_card_states;

		// Change AnimFrameSwapper.replaceWithThis for each card. This will make it show the
		// corrrect type of card when flipped.
		for( int i=0; i<cardsWide * cardsHigh; i++ )
		{
			AnimFrameSwapper afs = _cards[i].GetComponentInChildren<AnimFrameSwapper>();
			if( afs )
				afs.replaceWithThis = cardNames[ _cardValues[i] ];
		}


		// First, compare old values with new ones, and decide if we need to play sounds / animations / etc...
		bool new_match = false;
		int state_1_count = 0;   // if we get at least 2, that's a mismatch
		bool new_won_game = true;   // set to false if anything in new state isn't 2, or everything in old state is 2 (they already won)
		
		for( int card_i=0; card_i<numCards; card_i++)
		{
			// Set animator values...
			Animator animator = _cards[card_i].GetComponent<Animator>();

			animator.SetBool("flipped", new_card_states[card_i] > 0);
			animator.SetBool("matched", new_card_states[card_i] == 2);

			// Detect sound cues etc...
			if( new_card_states[card_i] == 2 && _oldCardStates[card_i] < 2 )   // just matched this one!
				new_match = true;
			if( new_card_states[card_i] == 1 )   // card is flipped, but not matched
				state_1_count++;

			if( new_card_states[card_i] != 2 )
				new_won_game = false;   // they still haven't won the game
		}

		// Make sure, if they won the game, that it's actually a new thing
		if( new_won_game )
		{
			bool old_state_won = true;
			for( int card_i=0; card_i<numCards; card_i++)
				if( _oldCardStates[card_i] != 2 )
					old_state_won = false;
			if( old_state_won )
				new_won_game = false;   // the win is not a new thing
		}

		new_card_states.CopyTo(_oldCardStates,0);   // don't need to access old state anymore. set new state (make sure they don't point to the same thing though)

		if( new_match && !_firstStateUpdate)
			Debug.Log("Play new match sound!");

		if( state_1_count >= 2 && !_firstStateUpdate)
			Debug.Log("Play mismatch sound...");
		
		if( new_won_game && !_firstStateUpdate)
			Debug.Log("Play won game sound!!!");

		
		_firstStateUpdate = false;
	}

    [PunRPC]
    public void RequestState(int requesting_player_actor_num)
    {
        // We should be the owner of this game, so if we could send an update to just the requesting player, that'd be ideal
        if( photonView.IsMine )  //just in case, though this should always be true
        {
            Photon.Realtime.Player player = PhotonUtil.GetPlayerByActorNumber( requesting_player_actor_num );               // Get the player we want to send it to...
            if( player != null )
                photonView.RPC("CardStateRPC", player, new object[] { _cardValues, _cardStates });    // Give them the info they requested
        }
    }

}
