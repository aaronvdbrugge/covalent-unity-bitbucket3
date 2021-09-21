using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Just attach this to the main camera, and you can use it to play named AudioSources
/// under a transform under the camera, like so:
/// 
/// Camera.main.SendMessage("PlaySound", "hop");
/// 
/// Or even better:
/// Camera.main.GetComponent<Camera_Sound>().PlaySoundAtPosition( "hop", tranform.position );
/// </summary>
public class Camera_Sound : MonoBehaviour
{
    [Tooltip("You can play AudioSources by gameObject name under this transform.")]
    public Transform soundParent; 

    [Tooltip("Play sounds from this far outside the camera view (proportionally)")]
    public float extendSoundBounds = 0.5f;


    //Cache string to AudioSource so we don't have to do GameObject.Find every time.
    Dictionary<string, AudioSource> _soundDict = new Dictionary<string, AudioSource>();


    Camera myCamera;

	private void Awake()
	{
		myCamera = GetComponent<Camera>();
	}

	public void PlaySound(string name)
    {
        if( !_soundDict.ContainsKey( name ) )  //cache it for future re-use
        {
            Transform found = soundParent.Find(name);
            if( found == null )
                Debug.LogWarning("TBD: Add sound \"" + name + "\"");   // It's OK to put in sound cues for sounds that don't exist yet. Just print a warning.
            else
                _soundDict[name] = found.GetComponent<AudioSource>();
        }
            

        if( _soundDict.ContainsKey( name ) )
            _soundDict[name].Play();
    }




    /// <summary>
    /// Plays a sound, but only if the position is in camera bounds for this client.
    /// </summary>
    public void PlaySoundAtPosition( string name, Vector2 sound_position )
    {
        float vert_extent = myCamera.orthographicSize;    
        float horz_extent = vert_extent * Screen.width / Screen.height;

        vert_extent *= 1 + extendSoundBounds;
        horz_extent *= 1 + extendSoundBounds;

        Vector2 position = transform.position;


        // See if it's out of bounds...
        if( sound_position.x < position.x - horz_extent )
            return;
        if( sound_position.x > position.x + horz_extent )
            return;
        if( sound_position.y < position.y - vert_extent )
            return;
        if( sound_position.y > position.y + vert_extent )
            return;

        PlaySound( name );
    }
    
}
