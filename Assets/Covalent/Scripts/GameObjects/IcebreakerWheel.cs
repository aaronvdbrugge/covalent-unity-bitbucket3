using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A giant spinner which, when two players stand on the designated pads, will play a "spinning" animation,
/// then display a UI element with an "icebreaker" question.
/// </summary>
public class IcebreakerWheel : MonoBehaviour
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


    [Header("Settings")]
    [Tooltip("Footpad takes this long to fade on or off.")]
    public float footpadFadeTime = 1.0f;



    float footpad1FadeProgress = 0;   // 1 = fully on
    float footpad2FadeProgress = 0;   // 1 = fully on

    bool footpad1Pressed;
    bool footpad2Pressed;


    void FixedUpdate()
    {
        ContactFilter2D contact_filter = new ContactFilter2D();
        contact_filter.SetLayerMask( LayerMask.GetMask("player_collider") );   // only consider overlaps with player colliders!


        
        Collider2D[] pad1_cols = new Collider2D[1];
        Collider2D[] pad2_cols = new Collider2D[1];
        footpad1Pressed = footpad1Trigger.OverlapCollider(contact_filter, pad1_cols ) >= 1;
        footpad2Pressed = footpad2Trigger.OverlapCollider(contact_filter, pad2_cols ) >= 1 && (!footpad1Pressed || pad2_cols[0] != pad1_cols[0]);   // Same player can't step on both footpads.

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


	}
}
