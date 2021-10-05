using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
using Photon.Pun;
using System.Runtime.InteropServices;

#if (UNITY_2018_3_OR_NEWER)
using UnityEngine.Android;
#endif

public class Agora_Manager : MonoBehaviour
{



    // External functions (to native app)
    [DllImport("__Internal")]
    private static extern void _failureToConnectAgora(string error);
    [DllImport("__Internal")]
    private static extern void _playerDidMute(uint player_id);
    [DllImport("__Internal")]
    private static extern void _playerDidUnmute(uint player_id);
    [DllImport("__Internal")]
    private static extern void _playerStartedTalking(uint player_id);
    [DllImport("__Internal")]
    private static extern void _playerEndedTalking(uint player_id);


    //Wrappers (don't call externs in editor)
    private static void failureToConnectAgora(string error)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: failureToConnectAgora(" + error + ")");
        else
            _failureToConnectAgora(error);
    }

    private static void playerDidMute(uint player_id)
    { 
        if( Application.isEditor)
            Debug.Log("EXTERN: playerDidMute(" + player_id + ")");
        else
            _playerDidMute( player_id );
    }

    private static void playerDidUnmute(uint player_id)
    { 
        if( Application.isEditor )
            Debug.Log("EXTERN: playerDidUnmute(" + player_id + ")");
        else
            _playerDidUnmute( player_id );
    }


    private static void playerStartedTalking(uint player_id)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: playerStartedTalking(" + player_id + ")");
        else
            _playerStartedTalking(player_id);
    }

    private static void playerEndedTalking(uint player_id)
    {
        if( Application.isEditor )
            Debug.Log("EXTERN: playerEndedTalking(" + player_id + ")");
        else
            _playerEndedTalking(player_id);
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


    [Header("Runtime")]
    public uint myUid;  //save our own uid in ChannelOnJoinChannelSuccess



    /// <summary>
    /// Remember if a uid was talking; this allows us to not spam the extern calls
    /// </summary>
    Dictionary<uint, bool> usersTalking = new Dictionary<uint, bool>();

    // Because Agora's calllback doesn't report anything when volume is 0, we'll have to figure that out on our own.
    // This is set to 0 every time we update usersTalking, but will increase in Update, and we'll assume they aren't talking
    // if it makes it to 
    Dictionary<uint, float> usersTalkingCooldown = new Dictionary<uint, float>();






    [SerializeField]
    private string appId = "ebc5c7daf04648c3bfa3083be4f7c53a";

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
    }

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

    private void Start()
    {
        mRtcEngine = IRtcEngine.GetEngine(appId);
        mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_SPEECH_STANDARD, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM_ENTERTAINMENT);
        mRtcEngine.SetDefaultAudioRouteToSpeakerphone(true);
        mRtcEngine.EnableAudioVolumeIndication(300, 3, true);


#if PLATFORM_ANDROID
         if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
         {
             Permission.RequestUserPermission(Permission.Camera);
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
                failureToConnectAgora("Error: " + error + " Message: " + msg);
            if (error == 9)
            {
                Debug.Log("Microphone not enabled on device");
                StartCoroutine(requestMicrophone());
            }
            Debug.Log("Error: " + error + " Message: " + msg);
        };

        mRtcEngine.OnJoinChannelSuccess += (string channelName, uint uid, int elapsed) =>
        {
            Debug.Log("Joined Channel: " + channelName);
            myUid = uid;
            if (!mRtcEngine.IsSpeakerphoneEnabled())
            {
                mRtcEngine.SetEnableSpeakerphone(true);
            }
        };
        
        mRtcEngine.OnLeaveChannel += (RtcStats stats) =>
        {
            //string leaveChannelMessage = string.Format("onLeaveChannel callback duration {0}, tx: {1}, rx: {2}, tx kbps: {3}, rx kbps: {4}", stats.duration, stats.txBytes, stats.rxBytes, stats.txKBitRate, stats.rxKBitRate);
            //Debug.Log(leaveChannelMessage);
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
                playerDidUnmute( myUid );
            else if( newState == STREAM_PUBLISH_STATE.PUB_STATE_NO_PUBLISHED )
                playerDidMute( myUid );
        };


        // This is the function that gets called when a REMOTE user changes mute state (but not this user)
        // NOTE: This is marked as "deprecated" in the Agora docs, but the function they recommend to replace it,
        // OnAudioDeviceStateChanged, doesn't seem to work for muting.
        // OnAudioPublishStateChanged doesn't seem to work either... so as far as I know, we're stuck using this "deprecated" function.
        mRtcEngine.OnUserMutedAudio += (uint uid, bool muted) =>
        {
            //Debug.Log("OnUserMutedAudio: " + uid + " : " + muted );
            if( muted )
                playerDidMute( uid );
            else
                playerDidUnmute( uid );
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

                usersTalkingCooldown[uid] = 0;   // This "talking" value is up to date.
            }

            // See Update() for logic which deals with assuming they aren't talking, when we haven't heard from them in long enough.
        };



    }


	private void Update()
	{
        // We have to look for users who stopped talking, because of the way Agora's callback works.
		foreach( KeyValuePair<uint, float> kvp in new Dictionary<uint, float>(usersTalkingCooldown) )   // new dictionary needed so we can modify during iteration
        {
            usersTalkingCooldown[kvp.Key] = kvp.Value + Time.deltaTime;   // add deltatime to cooldowns
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

    public void JoinChannel(string name)
    {
        mRtcEngine.JoinChannel(name, "extra", 0);

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

        mRtcEngine.LeaveChannel();

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
        }
    }


}
