using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Just attach this to the main camera, and you can use it to play named AudioSources
/// under a transform under the camera, like so:
/// 
/// Camera.main.SendMessage("PlaySound", "hop");
/// </summary>
public class Camera_Sound : MonoBehaviour
{
    [Tooltip("You can play AudioSources by gameObject name under this transform.")]
    public Transform soundParent; 


    //Cache string to AudioSource so we don't have to do GameObject.Find every time.
    Dictionary<string, AudioSource> _soundDict = new Dictionary<string, AudioSource>();

    public void PlaySound(string name)
    {
        if( !_soundDict.ContainsKey( name ) )  //cache it for future re-use
            _soundDict[name] = soundParent.Find( name ).GetComponent<AudioSource>();
            
        _soundDict[name].Play();
    }
}
