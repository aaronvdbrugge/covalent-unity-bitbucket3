using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Agora_Manager is generated at runtime, so we need this...
/// </summary>
public class MuteToggleButton : MonoBehaviour
{
    public Toggle muteToggle;
    Agora_Manager agoraManager;



	private void FixedUpdate()
	{
		if( agoraManager == null ) // try to find it now, not in Start
        {
            agoraManager = FindObjectOfType<Agora_Manager>();
            if( agoraManager != null )  // found it, do some init
            {
                // Figure out the visual state of the button.
                if( agoraManager.isMuted )
                    muteToggle.isOn = true;

                // NOW we can add listener...
                muteToggle.onValueChanged.AddListener( ToggleMute );
            }
        }
	}



	public void ToggleMute(bool is_on)
    {
        if( agoraManager != null )
            agoraManager.mute( is_on );
    }
}
