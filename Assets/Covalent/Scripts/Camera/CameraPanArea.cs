using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The camera will pan when the owned player enters this area.
/// </summary>
public class CameraPanArea : MonoBehaviour
{
	[Header("Settings")]
	public Vector2 targetPosWorld;
	public float targetZoom=1.0f;

    [Header("Runtime")]
    public bool wantsPan = false;

	private void Start()
	{
		Camera.main.GetComponent<CameraPanning>().cameraPanAreas.Add(this);    // CameraPanning will now look at us to see if wantsPan is true.
	}


    bool _queueWantsPan = false;  // prevents script execution order issues
	void OnTriggerStay2D(Collider2D other)
    {
        Player_Controller_Mobile plr = other.GetComponent<Player_Controller_Mobile>();
        if( plr && plr.photonView.IsMine )
            _queueWantsPan = true;   // since physics runs on fixed timestep, we should be getting one of these per FixedUpdate
    }


	private void FixedUpdate()
	{
		if( _queueWantsPan )
		{
			_queueWantsPan = false;   // "consume" this value
			wantsPan = true;
		}
		else
			wantsPan = false;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;   
		Gizmos.DrawWireSphere(targetPosWorld, 1.0f); // show the target position
	}

}
