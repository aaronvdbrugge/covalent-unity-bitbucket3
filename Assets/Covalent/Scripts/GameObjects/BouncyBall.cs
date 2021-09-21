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
    [Tooltip("In an isometric world, y velocity needs to be scaled down.")]
    public float yVelocityScale = 0.5f;

    [Tooltip("If our base is an ellipse, then imagine out collision bound as a cylinder extending upward by this amount.")]
    public float collisionHeight = 1.0f;

    [Tooltip("This is multiplied to normalized joystick value when the player touches the ball.")]
    public float velocityMultiplier = 1.0f;

    [Tooltip("This is multiplied to last direction when the player jumps up into a ball.")]
    public float headVelocity = 1.0f;

    [Tooltip("If kicked this percent of max, we'll add some up speed to it.")]
    public float upKickThreshold = 0.8f;

    [Tooltip("Up speed we add if kicked past upKickThreshold")]
    public float upKickSpeed = 5.0f;

    [Tooltip("Applied to rigidbody.velocity.x, more = faster rotation of sprite")]
    public float spriteRotationMultiplier = 1.0f;

    [Tooltip("On score / out of bounds, it will return to original spot at this height, then dorp down.")]
    public float startingHeight = 10.0f;
    
    [Tooltip("Applied to height in fixedUpdate, units per second per second.")]
    public float gravity = 1.0f;

    [Tooltip("On hitting ground, zVel gets set to zVel * -zBounce")]
    public float zBounce = 0.25f;

    [Tooltip("Total time it takes for the ball to respawn")]
    public float respawnAnimationTime = 3.0f;

    [Tooltip("Total time (subset of respawnAnimationTime) the ball is frozen in place before it lerps to respawn point.")]
    public float respawnFreezeTime = 1.5f;

    [Tooltip("Color for out of bounds animation")]
    public Color outOfBoundsColor;

    [Tooltip("Color for Goal animation")]
    public Color goalColor;


    [Header("Runtime")]
    [Tooltip("Pseudo-3D height in the isometric world. Will make the soccer ball sprite hover above its shadow. (network replicated)")]
    public float zPos;

    [Tooltip("Pseudo-3D height velocity, units per second per second. (network replicated)")]
    public float zVel;

    [Tooltip("If we are doing an out-of-bounds respawn or goal respawn. (not network replicated, uses an RPC instead)")]
    public RespawnType respawning;

    [Tooltip("Progress of the respawn. Starts at 0, goes to respawnAnimationTime. Not network replicated, ideally RespawnType should be sufficient?")]
    public float respawnProgress;


    // Internal references
    Rigidbody2D body;
    IsoSpriteSorting _ballSpriteSorter;

    //Start values
    Vector2 _originalPosition;
    float _ballSpriteYOriginal;   // so we can move the ball sprite by zpos
    float _ballSortingYOriginal;   // need to move the ball's sorting point counter to its height
    float _linearDragOriginal;   //we'll set linear drag to 0 in the air

    // Runtime
    float _spriteRotation = 0.0f;
    Vector3 _respawnStartPos;   // where we started the respawn animation; includes pseudo-3D z position


    public enum RespawnType
    {
        None = 0,
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
        _linearDragOriginal = body.drag;
	}



    /// <summary>
    /// Will eventually smoothly animate the ball back to its original position.
    /// For now, it just instantly goes back.
    /// </summary>
    public void Respawn(RespawnType type)
    {
        if( respawning == RespawnType.None )   // try not to send duplicate RPCs.
            this.photonView.RPC("RespawnRPC", RpcTarget.All, new object[] { (int)type });
    }


    [PunRPC]
    public void RespawnRPC(int respawn_type)
    {
        if( respawning == RespawnType.None )   // try not to accept duplicate RPCs.
        {
            respawning  = (RespawnType)respawn_type;
            respawnProgress = 0.0f;   // animation should now start in Update

            body.velocity = Vector2.zero;
            _respawnStartPos = new Vector3( body.position.x, body.position.y, zPos);
        }
    }


	void OnTriggerStay2D(Collider2D other)
    {
        Player_Controller_Mobile pcm = other.GetComponent<Player_Controller_Mobile>();

        
        //if( pcm != null && pcm.photonView.IsMine )   //Nope, actually only one client at a time can control the ball. We have to confirm that we have the authority to control it.
        //if( pcm != null && photonView.IsMine )   // THIS photonView, NOT the player. So if a non-controlled player hits the ball, it's our responsibility as the owner to handle it.
        if ( pcm != null )   // Allow ball to be kicked on non-owning clients? This may prevent some lag
        {
            Player_Hop ph = pcm.playerHop;

            // Make sure they are actually overlapping in pseudo-3D.
            // Use the two cylinder heights...
            // Just check if the base of either is between the bounds of the other.
            float player_z = ph.zPos;  //this does involve some slight calculation
            bool collision = false;
            if( player_z >= zPos && player_z <= zPos + collisionHeight )
                collision = true;
            else if( zPos >= player_z && zPos <= player_z + ph.collisionHeight )
                collision = true;


            if( collision )
            {
                Player_Movement pm = pcm.playerMovement;
                Vector2 new_vel;

                Vector2 flat_velocity = body.velocity;   //un-skewed from isometric view...
                flat_velocity.y /= yVelocityScale;


                if( ph.zPos > 0 )  // Player is airborne. "Head" the ball at max speed, also give it the player's z velocity.
                {
                    zVel = ph.zVel;
                    float cur_spd = flat_velocity.magnitude;
                    new_vel =pm.lastDirection * Mathf.Max(cur_spd, headVelocity);   // lastDirection is always normalized. Hit it at headVelocity, or current speed, whatever's faster.
                }
                else if ( zPos > ph.collisionHeight / 2 )   // Ball is landing in the head area, bounce it in Z, and also try to "reflect" its rigidbody velocity
                {
                    if( zVel < 0 )
                        zVel *= -zBounce;    // zVel is positive now, but reduced via zBounce

                    Vector2 reflect_vector =  ph.transform.position - transform.position;

                    // un-apply isometric skew 
                    reflect_vector.y /= yVelocityScale;


                    // https://math.stackexchange.com/questions/13261/how-to-get-a-reflection-vector
                    reflect_vector = reflect_vector.normalized;
                    new_vel = flat_velocity - 2 * Vector2.Dot(flat_velocity, reflect_vector) * reflect_vector;
                }
                else  // Regular hit.
                {
                    Vector2 last_movement_input = pm.GetLastMovementInput();
                    new_vel = last_movement_input * velocityMultiplier;

                    if( last_movement_input.magnitude > upKickThreshold )   // Make this ball go up a bit; it was a hard kick
                        zVel = Mathf.Max( upKickSpeed, zVel );
                }


                new_vel.y *= yVelocityScale;   // apply isometric velocity scale
                body.velocity = new_vel;
            }
        }
    }


    /// <summary>
    /// NOTE: This is for cosmetic stuff.
    /// Any events that affect actual gameplay should go in FixedUpdate.
    /// </summary>
	private void Update()
	{
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
        if( respawning == RespawnType.None )
        {
            // "Ground" the ball if it's pretty much not going anywhere in Z
            if( zPos < 0.05f && zVel < 0.1f && zVel > -0.1f )
            {
                zPos = zVel = 0;
                body.drag = _linearDragOriginal;
            }
            else
            {
                body.drag = 0;   // no drag in the air!

		        zPos += zVel * Time.fixedDeltaTime;
                if( zPos > 0.05f )
                    zVel += gravity * Time.fixedDeltaTime;
        
                if( zPos < 0 )
                {
                    zPos = 0;
                    if( zVel < 0 )
                        zVel *= -zBounce;    // zVel is positive now, but reduced via zBounce
                }
            }

	        _spriteRotation = body.velocity.x * spriteRotationMultiplier;   //stash this, because we'll want to keep spinning if we do respawn animation
            ballSprite.color = Color.white;   // respawn animations color the ball!
        }
        else
        {
            // Do respawn animation!
            // We should freeze in place for a bit, then smoothly lerp to the respawn position.
            respawnProgress = Mathf.Min(respawnAnimationTime, respawnProgress + Time.fixedDeltaTime);

            float pos_lerp = 0;
            if( respawnProgress > respawnFreezeTime )
                pos_lerp = (respawnProgress - respawnFreezeTime) / (respawnAnimationTime- respawnFreezeTime);

            // We're lerping from _respawnStartPos to _originalPos, startingHeight
            Vector3 dest = new Vector3( _originalPosition.x, _originalPosition.y, startingHeight);
            float smooth_lerp = EasingFunction.EaseInOutQuad( 0, 1, pos_lerp );

            Vector3 result = Vector3.Lerp(_respawnStartPos, dest, smooth_lerp);

            //OK, now we can set position and zpos
            transform.position = new Vector2(result.x, result.y);
            zPos = result.z;

            //Set to a fun color depending on out of bounds or goal
            if( respawning == RespawnType.OutOfBounds )
                ballSprite.color = outOfBoundsColor;
            else if (respawning == RespawnType.Goal )
                ballSprite.color = goalColor;

            zVel = 0;
            if( respawnProgress >= respawnAnimationTime )
                respawning = RespawnType.None;   // Back in play!


            // Make sure velocity stays zero
            body.velocity =  Vector2.zero;
        }
	}


    void OnDrawGizmos()
    {
        // Visualize collision height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3( 0, collisionHeight, 0 ) );
    }
}
