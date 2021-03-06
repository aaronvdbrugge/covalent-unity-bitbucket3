
//Include based on platform
using UnityEngine;


#if PLATFORM_ANDROID
using Covalent.Scripts.Util.Native_Proxy.Android;

#elif UNITY_IOS
using Covalent.Scripts.Util.Native_Proxy.IOS;

#endif

namespace Covalent.Scripts.Util.Native_Proxy
{
    public static  class NativeProxy
    {
        //private static INativeProxy messageProxy = new AndroidNativeProxy();
        // NOTE! Can't do it this way. Caused on-device NullReferenceExceptions.
        // I think this is due to a C# quirk where static member initialization
        // is implementation-dependent.
        // So, it's the compiler's fault.
        //  https://stackoverflow.com/questions/3580171/static-member-variable-not-being-initialized-in-release-compiler-clr-bug

        // Instead, I'll just make messageProxy into a getter to ensure it's initialized.
        private static INativeProxy messageProxy
        {
            get
            {
                if( _messageProxy == null )
                {
                    //Instantiate messageProxy to an instance of INativeProxy based on the platform
                    #if PLATFORM_ANDROID
                        _messageProxy = new AndroidNativeProxy();
                    #elif UNITY_IOS
                        _messageProxy = new IOSNativeProxy();
                    #endif
                }
                return _messageProxy;
            }
        }
        private static INativeProxy _messageProxy;



        /**
         * <summary>Notifies about the number of users currently in a photon room</summary>
         * <param name="unityJsonList"> List of current players serialized to strings </param>
         * <param name="count"> Number of users </param>
         */
        public static void UpdatePlayersInRoom(string[] unityJsonList, int count)
        {
            messageProxy.UpdatePlayersInRoom(unityJsonList, count);
        }
        /**
         * <summary>To be called when a player mutes their mic</summary>
         * <param name="player_id">The Id of the player that muted the microphone</param>
         */
        public static void PlayerDidMute(uint player_id)
        {
            messageProxy.PlayerDidMute(player_id);
        }
        /**
         * <summary>To be called when a player unmutes their mic</summary>
         * <param name="player_id">The Id of the player that unmuted the microphone</param>
         */
        public static void PlayerDidUnmute(uint player_id)
        {
            messageProxy.PlayerDidUnmute(player_id);
        }
        /**
         * <summary>Notifies when a player begins talking</summary>
         * <param name="player_id">Th Id of the player that started talking</param>
         */
        public static void PlayerStartedTalking(uint player_id)
        {
            messageProxy.PlayerStartedTalking(player_id);
        }
        /**
         * <summary>Notifies when a player stops talking</summary>
         * <param name="player_id">Th Id of the player that stopped talking</param>
         */
        public static void PlayerEndedTalking(uint player_id)
        {
            messageProxy.PlayerEndedTalking(player_id);
        }
        /**
         * <summary>Notifies when the user leaves the game due to network connection issues</summary>
         */
        public static void PlayerDidLeaveGame()
        {
            messageProxy.PlayerDidLeaveGame();
        }
        /**
         * <summary>Failure to connect to photon servers</summary>
         * <param name="error">Error message for the failure to connect</param>
         */
        public static void FailureToConnect(string error)
        {
            messageProxy.FailureToConnect(error);
        }
        /**
         * <summary>Failure to join a photon room</summary>
         * <param name="error">Error message</param>
         */
        public static void FailureToJoinRoom(string error)
        {
            messageProxy.FailureToJoinRoom(error);
        }
        /**
         * <summary>Failure to connect to Agora servers</summary>
         * <param name="error">Error message</param>
         */
        public static void FailureToConnectAgora(string error)
        {
            messageProxy.FailureToConnectAgora(error);
        }
        /**
         * <summary>Notifies that the device has not granted the app Microphone permissions</summary>
         */
        public static void MissingMicPermission()
        {
            messageProxy.MissingMicPermission();
        }
    }
}