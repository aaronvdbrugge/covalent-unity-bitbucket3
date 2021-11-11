
using System.Collections;
using System.Collections.Generic;
using Covalent.Scripts.Util.Native_Proxy;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Handles logic for connecting / disconnecting to Photon.
/// It works in tandem with TeamRoomJoin, which specifically handles logic for trying
/// to put both users in the same room.
/// </summary>
public class Dateland_Network : MonoBehaviourPunCallbacks
{ 
	#region Public Fields

    [Header("References")]
    
    [Tooltip("Handles room joining logic (making sure both users go into the same room")]
    public TeamRoomJoin teamRoomJoin;   

    [Tooltip("Will be fed to enterDateland automatically if createPlayer hasn't been called from native.")]
    public TextAsset testUserJson;
    public PopupManager popupManager;
    public GameObject playerPrefab;

    [Tooltip("We'll reference the nonPremiumSlots stored here to choose an initial skin.")]
    public InventoryPanel inventoryPanel; 

    [Tooltip("\"YOU WILL LEAVE THE ARCADE IN 0:60\"")]
    public TMP_Text partnerDisconnectText;  

    [Tooltip("Don't enable this until our date has arrived, and we're out of Limbo.")]
    public CameraPanning cameraPanning;


    public DebugSettings debugSettings;


    [Header("Settings")]
    public string gameVersion = "1";


    [Tooltip("Only used in Debug mode, when NativeEntryPoint.sandboxMode == true. In Release, we'll pick a random available room.")]
    public string sandboxRoomName = "SANDBOX";



    [Tooltip("We'll retry this long after disconnecting...")]
    public float reconnectTime = 65.0f;


    [Tooltip("We'll retry every this amount of seconds")]
    public float reconnectInterval = 10.0f;

    [Tooltip("We'll try our first reconnect this amount of seconds after disconnect (could be less than reconnectInterval, e.g. 1 second)")]
    public float initialReconnectDelay = 1.0f;

    [Tooltip("Kill player if they're backgrounded this long.")]
    public float maxBackgroundedTime = 60f;

    [Tooltip("Countdown to when the player disconnects because their partner disconnected.")]
    public float partnerDisconnectTime = 60.9f;


    [Tooltip("If the partner takes longer than this to connect initially, we'll switch to a different dialog.")]
    public float partnerTakingLongTime = 60.0f;



    [Header("Runtime")]

    [Tooltip("This will be set from the debug button in disconnecting_inactivity so you can keep playing for testing purposes.")]
    public bool disablePartnerDisconnectForDebug = false;

    public void DisablePartnerDisconnectForDebug() => disablePartnerDisconnectForDebug = true;
    public void DisableWaitForDate()    // could be called from a debug button in "Waiting for match" screen
    {
        _firstWaitForDate = false;
        disablePartnerDisconnectForDebug = true;
        Player_Controller_Mobile.mine.transform.position = _gotoWhenDateArrives;     // Move (hopefully teleport) to the spawn point (we were in limbo before)
    }

    #endregion


	#region Static Fields
	/// <summary>
	/// This will be set from CreatePlayerReceiver with real profile JSON
	/// from the native side. It will remain null when running in editor or
	/// non-integrated on device.
	/// </summary>
	public static string realUserJson = null;

    /// <summary>
    /// Parsed from input JSON.
    /// </summary>
    public static Player_Class playerFromJson = null;    
    
    /// <summary>
    /// Check this value before you start doing things with photonView.IsMine...
    /// It seems like photonView.IsMine can sometimes erroneously be true until we actually get
    /// things set up.
    /// </summary>
    public static bool initialized = false;


    /// <summary>
    /// Kippo ID of our partnered player
    /// </summary>
    public static int partnerPlayer = -1;   

    /// <summary>
    /// Were we the second player in the partyId pair?
    /// </summary>
    public static bool amPrimaryPlayer = false;   



    static bool _wantsToLeave = false;   // playerDidLeaveGame has been called, but we need to wait for Photon to disconnect before we leave the scene.
	#endregion


    #region Private Fields
    Agora_Manager agoraManager;  // cached with FindObjectOfType at start, if it exists


