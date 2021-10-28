using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Handles logic for player "emotes."
/// </summary>
public class Player_Emotes : MonoBehaviourPun
{
    [Header("References")]
    public Player_Animations playerAnimations;
    public SpriteRenderer emoji_bubble_player, emoji_icon_player;

    [Header("Settings")]
    [Tooltip("Time to show an emote for before it disappears.")]
    public float emoteTime = 1.7f; 

    [Header("Info")]
    public int emojiSlot=-1;   // last called "slot" from the RPC


    float _emoteTimer = 0;  //set to emoteTime when we show an emote

    [PunRPC]
    public void emoting(int slot)
    {
        emoji_icon_player.sprite = Emoji_Manager.inst.emojiSettings.emojis[slot].sprite;
        emojiSlot = slot;
        emoji_bubble_player.enabled = true;
        emoji_icon_player.enabled = true;
        
        playerAnimations.Emote(slot);
        _emoteTimer = emoteTime;


        //Play a sound!
        string sound_cue = Emoji_Manager.inst.emojiSettings.emojis[slot].soundCue;
        if( string.IsNullOrEmpty( sound_cue ) )  // I left it empty, so just infer it
        {
            if( string.IsNullOrEmpty(Emoji_Manager.inst.emojiSettings.emojis[slot].playerAnim) )  // no animation
                sound_cue = "emoji_none";
            else
                sound_cue = "emoji_" + Emoji_Manager.inst.emojiSettings.emojis[slot].playerAnim.ToLower();
        }
        
        if( !string.IsNullOrEmpty( sound_cue ) ) 
            Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( sound_cue, transform.position );


    }


    void Update()
    {
        // Count down to disabling the shown emote icon.
        if( _emoteTimer > 0 )
        {
            _emoteTimer -= Time.deltaTime;
            if( _emoteTimer <= 0 )
            {
                emoji_bubble_player.enabled = false;
                emoji_icon_player.enabled = false;
            }
        }
    }


    /// <summary>
    /// Triggers the Photon RPC.
    /// </summary>
    public void showEmote(int slot)
    {
        this.photonView.RPC("emoting", RpcTarget.All, slot);
    }

}
