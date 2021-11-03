using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Driveable go kart.
/// It has a fixed "start point." We can take advantage of this, and just leverage SitPoint to make sure only one person grabs the kart at a time.
/// It will give us a nice "hop" animation into the kart, and then the netcode for "claiming" it is already taken care of.
/// So, every GoKart has a designated SitPoint.
/// </summary>
public class GoKart : MonoBehaviour
{
    [Header("References")]
    [Tooltip("See class summary for how this is used.")]
    public SitPoint entryPoint;

    [Tooltip("Child. Shows front view of the cart, facing left")]
    public SpriteRenderer frontSprite;  

    [Tooltip("Child. Shows back view of the cart, facing left")]
    public SpriteRenderer backSprite;  

    [Tooltip("Show this when we're just sitting therer without a rider")]
    public SpriteRenderer idleSprite;

    [Tooltip("Turns on and off based on direction and movement.")]
    public ParticleSystem frontParticles;

    [Tooltip("Turns on and off based on direction and movement.")]
    public ParticleSystem backParticles;

    [Tooltip("Looping sound effect; we'll play and modify it based on our speed etc")]
    public AudioSource engineNoise;



    [Header("Settings")]
    [Tooltip("Minimum smoke particles to spawn")]
    public float minParticleRate = 20.0f;

    [Tooltip("Maximum smoke particles to spawn (at max speed)")]
    public float maxParticleRate = 60.0f;

    [Tooltip("After boarding vehicle, the player will be animated from entryPoint to entryPoint + driveInPointOffset")]
    public Vector2 driveInPointOffset;

    [Tooltip("Time to spend driving from entryPoint to entryPoint + driveInPointOffset")]
    public float driveInTime = 1.0f;

    [Tooltip("Amount of time it takes for this thing to gradually fade back into place.")]
    public float fadeBackTime = 2.0f;

    public EasingFunction.Ease driveInEasing;




    Player_Controller_Mobile _lastPlr;   //must remember to show their mesh renderer again before we do away with them

    // No sense in leaving these running while sprite renderer is disabled
    Animator _frontSpriteAnimator;
    Animator _backSpriteAnimator;

    float _driveInProgress = 0;   // Once it reaches 1, we can drive.
    Transform _followInLateUpdate;    // in non null, this transform must be followed in LateUpdate to avoid lag

    float _fadeBackProgress = 1;    // <0.5: fade out, >=0.5: fade in
    Vector2 _fadeAnimCoastVelocity;   // It's cool looking if we maintain our old speed as we fade out :)

	private void Awake()
	{
		_frontSpriteAnimator = frontSprite.GetComponent<Animator>();
        _backSpriteAnimator = backSprite.GetComponent<Animator>();
	}


    Camera_Sound _cameraSound;
	private void Start()
	{
		_cameraSound = Camera.main.GetComponent<Camera_Sound>();   // cache this so we aren't calling GetComponent every frame
	}

	/// <summary>
	/// Direction is 0-3 or -1 for idle
	/// 0: NE
	/// 1: NW
	/// 2: SW
	/// 3: SE
	/// -1 : idle
	/// </summary>
	void SetDirection( int dir, bool particles_enabled )
    {
        bool back_particles = false;
        bool front_particles = false;

        switch(dir)
        {
            case 0:
                backSprite.enabled = true;
                backSprite.transform.localScale = new Vector3(-1,1,1);  //flip
                back_particles = true;
                frontSprite.enabled = false;
                idleSprite.enabled = false;
                break;
            case 1:
                backSprite.enabled = true;
                backSprite.transform.localScale = new Vector3(1,1,1);  //don't flip
                back_particles = true;
                frontSprite.enabled = false;
                idleSprite.enabled = false;
                break;
            case 2:
                frontSprite.enabled = true;
                frontSprite.transform.localScale = new Vector3(1,1,1);  //don't flip
                front_particles = true;
                backSprite.enabled = false;
                idleSprite.enabled = false;
                break;
            case 3:
                frontSprite.enabled = true;
                frontSprite.transform.localScale = new Vector3(-1,1,1);  //flip
                front_particles = true;
                backSprite.enabled = false;
                idleSprite.enabled = false;
                break;
            case -1:
                idleSprite.enabled = true;
                backSprite.enabled = false;
                frontSprite.enabled = false;
                break;
        }


        //Enable / disable Animators with their corresponding sprites
        if( _frontSpriteAnimator )
            _frontSpriteAnimator.enabled = frontSprite.enabled;
        if( _backSpriteAnimator )
            _backSpriteAnimator.enabled = backSprite.enabled;


        // Start / stop particle emission
        if( back_particles && particles_enabled )
            backParticles.Play();
        else
            backParticles.Stop();   // allows smoke particles already spawned to hang around in world space.

        if( front_particles && particles_enabled)
            frontParticles.Play();
        else
            frontParticles.Stop();

    }



