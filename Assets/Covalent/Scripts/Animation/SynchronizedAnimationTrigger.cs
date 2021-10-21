using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Use an RPC to call an animation trigger on all clients periodically.
/// If multiple triggerNames are provided, we'll call them one after the other,
/// then loop around.
/// </summary>
public class SynchronizedAnimationTrigger : MonoBehaviourPun
{
    public Animator animator;
    public string[] triggerNames;
    public float interval = 10.0f;

    float _cooldown;
    int _nextIndex = 0;

    [PunRPC]
    void DoTrigger(int index)
    {
        animator.SetTrigger(triggerNames[index]);
    }


    void FixedUpdate()
    {
        if( PhotonNetwork.InRoom && photonView.IsMine )
        {
            _cooldown = Mathf.Max( 0.0f, _cooldown - Time.fixedDeltaTime );
            if( _cooldown <= 0)
            {
                photonView.RPC("DoTrigger", RpcTarget.All, new object[]{ _nextIndex });   // Sets animation trigger on all clients, including us
                _cooldown = interval;   // reset cooldown

                _nextIndex = (_nextIndex + 1) % triggerNames.Length;
            }
        }
    }
}
