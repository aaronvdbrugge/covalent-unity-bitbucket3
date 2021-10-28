using System.Collections.Generic;
using agora_gaming_rtc;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections;


/// <summary>
/// This used to be a huge monolithic class, but it's been refactored so now other classes handle the various
/// duties of a Player object. This class still oversees them and handles some chores regarding Photon and
/// disconnecting.
/// </summary>
public class Player_Controller_Mobile : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    #region Variables

    // Convenience pointer to the owned player.
    public static Player_Controller_Mobile mine;


    //Convenience pointers to all players, keyed by Kippo ID. Can use this to find our partner
    public static Dictionary<int, Player_Controller_Mobile> playersByKippoId = new Dictionary<int, Player_Controller_Mobile>();


    /// <summary>
    /// Lookup of gameobjects via actor number. Photon doesn't seem to provide this, so we'll set it up as we go.
    /// Bear in mind players could be destroyed, and the key will point to something that casts to false.
    /// </summary>
    public static Dictionary<int, Player_Controller_Mobile> fromActorNumber = new Dictionary<int, Player_Controller_Mobile>();


    [Header("Player component references")]
    [Tooltip("Handles actual movement of the player.")]
    public Player_Movement playerMovement; 

    [Tooltip("Handles OnTriggerEnter, etc")]
    public Player_Collisions playerCollisions;

    [Tooltip("Handles animation logic...")]
    public Player_Animations playerAnimations;

    [Tooltip("Handles emote RPCs")]
    public Player_Emotes playerEmotes;

    [Tooltip("Handles voice chat")]
    public Player_Agora playerAgora;

    [Tooltip("Handles player hopping up and down (and into benches).")]
    public Player_Hop playerHop;

    [Tooltip("Handles things like switching to Go Kart and Ice movements.")]
    public Player_Alternate_Movements playerAlternateMovements;

    [Tooltip("Handles complex sounds like the ice skating loop.")]
    public Player_Sounds playerSounds;

    [Tooltip("Handles skins")]
    public Spine_Player_Controller spinePlayerController;

    [Tooltip("Handles logic related to the user's partner, who they entered with and are voice chatting with.")]
    public Player_Partner playerPartner;


    [Header("References")]
    public TMP_Text playerName;



    [Header("Runtime")]
    public string username;
    public int kippoUserId;



    

    /// <summary>
    /// When the player is backgrounded, this coroutine will count down to a player
    /// disconnect.
    /// </summary>
    private IEnumerator disconnectCoroutine;
    private bool destroy_player = true;   // indicates that disconnectCoroutine should finish the job after waiting

    #endregion

    private void Awake()
    {
        //Application.targetFrameRate = 60;
        EventManager.StartListening("disable_joystick", disableMovement);
        EventManager.StartListening("enable_joystick", enableMovement);


        disconnectCoroutine = PlayerDisconnectCoroutine();
        EventManager.StartListening("cancel_destroy", cancel_destroy);
    }


    private void disableMovement()
    {
        playerMovement.enabled = false;
    }

    private void enableMovement()
    {
        playerMovement.enabled = true;
    }



    public IEnumerator PlayerDisconnectCoroutine()
    {
        yield return new WaitForSecondsRealtime(60f);
        if (destroy_player)
        {
            EventManager.TriggerEvent("player_removed");
            destroyMe();
        }
    }

    // This old logic is a little bit weird, but I'm not gonne mess with it right now --seb
	public void cancel_destroy()
    {
        if (disconnectCoroutine != null)
        {
            StopCoroutine(disconnectCoroutine);

            //Not sure it was like this:
            //disconnectCoroutine = PlayerDisconnectCoroutine();
            //destroy_player = true;

            //Shouldn't it just be this?
            destroy_player = false;
        }
    }




    [PunRPC]
    public void destroyMe()
    {
        PhotonNetwork.Disconnect();
        playerAgora.LeaveChannel();
        Destroy(this.gameObject);
    }





    public void backgroundMe()
    {
        disconnectCoroutine = PlayerDisconnectCoroutine();   //start counting down to disconnect / voice chat end
        StartCoroutine(disconnectCoroutine);
    }



    private void Start()
    {

        // Initialize based on whether this object belongs to the active player or not.
        if (photonView.IsMine)
        {
            mine = this;  // convenience global

            playerMovement.isMine = true;
            playerCollisions.isMine = true;

            // Just go ahead and hackily connect the MyOnScreenStick in the UI to playerMovement.
            MyOnScreenStick stick = FindObjectOfType<MyOnScreenStick>();
            if( stick )
                stick.onJoystickValues.AddListener( playerMovement.SetJoystick );




            // Relay playerHop to camera click handler, which needs to be able to trigger hops.
            FindObjectOfType<CameraClickHandler>().playerHop = playerHop;

            Camera.main.GetComponent<CameraPanning>().target = this;
        }
    }





    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        username = (string)instantiationData[1];
        kippoUserId = (int)instantiationData[2];

        // Add ourselves to a static dictionary of players by kippo id for easy reference and finding partner.
        playersByKippoId[kippoUserId] = this;


        playerName.text = username;
        Debug.Log("Inside Callback the name passed is: " + name + ".");

        // Set up this dictionary so we can get gameobjects easily, later.
        fromActorNumber[ photonView.Owner.ActorNumber ] = this;

        // Spine player controller stopped getting this call... Manually forward it
        spinePlayerController.OnPhotonInstantiate( info ); 
    }

    void OnDestroy()
    {
        // Clean up static dictionary of players
        if( playersByKippoId.ContainsKey( kippoUserId ) )
            playersByKippoId.Remove( kippoUserId );

        // Clean up "mine"
        if( this == mine )
            mine = null;

        // Clean up fromActorNumber
        if( fromActorNumber.ContainsKey( photonView.Owner.ActorNumber ) )
            fromActorNumber.Remove( photonView.Owner.ActorNumber );
    }





}
