using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
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


	GameObject[] _cards;   // instantiated objects
	int[] _cardValues;    // what type of card is it (balloon, seahorse, etc)


	public int numCardTypes => cardsWide * cardsHigh / 2;
	public int numCards => cardsWide * cardsHigh;


	float _lastRequestedState;   // Time.time when we last requested the game's state from its owner (limit to once a second or so)

	/// <summary>
	/// The sprite sorting script needs "movable" to be on for a frame to work properly
	/// </summary>
	public void Start()
	{
		_cards = new GameObject[cardsWide * cardsHigh];
		_cardValues = new int[cardsWide * cardsHigh];

		for( int x=0; x<cardsWide; x++)
			for( int y=0; y<cardsHigh; y++)
			{
				GameObject go = Instantiate( cardPrefab, transform );
				go.transform.localPosition = cardOffsetX * x + cardOffsetY * y;
				go.GetComponent<IsoSpriteSorting>().isMovable= true;    // fixed sprite sorting issue

				_cards[x + y*cardsWide] = go;  //save it for later


				// temp: flip all
				go.GetComponent<Animator>().SetBool("flipped", true );

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


			photonView.RPC("InitializeCardsRPC", RpcTarget.All, new object[] { _cardValues });   
		}
		else
		{
			// If the game isn't ours, but we still need initialization, we need to request that the game's owner send out another RPC.
			if( _lastRequestedState - Time.time >= 1.0f )
			{
				_lastRequestedState = Time.time;
				photonView.RPC("RequestState", RpcTarget.MasterClient, new object[] { PhotonNetwork.LocalPlayer.ActorNumber });   // asks the owner to send us back state in a call to InitializeCardsRPC.
			}
		}
	}



	[PunRPC]
	void InitializeCardsRPC(int[] card_values)
	{
		_cardValues = card_values;

		// Change AnimFrameSwapper.replaceWithThis for each card. This will make it show the
		// corrrect type of card when flipped.
		for( int i=0; i<cardsWide * cardsHigh; i++ )
		{
			AnimFrameSwapper afs = _cards[i].GetComponentInChildren<AnimFrameSwapper>();
			if( afs )
				afs.replaceWithThis = cardNames[ _cardValues[i] ];
		}
	}

    [PunRPC]
    public void RequestState(int requesting_player_actor_num)
    {
        // We should be the owner of this game, so if we could send an update to just the requesting player, that'd be ideal
        if( photonView.IsMine )  //just in case, though this should always be true
        {
            Photon.Realtime.Player player = PhotonUtil.GetPlayerByActorNumber( requesting_player_actor_num );               // Get the player we want to send it to...
            if( player != null )
                photonView.RPC("InitializeCardsRPC", player, new object[] { _cardValues });    // Give them the info they requested
        }
    }



}
