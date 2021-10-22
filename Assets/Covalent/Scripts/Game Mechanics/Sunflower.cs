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
	
	[Tooltip("FX for when it gets watered")]
	public GameObject rainCloudPrefab;

	[Tooltip("FX for when it gets plucked (different for each color)")]
	public GameObject[] pluckedPrefabPerColor;

	[Tooltip("Sound for when a sprout is planted.")]
	public AudioSource plantSproutAudioSource;

	[Tooltip("Used to trigger multiple rustle sounds")]
	public SelfSoundTrigger rustleSoundTrigger;

	[Header("Settings")]
	public float plantSproutSoundPitchMin = 0.75f;
	public float plantSproutSoundPitchMax = 1.5f;

	[Tooltip("A flower must be this much grown in order to pick it (0-1)")]
	public float minSizeToPick = 0.5f;



	[Header("Animation")]
	public float stalkShrinkTime = 3.0f;
	public EasingFunction.Ease stalkShrinkEase = EasingFunction.Ease.Linear;
	public float flowerGrowTime = 10.0f;
	public EasingFunction.Ease flowerGrowEase = EasingFunction.Ease.Linear;
	public float flowerGrowStartScale = 0.2f;   // Shouldn't start at 0, would be smaller than the sprout.

	[Tooltip("Make sprout a little punchier by giving it a different start scale")]
	public Vector2 sproutStartScale = Vector2.one;

	public float sproutAnimTime = 0.3f;
	public EasingFunction.Ease sproutAnimEase = EasingFunction.Ease.Linear;




	[Tooltip("We have a separate collider for rustling the flower, which is smaller than our interaction readius.")]
	public Collider2D rustleCollider;

	[Tooltip("(degrees) Add this much of player's x velocity to _rustleAngle")]
	public float rustleRatioX = 1.0f;
	
	[Tooltip("(degrees) Add this much of player's y velocity to _rustleAngle (probably less than X velocity, since horizontal velocity is visually tied to wobbling left and right more). This is arbitrary directionally, so we'll flip it by index%2")]
	public float rustleRatioY = 0.5f;

	[Tooltip("(degrees) Maximum amount the flower's _rustleAngle can be in either direction.")]
	public float maxRustleAngle = 45;

	[Tooltip("We'll use a loose interpretation of Hooke's law to spring the flower back to 0 after it's been disturbed. This will just be multiplies to -_rustleAngle on a per-frame basis")]
	public float rustleSpringK = 0.01f;

	[Tooltip("This is also a pretty loose interpretation of friction. We'll just set _rustleAngleVel to _rustleAngleVel * (1-rustleFriction) every FixedUpdate")]
	public float rustleFriction = 0.01f;

	[Tooltip("Can only play rustle sound once every this amount of time")]
	public float rustleSoundCooldownTime = 0.5f;

	[Tooltip("Can only play rustle sound if player normalized velocity is >= this")]
	public float rustleSoundMinSpd = 0.2f;




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

	Camera_Sound _cameraSound;


	/// <summary>
	/// Angle at which the flower is disturbed from its normal straight-up position. Clamped by maxRustleAngle and affected by _rustleAngleVel.
	/// </summary>
	float _rustleAngle = 0;
	float _rustleAngleVel = 0;
	float _rustleSoundCooldown = 0;    // must cool off before can play another rustle sound
	float _sproutAnim = 0;   // 0 to 1

	private void Start()
	{
		stalkScaler.localScale = Vector3.zero;   // Starts in "empty mound" state, so shrink away the flower stalk
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();
	}


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
				plr.playerHop.stifleHop = true;   // This will prevent the player from actually hopping here. It does a different action instead. Note this value is "consumed" and must be set every frame

				if( plr.playerHop.hoppedInPlace )
				{
					// Player did "action" on this sunflower.
					// Depending on our current state, this could plant a sprout, or water the sprout (triggering grow animation),
					// or pick the flower at whatever stage of growth it's in, returning to empty mound.

					// For ease of use reasons, we'll give the flowers big collision radii, and if a player is overlapping
					// multiple flowers, the SunflowerManager decides who gets an Interact() call based on which was closer to the player.
					sunflowerManager.PlayerInteractedWithFlower( plr, this );
				}

				// Handle rustling!
				// Our collider is pretty big, so need to test if they're overlapping the smaller rustle collider.

				if( plr.playerCollisions.GetEnabledCollider() )
				{
					ContactFilter2D contact_filter = new ContactFilter2D();
					contact_filter.SetLayerMask( LayerMask.GetMask("player_collider") );   // only looking for player colliders
					Collider2D[] cols = new Collider2D[8];   // can consider up to 8 collisions
        
					int num_cols = rustleCollider.OverlapCollider( contact_filter, cols );

					bool do_rustle = false;
					for( int i=0; i<num_cols; i++ )
						if( cols[i].GetComponent<Player_Controller_Mobile>() == plr )   // they're overlapping our rustle collider!
							do_rustle = true;

					if( do_rustle && (state != State.EmptyMound || _flowerSize > 0))   // don't do this to a completely empty mound
					{
						_rustleAngleVel += rustleRatioX * -plr.playerMovement.body.velocity.x;   // only consider player's X velocity
						_rustleAngleVel += rustleRatioY * -plr.playerMovement.body.velocity.y * ((index%2) * 2 - 1);   // direction of Y ratio is arbitrary, so flip it by index%2

						// Sound?
						if( _rustleSoundCooldown <= 0 && plr.playerMovement.body.velocity.magnitude / plr.playerMovement.maxSpeed >= rustleSoundMinSpd ) // must be going fast enough
						{
							_rustleSoundCooldown = rustleSoundCooldownTime;
							rustleSoundTrigger.PlaySound();
						}
					}
				}
			}

		if( state == State.Sprout && _sproutAnim < 1 )   // handle sprout animation!
		{
			_sproutAnim = Mathf.Min(1, _sproutAnim + Time.fixedDeltaTime / sproutAnimTime );

			float lerp = EasingFunction.GetEasingFunction( sproutAnimEase )(0, 1, _sproutAnim);
			stalkScaler.localScale = Vector3.Lerp( sproutStartScale, Vector3.one, lerp );
		}


		if( _rustleAngle != 0 || _rustleAngleVel != 0 )   // update rustling, go to sleep if everything's 0
		{
			_rustleAngle += _rustleAngleVel * Time.fixedDeltaTime;

			_rustleAngleVel *= (1 - rustleFriction);   // apply ""friction"" 
			_rustleAngleVel += rustleSpringK * -_rustleAngle * Time.fixedDeltaTime;   // apply ""Hooke's Law""

			if( _rustleAngle < -maxRustleAngle )
			{
				_rustleAngle = -maxRustleAngle;
				_rustleAngleVel = Mathf.Max(0, _rustleAngleVel);	// hit the negative side, stop negative velocity
			}
			else if( _rustleAngle > maxRustleAngle )
			{
				_rustleAngle = maxRustleAngle;
				_rustleAngleVel = Mathf.Min(0, _rustleAngleVel);	// hit the positive side, stop positive velocity
			}

			if( Mathf.Abs(_rustleAngle) <= 0.001f && Mathf.Abs(_rustleAngleVel) <= 0.001f)  // go to sleep
			{
				_rustleAngle = 0;
				_rustleAngleVel = 0;
			}

			// Set actual angle
			stalkScaler.transform.localRotation = Quaternion.Euler(0, 0, _rustleAngle);
		}

		
		// Cool off rustle sound
		_rustleSoundCooldown = Mathf.Max(0, _rustleSoundCooldown - Time.fixedDeltaTime);
	}


	/// <summary>
	/// Called from SunflowerManager when it confirms a player interacted with us.
	/// (It'll determine we were the closest overlapping flower.)
	/// </summary>
	public void Interact(Player_Controller_Mobile plr)
	{
		if( state == State.Flower && _flowerSize < minSizeToPick )   // can't pick it yet! too small
			return;

		// COLOR: 
		// I think it's more fun if you can actually control the color based on what direction you face.
		int new_color = -1;
		if( Mathf.Abs(plr.playerMovement.lastDirection.x) > Mathf.Abs(plr.playerMovement.lastDirection.y) )
		{
			if( plr.playerMovement.lastDirection.x > 0)  // facing right
				new_color = 0;
			else   // facing left
				new_color = 2;
		}
		else
		{
			if( plr.playerMovement.lastDirection.y > 0 )  // facing up
				new_color = 1;
			else  // facing down
				new_color = 3;
		}



		State new_state = (State)(((int)state + 1) % (int)State.COUNT);   // looping state
		if( new_state != State.Sprout )  // this is the only state where we're allowed to choose a new color
			new_color = -1;


		SetState(new_state, new_color);
		sunflowerManager.FlowerStateChanged(this);   // will net replicate the state change, if needed
	}



	/// <summary>
	/// Sets up whatever animation is necessary for the new state.
	/// </summary>
	public void SetState( State new_state, int new_color, bool skip_animation = false )
	{
		if( state == new_state )
			return;

		if( new_color != -1 )
			color = new_color;

		switch(new_state)
		{
			case State.Sprout:
				flowerSpriteRenderer.sprite = sproutSprite;

				_flowerSize = 0;   // reset this in anticipation of next state

				stalkScaler.transform.localScale = Vector3.one;   // no scaling, just display the simple sprite

				// Can play the "plant sprout" sound right here, since there's no FX prefab to play it
				if( !skip_animation )
					if( _cameraSound.CanPlaySoundAtPosition( transform.position ) )
					{
						plantSproutAudioSource.pitch = Random.Range( plantSproutSoundPitchMin, plantSproutSoundPitchMax );
						plantSproutAudioSource.Play();
					}

				// Set up sprout animation
				_sproutAnim = skip_animation ? 1 : 0;
				if( !skip_animation )
					stalkScaler.transform.localScale = sproutStartScale;

				break;

			case State.Flower:
				if( !skip_animation )
					Instantiate(rainCloudPrefab, transform.position, Quaternion.identity);

				flowerSpriteRenderer.sprite = sunflowerColorsSprites[ color ];
				stalkScaler.transform.localScale = Vector3.zero;   // start at zero and scale up

				if( skip_animation )     // would normally go from 0 to 1
					_flowerSize = 0.9999999f;  
				break;

			case State.EmptyMound:
				if( !skip_animation )
					Instantiate(pluckedPrefabPerColor[color], transform.position, Quaternion.identity);

				flowerSpriteRenderer.sprite = emptyStalkSprite;
				if( skip_animation )     // would normally go from 1 to 0
					_flowerSize = 0.0000001f;  
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
			float eased = EasingFunction.GetEasingFunction( flowerGrowEase )(flowerGrowStartScale, 1, _flowerSize);
			stalkScaler.localScale = Vector3.one * eased;
		}
	}




}
