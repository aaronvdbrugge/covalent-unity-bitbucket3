using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// As soon as partyId is available, we'll join voice chat.
/// </summary>
public class StartAgoraWithPartyId : MonoBehaviour
{
    [Tooltip("We'll create this once, then do DontDestroyOnLoad. Should have Agora_Manager attached.")]
    public GameObject agoraManagerPrefab;

    [Tooltip("In NativeEntryPoint.sandboxMode, all users join same chat room")]
    public string sandboxChatRoom = "SANDBOX";

    bool _didFirstConnect = false;

    void FixedUpdate()
    {
        if( !_didFirstConnect && Dateland_Network.playerFromJson != null )   // we have json data, so we should have a partyId
        {
            _didFirstConnect = true;

            // See if Agora_Manager needs to be created.
            Agora_Manager agora_manager = FindObjectOfType<Agora_Manager>();
            if( agora_manager == null )
            {
                GameObject go = Instantiate(agoraManagerPrefab);
                DontDestroyOnLoad( go );   // Keeps this object around through scene changes.
                agora_manager = go.GetComponent<Agora_Manager>();
            }

            agora_manager.JoinChannel( NativeEntryPoint.sandboxMode ? sandboxChatRoom : Dateland_Network.playerFromJson.partyId );    // Note that agoraManager should handle reconnecting if the connection to this channel name is lost, and will do so until LeaveChannel is called
        }
    }
}
