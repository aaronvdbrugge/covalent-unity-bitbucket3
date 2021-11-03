using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class used to animate an object with a single animation
//Use overrideRandomTime boolean to toggle whether a random time should be waited
//or if the requested time should be waited
public class Animating_Object : MonoBehaviour
{
    
    [Tooltip("Makes the animation interval be non-random.")]
    public bool overrideRandomTime;

    [Tooltip("Can be triggered by collisions.")]
    public bool triggerAnimate = false;


    [Tooltip("Wait this long before we can be triggered via collision again.")]
    public float timeToWait;

    [Tooltip("If non blank, we'll play this animation periodically using Animator.Play()")]
    public string animationName = "animation";

    [Tooltip("If non blank, we'll set an animation trigger rather than actually playing an animation.")]
    public string animationTriggerName;


    [Header("Randomness")]
    public bool doRandomStart = false;
    public float randomRangeStart = 7;
    public float randomRangeEnd = 14;
 


    Animator anim;


    /// <summary>
    /// If we animate repeatedly, counts down to when we animate next.
    /// If we animate via trigger, counts down to when we can animate again.
    /// </summary>
    float timerToNextAnimation = 0;



    
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

	private void Start()
	{
		if( doRandomStart )
            //anim.ForceStateNormalizedTime( Random.value );
            anim.Play(0, 0, Random.value);  // see:   https://forum.unity.com/threads/why-the-is-this-useful-feature-animator-forcestatenormalizedtime-deprecated.473024/
	}

	private void Update()
	{
		if( !triggerAnimate && timerToNextAnimation <= 0 )
        {
            DoAnimate();

            // Reset timerToNextAnimation based on settings
            if (overrideRandomTime )   // same time every time
                timerToNextAnimation = timeToWait > 0 ? timeToWait : 10.0f;
            else
                timerToNextAnimation = Random.Range(randomRangeStart, randomRangeEnd);
        }

        timerToNextAnimation = Mathf.Max(0, timerToNextAnimation - Time.deltaTime);  //progress timer
	}


    private void OnTriggerEnter2D(Collider2D collision)
    {
        int player_uid = -1;
        Player_Controller_Mobile plr = collision.GetComponent<Player_Controller_Mobile>();
        if( plr )
            player_uid = plr.kippoUserId;

        TriggeredByTouch(player_uid);
    }

   private void OnCollisionEnter2D(Collision2D collision)
    {
        int player_uid = -1;
        Player_Controller_Mobile plr = collision.gameObject.GetComponent<Player_Controller_Mobile>();
        if( plr )
            player_uid = plr.kippoUserId;

        TriggeredByTouch(player_uid);
    }

    void DoAnimate()
    {
        //Time to animate! Play animation, or set trigger... whatever we want
        if( animationName != "" )
            anim.Play(animationName, -1, 0f);
        if( animationTriggerName != "" )
            anim.SetTrigger( animationTriggerName );
    }

    void TriggeredByTouch(int triggered_by)
    {
        SendMessage("SetTriggeredByUid", triggered_by, SendMessageOptions.DontRequireReceiver);   // to whoever cares. Maybe SelfSoundTrigger

        if (timerToNextAnimation <= 0)   // not allowed to animate unless this has cooled down.
        {
            DoAnimate();
            timerToNextAnimation = timeToWait;
        }
    }

}
