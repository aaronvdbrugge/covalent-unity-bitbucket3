using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Use an RPC to call an animation trigger on all clients periodically.
/// </summary>
public class SynchronizedAnimationTrigger : MonoBehaviourPun
{
    public Animator animator;
    public string triggerName = "start";
    public float interval = 10.0f;

    float _cooldown;


    [PunRPC]
    void DoTrigger()
    {
        animator.SetTrigger(triggerName);
    }


    void FixedUpdate()
    {
        if( photonView.IsMine )
        {
            _cooldown = Mathf.Max( 0.0f, _cooldown - Time.fixedDeltaTime );
            if( _cooldown <= 0)
            {
                photonView.RPC("DoTrigger", RpcTarget.All);   // Sets animation trigger on all clients, including us
                _cooldown = interval;   // reset cooldown
            }
        }
    }
}
