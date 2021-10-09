using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// NOTE: There may be some code overlap here with BouncyBall.
/// See BouncyBall.cs for an explanation of why I'm not currently bothering to try and have them share code.
/// 
/// From spec
/// ---------
/// This is a perpetual game, there is no start, end, winners, losers, score
///	colliding into the ball makes it fly to a random location on the other side of the court.
///	An indicator (shadow) shows where the ball will land
/// A ring with radius greater than the shadow closes in on the shadow and reaches the same radius as the shadow when the ball will hit the ground
///	If someone is standing under the ball when it lands, it flies to the other side
///	The speed of the flight trajectory speeds up with each successful hit
///	When the ball hits the ground, the round is over
///	collide into the ball again to start a new round
///	
/// Seb's addition:
/// If you can hop and touch the ball in midair you'll "spike" it, this just means the ball will start its next motion at the point in the parabola
/// it was at when you hit it (giving opponent less time to respond)
/// 
/// 
/// Implementation note:
/// We can probably get away with not replicating x / y / zPos / zVel on the fly.
/// It'll be less network activity if we just replicate on each ball hit:
/// Vector2 lerpStart
/// Vector2 lerpEnd
/// float speedMultiplier
/// float lerpProgress    <---- NOTE: this may not always be 0 on hit. If they "spike" the ball, we'll start at an increased amount, and we'll have to artificially
///                                 step back ballLerpStart to compensate for the progress skip.
///                                 
/// The ball will always fly at the same height, so lerpStart / lerpEnd / lerpProgress is enough info to infer its position in 3D space.
/// If nobody hits the ball, confetti will spray on winner's side, and ballLerpProgress will remain at 1.
/// 
/// 
/// POSITIONING:
/// Place this ball on the southern corner of the arena, then set arenaNorthWest and arenaNorthEast vectors.
/// The gizmos will help you line up the actual arena boundaries.
/// </summary>
public class VolleyBall : MonoBehaviourPun
{
    [Header("Internal references")]
    public SpriteRenderer ballSprite;     // may want to rotate this, or change its color.

    [Header("External references")]
    public GameObject confettiPrefab;   //if non-null, we'll instantiate this on goal

    [Header("Settings")]
    [Tooltip("If our base is an ellipse, then imagine out collision bound as a cylinder extending upward by this amount.")]
    public float collisionHeight = 1.0f;
    
    [Tooltip("First hit takes this long to reach other side")]
    public float arcTimeSlow = 4.0f;

    [Tooltip("Lowest amount it could possibly take to reach other side.")]
    public float arcTimeFast = 1.0f;

    [Tooltip("Hits are always the same height.")]
    public float arcHeight = 5.0f;

    [Tooltip("Place ball on southern corner. Gizmos will help you find proper value here.")]
    public Vector2 arenaNorthWest = Vector2.left;
    [Tooltip("Place ball on southern corner. Gizmos will help you find proper value here.")]
    public Vector2 arenaNorthEast = Vector2.up;


    [Tooltip("Ball must land at least this far from net.")]
    public float netPadding = 1.0f;




    [Header("Runtime (Network Replicated)")]
    public Vector2 lerpStart;
    public Vector2 lerpEnd;
    public float lerpProgress;
    public float speedMultiplier;




    public float zPos => 0;   // this will be calculated via lerpStart / lerpEnd / lerpProgress


    // Internal references
    IsoSpriteSorting _ballSpriteSorter;

    //Start values
    Vector2 _originalPosition;
    float _ballSpriteYOriginal;   // so we can move the ball sprite by zpos
    float _ballSortingYOriginal;   // need to move the ball's sorting point counter to its height
    float _linearDragOriginal;   //we'll set linear drag to 0 in the air

    // Runtime
    float _spriteRotation = 0.0f;


    Player_Controller_Mobile lastEnteredTrigger;   // this makes it easier for us to play only one sound when the player is overlapping the ball



    /// <summary>
    /// Handles member synchronization. We just use this for pseudo-3D z positioning right now
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // TBD: on hit, serialize lerpStart, lerpEnd, lerpProgress, and speedMultiplier.
        // We could consider using an RPC for this instead, probably would be more responsive.

