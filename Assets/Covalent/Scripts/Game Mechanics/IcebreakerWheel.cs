using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// A giant spinner which, when two players stand on the designated pads, will play a "spinning" animation,
/// then display a UI element with an "icebreaker" question.
/// </summary>
public class IcebreakerWheel : MonoBehaviourPun, IPunObservable
{
    [Header("Internal references")]
    [Tooltip("Fades in when footpad 1 is being stood on.")]
    public SpriteRenderer footpad1OnSprite;

    [Tooltip("Fades in when footpad 2 is being stood on.")]
    public SpriteRenderer footpad2OnSprite;

    [Tooltip("We'll check this trigger to see if any players are overlapping it.")]
    public Collider2D footpad1Trigger;

    [Tooltip("We'll check this trigger to see if any players are overlapping it.")]
    public Collider2D footpad2Trigger;


    [Tooltip("When a question is selected, we'll activate this and smoothly scale it up to 1.0")]
    public GameObject questionDialogBox;

    [Tooltip("Actual question text goes here.")]
    public TMP_Text dialogBoxText;

    [Tooltip("We'll set trigger name \"spin\" on this animator when the wheel spins.")]
    public Animator animator;



    [Header("Settings")]
    [Tooltip("List of question texts that can be asked, separated by newline.")]
    public TextAsset icebreakerQuestions;


    [Tooltip("Footpad takes this long to fade on or off.")]
    public float footpadFadeTime = 1.0f;

    [Tooltip("Time to delay after triggering spin animation, before we show the dialog box.")]
    public float dialogBoxDelay = 2.0f;

    [Tooltip("Time to delay after the dialog box is shown, before we hide it again.")]
    public float dialogBoxTime = 30.0f;

    [Tooltip("Time spent scaling in or out the dialog box.")]
    public float dialogBoxScaleTime = 0.5f;

    [Tooltip("Easing function for dialog scale IN animation.")]
    public EasingFunction.Ease dialogScaleEaseIn = EasingFunction.Ease.EaseOutCubic;

    [Tooltip("Easing function for dialog scale OUT animation (NOTE: goes backwards).")]
    public EasingFunction.Ease dialogScaleEaseOut = EasingFunction.Ease.EaseOutCubic;



    [Header("Testing")]
    [Tooltip("This can be used to force on the footpad for testing purposes.")]
    public bool forceFootpad1 = false;

    [Tooltip("This can be used to force on the footpad for testing purposes.")]
    public bool forceFootpad2 = false;
    


    [Header("Runtime")]
    [Tooltip("Network-replicated. Index of the active question, or -1 if no question is active.")]
    public int activeIcebreakerQuestion = -1;
    [Tooltip("will be set to something like 30 seconds when activeIcebreakerQuestion != -1.  Does not need to be network replicated")]
    public float questionCooldown = 0; 



    float footpad1FadeProgress = 0;   // 1 = fully on
    float footpad2FadeProgress = 0;   // 1 = fully on

    bool footpad1Pressed;
    bool footpad2Pressed;



    int activeIcebreakerQuestionLastSerialized = -1;  // when != activeIcebreakerQuestion, we know we need to send a network update
    int activeIcebreakerQuestionOld = -1;   // Used to detect changes to the value and react to them regardless of who owns us.
    
    string[] _icebreakerQuestions;   // Parsed version of iceBreakerQuestions, set in Start()

    int _lastQuestionIndex = -1;    // Prevents same question twice in a row



    void Start()
    {
        // Clean up line endings
        string questions_clean = icebreakerQuestions.text.Replace("\r\n", "\n");
        questions_clean = questions_clean.Replace("\r", "\n");

        // Tokenize
        _icebreakerQuestions = questions_clean.Split('\n');

        //Trim each entry
        for( int i=0; i<_icebreakerQuestions.Length; i++)
            _icebreakerQuestions[i] = _icebreakerQuestions[i].Trim();

        // Final step to making sure this list is "clean..."
        // Look for any empty entries and remove them
        List<string> final_list = new List<string>();
        foreach( string s in _icebreakerQuestions )
            if( !string.IsNullOrEmpty(s) )
                final_list.Add(s);
        _icebreakerQuestions = final_list.ToArray();
    }


