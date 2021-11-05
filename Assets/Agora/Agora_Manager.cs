using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using Covalent.Scripts.Util.Native_Proxy;

#if (UNITY_2018_3_OR_NEWER)
using UnityEngine.Android;
#endif

public class Agora_Manager : MonoBehaviour
{
    private static int ERROR_NO_PERMISSION_TO_RECORD = 1027;

    private static int ERROR_NO_PERMISSION_TO_ACCESS = 9;

    public bool joinChatInEditor = false;


    //Wrappers (don't call externs in editor)
    private static void failureToConnectAgora(string error)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: failureToConnectAgora(" + error + ")");
        else
            NativeProxy.FailureToConnectAgora(error);
    }

    private static void playerDidMute(uint player_id)
    { 
        if( Application.isEditor)
            Debug.Log("EXTERN: playerDidMute(" + player_id + ")");
        else
            NativeProxy.PlayerDidMute( player_id );
    }

    private static void playerDidUnmute(uint player_id)
    { 
        if( Application.isEditor )
            Debug.Log("EXTERN: playerDidUnmute(" + player_id + ")");
        else
            NativeProxy.PlayerDidUnmute( player_id );
    }


    private static void playerStartedTalking(uint player_id)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: playerStartedTalking(" + player_id + ")");
        else
            NativeProxy.PlayerStartedTalking(player_id);
    }

    private static void playerEndedTalking(uint player_id)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: playerEndedTalking(" + player_id + ")");
        else
            NativeProxy.PlayerEndedTalking(player_id);
    }

    public Text logs;
    public IRtcEngine mRtcEngine = null;
    public AudioRecordingDeviceManager audio_manager;

    [Header("Settings")]
    [Range(0,255)]
    [Tooltip("We'll report to native that the user is \"talking\" if the volume is >= this amount.")]
    public int talkVolumeThreshold = 32;
    [Tooltip("Agora recommends over 200ms.")]
    public int talkingReportIntervalMs = 250;   
    [Tooltip("If no reports from Agora, we assume they aren't talking. Should probably be slightly more than talkingReportIntervalMs")]
    public int assumeNotTalkingTime = 300;   

    [Tooltip("We'll just keep trying to reconnect every this amount of time, if we get disconnected.")]
    public float disconnectRetryInterval = 10.0f;



    [Header("Runtime")]
    public uint myUid;  //save our own uid in ChannelOnJoinChannelSuccess

    public bool isMuted = false;   // informational



    /// <summary>
    /// Remember if a uid was talking; this allows us to not spam the extern calls
    /// </summary>
    Dictionary<uint, bool> usersTalking = new Dictionary<uint, bool>();

    // Because Agora's calllback doesn't report anything when volume is 0, we'll have to figure that out on our own.
    // This is set to 0 every time we update usersTalking, but will increase in Update, and we'll assume they aren't talking
    // if it makes it to 
    Dictionary<uint, float> timeSinceUserLastTalked = new Dictionary<uint, float>();

    [SerializeField]
    private string appId = "ebc5c7daf04648c3bfa3083be4f7c53a";

    float _disconnectRetryCooldown=0;  // set to disconnectRetryInterval on retry
    bool _wasConnected = false;   // were we ever connected? use to detect disconnect

    /// <summary>
    /// You should call this when you disconnect Agora due to inactivity, to prevent it from reconnecting.
    /// You can call it when getting backgrounded, as well.
    /// </summary>
    public void DisconnectWithoutRetry()
    {
        mRtcEngine.LeaveChannel();
        _wasConnected = false;  // prevents retrying connection
    }

    /// <summary>
    /// Ensures we'll try reconnecting to Agora right now, if it isn't already.
    /// </summary>
    public void ReconnectNextFixedUpdate()
    {
        _disconnectRetryCooldown = 0;
        _wasConnected = true;
    }



    string _lastChannel = null;   // if JoinChannel was ever called, this will be the parameter it was given

    private IEnumerator requestMicrophone()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {

        }
        else
        {

        }
    }

    private void Awake()
    {
        // Weird... could this have been necessary for some reason?
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;
    }

    private void Start()
    {
        mRtcEngine = IRtcEngine.GetEngine(appId);
        mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_HIGH_QUALITY_STEREO, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_MEETING);
        mRtcEngine.SetDefaultAudioRouteToSpeakerphone(true);
        mRtcEngine.EnableAudioVolumeIndication(300, 3, true);

#if PLATFORM_ANDROID
         if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
         {
             Permission.RequestUserPermission(Permission.Microphone);
         }
#elif UNITY_IOS
        StartCoroutine(requestMicrophone());
