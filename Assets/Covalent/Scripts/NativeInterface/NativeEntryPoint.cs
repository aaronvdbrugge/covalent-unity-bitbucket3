using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// --------------------------------------
/// HOW TO USE SANDBOX MODE / REGULAR MODE
/// --------------------------------------
/// SANDBOX MODE:
/// In this mode, everyone will join the same Agora channel (of name Agora_Manager.sandboxChatRoom).
/// Everyone will also join the same Photon room (of name Dateland_Network.sandboxRoomName).
/// 
/// The sandbox room will have a max player count of TeamRoomJoin.maxPlayersPerRoomSandboxMode, 
/// which is currently set to 64 to allow stress testing large amounts of players.
/// 
/// Dateland_Network will set its partnerPlayer to our own ID, which will bypass the partnering logic.
/// 
/// This mode won't be allowed if DebugSettings.mode is set to Release, so you don't have to worry
/// about leaving it on.
/// 
/// 
/// REAL MODE:
/// In this mode, players are supposed to join with a partner player, indicated by partyId in the JSON,
/// which has is consistent between both players. E.g., players 123 and 456 will both have the same partyId
/// of "123:456". This is the ID of the Agora channel they will both join, to chat with each other exclusively.
/// 
/// The first player in the partyId is the "primary" player, and it's up to them to choose
/// or create a new room (randomized ID) based on what's available, reserving a spot for their partner. 
/// The "secondary"  player must wait and continually check to see if the primary player has entered a room,
/// then join them.
/// 
/// At the time of this writing, max players for these rooms is set to 4, to allow for matchmaking tests
/// that ensure slots have been properly reserved for partners.
/// 
/// Both players must wait for the other one to join before they can start playing. There is a debug button
/// to bypass this, and there's no need to hide it, because it will be auto hidden when DebugSettings.mode
/// is set to Release.
/// 
/// 
/// TO SWITCH BETWEEN MODES:
/// From the Native side, it's just a case of whether you call enterArcadeAsPair, or enterArcadeSandbox.
/// 
/// If you are running from the editor, or in isolation, if you run the scene directly, it should default to
/// Sandbox mode, because the static variable NativeEntryPoint.sandboxMode defaults to true. Sandbox mode
/// is really what you want for iterating in editor, unless you're specifically testing matchmaking / partnering.
/// 
/// To change your room and avoid conflicting / causing bugs with other people using previous versions, you can
/// change Dateland_Network.sandboxRoomName to whatever you like, just something that hasn't been used before.
/// 
/// Note that to join the voice chat, you need to run LoadingScreen.unity, because voice chat connects as possible
/// (before the app is fully loaded).
/// 
/// When running LoadingScreen, it will still run in sandbox mode, UNLESS you check NativeEntryPoint.disableSandboxMode.
/// You use this value to test Real Mode in the editor.
/// If you see "Waiting for match" in the main scene, you are in Real Mode, NOT Sandbox Mode.
/// 
/// When you test Real Mode in the editor, you will likely also want to configure test_user_json.txt. You can change
/// user id, username, and partyId. To match two players you will need to set it up correctly -- with the partyId
/// consisting of [uid1]:[uid2], being the same order for both clients so they know to match with each other, and
/// who is the "primary" player who must find or create an available room.
/// 
/// 
/// 
/// 
/// ---------------------
/// NOTES ON NATIVE CALLS
/// ---------------------
/// Currently, the Native side sends the user's JSON profile to Unity via the following:
///     unity.sendMessageToGO(withName: "PhotonMono", functionName: "createPlayer", message: jsonString)
///     
/// This is deprecated now. The new calls should be either of these:
/// 
///     unity.sendMessageToGO(withName: "PhotonMono", functionName: "enterArcadeAsPair", message: jsonString)
///     Triggers proper matchmaking using partyId.
/// 
///     unity.sendMessageToGO(withName: "PhotonMono", functionName: "enterArcadeSandbox", message: jsonString)
///     Puts all players into the same room, no waiting for match, all connect to same Agora party.
/// 
/// This object must be placed on an object called PhotonMono to receive that message, and it must be placed
/// in at least the entry scene of the project (probably LoadingScreen.unity) to ensure we get that 
/// function call.
/// </summary>
public class NativeEntryPoint : MonoBehaviour
{
    /// <summary>
    /// Other scripts such as Agora_Manager and Dateland_Network should check sandboxMode
    /// to see if they should do real matchmaking, or just throw everyone into the sandbox room / chat.
    /// 
    /// NOTE that this defaults to true, for easy testing when you run Dateland directly.
    /// </summary>
    public static bool sandboxMode = true;