    void FixedUpdate()
    {
        ContactFilter2D contact_filter = new ContactFilter2D();
        contact_filter.SetLayerMask( LayerMask.GetMask("player_collider") );   // only consider overlaps with player colliders!


        
        Collider2D[] pad1_cols = new Collider2D[1];
        Collider2D[] pad2_cols = new Collider2D[1];
        footpad1Pressed = footpad1Trigger.OverlapCollider(contact_filter, pad1_cols ) >= 1;
        footpad2Pressed = footpad2Trigger.OverlapCollider(contact_filter, pad2_cols ) >= 1 && (!footpad1Pressed || pad2_cols[0] != pad1_cols[0]);   // Same player can't step on both footpads.


        //  "Force" on for testing
        footpad1Pressed = footpad1Pressed || forceFootpad1;
        footpad2Pressed = footpad2Pressed || forceFootpad2;



        if( Dateland_Network.initialized && photonView.IsMine && questionCooldown <= 0 && footpad1Pressed && footpad2Pressed )   // Conditions are met to launch a new question!  Only photon owner can determine this value, the rest must replicate it.
        {
            // Try 10 times to randomize a value that is different from _lastQuestionIndex (prevents same question twice in a row)
            for(int i=0; i<10; i++)
            {
                activeIcebreakerQuestion = Random.Range( 0, _icebreakerQuestions.Length );
                if( activeIcebreakerQuestion != _lastQuestionIndex )
                    break;
            }
            _lastQuestionIndex = activeIcebreakerQuestion;
        }
        


        // This value has changed, whether from network replication or from the line just above.
        if( activeIcebreakerQuestion != activeIcebreakerQuestionOld )
        {
            activeIcebreakerQuestionOld = activeIcebreakerQuestion;

            if( activeIcebreakerQuestion != -1 )
            {
                //Play sound!
                Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( "wheel_combined", transform.position );

                // Activate icebreaker question!
                dialogBoxText.text = _icebreakerQuestions[ activeIcebreakerQuestion ];

                // Set up wheel animation, and dialog box timer.  These are handled in Update
                animator.SetTrigger("spin");
                questionCooldown = dialogBoxDelay + dialogBoxTime;
            }
        }   

        
    }



	private void Update()
	{
        // Progress footpad animations...
        float footpad_progress_rate = (1 / footpadFadeTime) * Time.deltaTime;
        if( !footpad1Pressed )
            footpad1FadeProgress = Mathf.Max( 0, footpad1FadeProgress - footpad_progress_rate );
        else
            footpad1FadeProgress = Mathf.Min( 1, footpad1FadeProgress + footpad_progress_rate );

        if( !footpad2Pressed)
            footpad2FadeProgress = Mathf.Max( 0, footpad2FadeProgress - footpad_progress_rate );
        else
            footpad2FadeProgress = Mathf.Min( 1, footpad2FadeProgress + footpad_progress_rate );

		
        // Set footpad "on" transparency
        footpad1OnSprite.color = new Color(
            footpad1OnSprite.color.r,
            footpad1OnSprite.color.g,
            footpad1OnSprite.color.b,
            footpad1FadeProgress
            );

        footpad2OnSprite.color = new Color(
            footpad2OnSprite.color.r,
            footpad2OnSprite.color.g,
            footpad2OnSprite.color.b,
            footpad2FadeProgress
            );


        // Dialog box animation!
        if( questionCooldown <= 0 )
        {
            questionDialogBox.SetActive(false);  // hide dialog box
        }
        else
        {
            // Activate dialog box, and scale it in smooothly.
            questionDialogBox.SetActive(true);

            EasingFunction.Ease easing_func = EasingFunction.Ease.Linear;    //easing changes based on animation phase
            float dialog_progress = (dialogBoxDelay + dialogBoxTime) - questionCooldown;   // goes from 0 to (a+b)
            dialog_progress = Mathf.Max(0, dialog_progress - dialogBoxDelay);     // waits at 0 until it passes delay
            if( dialog_progress <= dialogBoxScaleTime )
            {
                dialog_progress /= dialogBoxScaleTime;    // scales linearly from 0 to 1
                easing_func = dialogScaleEaseIn;
            }
            else if( dialog_progress >= dialogBoxTime - dialogBoxScaleTime )
            {
                dialog_progress = (dialogBoxTime - dialog_progress) / dialogBoxScaleTime;   // scales linearly from 1 to 0
                easing_func = dialogScaleEaseOut;
            }
            else
                dialog_progress = 1;     // just keep scale at 1 in the middle of the animation

            // Apply easing to dialog_progress, use it to scale dialog box.
            questionDialogBox.transform.localScale = Vector3.one * EasingFunction.GetEasingFunction( easing_func )(0, 1, dialog_progress);

            questionCooldown -= Time.deltaTime;
        }
	}




    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            // From PUN docs:
            // If you skip writing any value into the stream, PUN will skip the update. Used carefully, this can conserve bandwidth and messages (which have a limit per room/second).
            // https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_pun_1_1_photon_transform_view.html

            if( activeIcebreakerQuestion != activeIcebreakerQuestionLastSerialized )     // Only bother sending this when it changes!
            {
                Debug.Log("Sending Icebreaker question: " + activeIcebreakerQuestion);

                // Note that new users joining the game may not get the wheel question if it's in progress.
                // This is a known issue, but I'm doing it this way for two reasons:
                // 1.) Easy
                // 2.) Doesn't require a lot of network traffic
                stream.SendNext( activeIcebreakerQuestion );
                activeIcebreakerQuestionLastSerialized = activeIcebreakerQuestion;
            }
        }
        else
        {
            activeIcebreakerQuestion = (int)stream.ReceiveNext();
            Debug.Log("Receiving Icebreaker question: " + activeIcebreakerQuestion);
        }
    }


}
