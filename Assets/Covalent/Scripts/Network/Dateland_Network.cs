
using System.Collections;
using System.Collections.Generic;
using Covalent.Scripts.Util.Native_Proxy;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dateland_Network : Network_Manager
{







	#region Public Fields

    [Header("References")]
    [Tooltip("Will be fed to enterDateland automatically if createPlayer hasn't been called from native.")]
    public TextAsset testUserJson;
    public PopupManager popupManager;
    public GameObject playerPrefab;
    [Tooltip("\"YOU WILL LEAVE THE ARCADE IN 0:60\"")]
    public TMP_Text partnerDisconnectText;  

    [Tooltip("Don't enable this until our date has arrived, and we're out of Limbo.")]
    public CameraPanning cameraPanning;


    [Header("Settings")]

    public string gameVersion = "1";
    public string SKIN_SLOT = "TESTING";
    public byte maxPlayersPerRoom = 16;

    [Tooltip("This will be prepended to the scene name. E.g., 'test_Dateland'")]
    public string roomNameBase = "test_";

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


    static bool _wantsToLeave = false;   // playerDidLeaveGame has been called, but we need to wait for Photon to disconnect before we leave the scene.
	#endregion


    #region Private Fields
    private bool initPlayer = false;
    private bool tryingToJoinRoom;
    public int maxSkins = 10;  //this had a compiler warning. just made it public to avoid that. -seb
    private string player_JSON;


    public string lastSceneName = null;   // used for determining room name.



    // Reconnect logic
    bool _reconnecting = false;
    float _reconnectTimer = 0;

    bool _gotLastKnownPlayerPosition = false;
    Vector3 _lastKnownPlayerPosition = Vector3.zero;   // We'll use this so we can try to put the player in the same place after we disconnect.

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

    private static void failureToConnect(string error)
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

        FindObjectOfType<Agora_Manager>().LeaveChannel();    // Clean up after Agora. Ready it for a possible different voice chat

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

    public void joinRoom(string name)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.CustomRoomPropertiesForLobby = new string[] { SKIN_SLOT, "skinOffset" };
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.BroadcastPropsChangeToAll = true;
        roomOptions.MaxPlayers = maxPlayersPerRoom;
        PhotonNetwork.JoinOrCreateRoom(name, roomOptions, TypedLobby.Default);
        tryingToJoinRoom = false;
        PhotonNetwork.SendRate = 30;  //10;
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


	public string GetRoomName()
	{
        return roomNameBase;

        // Could be useful in the future - see OnChangeScene
        /*
        if( lastSceneName == null )   //haven't left or entered other scenes yet
            lastSceneName = SceneManager.GetActiveScene().name;
        return roomNameBase + lastSceneName;  // append scene name to the base name of the room we're all in.
        */
    }




    public override void OnConnectedToMaster()
    {
        if (tryingToJoinRoom)
            joinRoom(GetRoomName());   // use name of the current scene to determine room.
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


        tryingToJoinRoom = false;
        Debug.Log("HELP ME IM DISCONNECTED AND HERE'S WHY: " + cause.ToString());
        //playerDidLeaveGame();   // don't call this yet! wait till they confirm they've been disconnected

        
        if( _wantsToLeave )   // This disconnect happened because they were trying to leave...
            DoLeaveGameActual();    // Can go back to loading screen
        else if( _disconnectedDueToInactivity )
        {
            // Don't do anything. Just display the dialog and wait for them to leave
        }
        else if( _gotLastKnownPlayerPosition && !_reconnecting )  // Show "reconnecting" popup. Only relevant if we ever even had a player, otherwise show "connecting" instead
        {
            _reconnecting = true;
            popupManager.ShowPopup("reconnecting");
            _reconnectTimer = 0.0f;  // reset it
        }
        else if( !_gotLastKnownPlayerPosition )
            popupManager.ShowPopup("connecting");
    }


    public override void OnJoinedRoom()
    {
        initPlayer = true;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
        failureToJoinRoom("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //Debug.Log("Number of players after someone coming in: " + PhotonNetwork.PlayerList.Length);
        updatePlayerListAfterJoin(targetPlayer);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //Debug.Log("Number of players after someone leaving: " + PhotonNetwork.PlayerList.Length);
        updatePlayerListAfterLeave(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        //Debug.Log("ROOM PROPERTIES CHANGED");
    }

    #endregion

    #region NativeApp Functions
    public void updatePlayerListAfterJoin(Photon.Realtime.Player player)
    {
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            string[] players = new string[PhotonNetwork.PlayerList.Length - 1];
            int count = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Photon.Realtime.Player p = (Photon.Realtime.Player)PhotonNetwork.PlayerList.GetValue(i);
                if (p.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    string j = (string)p.CustomProperties["myJSON"];
                    players[count] = j;
                    //Debug.Log(JsonUtility.FromJson<Player_Class>(j).user.name);
                    count++;
                    //Debug.Log("Found Another Actor Number" + p.ActorNumber);
                }
            }
            //Debug.Log("Players after someone else joined: " + count);
            //Uncomment for Native App Version

            updatePlayersInRoom(players, players.Length);
        }

    }
    public void updatePlayerListAfterLeave(Photon.Realtime.Player player)
    {
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            string[] players = new string[PhotonNetwork.PlayerList.Length - 1];
            int count = 0;
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Photon.Realtime.Player p = (Photon.Realtime.Player)PhotonNetwork.PlayerList.GetValue(i);
                if (p.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                {
                    string j = (string)p.CustomProperties["myJSON"];
                    players[count] = j;
                    count++;
                    //Debug.Log(JsonUtility.FromJson<Player_Class>(j).user.name);
                    //Debug.Log("Found Another Actor Number" + p.ActorNumber);
                }

            }
            //Debug.Log("Players after someone left: " + players.Length);
            //Uncomment for Native App Version
            updatePlayersInRoom(players, players.Length);
        }

    }
    #endregion

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        //Connect();

        // Start connecting to room, so we can create player
        if( !string.IsNullOrEmpty(realUserJson) )   // this would have been set in CreatePlayerreceiver in LoadScreen.cs
            enterDateland( realUserJson );
        else  // test environment, use test JSON
        {
            playerFromJson = JsonUtility.FromJson<Player_Class>(testUserJson.text);
            enterDateland( testUserJson.text );
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
                    object obj;
                    int skin_slot = -1, skinOffset;


                    // Here we store all data that will be sent to OnPhotonInstantiate on all clients.
                    // We'll deal with skin number below (default to 0 for now).
                    object[] initArray = new object[] { 0, playerFromJson.user.name, playerFromJson.user.id };    

                    //This logic below is used to determine players skins coming into the Sandbox
                    //It utilizes the Custom Properties of the Room to store which skins have been used
                    if (PlayerPrefs.HasKey("skinNum"))
                    {
                        //Debug.Log("I HAVE A PREFERENCE!");
                        initArray[0] = PlayerPrefs.GetInt("skinNum");
                    }
                    else
                    {
                        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("skinOffset", out obj))
                        {
                            int[] availableSkins = (int[])PhotonNetwork.CurrentRoom.CustomProperties[SKIN_SLOT];
                            skinOffset = (int)PhotonNetwork.CurrentRoom.CustomProperties["skinOffset"];
                            if (availableSkins.Length == 1)
                            {
                                skin_slot = availableSkins[0];
                                availableSkins = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                                skinOffset++;
                                if (skinOffset > 3)
                                {
                                    skinOffset = 0;
                                }
                            }
                            else
                            {
                                int random = Random.Range(0, availableSkins.Length);
                                skin_slot = availableSkins[random];
                                List<int> temp = new List<int>();
                                for (int i = 0; i < availableSkins.Length; i++)
                                {
                                    temp.Add(availableSkins[i]);
                                }
                                temp.RemoveAt(random);
                                availableSkins = temp.ToArray();
                            }


                            ExitGames.Client.Photon.Hashtable Skin_Slot = new ExitGames.Client.Photon.Hashtable();
                            Skin_Slot.Add(SKIN_SLOT, availableSkins);
                            Skin_Slot.Add("skinOffset", skinOffset);
                            PhotonNetwork.CurrentRoom.SetCustomProperties(Skin_Slot, null, null);
                            initArray[0] = skin_slot * 4 + skinOffset;
                        }
                        else
                        {
                            //Debug.Log("No custom Properties in this room");
                            skinOffset = 0;
                            List<int> excludedInts = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                            int random = Random.Range(0, excludedInts.Count);
                            skin_slot = excludedInts[random];
                            excludedInts.RemoveAt(random);
                            int[] intArray = excludedInts.ToArray();

                            ExitGames.Client.Photon.Hashtable Skin_Slot = new ExitGames.Client.Photon.Hashtable();
                            Skin_Slot.Add(SKIN_SLOT, intArray);
                            Skin_Slot.Add("skinOffset", skinOffset);
                            PhotonNetwork.CurrentRoom.SetCustomProperties(Skin_Slot, null, null);
                            initArray[0] = skin_slot * 4 + skinOffset;
                        }
                    }

                    /*
                    if (inBackground)
                    {
                        float x = PlayerPrefs.GetFloat("xPos");
                        float y = PlayerPrefs.GetFloat("yPos");
                        float z = PlayerPrefs.GetFloat("zPos");
                        Vector3 playerPos = new Vector3(x, y, z);
                        PhotonNetwork.Instantiate(this.playerPrefab.name, playerPos, Quaternion.identity, 0, initArray);
                        inBackground = false;
                    }
                    else*/   // deprecated?

                    {
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
                    }
                   
                    //madePlayer.GetComponent<Spine_Player_Controller>().characterSkinSlot = skin_slot*4 + skinOffset;
                    ExitGames.Client.Photon.Hashtable me = new ExitGames.Client.Photon.Hashtable();
                    me.Add("myJSON", player_JSON);
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
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        tryingToJoinRoom = true;    // added by seb, bugfix
    }



  
    public void enterDateland(string json_string)
    {
        player_JSON = json_string;
        PlayerPrefs.SetString("name", playerFromJson.user.name);
        PlayerPrefs.SetInt("id", playerFromJson.user.id);
        PlayerPrefs.SetString("partyId", playerFromJson.partyId);

        // Determine the ID of our partner player.
        // partyID is in the format  123:456
        // Partner player is the ID in this string that isn't our ID. One of them should be ours
        string[] str_ids = playerFromJson.partyId.Split(':');
        int partner_id = -1;
        foreach( string str_id in str_ids )
            if( int.Parse(str_id) != playerFromJson.user.id )  // different than our ID
            {
                if( partner_id != -1 )   // So was the other one.. print error
                    Debug.LogError("Both of the IDs in partyID \"" + playerFromJson.partyId + "\" were different from our ID (" + playerFromJson.user.id + ")");
                else
                    partner_id = int.Parse(str_id);
            }
        PlayerPrefs.SetInt("partnerId", partner_id);




        //Debug.Log("I ON THE OTHER HAND AM THE JSON RECIEVED: " + json_string);
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
        if (!_disconnectedDueToInactivity)
            CancelPlayerBackgroundedCoroutine();
    }


    [ContextMenu("Simulate application backgrounded")]
    public void appDidEnterBackground()
    {
        /*
        if( Player_Controller_Mobile.mine )
        {
            PlayerPrefs.SetFloat("xPos", Player_Controller_Mobile.mine.transform.position.x);
            PlayerPrefs.SetFloat("yPos", Player_Controller_Mobile.mine.transform.position.y);
            PlayerPrefs.SetFloat("zPos", Player_Controller_Mobile.mine.transform.position.z);
            PlayerPrefs.SetInt("skinNum", Player_Controller_Mobile.mine.GetComponent<Spine_Player_Controller>().characterSkinSlot);
        }
        */

        PhotonNetwork.SendAllOutgoingCommands();
        StartPlayerBackgroundedCoroutine();
    }


	private void FixedUpdate()
	{
		if( _reconnecting && !_disconnectedDueToInactivity )   // We've been disconnected, but try to reconnect
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
        if( initialized && !_disconnectedDueToInactivity && _firstWaitForDate && Player_Controller_Mobile.mine != null  )   
        {
            if( Player_Controller_Mobile.mine.playerPartner.GetPartner() != null )   // Date connected! Proceed...
            {
                _firstWaitForDate = false;    // Note that ConnectingPopup will detect the partner, so we don't need to do anything more regarding popups.
                Player_Controller_Mobile.mine.transform.position = _gotoWhenDateArrives;     // Move (hopefully teleport) to the spawn point (we were in limbo before)
            }
            else
            {
                // We're connected, but our date isn't.
                // Display the first popup, until it's been a while, then display the second popup.
                _firstWaitForDateTimer += Time.fixedDeltaTime;
                if( _firstWaitForDateTimer < partnerTakingLongTime )
                    popupManager.ShowPopup( "waiting_for_partner" );
                else
                    popupManager.ShowPopup( "partner_long_time" );   // they're taking their sweet time...
            }
        }

        cameraPanning.enabled = !_firstWaitForDate;   //only enable camera panning when we aren't waiting for date


        // PARTNER DISCONNECT
        // See if our partner is in the room...
        // Don't do it if we're disconnected. Might not be enabled if we're in "wait for date" mode. 
        if( initialized && !_disconnectedDueToInactivity && !_firstWaitForDate && !disablePartnerDisconnectForDebug && Player_Controller_Mobile.mine != null && Player_Controller_Mobile.mine.playerPartner.GetPartner() == null )   // Partner is MIA!
        {
            popupManager.ShowPopup( "disconnecting_partner" );

            _partnerDisconnectTimer += Time.fixedDeltaTime;
            if( _partnerDisconnectTimer >= partnerDisconnectTime )
            {
                // Time to disconnect.
                _disconnectedDueToInactivity = true;   // not strictly true, but this will prevent us from trying to reconnect all the same
                _firstWaitForDate = false;   // not needed anymore
                popupManager.ShowPopup( "disconnected_partner" );
                PhotonNetwork.Disconnect();
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



    
    /*
     * "Backgrounded" logic.
     * The coroutine makes me a little nervous, but it may be necessary
     * to keep things going while the app is backgrounded.
     */

    /// <summary>
    /// When the player is backgrounded, this coroutine will count down to a player
    /// disconnect.
    /// </summary>
    private Coroutine playerBackgroundedCoroutine;
    private bool playerBackgroundedCoroutineEnabled = false;   // indicates that playerBackgroundedCoroutine should finish the job after waiting


    /// <summary>
    /// Will count down to a disconnect due to inactivity.
    /// </summary>
    void StartPlayerBackgroundedCoroutine()
    {
        if( !playerBackgroundedCoroutineEnabled )
        {
            playerBackgroundedCoroutineEnabled = true;
            playerBackgroundedCoroutine = StartCoroutine( PlayerBackgroundedCoroutine() );
        }
    }

    public IEnumerator PlayerBackgroundedCoroutine()
    {
        Debug.Log("Started player backgrounded coroutine.");
        yield return new WaitForSecondsRealtime( maxBackgroundedTime );
        if (playerBackgroundedCoroutineEnabled)
        {
            _disconnectedDueToInactivity = true;
            initialized = false;

            Debug.Log("Disconnecting player due to inactivity.");

            popupManager.ShowPopup( "disconnected_inactivity" );
            PhotonNetwork.Disconnect();
        }
        else
            Debug.Log("Player backgrounded coroutine cancelled.");
    }

	public void CancelPlayerBackgroundedCoroutine()
    {
        Debug.Log("Cancelling player backgrounded coroutine.");
        if (playerBackgroundedCoroutine != null)
        {
            StopCoroutine(playerBackgroundedCoroutine);
            playerBackgroundedCoroutineEnabled = false;
        }
    }




}
