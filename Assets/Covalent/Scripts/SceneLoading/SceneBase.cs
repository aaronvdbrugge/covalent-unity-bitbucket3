using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this to the parent of all objects specific to a scene.
/// NOTE its name should match that of the scene it belongs to.
/// </summary>
public class SceneBase : MonoBehaviour
{
	public bool ignorePvpCollisionsInThisScene = false;  // Will actually alter the app's collision matrix if this is true.

	public void Start()
	{
		// Add ourselves to the SceneLoader's list of loaded scenes, so they can be swapped out more quickly
		SceneLoader sl = SceneLoader.Instance;
		sl.allBases.Add( gameObject.name, gameObject );
		sl.sceneId = gameObject.name;   //If this object just started, that must be the scene we're currenlty in.
	}


	private void OnEnable()
	{
		// Disable or enable pvp collisions in the Physics2D collision matrix.
		int player_collider_layer = LayerMask.NameToLayer("player_collider");
		Physics2D.IgnoreLayerCollision(player_collider_layer, player_collider_layer, ignorePvpCollisionsInThisScene);
	}
}
