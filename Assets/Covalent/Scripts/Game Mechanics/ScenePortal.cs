using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// When touched, this object will take the player to a different scene.
/// 
/// </summary>
public class ScenePortal : MonoBehaviour
{
    [Tooltip("This scene name must be added to build settings.")]
    public string sceneName;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Determine if it's the owned player that collided with us.
        PhotonView photon_view = collision.GetComponent<PhotonView>();
        if( photon_view == null ) return;   //not a player
        if( !photon_view.IsMine ) return;   //it's a player, but not ours!


        SceneLoader sl = SceneLoader.Instance;
        string old_scene = sl.sceneId;
        
        if( sl.sceneId == null )
            Debug.LogError( "Error: SceneLoader.sceneId was null.");

        // Find the SceneLoader, and tell them to take care of it...
        sl.MoveToScene( sceneName, collision.gameObject );
    }




}
