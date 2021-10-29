using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Agora_Manager is generated at runtime, so we need this...
/// </summary>
public class MuteToggleButton : MonoBehaviour
{
    Agora_Manager agoraManager;

    void Start()
    {
        agoraManager = FindObjectOfType<Agora_Manager>();
    }

    public void ToggleMute(Toggle toggle)
    {
        if( agoraManager != null )
            agoraManager.MuteToggle( toggle );
    }
}
