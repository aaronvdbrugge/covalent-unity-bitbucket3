using Photon.Pun;
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

    public void PlaySoundPitched( string name, float pitch = 1.0f)
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
        {
            _soundDict[name].pitch = pitch;
            _soundDict[name].Play();
        }
    }


	public void PlaySound(string name)
    {
        PlaySoundPitched(name, 1.0f);
    }



    /// <summary>
    /// Call this function to make sure it's OK before you play a soundsource via some method
    /// other than PlaySoundAtPosition.
    /// </summary>
    public bool CanPlaySoundAtPosition(Vector2 sound_position, bool party_only = false, int triggered_by_uid=-1)
    {
        if( party_only )
        {
            // Check to see if it's triggered by either us or our partner. If not, don't play it!
            if( triggered_by_uid != Dateland_Network.playerFromJson.user.id && triggered_by_uid != Dateland_Network.partnerPlayer )
                return false;
        }


        float vert_extent = myCamera.orthographicSize;    
        float horz_extent = vert_extent * Screen.width / Screen.height;

        vert_extent *= 1 + extendSoundBounds;
        horz_extent *= 1 + extendSoundBounds;

        Vector2 position = transform.position;


        // See if it's out of bounds...
        if( sound_position.x < position.x - horz_extent )
            return false;
        if( sound_position.x > position.x + horz_extent )
            return false;
        if( sound_position.y < position.y - vert_extent )
            return false;
        if( sound_position.y > position.y + vert_extent )
            return false;

        return true;
    }



    /// <summary>
    /// Plays a sound, but only if the position is in camera bounds for this client.
    /// </summary>
    public void PlaySoundAtPosition( string name, Vector2 sound_position, float pitch = 1.0f )
    {
        if( CanPlaySoundAtPosition( sound_position ) )
            PlaySoundPitched( name, pitch );
    }
    
    /// <summary>
    /// You need to StartCoroutine with this.
    /// </summary>
    public IEnumerator PlaySoundAtPositionAfterDelay(float delay, string name, Vector2 sound_position, float pitch = 1.0f )
    {
        yield return new WaitForSeconds(delay);
        PlaySoundAtPosition(name, sound_position, pitch);
    }

}
