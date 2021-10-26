using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Just destroy it once a player has been created.
/// </summary>
public class ConnectingText : MonoBehaviour
{
    void FixedUpdate()
    {
        if( Player_Controller_Mobile.mine != null )
            Destroy( gameObject );
    }
}
