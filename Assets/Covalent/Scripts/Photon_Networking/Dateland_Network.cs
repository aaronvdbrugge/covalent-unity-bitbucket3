
using System.Collections.Generic;
using Covalent.Scripts.Util.Native_Proxy;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Dateland_Network : Network_Manager
{
    [Tooltip("Will be fed to enterDateland automatically if enterDatelandTest is true")]
    public TextAsset testUserJson;

    [Tooltip("Will immediately enter dateland on Start, using testUserJson data.")]
    public bool enterDatelandTest = false;

    #region Private Serializable Fields
    [SerializeField]
    private byte maxPlayersPerRoom = 16;
    #endregion

    #region Public Fields
    public GameObject Connecting;
    public GameObject playerPrefab;
    public GameObject madePlayer;
    public Player_Class player;
    public string gameVersion = "1";
    public string SKIN_SLOT = "TESTING";


    [Tooltip("This will be prepended to the scene name. E.g., 'test_Dateland'")]
    public string roomNameBase = "test_";



    /// <summary>
    /// Check this value before you start doing things with photonView.IsMine...
    /// It seems like photonView.IsMine can sometimes erroneously be true until we actually get
    /// things set up.
    /// </summary>
    public static bool initialized = false;


    #endregion

    #region Private Fields
    private bool initPlayer = false;
    private bool player_removed;
    private bool createPlayerCalled = false;
    private bool isConnecting, inBackground;
    public int maxSkins = 10;  //this had a compiler warning. just made it public to avoid that. -seb
    private int connectToMasterFails, connectToRoomFail;
    private string player_JSON;


    public string lastSceneName = null;   // used for determining room name.


    /// <summary>
    /// This will be set from CreatePlayerReceiver with real profile JSON
    /// from the native side. It will remain null when running in editor or
    /// non-integrated on device.
    /// </summary>
    public static string realUserJson = null;

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
    /// Native should handle the rest
    /// </summary>
    public static void playerDidLeaveGame()
    { 
        if( Application.isEditor )
            Debug.Log("EXTERN: playerDidLeaveRoom"); 
        else
            NativeProxy.PlayerDidLeaveGame();
    }




    #region MonoBehaviour CallBacks

    /*
    private void OnApplicationFocus(bool focused)
    {
        if (focused)
        {
            Debug.Log("UNITY HAS BEEN FOCUSED");
            appWillEnterForeground();
        }
        else if (!focused)
        {
            Debug.Log("UNITY HAS BEEN UNFOCUSED");
            appDidEnterBackground();
        }
    }
    */

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
        isConnecting = false;
        PhotonNetwork.SendRate = 10;
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

        if (isConnecting)
        {
            joinRoom(GetRoomName());   // use name of the current scene to determine room.
        }
        else
        {
            connectToMasterFails += 1;
            if (connectToMasterFails == 3)
            {
                Debug.Log("Failed to connect to master 3 times, iOS failureToConnect(string error) should have been called");
                failureToConnect("Failed to connect to Photon Server");
            }
            else
            {
                Debug.Log("Failed to connect to master " + connectToMasterFails + " times, connecting again");
                Connect();
            }
            
        }
    }
    public void disconnectPhoton()
    {
        PhotonNetwork.Disconnect();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        Debug.Log("HELP ME IM DISCONNECTED AND HERE'S WHY: " + cause.ToString());
        //playerDidLeaveGame();   // don't call this yet! wait till they confirm they've been disconnected

        // Show disconnected popup
        Camera.main.GetComponent<Dateland_Camera>().popupManager.ShowPopup("disconnected");
    }


    public override void OnJoinedRoom()
    {
        initPlayer = true;
        Connecting.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
        connectToRoomFail += 1;
        if (connectToRoomFail == 3)
        {
            Debug.Log("Failed to connect to room 3 times, iOS failureToJoinRoom(string error) should have been called");
            failureToJoinRoom("Failed to join room. Error Code: " + returnCode + " Error Message: " + message);
        }
        else
        {
            Debug.Log("Failed to join room " + connectToRoomFail + " times, joining room again");
            joinRoom( GetRoomName() );   // Use scene name to determine proper room
        }
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
    public void updatePlayerRemoved()
    {
        Debug.Log("PLAYER REMOVED WAS CALLED");
        player_removed = true;
    }
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        inBackground = false;
        EventManager.StartListening("player_removed", updatePlayerRemoved);
    }
    private void Start()
    {
        player_removed = false;
        connectToMasterFails = 0;
        connectToRoomFail = 0;
        //Connect();

        // Start connecting to room, so we can create player
        if( !string.IsNullOrEmpty(realUserJson) )   // this would have been set in CreatePlayerreceiver in LoadScreen.cs
            enterDateland( realUserJson );
        else  // test environment, use test JSON
            enterDateland( testUserJson.text );
    }
    private void Update()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Joined)
        {
            if (initPlayer && createPlayerCalled)
            {
                initPlayer = false;
                createPlayerCalled = false;
                if (Player_Controller_Mobile.LocalPlayerInstance == null)
                {
                    object obj;
                    int skin_slot = -1, skinOffset;
                    object[] initArray = new object[2];
                    //This logic below is used to determine players skins coming into the Sandbox
                    //It utilizes the Custom Properties of the Room to store which skins have been used
                    if (PlayerPrefs.HasKey("skinNum"))
                    {
                        Debug.Log("I HAVE A PREFERENCE!");
                        initArray = new object[] { PlayerPrefs.GetInt("skinNum"), player.user.name };
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
                            initArray = new object[] { skin_slot * 4 + skinOffset, player.user.name };
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
                            initArray = new object[] { skin_slot * 4 + skinOffset, player.user.name };
                        }
                    }

                    if (inBackground)
                    {
                        float x = PlayerPrefs.GetFloat("xPos");
                        float y = PlayerPrefs.GetFloat("yPos");
                        float z = PlayerPrefs.GetFloat("zPos");
                        Vector3 playerPos = new Vector3(x, y, z);
                        madePlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, playerPos, Quaternion.identity, 0, initArray);
                        inBackground = false;
                    }
                    else
                    {
                        // Try to find a spawn point, else use (0,0,0)
                        Vector3 spawn_point = Vector3.zero;
                        foreach( var spawn in FindObjectsOfType<DefaultPlayerSpawn>() )
                            if( spawn.comingFromScene == "" )   // the player isn't coming from any scene... so this is a match
                            {
                                spawn_point = spawn.transform.position;
                                break;
                            }


                        madePlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, spawn_point, Quaternion.identity, 0, initArray);
                    }
                   
                    //madePlayer.GetComponent<Spine_Player_Controller>().characterSkinSlot = skin_slot*4 + skinOffset;
                    ExitGames.Client.Photon.Hashtable me = new ExitGames.Client.Photon.Hashtable();
                    me.Add("myJSON", player_JSON);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(me, null, null);

                    initialized = true;
                }
            }
        }
        /*else if (PhotonNetwork.NetworkClientState == ClientState.Disconnected && madePlayer == null)
        {
            Connect();
        }*/

    }
    public void Connect()
    {
        isConnecting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        isConnecting = true;    // added by seb, bugfix
    }
    public void destroyPlayer()
    {
        if (madePlayer != null)
        {
            //madePlayer.GetComponent<Player_Controller_Mobile>().cameraMain.transform.position = new Vector3(0, 0, -10);
            madePlayer.GetComponent<Player_Controller_Mobile>().photonView.RPC("destroyMe", RpcTarget.All, null);
            PhotonNetwork.SendAllOutgoingCommands();
            PhotonNetwork.Disconnect();
        }
    }
    public void backgroundPlayer()
    {
        if (madePlayer != null)
        {
            //madePlayer.GetComponent<Player_Controller_Mobile>().cameraMain.transform.position = new Vector3(0, 0, -10);
            madePlayer.GetComponent<Player_Controller_Mobile>().backgroundMe();
            PhotonNetwork.SendAllOutgoingCommands();
            //PhotonNetwork.Disconnect();
        }
    }

    public void enterDateland(string json_string)
    {
        player_JSON = json_string;
        player = JsonUtility.FromJson<Player_Class>(json_string); 
        PlayerPrefs.SetString("name", player.user.name);
        PlayerPrefs.SetString("partyId", player.partyId);
        //Debug.Log("I ON THE OTHER HAND AM THE JSON RECIEVED: " + json_string);
        Debug.Log("This is my Match ID: " + player.partyId);
        createPlayerCalled = true;
        Connect();
    }

    public void appWillEnterForeground()
    {
            if (player_removed)
            {
                player_removed = false;
                enterDateland(player_JSON);
            }
            else
            {
                EventManager.TriggerEvent("cancel_destroy");
                Connecting.SetActive(false);
                inBackground = false;
            }
        
    }
    public void appDidEnterBackground()
    {
            PlayerPrefs.SetFloat("xPos", madePlayer.transform.position.x);
            PlayerPrefs.SetFloat("yPos", madePlayer.transform.position.y);
            PlayerPrefs.SetFloat("zPos", madePlayer.transform.position.z);
            PlayerPrefs.SetInt("skinNum", madePlayer.GetComponent<Spine_Player_Controller>().characterSkinSlot);
            inBackground = true;
            Connecting.SetActive(true);
            backgroundPlayer();
        
    }


}