    private bool initPlayer = false;
    private bool needsToJoinRoom;
    public string previousRoom = null;     // the room we'll try to rejoin, if we lose connection.
    public int maxSkins = 10;  //this had a compiler warning. just made it public to avoid that. -seb


    public string lastSceneName = null;   // used for determining room name.



    // Reconnect logic
    bool _reconnecting = false;
    float _reconnectTimer = 0;

    bool _gotLastKnownPlayerPosition = false;
    Vector3 _lastKnownPlayerPosition = Vector3.zero;   // We'll use this so we can try to put the player in the same place after we disconnect.

    bool _backgrounded = false;
    System.DateTime _dateTimeBackgrounded;    // when put into the foreground, we'll compare DateTime.Now with maxBackgroundedTime

    bool _disconnectedDueToInactivity = false;  // If true, don't bother to reconnect



    /// <summary>
    /// We need to start handling the special "wait for date"
    /// case, where a player might actually have to wait several minutes for their match (with bad internet)
    /// to load their Dateland. At least they should be able to chat through Agora while one date is waiting.
    /// 
    /// Once _firstWaitForDate is false, then we can worry about dhowing the intro screen, then doing _partnerDisconnectTimer, etc.,
    /// when they have had a partner in play at least once.
    /// </summary>
    bool _firstWaitForDate = true;
    float _firstWaitForDateTimer = 0;   // while _firstWaitForDate = true
    float _partnerDisconnectTimer = 0;   // counts up to partnerDisconnectTime (while _firstWaitForDate = false, meaning our partner has been in the Photon room at some point)


    Vector3 _gotoWhenDateArrives;   // Players will be spawned in "limbo," then go here once it's verified their date has arrived.

    #endregion

    // Wrappers (don't call extern in editor)
    private static void updatePlayersInRoom(string[] unityJSONList, int count)
    { 
        if( Application.isEditor ) 
            Debug.Log("EXTERN: updatePlayersInRoom(" + unityJSONList + ", " + count + ")");
        else
            NativeProxy.UpdatePlayersInRoom(unityJSONList, count);
    }

    public static void failureToConnect(string error)
    { 
        if( Application.isEditor )
            Debug.Log("EXTERN: failureToConnect(" +  error + ")"); 
        else
            NativeProxy.FailureToConnect( error );
    }

    private static void failureToJoinRoom(string error)
    { 
        if( Application.isEditor )
            Debug.Log("EXTERN: failureToJoinRoom(" + error + ")");
        else
            NativeProxy.FailureToJoinRoom( error );
    }

    /// <summary>
    /// NOTE: Call this when the player pushes a button indicating they actually want to leave.
    /// Native will need to handle this, but we also need to go back to the loading screen.
    /// </summary>
    public static void playerDidLeaveGame()
    { 
        PhotonNetwork.Disconnect();
        _wantsToLeave = true;

        // Forego the logic in DoLeaveGameActual until we've determined Photon disconnected OK
	}

    /// <summary>
    /// Only called after we're sure Photon disconnected.
    /// </summary>
    void DoLeaveGameActual()
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: playerDidLeaveRoom"); 
        else
            NativeProxy.PlayerDidLeaveGame();

        _wantsToLeave = false;   // reset static value
        initialized = false;   // reset static value
        realUserJson = null;    // This is a static variable. It signals to LoadingScreen that we'll need to wait for another createPlayer call before going back into Dateland.
        playerFromJson = null;  // static, gleaned from realUserJson
        partnerPlayer = -1;
        amPrimaryPlayer = false;

        Agora_Manager agora_manager = FindObjectOfType<Agora_Manager>();
        if( agora_manager )
            agora_manager.LeaveChannel();    // Clean up after Agora. Ready it for a possible different voice chat

        AddressablesLoadingScreen.comingFromDateland = true;   // ensures we will stay on that screen, no matter what, until we get a createPlayer call