#endif

         
        mRtcEngine.OnConnectionStateChanged += (CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason) =>
        {
            Debug.Log("on connection state changed to " + state + " reason: " + reason);
        };

        mRtcEngine.OnError += (int error, string msg) =>
        {
            if( !Application.isEditor ) 
                failureToConnectAgora("Agora Error: " + error + " Message: " + msg);
            if (error == ERROR_NO_PERMISSION_TO_ACCESS || error == ERROR_NO_PERMISSION_TO_RECORD)
            {
                Debug.Log("Microphone not enabled on device");
                StartCoroutine(requestMicrophone());
            }
            Debug.Log("Error: " + error + " Message: " + msg);
        };


        mRtcEngine.OnJoinChannelSuccess += (string channelName, uint uid, int elapsed) =>
        {
            isMuted = false;    // we probably won't be joining the channel muted

            Debug.Log("Joined Agora Channel: " + channelName);
            myUid = uid;
            if (!mRtcEngine.IsSpeakerphoneEnabled())
            {
                mRtcEngine.SetEnableSpeakerphone(true);
            }
        };
        





        
        // This is the recommended replacement for the "deprecated" OnUserMutedAudio, but it doesn't
        // seem to work....
        //mRtcEngine.OnAudioDeviceStateChanged += (string deviceId, int deviceType, int deviceState) =>
        //{
        //    Debug.Log("OnAudioDeviceStateChanged: " + deviceId + " : " + deviceType + " : " + deviceState);
        //};


        // This is the function that gets called when THIS user changes mute state (not the remote user)
        mRtcEngine.OnAudioPublishStateChanged += (string channel, STREAM_PUBLISH_STATE oldState, STREAM_PUBLISH_STATE newState, int elapseSinceLastState) =>
        {
            //Debug.Log("OnAudioPublishStateChanged: " + channel + " : " + oldState + " : " + newState + " : " + elapseSinceLastState);
            if( newState == STREAM_PUBLISH_STATE.PUB_STATE_PUBLISHING )
            {
                isMuted = false;
                playerDidUnmute( myUid );
            }
            else if( newState == STREAM_PUBLISH_STATE.PUB_STATE_NO_PUBLISHED )
            {
                isMuted = true;
                playerDidMute( myUid );
            }
        };


        // This is the function that gets called when a REMOTE user changes mute state (but not this user)
        // NOTE: This is marked as "deprecated" in the Agora docs, but the function they recommend to replace it,
        // OnAudioDeviceStateChanged, doesn't seem to work for muting.
        // OnAudioPublishStateChanged doesn't seem to work either... so as far as I know, we're stuck using this "deprecated" function.
        mRtcEngine.OnUserMutedAudio += (uint uid, bool muted) =>
        {
            //Debug.Log("OnUserMutedAudio: " + uid + " : " + muted );
            if( muted )
            {
                isMuted = true;
                playerDidMute( uid );
            }
            else
            {
                isMuted = false;
                playerDidUnmute( uid );
            }
        };



        // Enables user volume reporting, including for the local user.
        // For recommended parameters:
        //   https://docs.agora.io/en/Video/API%20Reference/unity/classagora__gaming__rtc_1_1_i_rtc_engine.html#aebdcd5d2d8a05e76532c5d55b768235d
        mRtcEngine.EnableAudioVolumeIndication(talkingReportIntervalMs, 3, true);
        mRtcEngine.OnVolumeIndication += (AudioVolumeInfo[] speakers, int speakerNumber, int totalVolume) =>
        {
            // This callback makes things difficult, in that it just gives an empty array if nobody's talking.
            // So, in order to get this to work how we want, we're going to have to keep track of when the last
            // volume level was reported, and if we don't hear back in time, we'll assume their volume dropped to 0.

            //Debug.Log("OnVolumeIndication( " + speakers + " , " + speakerNumber + " , " + totalVolume + ")");

            // Agora sends two of these. One I believe just has a speakers of length 1, and a uid of 0 (BUT it'll be empty if the local volume is 0).
            // This is the volume of the local user.
            // The other callback actually has a list of volumes and uids in speakers.

            HashSet<uint> found_uids = new HashSet<uint>();  //keep track of which uids were reported
            foreach( AudioVolumeInfo info in speakers )
            {
                uint uid = info.uid;
                if( info.uid == 0 )   // it's us. use our uID
                    uid = myUid;
                bool talking = info.volume >= talkVolumeThreshold;

                if( !usersTalking.ContainsKey(uid) || usersTalking[uid] != talking )   //Talking state changed from before.
                {
                    usersTalking[uid] = talking;  // will prevent spamming of the extern calls
                    if( talking )
                        playerStartedTalking( uid );
                    else
                        playerEndedTalking( uid );
                }

                timeSinceUserLastTalked[uid] = 0;   // This "talking" value is up to date.
            }

            // See Update() for logic which deals with assuming they aren't talking, when we haven't heard from them in long enough.
        };



    }


	private void Update()
	{
        // We have to look for users who stopped talking, because of the way Agora's callback works.
		foreach( KeyValuePair<uint, float> kvp in new Dictionary<uint, float>(timeSinceUserLastTalked) )   // new dictionary needed so we can modify during iteration
        {
            timeSinceUserLastTalked[kvp.Key] = kvp.Value + Time.deltaTime;   // add deltatime to cooldowns
            if( kvp.Value >= assumeNotTalkingTime / 1000.0f )   // We haven't heard from them in a while. Assume they aren't talking
                if( usersTalking.ContainsKey( kvp.Key ) && usersTalking[kvp.Key] )   // We thought they were talking. Guess not
                {
                    usersTalking[kvp.Key] = false;
                    playerEndedTalking( kvp.Key );
                }
        }
	}


	public void mute(bool muted)
    {
        mRtcEngine.MuteLocalAudioStream( muted );
    }

    [ContextMenu("Do Mute")]
    public void DoMute() => mute(true);
    [ContextMenu("Do Unmute")]
    public void DoUnmute() => mute(false);



    /// <summary>
    /// Called from mute toggle button
    /// </summary>
    public void MuteToggle( Toggle mute_toggle )
    {
        mute( mute_toggle.isOn );
    }


    string _doJoinChannel = null;   //delays a JoinChannel call until FixedUpdate, allowing Start() to occur first
    /// <summary>
    /// NOTE: don't call this before Dateland_Network.playerFromJson has been initialized. We need the Kippo ID to be our Agora ID.
    /// </summary>
    public void JoinChannel(string channel_name)
    {
        _doJoinChannel = channel_name;  // defer action to FixedUpdate
    }


    public AgoraChannel Obj_JoinChannel(string name)
    {
        AgoraChannel newChannel = mRtcEngine.CreateChannel(name);

        newChannel.ChannelOnJoinChannelSuccess += (string channelName, uint uid, int elapsed) =>
        {
            Debug.Log("Joined Channel: " + channelName);
            if (!mRtcEngine.IsSpeakerphoneEnabled())
            {
                mRtcEngine.SetEnableSpeakerphone(true);
            }
        };
        ChannelMediaOptions mediaOptions = new ChannelMediaOptions();
        mediaOptions.autoSubscribeAudio = true;
        newChannel.JoinChannel("", "", 0, mediaOptions);
        return newChannel;
    }

    public void LeaveChannel()
    {
        Debug.Log("Leaving Agora channel");
        mRtcEngine.LeaveChannel();
        _lastChannel = null;  // so we don't try to reconnect
        _wasConnected = false;
    }

    public void LeaveChannel(AgoraChannel channel)
    {
        channel.LeaveChannel();
    }

    void OnApplicationQuit()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }

    void OnDestroy()
    {
        if (mRtcEngine != null)
        {
            mRtcEngine.LeaveChannel();
            IRtcEngine.Destroy();
            mRtcEngine = null;
        }
    }


	private void FixedUpdate()
	{
        if( _doJoinChannel!=null && (joinChatInEditor || !Application.isEditor) )
        {
            _lastChannel = _doJoinChannel;  // in case we get disconnected
            _wasConnected = false;
            Debug.Log("Joining Agora channel: " + _doJoinChannel + " with ID " + Dateland_Network.playerFromJson.user.id);


            uint use_id = (uint)Dateland_Network.playerFromJson.user.id;   // Use Kippo ID for our Agora ID.
            if( NativeEntryPoint.sandboxMode )
                use_id = 0;   // ID is probably not correct, so avoid duplicate IDs and just let Agora choose.


            
            mRtcEngine.JoinChannel(_doJoinChannel, "extra", use_id);   
            isMuted = false;
            _doJoinChannel = null;  // reset
        }

		// Look for disconnect
        if( mRtcEngine.GetConnectionState() == CONNECTION_STATE_TYPE.CONNECTION_STATE_CONNECTED )  
            _wasConnected = true;   // were we connected at least once? prerequisite for disconnect

        _disconnectRetryCooldown = Mathf.Max(0, _disconnectRetryCooldown - Time.fixedDeltaTime);
        if( _wasConnected && _lastChannel != null && _disconnectRetryCooldown <= 0 &&  mRtcEngine.GetConnectionState() != CONNECTION_STATE_TYPE.CONNECTION_STATE_CONNECTED )
        {
            Debug.Log("Disconnected from Agora. Trying to reconnect to channel " + _lastChannel );

            uint use_id = (uint)Dateland_Network.playerFromJson.user.id;   // Use Kippo ID for our Agora ID.
            if( NativeEntryPoint.sandboxMode )
                use_id = 0;   // ID is probably not correct, so avoid duplicate IDs and just let Agora choose.

            mRtcEngine.JoinChannel(_lastChannel, "extra", use_id);
            _disconnectRetryCooldown = disconnectRetryInterval;   // don't retry for a while
        }
	}


    public bool IsConnected() => mRtcEngine == null ? false : mRtcEngine.GetConnectionState() == CONNECTION_STATE_TYPE.CONNECTION_STATE_CONNECTED;

}
