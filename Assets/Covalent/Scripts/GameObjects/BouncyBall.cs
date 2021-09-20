using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This script will be used for both soccer ball and volleyball.
/// 
/// We do not actually use rigidbody pushout / velocity, but just detect
/// when we overlap a player, and handle velocity on our own.
/// 
/// We also handle pseudo-3D height changes (where the ball moves up but its
/// shadow stays on the ground), and only process the collision between player
/// and ball if they are at similar heights (taking player hops into account).
/// </summary>
public class BouncyBall : MonoBehaviourPun
{
    [Header("Internal references")]
    public SpriteRenderer ballSprite;     // may want to rotate this, or change its color.

    [Header("Settings")]
    [Tooltip("This is multiplied to normalized joystick value when the player touches the ball.")]
    public float velocityMultiplier = 1.0f;

    [Tooltip("Applied to rigidbody.velocity.x, more = faster rotation of sprite")]
    public float spriteRotationMultiplier = 1.0f;

    [Tooltip("On score / out of bounds, it will return to original spot at this height, then dorp down.")]
    public float startingHeight = 10.0f;
    
    [Tooltip("Applied to height in fixedUpdate, units per second per second.")]
    public float gravity = 1.0f;

    [Tooltip("On hitting ground, zVel gets set to zVel * -zBounce")]
    public float zBounce = 0.25f;



    [Header("Runtime")]
    [Tooltip("Pseudo-3D height in the isometric world. Will make the soccer ball sprite hover above its shadow.")]
    public float zPos;

    [Tooltip("Pseudo-3D height velocity, units per second per second.")]
    public float zVel;


    // Internal references
    Rigidbody2D body;
    IsoSpriteSorting _ballSpriteSorter;

    //Start values
    Vector2 _originalPosition;
    float _ballSpriteYOriginal;   // so we can move the ball sprite by zpos
    float _ballSortingYOriginal;   // need to move the ball's sorting point counter to its height

    // Runtime
    float _spriteRotation = 0.0f;


    public enum RespawnType
    {
        OutOfBounds,
        Goal
    }


    /// <summary>
    /// Handles member synchronization. We just use this for pseudo-3D z positioning right now
    /// </summary>
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(zPos);
            stream.SendNext(zVel);
        }
        else
        {
            zPos = (float)stream.ReceiveNext();
            zVel = (float)stream.ReceiveNext();
        }
    }


	private void Awake()
	{
		body = GetComponent<Rigidbody2D>();
        _ballSpriteSorter = ballSprite.GetComponent<IsoSpriteSorting>();
    }

	private void Start()
	{
		zPos = startingHeight;
        _originalPosition = transform.position;
        _ballSpriteYOriginal = ballSprite.transform.localPosition.y;
        _ballSortingYOriginal = _ballSpriteSorter.SorterPositionOffset.y;
	}



    /// <summary>
    /// Will eventually smoothly animate the ball back to its original position.
    /// For now, it just instantly goes back.
    /// </summary>
    public void Respawn(RespawnType type)
    {
        body.velocity = Vector2.zero;
        body.position = _originalPosition;
        zPos = startingHeight;
    }


	void OnTriggerEnter2D(Collider2D other)
    {
        Player_Controller_Mobile pcm = other.GetComponent<Player_Controller_Mobile>();


        // We ONLY respond to overlaps with My player. Other players will apply their own velocities
        // through this function, which will be mirrored through Pun's rigidbody replication.
        // It's nicer this way, because we can use the exact joystick position to accurately aim the ball,
        // which is info that isn't network replicated.
        // We also don't want every player to pile on top of eachother to update the change in velocity;
        // Only one should send it out, and the rest just respond.
        if( pcm != null && pcm.photonView.IsMine )
        {
            Player_Movement pm = other.GetComponent<Player_Movement>();
            body.velocity = pm.lastMovementInput * velocityMultiplier;
        }
    }


    /// <summary>
    /// NOTE: This is for cosmetic stuff.
    /// Any events that affect actual gameplay should go in FixedUpdate.
    /// </summary>
	private void Update()
	{
	    _spriteRotation = body.velocity.x * spriteRotationMultiplier;   //stash this, because we'll want to keep spinning if we go out of bounds etc
        
        ballSprite.transform.localRotation = Quaternion.Euler( 
            new Vector3(
                ballSprite.transform.localRotation.eulerAngles.x,
                ballSprite.transform.localRotation.eulerAngles.y,  
                ballSprite.transform.localRotation.eulerAngles.z - _spriteRotation * Time.deltaTime
            )
        );

        // Move the ball sprite up and down, independent of shadow...
        ballSprite.transform.localPosition = new Vector2(
                ballSprite.transform.localPosition.x,
                _ballSpriteYOriginal + zPos
            );

        // Move the ball's sorting offset
        _ballSpriteSorter.SorterPositionOffset.y = _ballSortingYOriginal - zPos;
	}


	private void FixedUpdate()
	{        
		zPos += zVel * Time.fixedDeltaTime;
        zVel += gravity * Time.fixedDeltaTime;
        
        if( zPos < 0 )
        {
            zPos *= -1;
            zPos *= zBounce;   //should take care of the distance it travelled after "bouncing" this frame
            if( zVel < 0 )
                zVel *= -1 * zBounce;    // zVel is positive now, but reduced via zBounce
        }
	}
}
