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


    bool _initialized = false;

	private void FixedUpdate()
	{
		if( agoraManager == null ) // try to find it now, not in Start
            agoraManager = FindObjectOfType<Agora_Manager>();   // try to find it. it still might be null

        if( !_initialized && agoraManager != null && !string.IsNullOrEmpty(agoraManager.joinedChannel) )  // Can only init once we joined a channel! Otherwise, clicking mute could cause problems
        {
            _initialized =true;

            // Figure out the visual state of the button.
            if( agoraManager.isMuted )
                muteToggle.isOn = true;

            muteToggle.interactable = true;   // should start non-interactable

            // NOW we can add listener...
            muteToggle.onValueChanged.AddListener( ToggleMute );
        }

	}


    
	public void ToggleMute(bool is_on)
    {
        if( agoraManager != null )
            agoraManager.mute( is_on );
    }
}
