using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// The idea is to have a couple hundred of these nested under SunflowerManager.
/// Since Animator could be pretty heavy at that scale, we'll animate them procedurally.
/// We'll handle collisions here, animate accordingly, and let SunflowerManager know
/// if something important happens.
/// </summary>
public class Sunflower : MonoBehaviour
{
	[Header("References")]
	[Tooltip("We'll modify this localScale to animate stalk growing")]
	public Transform stalkScaler;

	[Tooltip("We'll change the sprite out ourselves.")]
	public SpriteRenderer flowerSpriteRenderer;

	public Sprite sproutSprite;
	public Sprite[] sunflowerColorsSprites;
	public Sprite emptyStalkSprite;
	

	[Header("Animation")]
	public float stalkShrinkTime = 3.0f;
	public EasingFunction.Ease stalkShrinkEase = EasingFunction.Ease.Linear;
	public float flowerGrowTime = 10.0f;
	public EasingFunction.Ease flowerGrowEase = EasingFunction.Ease.Linear;

	[Header("Runtime")]
	[Tooltip("No need to set this in inspector, SunflowerManager will set it up in Start if we're nested under it.")]
	public SunflowerManager sunflowerManager;

	[Tooltip("Convenience value, so SunflowerManager knows where we are in its _sunflowers array")]
	public int index;

	/// <summary>
	/// Set via SetState
	/// </summary>
	public State state{get; private set; } = State.EmptyMound;

	[Tooltip("Will be set to an value < sunflowerColorsSprites.Length")]
	public int color;


	public enum State
	{
		EmptyMound = 0,    // if follows flower, will spawn a "poof," then animate an empty stalk shrinking down to nothing
		Sprout,
		Flower,			// will spawn a rain cloud, followed by a flower scaling up from 0 to fullsize
		COUNT
	}



	/// <summary>
	/// Keep track of players who overlapped us in OnTriggerEnter, so we can check up on them.
	/// </summary>
	HashSet<Player_Controller_Mobile> _overlappingPlayers = new HashSet<Player_Controller_Mobile>();

	/// <summary>
	/// Cosmetic, goes from 1 to 0 in EmptyMound state, stays 0 in Sprout, and goes from 0 to 1 in Flower state.
	/// </summary>
	float _flowerSize;



	private void OnTriggerEnter2D(Collider2D collision)
	{
		Player_Controller_Mobile plr = collision.GetComponent<Player_Controller_Mobile>();
		if( plr )
			_overlappingPlayers.Add( plr );
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		Player_Controller_Mobile plr = collision.GetComponent<Player_Controller_Mobile>();
		if( plr && _overlappingPlayers.Contains(plr) )
			_overlappingPlayers.Remove( plr );
	}

	private void FixedUpdate()
	{
		// Will have set up _overlappingPlayers in OnTriggerEnter2D / OnTriggerExit2D
		if( _overlappingPlayers.Count > 0 )
			foreach( var plr in _overlappingPlayers )
			{
				if( plr.playerHop.hoppedInPlace )
				{
					// Player did "action" on this sunflower.
					// Depending on our current state, this could plant a sprout, or water the sprout (triggering grow animation),
					// or pick the flower at whatever stage of growth it's in, returning to empty mound.

					// For ease of use reasons, we'll give the flowers big collision radii, and if a player is overlapping
					// multiple flowers, the SunflowerManager decides who gets an Interact() call based on which was closer to the player.
					sunflowerManager.PlayerInteractedWithFlower( plr, this );
				}
			}


	}


	/// <summary>
	/// Called from SunflowerManager when it confirms a player interacted with us.
	/// (It'll determine we were the closest overlapping flower.)
	/// </summary>
	public void Interact()
	{
		State new_state = (State)(((int)state + 1) % (int)State.COUNT);   // looping state
		SetState(new_state);
		sunflowerManager.FlowerStateChanged(this);   // will net replicate the state change, if needed
	}



	/// <summary>
	/// Sets up whatever animation is necessary for the new state.
	/// </summary>
	public void SetState( State new_state )
	{
		if( state == new_state )
			return;

		switch(new_state)
		{
			case State.Sprout:
				flowerSpriteRenderer.sprite = sproutSprite;

				color = Random.Range(0, sunflowerColorsSprites.Length);   // Pick random color, if it's grown later.  NOTE: in the future, it might be neat to have it depend on player direction? Would be a fun trick to learn

				stalkScaler.transform.localScale = Vector3.one;   // no scaling, just display the simple sprite\
				break;

			case State.Flower:
				// TBD: spawn rain cloud

				flowerSpriteRenderer.sprite = sunflowerColorsSprites[ color ];
				stalkScaler.transform.localScale = Vector3.zero;   // start at zero and scale up
				break;

			case State.EmptyMound:
				// TBD: spawn "picked" poof

				flowerSpriteRenderer.sprite = emptyStalkSprite;
				break;
		}

		state = new_state;
	}





	private void Update()
	{
		if( state == State.EmptyMound && _flowerSize > 0 )    // shrink down stalk animation after flower picked.
		{
			_flowerSize = Mathf.Max(0, _flowerSize - Time.deltaTime / stalkShrinkTime);
			float eased = EasingFunction.GetEasingFunction( stalkShrinkEase )(0, 1, _flowerSize);
			stalkScaler.localScale = Vector3.one * eased;
		}
		else if( state == State.Flower && _flowerSize < 1 )   // scale up flower animation after it's watered.
		{
			_flowerSize = Mathf.Min(1, _flowerSize + Time.deltaTime / flowerGrowTime);
			float eased = EasingFunction.GetEasingFunction( flowerGrowEase )(0, 1, _flowerSize);
			stalkScaler.localScale = Vector3.one * eased;
		}
	}




}
