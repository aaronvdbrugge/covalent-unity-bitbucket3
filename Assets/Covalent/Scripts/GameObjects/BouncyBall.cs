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
public class BouncyBall : MonoBehaviour
{
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
