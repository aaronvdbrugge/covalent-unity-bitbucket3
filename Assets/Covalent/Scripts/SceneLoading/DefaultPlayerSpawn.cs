using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Blank script, just spawn a player here by default if they're started in this scene.
/// </summary>
public class DefaultPlayerSpawn : MonoBehaviour
{
	[Tooltip("If you set comingFromScene, then they'll get put here by the ScenePortal instead of at ScenePortal.newPosition.")]
	public string comingFromScene = "";
}
