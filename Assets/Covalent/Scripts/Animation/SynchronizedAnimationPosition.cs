using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This intermittently uses an RPC to set all animations to the same
/// time, syncing all clients. This works differently than SynchronizedAnimationTrigger,
/// which uses animation triggers instead of setting the time directly.
/// </summary>
public class SynchronizedAnimationPosition : MonoBehaviourPun
{
    public Animator animator;

    [Tooltip("Animator layer index (can usually leave this at 0)")]
    public int layerIndex = 0;   


    [Tooltip("Will sync animation time every this amount of seconds")]
    public float interval = 10.0f;

    float _cooldown;   // between SetAnimationTime calls
    float _requestStateCooldown;      // between RequestAnimationTime calls
    bool _haveSyncedAtLeastOnce = false;

    [PunRPC]
    void SetAnimationTime(float normalized_time)
    {
        animator.Play(0, layerIndex, normalized_time);
        _haveSyncedAtLeastOnce = true;
    }


    [PunRPC]
    public void RequestAnimationTime(int requesting_player_actor_num)
    {
        // Send response only to the requesting player.
        if( photonView.IsMine && Dateland_Network.initialized )  //just in case, though this should always be true
        {
            Photon.Realtime.Player player = PhotonUtil.GetPlayerByActorNumber( requesting_player_actor_num );               // Get the player we want to send it to...
            if( player != null )
                photonView.RPC("SetAnimationTime", player, new object[]{ animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime });   // Sets animation trigger on all clients, including us
        }
    }


    void FixedUpdate()
    {
        if( PhotonNetwork.InRoom )
        {
            if( photonView.IsMine )
            {
                _cooldown = Mathf.Max( 0.0f, _cooldown - Time.fixedDeltaTime );
                if( _cooldown <= 0)
                {
                    photonView.RPC("SetAnimationTime", RpcTarget.Others, new object[]{ animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime });   // Sets animation trigger on all clients, including us
                    _cooldown = interval;   // reset cooldown
                }
            }
            else
            {
                if( !_haveSyncedAtLeastOnce )   // We don't own this object, so we need to request the state.
                {
                    if( _requestStateCooldown <= 0 )
                    {
                        _requestStateCooldown = 1.0f;   // can request once per second
                        photonView.RPC("RequestAnimationTime", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    else
                        _requestStateCooldown = Mathf.Max(0, _requestStateCooldown - Time.fixedDeltaTime);
                }
                
            }
        }
    }
}