    [Header("References")]
    public TextAsset spoofJson;
    public DebugSettings debugSettings;




    [Header("Testing")]
    [Tooltip("Allows easy testing of matchmaking mode in editor, or deployed app in isolation. Usually, we actually want to leave" +
            " this on (it won't hurt anything in Release, as sandbox mode must still be started explicitly")]
    public bool disableSandboxMode = false;

    [Tooltip("Feeds spoofJson to enterArcadeAsPair automatically. Will only take effect in debug mode")]
    public bool doSpoof = false;    // 
    [Tooltip("Feeds spoofJson to enterArcadeSandbox automatically. Will only take effect in debug mode")]
    public bool doSpoofSandbox = false;    // Will only take effect in debug mode



    [Header("Runtime")]
    [Tooltip("You might set this to true if an error occurs while loading the scene. Next time they call createPlayer, they might have gone back out into the app, and back in.")]
    public bool restartSceneOnCreatePlayer = false;


    void Start()
    {
        if( gameObject.name != "PhotonMono" || transform.parent != null )
            Debug.LogError("CreatePlayerReceiver must be a root-level game object named \"PhotonMono\" to properly receive native function call!");

        if( disableSandboxMode )   // note that this only affects debugging. The native entry point functions enterArcadeAsPair and enterArcadeSandbox always set sandboxMode explicitly.
            sandboxMode = false;

        if( doSpoof && debugSettings.mode == DebugSettings.BuildMode.Debug )
            enterArcadeAsPair( spoofJson.text );
        else if( doSpoofSandbox && debugSettings.mode == DebugSettings.BuildMode.Debug )
            enterArcadeSandbox( spoofJson.text );
    }


    /// <summary>
    /// NOTE: This function gets called from the native side, and is given
    /// a payload of the user's real profile info.
    /// 
    /// In addition, the  "partyId" key will tell us what user we've been paired with,
    /// by including both userIDs in the string. E.g. "12:74" means users 12 and 74 are paired
    /// together. This would also mean that "user"->"id" should be either 12 or 74.
    /// 
    /// "partyId" must be exactly the same for both users (same user ID must come first in the pair)
    /// </summary>
    public void enterArcadeAsPair(string json_string)
    {
        Debug.Log("FROM NATIVE: enterArcadeAsPair(" + json_string.Substring(0, 100) + "...)");

        sandboxMode = false;
        InitInternal(json_string);
    }

    /// <summary>
    /// For this function, you can still provide user info such as username
    /// </summary>
    /// <param name="json_string"></param>
    public void enterArcadeSandbox(string json_string)
    {
        if( debugSettings.mode == DebugSettings.BuildMode.Release )
        {
            Debug.LogError("Called enterArcadeSandbox in Release mode. For safety reasons, this is not allowed! Starting in enterArcadeAsPair mode instead...");
            enterArcadeAsPair(json_string);
            return;
        }

        Debug.Log("FROM NATIVE: enterArcadeSandbox(" + json_string.Substring(0, 100) + "...)");
        sandboxMode = true;
        InitInternal(json_string);
    }


    [Obsolete("createPlayer is deprecated. Please use enterArcadeAsPair for real matchmaking in Release, or enterArcadeSandbox for testing things unrelated to matchmaking.")]
    public void createPlayer(string json_string)
    {
        Debug.Log("FROM NATIVE: createPlayer(" + json_string.Substring(0, 100) + "...)");
        Debug.LogWarning("NOTE: createPlayer is deprecated.");

        sandboxMode = false;
        InitInternal(json_string);
    }


    void InitInternal(string json_string)
    {
        Dateland_Network.realUserJson = json_string;
        Dateland_Network.playerFromJson = JsonUtility.FromJson<Player_Class>(json_string);    // Parse the JSON right away. Now it's available to everyone in Dateland_Network.playerFromJson

        if( restartSceneOnCreatePlayer )
        {
            restartSceneOnCreatePlayer = false;   // just in case, though this really shouldn't be necessary
            SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
        }
    }






    /// <summary>
    /// Does a test enterArcadeAsPair call through inspector, using spoof data
    /// </summary>
    [ContextMenu("Test Enter Arcade As Pair")]
    public void TestEnterArcadeAsPair()
    {
        enterArcadeAsPair( spoofJson.text );
    }

    /// <summary>
    /// Does a test enterArcadeSandbox call through inspector, using spoof data
    /// </summary>
    [ContextMenu("Test Enter Arcade As Pair")]
    public void TestEnterArcadeSandbox()
    {
        enterArcadeSandbox( spoofJson.text );
    }


}
