using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The goal of this class is to allow us to additively load different scenes as the players
/// walk between them, while also just allowing the scene itself to be run in-editor for
/// easy testing.
/// It'll need to delete itself if another one is already found in the scene.
/// If another one isn't found, it's safe to activate its children.
/// </summary>
public class SceneLoader : MonoBehaviour
{
	public static SceneLoader Instance;

	public Dictionary<string, GameObject> allBases = new Dictionary<string, GameObject>();   //keep track of all the scenes we've additively loaded in.  SceneBase adds itself here
	public string sceneId;

	string teleportFrom = null;  //set to non-null if we just teleported from a different scene.
    GameObject objectToTeleport = null;
	int teleportWaitFrames = 100;   //we literally have to wait a frame before the scene is loaded enough to teleport... it sucks!!


	void Awake()
	{
		if( Instance != null )   // There's already one of these...
			Destroy( gameObject );
		else
		{
			Instance = this;

			// We can enable our children (camera, managers etc)
			gameObject.SetChildrenActive(true);

			if( FindObjectOfType<SceneBase>() == null )
				Debug.LogError("Don't forget to add SceneBase to your scene!");
			else if(FindObjectOfType<SceneBase>().gameObject.name != SceneManager.GetActiveScene().name)
				Debug.LogError("SceneBase name should match scene name.");
		}
	}



	public void MoveToScene(string scene_name, GameObject player_obj)
	{
		string old_scene = sceneId;

		// Deactivate all known sceneBases...
		foreach( var kvp in allBases )
			kvp.Value.SetActive(false);


		BroadcastMessage( "OnChangeScene", scene_name );   // Should cause player to leave the network room and join a new one.

		//If we already loaded this scene, we can just swap to it. Don't need to load anything!
		if( allBases.ContainsKey(scene_name) )
		{
			allBases[scene_name].SetActive(true);
			sceneId = scene_name;    // we're already done!
		}
		else
		{
			// Adds a scene into our current one. The presence of this SceneManager class should prevent it
			// from having any duplicates of things we only need one of.
			SceneManager.LoadScene( scene_name, LoadSceneMode.Additive );
		}

		// Once SceneBase hits Start(), it'll add itself to allBases and set sceneId.

		//Due to scene loading, seems like we might have to wait till LateUpdate to do teleport...
        teleportFrom = old_scene;
        objectToTeleport = player_obj;
		teleportWaitFrames = 1;  //wait a frame before teleporting
	}



	private void LateUpdate()
	{
		// See if we have any teleports loaded from MoveToScene
		if( teleportFrom != null && teleportWaitFrames-- <= 0)
        {
            // Find a DefaultPlayerSpawn with comingFromScene set to the scene we just came from.
            foreach( var spawn in FindObjectsOfType<DefaultPlayerSpawn>() )
                if( spawn.comingFromScene == teleportFrom )  //found a match!
                    objectToTeleport.transform.position = spawn.transform.position;

			teleportFrom = null;
        }
	}

}
