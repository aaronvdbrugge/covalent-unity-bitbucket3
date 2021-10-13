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


    [Header("Settings")]
    [Tooltip("Minimum smoke particles to spawn")]
    public float minParticleRate = 20.0f;

    [Tooltip("Maximum smoke particles to spawn (at max speed)")]
    public float maxParticleRate = 60.0f;




    Player_Controller_Mobile _lastPlr;   //must remember to show their mesh renderer again before we do away with them

    // No sense in leaving these running while sprite renderer is disabled
    Animator _frontSpriteAnimator;
    Animator _backSpriteAnimator;



	private void Awake()
	{
		_frontSpriteAnimator = frontSprite.GetComponent<Animator>();
        _backSpriteAnimator = backSprite.GetComponent<Animator>();
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



    void LateUpdate()
    {
        if( entryPoint.occupyingActor != -1 && Player_Controller_Mobile.fromActorNumber.ContainsKey(entryPoint.occupyingActor) )   // Someone is sitting in this kart!!
        {
            Player_Controller_Mobile plr = Player_Controller_Mobile.fromActorNumber[entryPoint.occupyingActor];   // Retrieve the player object sitting in the kart
            _lastPlr = plr;

            if( plr.playerHop.hopProgress <= 0 )  // hop is complete! ready to go
            {
                plr.playerAnimations.meshRenderer.enabled = false;   // Hide the actual player. They're a car now.

                transform.position = plr.transform.position;    // Our visual follows the player wherever

                entryPoint.returnTransform.position = transform.position;   // Move the SitPoint's return transform so we hop out right where this car is when we're done.


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
                float speed_norm = plr.playerMovement.body.velocity.magnitude / plr.playerMovement.maxSpeed;

                var emission = frontParticles.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve( Mathf.Lerp(minParticleRate, maxParticleRate, speed_norm) );

                emission = backParticles.emission;
                emission.rateOverTime = new ParticleSystem.MinMaxCurve( Mathf.Lerp(minParticleRate, maxParticleRate, speed_norm) );
            }
        }
        else
        {
            // Just camp at the SitPoint
            transform.position = entryPoint.transform.position;
            SetDirection(-1, false);

            if( _lastPlr != null )
            {
                _lastPlr.playerAnimations.meshRenderer.enabled = true;   //show previous player again, they got out of the car
                _lastPlr = null;
            }
        }
    }
}
