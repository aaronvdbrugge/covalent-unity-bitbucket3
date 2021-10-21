using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// If a player enters this trigger, we'll spawn an object where the
/// player is.
/// </summary>
public class SpawnAtPlayerOnTriggerEnter : MonoBehaviour
{
    public GameObject objectToSpawn;


	private void OnTriggerEnter2D(Collider2D collision)
	{
		Player_Controller_Mobile plr = collision.GetComponent<Player_Controller_Mobile>();

		if( plr )
		{
			GameObject go = Instantiate(objectToSpawn);
			go.transform.position = plr.transform.position;
		}
	}
}
