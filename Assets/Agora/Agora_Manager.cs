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
    private static extern void _playerDidUnmute(uint player_id) ;

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






    public Text logs;
    public IRtcEngine mRtcEngine = null;
    public AudioRecordingDeviceManager audio_manager;
    public uint myUid;  //save our own uid in ChannelOnJoinChannelSuccess

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