        //if (stream.IsWriting)
        //{
        //    stream.SendNext(zPos);
        //    stream.SendNext(zVel);
        //}
        //else
        //{
        //    zPos = (float)stream.ReceiveNext();
        //    zVel = (float)stream.ReceiveNext();
        //}
    }



	private void Awake()
	{
        _ballSpriteSorter = ballSprite.GetComponent<IsoSpriteSorting>();
    }

	private void Start()
	{
        _originalPosition = transform.position;
        _ballSpriteYOriginal = ballSprite.transform.localPosition.y;
        _ballSortingYOriginal = _ballSpriteSorter.SorterPositionOffset.y;
	}



	void OnTriggerEnter2D(Collider2D other)
	{
        Player_Controller_Mobile pcm = other.GetComponent<Player_Controller_Mobile>();
	    if( pcm != null )
            lastEnteredTrigger = pcm;  //remember this, so we can know to play a sound in OnTriggerStay2D
	}


	void OnTriggerStay2D(Collider2D other)
    {
        Player_Controller_Mobile pcm = other.GetComponent<Player_Controller_Mobile>();

        
        if( pcm != null && photonView.IsMine )   // Only the owner can decide the random position at which the ball will go next.
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
                // Test randomization
                transform.position = GetRandomLandingPosition( !IsOnNorthSide(transform.position) );   // flip side back and forth

                // TBD: start random lerp to other side of arena
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
        _ballSpriteSorter.SorterPositionOffset.y = _ballSortingYOriginal - (zPos / _ballSpriteSorter.transform.localScale.y);
	}



    /// <summary>
    /// Gets a random landing position on either the north side or south side of the arena,
    /// taking net padding into account.
    /// </summary>
    Vector2 GetRandomLandingPosition(bool north_side)
    {
        Vector2 nw_trans = transform.TransformVector( arenaNorthWest );   // take scaling into account, just in case
        Vector2 ne_trans = transform.TransformVector( arenaNorthEast );   


        Vector2 start = _originalPosition;   //starts at southern corner
        Vector2 v1 = nw_trans.normalized * (nw_trans.magnitude/2 - netPadding);   // subtract that inset from area
        Vector2 v2 = ne_trans;

        if( north_side )
            start += nw_trans / 2 + nw_trans.normalized * netPadding;   // start in northern half, at a point away from net
            
        return start + Random.value * v1 + Random.value * v2;
    }


    /// <summary>
    /// Detect if a position is on the north side of the arena.
    /// </summary>
    bool IsOnNorthSide(Vector2 pos)
    {
        // Can't use dot product because the two vectors aren't orthogonal.
        // I think the easiest way is to take atan2 to the southwest corner of the net,
        // then compare this angle to the atan2 angle of arenaNorthEast.
        Vector2 nw_trans = transform.TransformVector( arenaNorthWest );   // take scaling into account, just in case

        Vector2 diff = (Vector2)transform.position - (_originalPosition + nw_trans/2 );
        float pos_ang = Mathf.Atan2( diff.y, diff.x );

        float net_ang = Mathf.Atan2( arenaNorthEast.y, arenaNorthEast.x );

        return Mathf.DeltaAngle( net_ang, pos_ang ) > 0;
    }



    void OnDrawGizmos()
    {
        // Visualize collision height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3( 0, collisionHeight, 0 ) );

        if( !Application.isPlaying )
        {
            //Visualize arena
            Gizmos.color = Color.blue;
            Vector3 arena_northwest_world = transform.TransformVector( arenaNorthWest );   // take scaling into account, just in case
            Vector3 arena_northeast_world = transform.TransformVector( arenaNorthEast );  


            //Draw all the boundaries
            Gizmos.DrawLine(transform.position, transform.position + arena_northwest_world );
            Gizmos.DrawLine(transform.position, transform.position + arena_northeast_world );
            Gizmos.DrawLine(transform.position + arena_northwest_world, transform.position + arena_northwest_world + arena_northeast_world);
            Gizmos.DrawLine(transform.position + arena_northeast_world, transform.position + arena_northwest_world + arena_northeast_world);

            //Draw the halfway line
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + arena_northwest_world/2, transform.position + arena_northeast_world + arena_northwest_world/2);
        }
        else
        {
            // In play mode, highlight red on north side
            if( IsOnNorthSide( transform.position ) )
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere( transform.position, 1.0f );
            }
        }
    }
}
