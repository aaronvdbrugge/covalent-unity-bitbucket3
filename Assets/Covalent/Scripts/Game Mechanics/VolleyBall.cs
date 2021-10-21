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
    [Tooltip("Nested under us. We'll rotate it")]
    public SpriteRenderer ballSprite;     

    [Tooltip("shows where the ball will land, and changes color")]
    public SpriteRenderer indicatorSprite;  

    [Tooltip("shows where the ball will land, and changes size and color. Should be parented under indicatorSprite")]
    public SpriteRenderer ringSprite;    


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

    [Tooltip("In this many hits, it'll go from arcTimeSlow to arcTimeFast.")]
    public int maxSpeedupHits = 10;

    [Tooltip("We'll pull from this gradient to change the color of the indicator ring.")]
    public Gradient indicatorRingColors;

    public float indicatorRingStartScale = 1.0f;
    public float indicatorRingEndScale = 0.5f;


    [Tooltip("Step in the indicator. Where we ant to actually stand is a little bit before where the ball will hit. 1.0 = no step")]
    public float indicatorStepRatio = 0.9f;


    [Tooltip("Place ball on southern corner. Gizmos will help you find proper value here.")]
    public Vector2 arenaNorthWest = Vector2.left;
    [Tooltip("Place ball on southern corner. Gizmos will help you find proper value here.")]
    public Vector2 arenaNorthEast = Vector2.up;


    [Tooltip("Ball must land at least this far from net.")]
    public float netPadding = 1.0f;


    [Tooltip("We'll set rotation to a random vaue on hit, with this being the maximum (rotations per second).")]
    public float rotationMax = 4;


    [Tooltip("Height to bounce when we hit thr ground.")]
    public float bounceHeight = 1.0f;

    [Tooltip("Time our ground bounce animation takes.")]
    public float bounceTime = 0.33f;

    [Tooltip("If an arc has over this amount remaining when hit, it's considered a \"spike\" and will play a louder sound.")]
    public float spikeSoundThreshold = 0.33f;


    [Header("Runtime (Network Replicated)")]
    public Vector2 lerpStart;
    public Vector2 lerpEnd;
    public float lerpProgress = 1;   // make sure to start this at 1 so the ball doesn't move at the start
    public int hitStreak;
    public float spriteRotation = 0.0f;   // even though this is strictly cosmetic, it's nice to replicate it so all users experience the same thing.



    public float zPos => GetZPos();   // this will be calculated via lerpStart / lerpEnd / lerpProgress


    // Internal references
    IsoSpriteSorting _ballSpriteSorter;

    //Start values
    Vector2 _originalPosition;
    float _ballSpriteYOriginal;   // so we can move the ball sprite by zpos
    float _ballSortingYOriginal;   // need to move the ball's sorting point counter to its height
    
    bool _netInitialized = false;   // Need to wait until we join a room. Either we own this object, or we we got a response from RequestBallState
    float _lastAskedForState = 0.0f;    // If we don't own this, this is Time.time when we last asked for state. Don't count on a response if the master client disconnected; will have to try again.


    float _bounceAnimCooldown;  // just do a single bounce when we hit the ground. goes from 1 to 0



	private void Awake()
	{
        _ballSpriteSorter = ballSprite.GetComponent<IsoSpriteSorting>();
    }

	private void Start()
	{
        _originalPosition = transform.position;
        _ballSpriteYOriginal = ballSprite.transform.localPosition.y;
        _ballSortingYOriginal = _ballSpriteSorter.SorterPositionOffset.y;

        // Place it in the middle of south side...
        transform.position = transform.localToWorldMatrix.MultiplyPoint( arenaNorthEast/2 + arenaNorthWest/4 );
        lerpProgress = 1.0f;
        lerpEnd = transform.position;
	}


	void OnTriggerStay2D(Collider2D other)
    {
        // NOTE: a ball can only be hit if it has passed over the net, to the opposite side of where it started.
        // If not, its collisions are irrelevant.
        if( IsOnNorthSide( lerpStart ) == IsOnNorthSide( transform.position ) && lerpProgress < 1)
        {
            Debug.Log("Can't hit ball: " + lerpStart + " -> " + IsOnNorthSide( lerpStart ) + ", " + transform.position + " -> " + IsOnNorthSide( transform.position ));
            return;    // We're still leaving our original side. 
        }

        //  ^^ this early return also prevents multi-hits.



        Player_Controller_Mobile pcm = other.GetComponent<Player_Controller_Mobile>();

        
        if( pcm != null && photonView.IsMine && Dateland_Network.initialized )   // Only the owner can decide the random position at which the ball will go next.
        {
            //Prevent double-spike bug where you can hit it from the other side of the net.
            if( IsOnNorthSide( pcm.transform.position ) != IsOnNorthSide( transform.position ) )
                return;

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
                //transform.position = GetRandomLandingPosition( !IsOnNorthSide(transform.position) );   // flip side back and forth

                Vector2 new_lerp_start = transform.position;
                Vector2 new_lerp_end = GetRandomLandingPosition( !IsOnNorthSide(transform.position) );   // flip side back and forth;

				// If lerp progress didn't fully make it to 1.0, we have to artificially step lerp start backward so that progress can just be set to 1 - progress,
				// keeping its Z position, but on the opposite side. This also has the added benefit of letting players "spike" the ball, so that if they hit it high in
				// its arc, it will take less time to reach the opponent's side.
                float new_lerp_progress = 0;
				if (lerpProgress < 1)
				{ 
                    new_lerp_progress = 1 - lerpProgress;

                    // Step new_lerp_start such that transform.position is new_lerp_progress of the way from new_lerp_start to new_lerp_end!
                    //  t.p = s + (e-s) * p
                    // t.p = s + ep - sp
                    // t.p = s(1-p) + ep
                    // t.p - ep = s(1-p)
                    //  s = (t.p - ep) / (1 - p)
                    new_lerp_start = ((Vector2)transform.position - new_lerp_end * new_lerp_progress) / (1 - new_lerp_progress);
                }

                float new_rotation_speed = Random.Range(-360.0f, 360.0f) * rotationMax;

                photonView.RPC("HitBall", RpcTarget.All, new object[] { new_lerp_start, new_lerp_end, new_lerp_progress, hitStreak+1, new_rotation_speed });    // Will execute on us as well... incrementing hitStreak and preventing double-hits
            }
        }
    }


    void FixedUpdate()
    {
        if( !_netInitialized )
        {
            if( Dateland_Network.initialized )  // otherwise, wait for room join
            {
                if( !photonView.IsMine && Time.time - _lastAskedForState >= 0.5f)   // Can only ask for state every half second. The only likely reason we'd need to ask more than once is if the master client disconnected.
                {
                    //Need to request the ball's actual state! We just joined, and these requests are sent sparsely. Supplying our ActorNumber means it only needs to be sent to us.
                    photonView.RPC("RequestBallState", RpcTarget.MasterClient, new object[] { PhotonNetwork.LocalPlayer.ActorNumber });
                    _lastAskedForState = Time.time;
                }
                else
                    _netInitialized = true;   //if we own it, this is all we needed.
            }
        }
        else
        {

            if( lerpProgress >= 1.0f && hitStreak > 0)  // Ball made it to the ground without getting hit...
            {
                if( photonView.IsMine && Dateland_Network.initialized )  // we can reset the hit streak
                {
                    hitStreak = 0;  //set this immediately... avoid possibility of repeat calls
                    photonView.RPC("ResetHitStreak", RpcTarget.All );
                }
            }

            // Animate the volleyball from point A to point B. This affects gameplay
            transform.position = Vector2.Lerp( lerpStart, lerpEnd, lerpProgress );
		    lerpProgress = Mathf.Min(1.0f, lerpProgress + Time.fixedDeltaTime / GetTotalArcTime() );


            //Update "bounce" animation after it hits the ground.
            _bounceAnimCooldown = Mathf.Max(0.0f, _bounceAnimCooldown - Time.fixedDeltaTime / bounceTime);

            // "Cheat code": in editor, can hit V to go to lerpEnd.
            if( Application.isEditor && Input.GetKey(KeyCode.V) )
            {
                foreach( var player_controller in FindObjectsOfType<Player_Controller_Mobile>() )
                    if( player_controller.photonView.IsMine )
                        player_controller.transform.position = lerpEnd;
            }
        }
    }


    /// <summary>
    /// Similar to Player_Hop.GetCurrentHopHeight.
    /// </summary>
    public float GetZPos()
    {
		// We want a parabola that goes from y=0 to 1 to 0, from x=0 to y=1.
		// Should be:    y = -(2x - 1)^2 +1
		//    https://www.desmos.com/calculator/qphzqwrput
		float hop_parabolic = -Mathf.Pow(2 * lerpProgress - 1, 2) + 1;

		float ret = hop_parabolic * arcHeight;   // de-normalize


        // NOTE: add in bounce animation as well. Keeping it here will ensure no stutter if they hit it mid-bounce
        float bounce_norm = 1 - _bounceAnimCooldown;  // goes from 0 to 1 as bounce anim progresses
        float bounce_parabolic = -Mathf.Pow(2 * bounce_norm - 1, 2) + 1;
        ret += bounce_parabolic * bounceHeight;

        return ret;
    }

    /// <summary>
    /// Similar to Player_Hop.GetCurrentHopVelocity.
    /// </summary>
	public float GetCurrentHopVelocity()
	{
		// Best way to do this would be calculus?
		// I believe it would be:
		/*
		y = -(2x - 1)^2 + 1
		y = -(4x^2 - 4x + 1) + 1
		y = -4x^2 + 4x

		..multiply by hopheight...
		y = -4hx^2 + 4hx

		derivitave
		y' = -8hx + 4h
		 */
		return -8 * arcHeight * lerpProgress + 4 * arcHeight;
	}


    /// <summary>
    /// NOTE: This is for cosmetic stuff.
    /// Any events that affect actual gameplay should go in FixedUpdate.
    /// </summary>
	private void Update()
	{
        indicatorSprite.gameObject.SetActive(false);   // ring is nested under indicator.

        if( lerpProgress < 1 )
        {
            // show where it'll land!
            indicatorSprite.transform.position = Vector3.Lerp(lerpStart, lerpEnd, indicatorStepRatio);   // want to stand a bit before where it'll hit the ground
            indicatorSprite.gameObject.SetActive(true);   

            // Change color
            Color sprite_color = indicatorRingColors.Evaluate( lerpProgress );
            indicatorSprite.color = sprite_color;
            ringSprite.color = sprite_color;
            
            //Scale ring
            ringSprite.transform.localScale = Mathf.Lerp(indicatorRingStartScale, indicatorRingEndScale, lerpProgress) * Vector2.one;
        }



        // Rotate sprite based on _spriteRotation speed
        ballSprite.transform.localRotation = Quaternion.Euler( 
            new Vector3(
                ballSprite.transform.localRotation.eulerAngles.x,
                ballSprite.transform.localRotation.eulerAngles.y,  
                ballSprite.transform.localRotation.eulerAngles.z - spriteRotation * Time.deltaTime
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
    /// Ball was hit by any player, and should start a new arc.
    /// </summary>
    [PunRPC]
    public void HitBall(Vector2 lerpStart, Vector2 lerpEnd, float lerpProgress, int hitStreak, float spriteRotation)
    {
        bool was_initialized = _netInitialized;    // can only play sounds if we're initialized, otherwise it could play a sound when we shouldn't
        _netInitialized = true;  // if we don't own the ball, we have at least gotten one update on its wherabouts.

        this.lerpStart = lerpStart;
        this.lerpEnd = lerpEnd;
        this.lerpProgress = lerpProgress;
        this.hitStreak = hitStreak;
        this.spriteRotation = spriteRotation;

        
        if( was_initialized )
        {
            // We can choose to play either a light hit or heavy hit.
            if( lerpProgress >= spikeSoundThreshold )  // we're starting so late in the arc that it must be a "spike"
                Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( "volleyball_heavy_hit", transform.position );
            else
                Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( "volleyball_light_hit", transform.position );
        }
    }

    /// <summary>
    /// Ball hit the ground.
    /// </summary>
    [PunRPC]
    public void ResetHitStreak()
    {
        hitStreak = 0;
        spriteRotation = 0;  // hit the ground, not rotating anymore
        _bounceAnimCooldown = 1.0f;   //plays bounce animation

        //Play sound, and spawn confetti! Landing on the ground means somebody "won."
        Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( "volleyball_sand", transform.position );


        Vector2 nw_trans = transform.TransformVector( arenaNorthWest );
        Vector2 ne_trans = transform.TransformVector( arenaNorthEast );
        Vector2 confetti_spawn = _originalPosition + ne_trans/2 + nw_trans/4;   // Center of south arena
        if( !IsOnNorthSide( transform.position ) )
            confetti_spawn += nw_trans/2;   // move to north side

        Instantiate(confettiPrefab, confetti_spawn, Quaternion.identity);
    }
    

    /// <summary>
    /// Called when a new player joins, this will request an extra HitBall RPC to be sent out.
    /// </summary>
    [PunRPC]
    public void RequestBallState(int requesting_player_actor_num)
    {
        // We should be the owner of this ball, so if we could send an update to just the requesting player, that'd be ideal
        if( photonView.IsMine && Dateland_Network.initialized )  //just in case, though this should always be true
        {
            Photon.Realtime.Player player = PhotonUtil.GetPlayerByActorNumber( requesting_player_actor_num );               // Get the player we want to send it to...
            if( player != null )
                photonView.RPC("HitBall", player, new object[] { lerpStart, lerpEnd, lerpProgress, hitStreak, spriteRotation });    // Give them the info they requested
        }
    }


    /// <summary>
    /// Get our total arc time based on hitStreak value  (it speeds up as hitStreak progresses)
    /// </summary>
    public float GetTotalArcTime()
    {
        return Mathf.Lerp(arcTimeSlow, arcTimeFast, Mathf.Min(1.0f, hitStreak / (float)maxSpeedupHits) );
    }

    /// <summary>
    /// Gets speed in XY plane for this lerp, ignoring Z
    /// </summary>
    public Vector2 GetXYSpeed()
    {
        return (lerpEnd - lerpStart) / GetTotalArcTime();
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
        Vector2 ne_trans = transform.TransformVector( arenaNorthEast );

        Vector2 diff = (Vector2)pos - (_originalPosition + nw_trans/2 - ne_trans*10);   // Scoot it back far enough that the ball can't possibly go behind it
        float pos_ang = Mathf.Atan2( diff.y, diff.x );

        float net_ang = Mathf.Atan2( ne_trans.y, ne_trans.x );

        return Mathf.DeltaAngle( net_ang, pos_ang ) > 0;
    }



    void OnDrawGizmos()
    {
        // Visualize collision height
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3( 0, collisionHeight, 0 ) );

        Vector3 origin = transform.position;
        if( Application.isPlaying )
            origin = _originalPosition;
        
        //Visualize arena
        Gizmos.color = Color.blue;
        Vector3 arena_northwest_world = transform.TransformVector( arenaNorthWest );   // take scaling into account, just in case
        Vector3 arena_northeast_world = transform.TransformVector( arenaNorthEast );  


        //Draw all the boundaries
        Gizmos.DrawLine(origin, origin + arena_northwest_world );
        Gizmos.DrawLine(origin, origin + arena_northeast_world );
        Gizmos.DrawLine(origin + arena_northwest_world, origin + arena_northwest_world + arena_northeast_world);
        Gizmos.DrawLine(origin + arena_northeast_world, origin + arena_northwest_world + arena_northeast_world);

        //Draw the halfway line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin + arena_northwest_world/2, origin + arena_northeast_world + arena_northwest_world/2);
        

        if( Application.isPlaying )
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