    void FixedUpdate()
    {
        _followInLateUpdate = null;
        entryPoint.canMoveWhileSitting = false;   // only set this to true once movement is allowed.

        if( entryPoint.occupyingActor != -1 && Player_Controller_Mobile.fromActorNumber.ContainsKey(entryPoint.occupyingActor) )   // Someone is sitting in this kart!!
        {
            Player_Controller_Mobile plr = Player_Controller_Mobile.fromActorNumber[entryPoint.occupyingActor];   // Retrieve the player object sitting in the kart
            _lastPlr = plr;

            if( plr.playerHop.hopProgress <= 0 )  // hop is complete! ready to go
            {
                if( _driveInProgress < 1.0f )   // Have to do the drive-in animation.
                {
                    // NOTE: there is a netcode quirk here.
                    // We are going to move the player from entryPoint to entryPoint + driveInPointOffset
                    // We'll disable all colliders during animations, then switch to gokarts layer (so it's fenced in by the track)
                    // But, we could join a server where players are already in cars, and don't need this animation.
                    // So, just give only the owning player the authority to actually move and do this animation.

                    // The movement will be net replicated.
                    // We'll switch the collision layers no matter what -- could be slightly odd if you somehow manage to dive straight into the Go Kart track within
                    // seconds of joining the server, but I doubt that special case will ever be an issue. Worth noting though just in case.

                    // SWITCHING COLLISION LAYERS is handled by Player_Alternate_Movements which is attached to the player and set up in the inspector.
                    // DISABLING COLLIDERS ALTOGETHER is handled by Player_Collisions, which is aware of Player_Alternate_Movements and knows to re-enable the right collider.

                    _driveInProgress = Mathf.Min(1.0f, _driveInProgress + Time.fixedDeltaTime / driveInTime );

                    if( plr.photonView.IsMine )  // We have authority to move this player around.
                    {
                        float lerp_amt = EasingFunction.GetEasingFunction( driveInEasing )(0, 1, _driveInProgress);
                        float next_lerp_amt = EasingFunction.GetEasingFunction( driveInEasing )(0, 1, Mathf.Min(1, _driveInProgress + Time.fixedDeltaTime) );   // Use this prediction to calculate speed. Setting body speed should result in smoother network animation

                        Vector2 anim_position = (Vector2)entryPoint.transform.position + driveInPointOffset * lerp_amt;
                        Vector2 next_anim_position = (Vector2)entryPoint.transform.position + driveInPointOffset * next_lerp_amt;

                        plr.transform.position = anim_position;
                        plr.playerMovement.body.velocity = (next_anim_position - anim_position) / Time.fixedDeltaTime;  //set velocity for smoother network animation. will also result in better particle emission rate changes
                    }

                    plr.playerMovement.lastDirection = driveInPointOffset.normalized;     // just force player to go this way so we animate correctly (regardless of whether IsMine)
                    // Player will remain on normal movement until we reach the next phase below:

                    plr.playerCollisions.EnableColliders(false);  // keep colliders off while we're driving in, to avoid glitchy movement
                }
                else   // Drive-in animation complete; we can move around.
                {
                    entryPoint.canMoveWhileSitting = true;     // player is now allowed to move around.
                    plr.playerAlternateMovements.currentMovement = 0;   // Set to Go Kart movement / layer   (-1 is original movement)
                    plr.playerCollisions.EnableColliders(true);   // This will have been turned off during the drive-in animation.

                    _fadeAnimCoastVelocity = plr.playerMovement.body.velocity;  //in case we jump out soon
                }

                plr.playerAnimations.meshRenderer.enabled = false;   // Hide the actual player. They're a car now.

                _followInLateUpdate = plr.transform;   // Our visual follows the player wherever

                // Choose direction of the visual sprite based on player's movement direction.
                if( plr.playerMovement.lastDirection.x > 0 )
                {
                    if( plr.playerMovement.lastDirection.y > 0 )
                        SetDirection(0, true);
                    else
                        SetDirection(3, true);
                }
                else
                {
                    if( plr.playerMovement.lastDirection.y > 0 )
                        SetDirection(1, true);
                    else
                        SetDirection(2, true);
                }


                // Decide how fast to spawn particles
                float speed_norm = plr.playerMovement.body.velocity.magnitude / plr.playerMovement.maxSpeed;  // will be used to 
                var emission = frontParticles.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve( Mathf.Lerp(minParticleRate, maxParticleRate, speed_norm) );

                emission = backParticles.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve( Mathf.Lerp(minParticleRate, maxParticleRate, speed_norm) );


                // Handle sound!
                bool can_play_sound = _cameraSound.CanPlaySoundAtPosition(transform.position);

                if( !engineNoise.isPlaying && can_play_sound)
                    engineNoise.Play();
                else if(engineNoise.isPlaying && !can_play_sound)
                    engineNoise.Stop();

                if( can_play_sound )   // modify pitch and volume with speed
                {
                    engineNoise.pitch = Mathf.Lerp(0.5f, 1.75f, Mathf.Min( speed_norm*2, 1) );
                    engineNoise.volume = Mathf.Lerp(0.05f, 0.1f, speed_norm);
                }
            }


            // NOTE: if we're still in the middle of a fade from a previous state, make sure to keep fading in
            _fadeBackProgress = Mathf.Clamp(_fadeBackProgress + Time.fixedDeltaTime / fadeBackTime, 0.5f, 1.0f);   // "fade in" starts at 0.5f
            float fade_lerp = (_fadeBackProgress - 0.5f) * 2;  // from 0 to 1 in this phase
            idleSprite.color = new Color(1, 1, 1, fade_lerp);
            frontSprite.color = new Color(1, 1, 1, fade_lerp);  
            backSprite.color = new Color(1, 1, 1, fade_lerp);
        }
        else   // SITTING IDLE & EMPTY
        {
            _driveInProgress = 0.0f;

            if( engineNoise.isPlaying )
                engineNoise.Stop();

            if( _lastPlr != null )
            {
                _lastPlr.playerAnimations.meshRenderer.enabled = true;   //show previous player again, they got out of the car
                _lastPlr.playerAlternateMovements.currentMovement = -1;   // Undo Go Kart movement
                _lastPlr.playerCollisions.EnableColliders(true);

                _fadeBackProgress = 0.0f;   //start fade animation

                _lastPlr = null;
            }


            // Just camp at the SitPoint
            //transform.position = entryPoint.transform.position;
            //SetDirection(-1, false);

            _fadeBackProgress = Mathf.Min(1.0f, _fadeBackProgress + Time.fixedDeltaTime / fadeBackTime);

            // Actually, we'll need to smoothly fade out, then fade in again at the SitPoint.
            if( _fadeBackProgress < 0.5f )
            {
                float fade_lerp = _fadeBackProgress * 2;   // from 0 to 1 in this phase

                transform.position = transform.position + (Vector3)_fadeAnimCoastVelocity * Time.fixedDeltaTime;  // keep moving!! looks cool
                frontSprite.color = new Color(1, 1, 1, 1-fade_lerp);
                backSprite.color = new Color(1, 1, 1, 1-fade_lerp);
            }
            else   // fade back in to idle spot
            {
                float fade_lerp = (_fadeBackProgress - 0.5f) * 2;  // from 0 to 1 in this phase

                transform.position = entryPoint.transform.position;
                SetDirection(-1, false);
                idleSprite.color = new Color(1, 1, 1, fade_lerp);
                frontSprite.color = new Color(1, 1, 1, 1);  // reset these
                backSprite.color = new Color(1, 1, 1, 1);
            }
        }
    }

	private void LateUpdate()
	{
        if( _followInLateUpdate )   // Should be following player. Here is the best place to do it lag-free
        {
            transform.position = _followInLateUpdate.position;    
            //entryPoint.returnTransform.position = transform.position;   // Move the SitPoint's return transform so we hop out right where this car is when we're done.		
            // Actually, don't... since player's can't go on the track, they need to hop all the way out of the track when they're done.
        }
	}


	private void OnDrawGizmos()
	{
        // Draw the point we'll drive in to.
		Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere( transform.position + (Vector3)driveInPointOffset, 1.0f );
	}

}
