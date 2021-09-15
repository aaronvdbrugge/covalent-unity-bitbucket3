using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    [SerializeField]
    private byte maxPlayersPerRoom = 10;
    #endregion

    #region Public Fields
    public GameObject playerPrefab;
    public GameObject madePlayer;
    public Player_Class player;
    public string gameVersion = "1";
    public string SKIN_SLOT = "TESTING";
    #endregion

    #region Private Fields
    private bool initPlayer = false;
    private bool createPlayerCalled = false;
    private bool isConnecting;
    public int maxSkins = 10;   //this had a compiler warning. just made it public to avoid that. -seb
    private string player_JSON;
    #endregion

    [DllImport("__Internal")]
    private static extern bool _updatePlayersInRoom(string[] unityJSONList, int count);


    #region MonoBehaviour CallBacks

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.CustomRoomPropertiesForLobby = new string[] { SKIN_SLOT, "skinOffset" };
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.BroadcastPropsChangeToAll = true;
            roomOptions.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.JoinOrCreateRoom("Social_Hour", roomOptions, TypedLobby.Default);
            isConnecting = false;
            PhotonNetwork.SendRate = 10;
        }
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnecting = false;
        Debug.Log("HELP ME IM DISCONNECTED AND HERE'S WHY: " + cause.ToString());
    }

    public override void OnJoinedRoom()
    {
        initPlayer = true;
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
        if (PhotonNetwork.PlayerList.Length > 1)
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
            //_updatePlayersInRoom(players, players.Length);
        }

    }
    public void updatePlayerListAfterLeave(Photon.Realtime.Player player)
    {
        if (PhotonNetwork.PlayerList.Length > 1)
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
            //_updatePlayersInRoom(players, players.Length);
        }

    }
    #endregion

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        Connect();
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

                    //This logic below is used to determine players skins coming into the Sandbox
                    //It utilizes the Custom Properties of the Room to store which skins have been used
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
                    }
                    object[] initArray = new object[] { skin_slot * 4 + skinOffset , player.user.name}; 
                    madePlayer = PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0, initArray);
                    //madePlayer.GetComponent<Spine_Player_Controller>().characterSkinSlot = skin_slot*4 + skinOffset;
                    ExitGames.Client.Photon.Hashtable me = new ExitGames.Client.Photon.Hashtable();
                    me.Add("myJSON", player_JSON);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(me, null, null);

                }
            }
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.Disconnected && madePlayer == null)
        {
            Connect();
        }

    }
    public void Connect()
    {
        isConnecting = PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
    }
    private void destroyPlayer()
    {
        if (madePlayer != null)
        {
            //madePlayer.GetComponent<Player_Controller_Mobile>().cameraMain.transform.position = new Vector3(0, 0, -10);   //necessary? -seb
            madePlayer.GetComponent<Player_Controller_Mobile>().photonView.RPC("destroyMe", RpcTarget.All, null);
            PhotonNetwork.Disconnect();
        }
    }

    private void createPlayer(string json_string)
    {
        player_JSON = json_string;
        player = JsonUtility.FromJson<Player_Class>(json_string);
        PlayerPrefs.SetString("name", player.user.name);
        createPlayerCalled = true;
    }


}