		SceneManager.LoadScene("LoadingScreen");
    }




	#region MonoBehaviour CallBacks

    private void OnApplicationFocus(bool focused)
    {
        if( !Application.isEditor )
        {
            if (focused)
                appWillEnterForeground();
            else 
                appDidEnterBackground();
        }
    }

	public override void OnErrorInfo(ErrorInfo errorInfo)
    {
        Debug.Log(errorInfo.Info);
    }

    public void joinRoom()
    {
        PhotonNetwork.SendRate = 30;  //10;

        if( string.IsNullOrEmpty(previousRoom) )   // Should look for a room.
        {
            teamRoomJoin.myId = playerFromJson.user.id.ToString();
            teamRoomJoin.matchId = partnerPlayer.ToString();
            teamRoomJoin.amPrimaryPlayer = amPrimaryPlayer;

            if( string.IsNullOrEmpty(sandboxRoomName) || NativeEntryPoint.sandboxMode == false)
                teamRoomJoin.StartJoin();  // Starts the process of real matchmaking.
            else  // START SANDBOX ROOM
            {
                // Join designated sandbox room
                //PhotonNetwork.JoinOrCreateRoom( sandboxRoomName, teamRoomJoin.GetRoomOptions(), TypedLobby.Default );   // note that teamRoomJoin should check NativeEntryPoint.sandboxMode and give appropriate room options

                // NEW: Sandbox mode players are tossed in with real players.
                // Note that we have to respond to OnJoinRandomFailed in case there was no room available.
                teamRoomJoin.StartJoinSandbox();
            }
        }
        else
        {
            // We probably just want to reconnect to a previously existing room, but allow the room to be recreated in case we were the last player there.
            PhotonNetwork.JoinOrCreateRoom( previousRoom, teamRoomJoin.GetRoomOptions(), TypedLobby.Default );
        }
        
        needsToJoinRoom = false;
    }




    /// <summary>
    /// Broadcast message from SceneLoader. We need to leave the room
    /// </summary>
    void OnChangeScene(string scene_id)
    {
        // While changing rooms might, in theory, be a good idea, I'm going to avoid it for now
        // and just go for the simpler method of having each scene be located at different coordinates.

        /*
        if( !isConnecting )
        {
            isConnecting = true;
            lastSceneName = scene_id;
            PhotonNetwork.LeaveRoom();

            // When the OnConnectedToMaster callback occurs, it shohuld do joinRoom appropriately
        }
        */
    }






    public override void OnConnectedToMaster()
    {
        if (needsToJoinRoom)
            joinRoom();   // use name of the current scene to determine room.
        else
            failureToConnect("Failed to connect to Photon Server");
    }

    public void disconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        initialized = false;   // This should hopefully prevent net replicated objects from trying to use Photon
        initPlayer = true;   // This should cause us to make a new player, if we ever connect again


        needsToJoinRoom = false;
        Debug.Log("Photon disconnected: " + cause.ToString());
        //playerDidLeaveGame();   // don't call this yet! wait till they confirm they've been disconnected

        
        if( _wantsToLeave )   // This disconnect happened because they were trying to leave...
            DoLeaveGameActual();    // Can go back to loading screen
        else if( _disconnectedDueToInactivity )
        {
            // Don't do anything. Just display the dialog and wait for them to leave
        }
        else if( /*_gotLastKnownPlayerPosition && */ !_reconnecting )  // Show "reconnecting" popup (even if they were never really connected... this shows that a problem's happening)
        {
            _reconnecting = true;
            _reconnectTimer = 0.0f;  // reset it
        }

        if( _reconnecting )
            popupManager.ShowPopup("reconnecting");
    }


    public override void OnJoinedRoom()
    {

        previousRoom = PhotonNetwork.CurrentRoom.Name;   // save this in case we get disconnected.
        initPlayer = true;

        updatePlayerListAfterJoin();   // Native wants an initial updatePlayerList call.
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if( returnCode == ErrorCode.GameDoesNotExist )
            previousRoom = null;   // stop trying to reconnect to this room.

        Debug.Log("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
        failureToJoinRoom("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //Debug.Log("Number of players after someone coming in: " + PhotonNetwork.PlayerList.Length);
        updatePlayerListAfterJoin();
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //Debug.Log("Number of players after someone leaving: " + PhotonNetwork.PlayerList.Length);
        updatePlayerListAfterLeave();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //Debug.Log("ROOM PROPERTIES CHANGED");
    }

    #endregion

    #region NativeApp Functions

    public void UpdatePlayerList()
    {
        /*
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            string[] players = new string[PhotonNetwork.PlayerList.Length];

            // New: player list must always start with us, then include other players.
            int count = 1;   // start with index 1. Index 0 is reserved for this player.
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Photon.Realtime.Player p = (Photon.Realtime.Player)PhotonNetwork.PlayerList.GetValue(i);
                string json = (string)p.CustomProperties["myJSON"];

                if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)   // Found our own actor, put it in first index.
                    players[0] = json;
                else   // Actor besides the owned player; put in count index and update count
                {
                    players[count] = json;
                    count++;
                    //Debug.Log(JsonUtility.FromJson<Player_Class>(j).user.name);
                    //Debug.Log("Found Another Actor Number" + p.ActorNumber);
                }
            }
            //Debug.Log("Players after someone else joined: " + count);
            //Uncomment for Native App Version

            updatePlayersInRoom(players, players.Length);   // Makes its way to native app.
        }*/


        List<string> player_jsons = new List<string>();

        player_jsons.Add( realUserJson );   // First user must always be local player. We have this data stored already

        foreach( var plr in PhotonNetwork.PlayerList )
            if(plr.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber && plr.CustomProperties.ContainsKey("myJSON") )  // we already added local player
                player_jsons.Add((string)plr.CustomProperties["myJSON"]);


        updatePlayersInRoom(player_jsons.ToArray(), player_jsons.Count);   // Makes its way to native app.
    }


    public void updatePlayerListAfterJoin() => UpdatePlayerList();
    
    public void updatePlayerListAfterLeave() => UpdatePlayerList();    
    #endregion

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        agoraManager = FindObjectOfType<Agora_Manager>();

        //Connect();

        // Start connecting to room, so we can create player
        if( !string.IsNullOrEmpty(realUserJson) )   // this would have been set in CreatePlayerreceiver in LoadScreen.cs
            enterDateland();
        else  // test environment, use test JSON
        {
            realUserJson = testUserJson.text;
            playerFromJson = JsonUtility.FromJson<Player_Class>(realUserJson);
            enterDateland();
        }
    }

    private void Update()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            _reconnecting = false;   // We're connected, so reconnecting no longer applies.
            // Note that ConnectingPopup.cs handles disabling the connecting / reconnecting popups we've started, once everything's initialized.

            if (initPlayer)
            {
                initPlayer = false;
                if (Player_Controller_Mobile.mine == null)
                {


                    // Here we store all data that will be sent to OnPhotonInstantiate on all clients.
                    // We'll deal with skin number below (default to 0 for now).
                    object[] initArray = new object[] { 0, playerFromJson.user.name, playerFromJson.user.id, partnerPlayer };    


                    if( PlayerPrefs.HasKey( "skinNum" ) && !playerFromJson.user.isPaidPremium )
                    {
                        // Handle the special case that they used to have premium, but don't anymore.
                        // If they're using a premium skin, they'll have to re-randomize.
                        HashSet<int> non_premium_slots_hash = new HashSet<int>(inventoryPanel.nonPremiumSlots);
                        if( !non_premium_slots_hash.Contains( PlayerPrefs.GetInt( "skinNum" ) ) )
                        {
                            Debug.Log("Skin " + PlayerPrefs.GetInt("skinNum") + " can no longer be chosen. It's a premium skin, but we aren't a premium subscriber.");
                            PlayerPrefs.DeleteKey( "skinNum" );   // can't use this key anymore.
                        }
                    }


                    //This logic below is used to determine players skins coming into the Sandbox
                    //It utilizes the Custom Properties of the Room to store which skins have been used
                    if (PlayerPrefs.HasKey("skinNum"))
                    {
                        Debug.Log("Preferred skin retrieved from PlayerPrefs.");
                        initArray[0] = PlayerPrefs.GetInt("skinNum");
                    }
                    else
                    {
                        // The player has not explicitly selected a skin.
                        // Our goal is to get an even distribution of the default non-premium skins.
                        // So, we'll just count up how much of each of these skins are currently in use in the room.
                        // We'll find the minimum count (0 if there are none of certain skins, 1 if there is at least a set of one of each, etc)
                        // Then, we'll pick randomly among the skins that equal this count.
                        var skin_counts = new Dictionary<int, int>();   // skin slot number to number of them in the room.


                        Debug.Log("Searching for random skin slot.");

                        foreach( var plr in PhotonNetwork.CurrentRoom.Players )
                        {
                            // Note that players may not be instantiated yet, but we should still be able to read their CustomProperties
                            if( plr.Value.CustomProperties["CharacterSkinSlot"] != null )
                            {
                                Debug.Log("Found player with skin " + (int)plr.Value.CustomProperties["CharacterSkinSlot"]);
                                int slot = (int)plr.Value.CustomProperties["CharacterSkinSlot"];
                                if( !skin_counts.ContainsKey(slot) )
                                    skin_counts[slot] = 1;   // first entry in this count dictionary
                                else
                                    skin_counts[slot]++;   // add to existing count
                            }
                        }


                        // We've tallied up how much of each skin exists in the room, now find minimum among non premium slots.
                        int minimum = int.MaxValue;
                        foreach( int slot in inventoryPanel.nonPremiumSlots )
                        {
                            if( !skin_counts.ContainsKey(slot) )   // if we didn't count any of this key, that means there aren't any! set minimum to 0
                                minimum = 0;
                            else
                                minimum = Mathf.Min(minimum, skin_counts[slot]);   // take minimum...
                        }

                        Debug.Log("Full skin set is present " + minimum + " times over.");

                        // Now choose randomly among skins that were at this minimum value
                        List<int> rand_skin_pool = new List<int>();
                        foreach( int slot in inventoryPanel.nonPremiumSlots )
                        {
                            if( !skin_counts.ContainsKey(slot) || skin_counts[slot] <= minimum )   // if it's 0 add it no matter what, or if it's lower than minimum
                                rand_skin_pool.Add(slot);
                        }

                        // NOW we can choose it.
                        initArray[0] = rand_skin_pool[ Random.Range(0, rand_skin_pool.Count) ];
                        Debug.Log("Chose random skin slot: " + initArray[0] );
                    }




                    // Try to find a spawn point, else use (0,0,0)
                    Vector3 spawn_point = _lastKnownPlayerPosition;
                    if( !_gotLastKnownPlayerPosition )   // we've never actually had a last known player position, so find a spawn point
                    {
                        foreach( var spawn in FindObjectsOfType<DefaultPlayerSpawn>() )
                        if( spawn.comingFromScene == "" )   // the player isn't coming from any scene... so this is a match
                        {
                            spawn_point = spawn.transform.position;
                            break;
                        }
                    }


                    if( _firstWaitForDate )     // We haven't yet verified that our date is here...
                    {
                        Limbo limbo = FindObjectOfType<Limbo>();
                        if( limbo )  // Forget the spawn_point... we'll wait in Limbo until our date arrives, THEN go to the spawn point.
                        {
                            _gotoWhenDateArrives = spawn_point;
                            spawn_point = FindObjectOfType<Limbo>().transform.position;
                        }
                    }



                    PhotonNetwork.Instantiate(this.playerPrefab.name, spawn_point, Quaternion.identity, 0, initArray);
                   

                    ExitGames.Client.Photon.Hashtable me = new ExitGames.Client.Photon.Hashtable();
                    me.Add("myJSON", realUserJson);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(me, null, null);

                    initialized = true;
                    EventManager.TriggerEvent("OnPhotonConnect");   // Lets net replicated objects they need to request state again.
                }
            }
        }
        else if( _wantsToLeave && PhotonNetwork.NetworkClientState == ClientState.Disconnected )
        {
            // OK, we are allowed to leave now
            DoLeaveGameActual();
        }
        /*else if (PhotonNetwork.NetworkClientState == ClientState.Disconnected && madePlayer == null)
        {
            Connect();
        }*/

    }
    public void Connect()
    {
        // Before connecting, set up the user ID.
        // This will allow our match to find which room we're in.
        if( !NativeEntryPoint.sandboxMode )
        {
            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.UserId = playerFromJson.user.id.ToString();;
        }

        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        needsToJoinRoom = true;    // added by seb, bugfix
    }



  
    public void enterDateland()
    {
        PlayerPrefs.SetString("name", playerFromJson.user.name);
        PlayerPrefs.SetInt("id", playerFromJson.user.id);
        PlayerPrefs.SetString("partyId", playerFromJson.partyId);

        // Determine the ID of our partner player.
        // partyID is in the format  123:456
        // Partner player is the ID in this string that isn't our ID. One of them should be ours
        if( !NativeEntryPoint.sandboxMode )
        {
            string[] str_ids = playerFromJson.partyId.Split(':');
            bool test_mode = false;
            partnerPlayer = -1;
            foreach( string str_id in str_ids )
                if( int.Parse(str_id) != playerFromJson.user.id )  // different than our ID
                {
                    if( partnerPlayer != -1 )   // So was the other one.. print error
                    {
                        Debug.LogWarning("Both of the IDs in partyID \"" + playerFromJson.partyId + "\" were different from our ID (" + playerFromJson.user.id + "). Assuming test mode");
                        test_mode = true;
                    }
                    else
                        partnerPlayer = int.Parse(str_id);
                }

            // Determine if we are the primary player, who is in charge of finding a room
            amPrimaryPlayer = test_mode || (playerFromJson.user.id.ToString() == str_ids[0]);
        }
        else
        {
            // For sandbox mode, we should be able to just partner with ourselves and skip all the partner logic
            partnerPlayer = playerFromJson.user.id;
            amPrimaryPlayer = true;
        }



        Debug.Log("This is my Match ID: " + playerFromJson.partyId);
        //Connect();

        // The new-and-improved "reconnect" logic also works best for initial connection.
        // Just use that, so we'll keep trying for 60 seconds, then give up.
        _reconnecting = true;
        _reconnectTimer = 0.0f;
    }


    [ContextMenu("Simulate application foregrounded")]
    public void appWillEnterForeground()
    {
        Debug.Log("Application foregrounded.");
        if( _backgrounded )
        {
            _backgrounded = false;
            double seconds = (float)(System.DateTime.Now - _dateTimeBackgrounded).TotalSeconds;

            Debug.Log("Time backgrounded: " + seconds);

            if( seconds > (double)maxBackgroundedTime )  // They were backgrounded too long...
            {
                _disconnectedDueToInactivity = true;
                initialized = false;
                Debug.Log("Disconnecting player due to inactivity.");
                popupManager.ShowPopup( "disconnected_inactivity" );
                PhotonNetwork.Disconnect();   
                
                agoraManager.DisconnectWithoutRetry(); // disconnect Agora along with Photon.
            }
            else
            {
                // They weren't backgrounded too long. Can restore
                //if( agoraManager ) agoraManager.ReconnectNextFixedUpdate();   // Tells it to reconnect immediately after being backgrounded.
                // ^^^ this won't be necessary for now, because we decided to keep Agora connected while backgrounded
            }

        }
    }


    [ContextMenu("Simulate application backgrounded")]
    public void appDidEnterBackground()
    {
        PhotonNetwork.SendAllOutgoingCommands();
        _backgrounded = true;
        _dateTimeBackgrounded = System.DateTime.Now;

        
        //if( agoraManager ) agoraManager.DisconnectWithoutRetry();    // It will start trying to reconnect as soon as the app is running again.
        // ^^^ this won't be necessary for now, because we decided to keep Agora connected while backgrounded
        

        Debug.Log("Application backgrounded at time: " + _dateTimeBackgrounded );
    }


	private void FixedUpdate()
	{
        // We've been disconnected, but try to reconnect.  
        // If we're the secondary player waiting for the "friend" primary player to join a room (teamRoomJoin.isWaitingForFriend), we 
        // DON'T want this logic, as we just want to let TeamRoomJoin poll FindFriends until primary player has joined a room, and then
        // it does the join room itself.
		if( _reconnecting && !_disconnectedDueToInactivity && !teamRoomJoin.isWaitingForFriend )   
        {
            _reconnectTimer += Time.fixedDeltaTime;
            if( _reconnectTimer >= reconnectTime )   // We reached the end of our reconnect period, and no luck... show "disconnected" and give up
                popupManager.ShowPopup( "disconnected" );
            else if( Mathf.Floor( (_reconnectTimer-initialReconnectDelay) / reconnectInterval ) > Mathf.Floor( ((_reconnectTimer-initialReconnectDelay) - Time.fixedDeltaTime) / reconnectInterval ) )  // We just passed a reconnectInterval
            {
                Debug.Log("Attempting reconnect (attempt " + Mathf.Floor( _reconnectTimer / reconnectInterval ) + ")");
                Connect();
            }
        }

        // Save last known player position in case we get disconnected.
        if( Player_Controller_Mobile.mine != null )
        {
            _lastKnownPlayerPosition = Player_Controller_Mobile.mine.transform.position;
            _gotLastKnownPlayerPosition = true;
        }


        // FIRST WAIT FOR DATE
        // They can basically wait indefinitely until their partner joins.
        // Once the partner joins, we enable partnet disconnect logic.
        bool date_wait_popups = false;
        if( initialized && !_disconnectedDueToInactivity && _firstWaitForDate && Player_Controller_Mobile.mine != null  )   
        {

            if( Player_Controller_Mobile.mine.playerPartner.GetPartner() != null )   // Date connected! Proceed...
            {
                _firstWaitForDate = false;    // Note that ConnectingPopup will detect the partner, so we don't need to do anything more regarding popups.
                Player_Controller_Mobile.mine.transform.position = _gotoWhenDateArrives;     // Move (hopefully teleport) to the spawn point (we were in limbo before)
            }
            else
                date_wait_popups = true;
        }


        // Note: if we're the secondary player waiting for friend, we're not even initialized, and don't have a player controller,
        // but we still need to show the "waiting_for_partner" popup.
        if( teamRoomJoin.isWaitingForFriend && !_disconnectedDueToInactivity && _firstWaitForDate && PhotonNetwork.IsConnectedAndReady )
            date_wait_popups = true;

        
        if( date_wait_popups )
        {
            // We're connected, but our date isn't.
            // Display the first popup, until it's been a while, then display the second popup.
            _firstWaitForDateTimer += Time.fixedDeltaTime;
            if( _firstWaitForDateTimer < partnerTakingLongTime )
                popupManager.ShowPopup( "waiting_for_partner" );
            else
                popupManager.ShowPopup( "partner_long_time" );   // they're taking their sweet time...
        }


        cameraPanning.enabled = !_firstWaitForDate;   //only enable camera panning when we aren't waiting for date


        // PARTNER DISCONNECT
        // See if our partner is in the room...
        // Don't do it if we're disconnected. Might not be enabled if we're in "wait for date" mode. 
        if( initialized && !_disconnectedDueToInactivity && !_firstWaitForDate && !disablePartnerDisconnectForDebug && Player_Controller_Mobile.mine != null && Player_Controller_Mobile.mine.playerPartner.GetPartner() == null )   // Partner is MIA!
        {
            if( popupManager.curPopup != "leave_ok")   // The "leave game" popup is the only thing that can override disconnecting_partnet
                popupManager.ShowPopup( "disconnecting_partner" );

            _partnerDisconnectTimer += Time.fixedDeltaTime;
            if( _partnerDisconnectTimer >= partnerDisconnectTime )
            {
                // Time to disconnect.
                _disconnectedDueToInactivity = true;   // not strictly true, but this will prevent us from trying to reconnect all the same
                _firstWaitForDate = false;   // not needed anymore
                popupManager.ShowPopup( "disconnected_partner" );
                PhotonNetwork.Disconnect();

                agoraManager.DisconnectWithoutRetry();   // disconnect Agora as well
            }
            else
                partnerDisconnectText.text = "YOU WILL LEAVE THE ARCADE IN 0:" + (int)(partnerDisconnectTime - _partnerDisconnectTimer);
        }
        else
        {
            _partnerDisconnectTimer = 0;  // reset 
            if( popupManager.curPopup == "disconnecting_partner" )   // close popup
                popupManager.ShowPopup("");
        }
        

        // DISCONNECTED DUE TO INACTIVITY
        if( _disconnectedDueToInactivity )   // ensure the proper popup for this is shown no matter what!
             popupManager.ShowPopup( "disconnected_inactivity" );
	}






    /// <summary>
    /// Convenience function for inspector
    /// </summary>
    [ContextMenu("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

}
